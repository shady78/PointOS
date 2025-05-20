/**
 * Product selection and management for the Order form
 */
const OrderProducts = {
    // Store for selected products
    selectedProducts: [],

    // Initialize the product selection functionality
    init: function () {
        console.log("Initializing OrderProducts...");

        // Initialize event handlers
        this.initEventHandlers();

        // If editing an existing order, load the existing products
        this.loadExistingProducts();

        // Update order totals initially
        this.updateOrderTotals();
    },

    // Set up event handlers
    initEventHandlers: function () {
        // Product search in modal
        $('#searchProducts').on('input', function () {
            const searchTerm = $(this).val().trim();
            if (searchTerm.length >= 2) {
                OrderProducts.searchProducts(searchTerm);
            } else if (searchTerm.length === 0) {
                OrderProducts.searchProducts(''); // Show all if cleared
            }
        });

        // Add selected products button
        $('#addSelectedProducts').on('click', function () {
            OrderProducts.addSelectedProductsFromModal();
            $('#addProductModal').modal('hide');
        });

        // Handle "Same as Billing" checkbox
        $('#sameAsBilling').on('change', function () {
            if ($(this).is(':checked')) {
                // Copy billing address to shipping address
                $('#ShippingAddress').val($('#BillingAddress').val());
                $('#ShippingCity').val($('#BillingCity').val());
                $('#ShippingPostalCode').val($('#BillingPostalCode').val());
                $('#ShippingState').val($('#BillingState').val());
                $('#ShippingCountry').val($('#BillingCountry').val());

                // Disable shipping fields
                $('#shippingAddressFields input, #shippingAddressFields select').prop('disabled', true);
            } else {
                // Enable shipping fields
                $('#shippingAddressFields input, #shippingAddressFields select').prop('disabled', false);
            }
        });

        // Update shipping rate based on shipping method
        $('#ShippingMethod').on('change', function () {
            const method = $(this).val();
            let rate = 0;

            switch (method) {
                case 'Standard Rate':
                    rate = 10.00;
                    break;
                case 'Express Delivery':
                    rate = 25.00;
                    break;
                case 'Next Day Delivery':
                    rate = 35.00;
                    break;
                case 'In-Store Pickup':
                    rate = 0;
                    break;
                default:
                    rate = 0;
            }

            // Update shipping rate input
            if ($('#shippingRateInput').length) {
                $('#shippingRateInput').val(rate.toFixed(2));
            } else {
                $('#hiddenShippingRate').val(rate);
            }

            OrderProducts.updateOrderTotals();
        });

        // Handle increasing quantity buttons
        $(document).on('click', '.btn-increase-qty', function () {
            const input = $(this).siblings('input.item-quantity');
            const productId = $(this).closest('[data-product-id]').data('product-id');
            const currentValue = parseInt(input.val()) || 1;

            // Find the product in the array
            const product = OrderProducts.findProductById(productId);
            if (product && currentValue < product.stock) {
                input.val(currentValue + 1);
                input.trigger('change');
            } else if (!product) {
                // If product not found in our array, just increment
                input.val(currentValue + 1);
                input.trigger('change');
            }
        });

        // Handle decreasing quantity buttons
        $(document).on('click', '.btn-decrease-qty', function () {
            const input = $(this).siblings('input.item-quantity');
            const currentValue = parseInt(input.val()) || 2;
            if (currentValue > 1) {
                input.val(currentValue - 1);
                input.trigger('change');
            }
        });

        // Handle removing order items
        $(document).on('click', '.btn-remove-item', function () {
            const container = $(this).closest('[data-product-id]');
            const productId = container.data('product-id');

            // Remove the product from internal array
            OrderProducts.removeProduct(productId);

            // Remove the UI element
            container.remove();

            // Update order totals
            OrderProducts.updateOrderTotals();

            // Show no products message if needed
            if (OrderProducts.selectedProducts.length === 0) {
                $('#noProductsMessage').show();
            }
        });

        // Handle quantity or price changes
        $(document).on('change', '.item-quantity, .item-price', function () {
            OrderProducts.updateItemSubtotal(this);
        });

        // Monitor VAT and shipping changes for totals
        $('#vatRateInput, #shippingRateInput').on('change', function () {
            OrderProducts.updateOrderTotals();
        });
    },

    // Load existing products when editing an order
    loadExistingProducts: function () {
        // Check if we're in edit mode by looking for existing order items
        const existingItems = $('#orderItemsTableBody tr[data-item-id]');
        if (existingItems.length > 0) {
            console.log("Loading existing products...");

            // For each existing item, add it to our internal array
            existingItems.each(function () {
                const row = $(this);
                const itemId = row.data('item-id');
                const productId = parseInt(row.find('input[name$=".ProductId"]').val());
                const productName = row.find('input[name$=".ProductName"]').val();
                const sku = row.find('input[name$=".SKU"]').val();
                const price = parseFloat(row.find('.item-price').val());
                const quantity = parseInt(row.find('.item-quantity').val());
                const imageUrl = row.find('img').attr('src');

                // Add to internal array
                OrderProducts.selectedProducts.push({
                    id: productId,
                    itemId: itemId,
                    name: productName,
                    sku: sku,
                    price: price,
                    quantity: quantity,
                    imageUrl: imageUrl,
                    // We don't know the stock, so assume it's enough
                    stock: 100
                });
            });

            // Update the UI
            OrderProducts.updateOrderTotals();
        }
    },

    // Search for products using the API
    searchProducts: function (searchTerm) {
        const tbody = $('#productsTable tbody');

        // Show loading indicator
        tbody.html('<tr><td colspan="6" class="text-center py-3"><div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div> Searching products...</td></tr>');

        // Make API call
        $.ajax({
            url: '/api/ProductApi/Search',
            type: 'GET',
            data: { searchTerm: searchTerm },
            dataType: 'json',
            success: function (response) {
                OrderProducts.populateProductsTable(response.data);
            },
            error: function () {
                tbody.html('<tr><td colspan="6" class="text-center py-3 text-danger">Error loading products. Please try again.</td></tr>');
            }
        });
    },

    // Populate products table with search results
    populateProductsTable: function (products) {
        const tbody = $('#productsTable tbody');
        tbody.empty();

        if (!products || products.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="6" class="text-center py-3">No products found</td>
                </tr>
            `);
            return;
        }

        // Get IDs of already selected products
        const selectedIds = OrderProducts.selectedProducts.map(p => p.id);

        products.forEach(product => {
            // Check if this product is already selected
            const isInOrder = selectedIds.includes(product.id);

            tbody.append(`
                <tr>
                    <td class="text-center">
                        <div class="form-check">
                            <input class="form-check-input product-checkbox" type="checkbox" value="${product.id}" ${isInOrder ? 'checked disabled' : ''}>
                        </div>
                    </td>
                    <td>
                        <img src="${product.imageUrl}" alt="${product.name}" width="50" height="50" class="rounded border object-fit-cover">
                    </td>
                    <td>${product.name}</td>
                    <td>${product.sku}</td>
                    <td>$${product.price.toFixed(2)}</td>
                    <td>${product.stock} ${product.lowStock ? '<span class="badge bg-warning">Low stock</span>' : ''}</td>
                </tr>
            `);
        });
    },

    // Add products selected in the modal to the order
    addSelectedProductsFromModal: function () {
        // Get all checked products
        const checkedProducts = $('#productsTable .product-checkbox:checked:not(:disabled)');
        let productsAdded = false;

        if (checkedProducts.length === 0) {
            alert("Please select at least one product to add.");
            return;
        }

        // Get all product IDs
        const productIds = checkedProducts.map(function () {
            return $(this).val();
        }).get().join(',');

        // Fetch full product details from API
        $.ajax({
            url: '/api/ProductApi/GetByIds',
            type: 'GET',
            data: { ids: productIds },
            dataType: 'json',
            success: function (response) {
                if (response.success && response.data) {
                    // Add each product to the order
                    response.data.forEach(product => {
                        OrderProducts.addProduct(product);
                        productsAdded = true;
                    });

                    // Update UI
                    if (productsAdded) {
                        // Hide no products message
                        $('#noProductsMessage').hide();

                        // Update order totals
                        OrderProducts.updateOrderTotals();
                    }
                }
            },
            error: function () {
                alert("Error adding products. Please try again.");
            }
        });
    },

    // Add a single product to the order
    addProduct: function (product) {
        // Check if product is already in the order
        if (this.selectedProducts.some(p => p.id === product.id)) {
            return false;
        }

        // Add to internal array with quantity 1
        const newProduct = {
            ...product,
            quantity: 1
        };

        this.selectedProducts.push(newProduct);

        // Add to UI - check if we're using table or card view
        if ($('#selectedProductsList').length) {
            // Card view (Create form)
            this.addProductCard(newProduct);
        } else if ($('#orderItemsTableBody').length) {
            // Table view (Edit form)
            this.addProductTableRow(newProduct);
        }

        return true;
    },

    // Add a product card to the UI (for Create form)
    addProductCard: function (product) {
        const container = $('#selectedProductsList');
        const index = this.selectedProducts.length - 1;

        container.append(`
            <div class="col-md-6" data-product-id="${product.id}">
                <div class="card product-card position-relative mb-3">
                    <button type="button" class="btn btn-sm btn-remove text-danger" 
                            aria-label="Remove">
                        <i class="fas fa-times"></i>
                    </button>
                    <div class="card-body">
                        <div class="d-flex">
                            <div class="me-3">
                                <img src="${product.imageUrl}" alt="${product.name}" 
                                     class="rounded border object-fit-cover" width="60" height="60">
                            </div>
                            <div class="flex-grow-1">
                                <h6 class="card-title mb-1">${product.name}</h6>
                                <p class="card-text text-muted small mb-2">SKU: ${product.sku}</p>
                                <div class="d-flex justify-content-between align-items-center">
                                    <span class="fw-medium">$${product.price.toFixed(2)}</span>
                                    <div class="input-group input-group-sm" style="width: 100px;">
                                        <button class="btn btn-outline-secondary btn-decrease-qty" type="button">-</button>
                                        <input type="number" class="form-control text-center item-quantity" 
                                               min="1" max="${product.stock}" value="${product.quantity}">
                                        <button class="btn btn-outline-secondary btn-increase-qty" type="button">+</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <input type="hidden" name="OrderItems[${index}].ProductId" value="${product.id}">
                <input type="hidden" name="OrderItems[${index}].ProductName" value="${product.name}">
                <input type="hidden" name="OrderItems[${index}].SKU" value="${product.sku}">
                <input type="hidden" name="OrderItems[${index}].ProductImageUrl" value="${product.imageUrl}">
                <input type="hidden" name="OrderItems[${index}].Quantity" value="${product.quantity}" class="item-quantity-hidden">
                <input type="hidden" name="OrderItems[${index}].UnitPrice" value="${product.price}" class="item-price-hidden">
                <input type="hidden" name="OrderItems[${index}].Discount" value="0">
                <input type="hidden" name="OrderItems[${index}].DeliveryDate" value="${new Date().toISOString().split('T')[0]}">
            </div>
        `);

        // Connect quantity input to hidden field
        const quantityInput = container.find(`[data-product-id="${product.id}"] .item-quantity`);
        const quantityHidden = container.find(`[data-product-id="${product.id}"] .item-quantity-hidden`);

        quantityInput.on('change', function () {
            const qty = parseInt($(this).val()) || 1;
            quantityHidden.val(qty);

            // Update the product in our array
            const productIndex = OrderProducts.selectedProducts.findIndex(p => p.id === product.id);
            if (productIndex !== -1) {
                OrderProducts.selectedProducts[productIndex].quantity = qty;
            }

            OrderProducts.updateOrderTotals();
        });
    },

    // Add a product table row (for Edit form)
    addProductTableRow: function (product) {
        const tbody = $('#orderItemsTableBody');
        const rowCount = tbody.find('tr').not('#noItemsRow').length;

        // Remove "no items" row if it exists
        $('#noItemsRow').remove();

        tbody.append(`
            <tr data-product-id="${product.id}" data-item-id="new-${rowCount}">
                <td>
                    <div class="d-flex align-items-center">
                        <img src="${product.imageUrl}" alt="${product.name}" width="40" height="40" class="rounded border me-2 object-fit-cover">
                        <span>${product.name}</span>
                    </div>
                    <input type="hidden" name="OrderItems[${rowCount}].Id" value="0" />
                    <input type="hidden" name="OrderItems[${rowCount}].ProductId" value="${product.id}" />
                    <input type="hidden" name="OrderItems[${rowCount}].ProductName" value="${product.name}" />
                    <input type="hidden" name="OrderItems[${rowCount}].ProductImageUrl" value="${product.imageUrl}" />
                </td>
                <td>
                    ${product.sku}
                    <input type="hidden" name="OrderItems[${rowCount}].SKU" value="${product.sku}" />
                </td>
                <td>
                    <div class="input-group input-group-sm" style="width: 120px;">
                        <span class="input-group-text">$</span>
                        <input type="number" class="form-control item-price" 
                              name="OrderItems[${rowCount}].UnitPrice" value="${product.price.toFixed(2)}" 
                              min="0" step="0.01" />
                    </div>
                </td>
                <td>
                    <div class="input-group input-group-sm">
                        <button class="btn btn-outline-secondary btn-decrease-qty" type="button">-</button>
                        <input type="number" class="form-control text-center item-quantity" 
                              name="OrderItems[${rowCount}].Quantity" value="1" 
                              min="1" max="${product.stock}" />
                        <button class="btn btn-outline-secondary btn-increase-qty" type="button">+</button>
                    </div>
                </td>
                <td>
                    <span class="item-subtotal">$${product.price.toFixed(2)}</span>
                    <input type="hidden" name="OrderItems[${rowCount}].Discount" value="0" />
                    <input type="hidden" name="OrderItems[${rowCount}].DeliveryDate" value="${new Date().toISOString().split('T')[0]}" />
                </td>
                <td>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-remove-item">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `);
    },

    // Remove a product from the order
    removeProduct: function (productId) {
        // Remove from internal array
        const index = this.selectedProducts.findIndex(p => p.id === parseInt(productId));
        if (index !== -1) {
            this.selectedProducts.splice(index, 1);
            return true;
        }
        return false;
    },

    // Find a product by ID in the internal array
    findProductById: function (productId) {
        return this.selectedProducts.find(p => p.id === parseInt(productId));
    },

    // Update subtotal for a specific item
    updateItemSubtotal: function (element) {
        const row = $(element).closest('[data-product-id]');
        const productId = row.data('product-id');

        // Different approaches for table vs card view
        if (row.is('tr')) {
            // Table row (Edit form)
            const priceInput = row.find('.item-price');
            const quantityInput = row.find('.item-quantity');

            const price = parseFloat(priceInput.val()) || 0;
            const quantity = parseInt(quantityInput.val()) || 1;

            const subtotal = price * quantity;
            row.find('.item-subtotal').text('$' + subtotal.toFixed(2));

            // Update the product in our array
            const productIndex = this.selectedProducts.findIndex(p => p.id === parseInt(productId));
            if (productIndex !== -1) {
                this.selectedProducts[productIndex].price = price;
                this.selectedProducts[productIndex].quantity = quantity;
            }
        } else {
            // Card (Create form)
            const priceDisplay = row.find('.fw-medium');
            const quantityInput = row.find('.item-quantity');
            const quantityHidden = row.find('.item-quantity-hidden');

            // Get price from the product array
            const product = this.findProductById(productId);
            if (product) {
                const quantity = parseInt(quantityInput.val()) || 1;
                product.quantity = quantity;

                // Update hidden field
                quantityHidden.val(quantity);
            }
        }

        // Update overall order totals
        this.updateOrderTotals();
    },

    // Update order totals
    updateOrderTotals: function () {
        // Calculate subtotal
        let subtotal = 0;

        // Sum up all products
        this.selectedProducts.forEach(product => {
            subtotal += product.price * product.quantity;
        });

        // Get VAT rate and shipping rate
        let vatRate = 5; // Default
        let shippingRate = 0;

        // Check for specific inputs
        if ($('#vatRateInput').length) {
            vatRate = parseFloat($('#vatRateInput').val()) || 0;
        } else if ($('#hiddenVAT').length) {
            vatRate = parseFloat($('#hiddenVAT').val()) || 0;
        }

        if ($('#shippingRateInput').length) {
            shippingRate = parseFloat($('#shippingRateInput').val()) || 0;
        } else if ($('#hiddenShippingRate').length) {
            shippingRate = parseFloat($('#hiddenShippingRate').val()) || 0;
        }

        // Calculate VAT amount
        const vatAmount = subtotal * (vatRate / 100);

        // Calculate grand total
        const grandTotal = subtotal + vatAmount + shippingRate;

        // Update UI elements
        if ($('#subtotalValue').length) {
            $('#subtotalValue').text('$' + subtotal.toFixed(2));
        }

        if ($('#vatRate').length) {
            $('#vatRate').text(vatRate);
        }

        if ($('#vatValue').length) {
            $('#vatValue').text('$' + vatAmount.toFixed(2));
        }

        if ($('#shippingValue').length) {
            $('#shippingValue').text('$' + shippingRate.toFixed(2));
        }

        if ($('#totalCostValue').length) {
            $('#totalCostValue').text('$' + grandTotal.toFixed(2));
        }

        // Update hidden fields
        if ($('#hiddenSubtotal').length) {
            $('#hiddenSubtotal').val(subtotal);
        }

        if ($('#hiddenVAT').length) {
            $('#hiddenVAT').val(vatRate);
        }

        if ($('#hiddenShippingRate').length) {
            $('#hiddenShippingRate').val(shippingRate);
        }

        if ($('#hiddenGrandTotal').length) {
            $('#hiddenGrandTotal').val(grandTotal);
        }
    }
};

// Initialize when document is ready
$(document).ready(function () {
    OrderProducts.init();

    // Show loading message in product modal when it opens
    $('#addProductModal').on('show.bs.modal', function () {
        const tbody = $('#productsTable tbody');
        tbody.html('<tr><td colspan="6" class="text-center py-3">Start typing to search products</td></tr>');
    });
});