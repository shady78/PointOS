/**
 * PointOS - Point of Sale System
 * Main JavaScript file for common functionality
 */

// Initialize components when DOM is fully loaded
document.addEventListener('DOMContentLoaded', function () {
    initializeTooltips();
    initializePopovers();
    initializeSidebar();
    initializeDataTables();
    initializeFormValidation();
    initializeImagePreviews();
    initializeDropdownActions();
    initializeSearchFunctionality();
});

/**
 * Initialize Bootstrap tooltips
 */
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

/**
 * Initialize Bootstrap popovers
 */
function initializePopovers() {
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

/**
 * Initialize sidebar functionality
 */
function initializeSidebar() {
    const toggleSidebar = document.getElementById('toggle-sidebar');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('main-content');

    if (toggleSidebar && sidebar && mainContent) {
        // Toggle sidebar on button click
        toggleSidebar.addEventListener('click', function () {
            sidebar.classList.toggle('active');
            mainContent.classList.toggle('expanded');
        });

        // Handle responsive behavior
        function handleResize() {
            if (window.innerWidth < 992) {
                sidebar.classList.remove('active');
                mainContent.classList.remove('expanded');
            } else {
                sidebar.classList.add('active');
                mainContent.classList.add('expanded');
            }
        }

        // Run on load and resize
        window.addEventListener('resize', handleResize);
        handleResize();
    }
}

/**
 * Initialize DataTables if the library is available
 */
function initializeDataTables() {
    if (typeof $.fn.DataTable !== 'undefined') {
        $('.datatable').each(function () {
            $(this).DataTable({
                responsive: true,
                language: {
                    search: "",
                    searchPlaceholder: "Search...",
                    lengthMenu: "_MENU_ items per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ items",
                    infoEmpty: "Showing 0 to 0 of 0 items",
                    infoFiltered: "(filtered from _MAX_ total items)"
                },
                dom: '<"top"lf>rt<"bottom"ip><"clear">',
                pageLength: 10,
                lengthMenu: [5, 10, 25, 50, 100]
            });
        });
    }
}

/**
 * Initialize form validation
 */
function initializeFormValidation() {
    // Get all forms with the 'needs-validation' class
    const forms = document.querySelectorAll('.needs-validation');

    // Loop over them and prevent submission if invalid
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');
        }, false);
    });
}

/**
 * Initialize image preview for file inputs
 */
function initializeImagePreviews() {
    const imageInputs = document.querySelectorAll('.image-upload-input');

    imageInputs.forEach(input => {
        input.addEventListener('change', function () {
            const previewContainer = document.querySelector(this.dataset.preview);

            if (previewContainer) {
                if (this.files && this.files[0]) {
                    const reader = new FileReader();

                    reader.onload = function (e) {
                        previewContainer.src = e.target.result;
                    }

                    reader.readAsDataURL(this.files[0]);
                }
            }
        });
    });
}

/**
 * Initialize dropdown actions
 */
function initializeDropdownActions() {
    // Delete confirmation for any delete link
    const deleteLinks = document.querySelectorAll('.delete-link, .delete-item');

    deleteLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });

    // Modal delete confirmation
    const deleteButtons = document.querySelectorAll('[data-action="delete"]');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const itemId = this.dataset.id;
            const itemName = this.dataset.name;
            const deleteUrl = this.dataset.url;

            // If we have a modal, use it
            const confirmModal = document.getElementById('deleteConfirmModal');

            if (confirmModal) {
                const itemNameEl = confirmModal.querySelector('.item-name');
                const confirmBtn = confirmModal.querySelector('.confirm-delete');

                if (itemNameEl) {
                    itemNameEl.textContent = itemName || 'this item';
                }

                if (confirmBtn) {
                    confirmBtn.dataset.id = itemId;
                    confirmBtn.href = deleteUrl;
                }

                // Show the modal
                const modal = new bootstrap.Modal(confirmModal);
                modal.show();
            } else {
                // Fall back to simple confirmation
                if (confirm(`Are you sure you want to delete ${itemName || 'this item'}? This action cannot be undone.`)) {
                    window.location.href = deleteUrl;
                }
            }
        });
    });
}

/**
 * Initialize search functionality
 */
function initializeSearchFunctionality() {
    const searchInputs = document.querySelectorAll('.search-input, .table-search');

    searchInputs.forEach(input => {
        input.addEventListener('keyup', function () {
            const searchTerm = this.value.toLowerCase();
            const tableId = this.dataset.table;

            if (tableId) {
                const table = document.getElementById(tableId);

                if (table) {
                    const rows = table.querySelectorAll('tbody tr');

                    rows.forEach(row => {
                        const text = row.textContent.toLowerCase();

                        if (text.indexOf(searchTerm) > -1) {
                            row.style.display = '';
                        } else {
                            row.style.display = 'none';
                        }
                    });
                }
            }
        });
    });
}

/**
 * Handle checkbox selection in tables
 * @param {string} tableId - ID of the table
 * @param {string} selectAllId - ID of the "select all" checkbox
 * @param {string} checkboxClass - Class of the individual checkboxes
 */
function initializeTableSelection(tableId, selectAllId, checkboxClass) {
    const table = document.getElementById(tableId);
    const selectAll = document.getElementById(selectAllId);

    if (table && selectAll) {
        // Handle "select all" checkbox
        selectAll.addEventListener('change', function () {
            const checkboxes = table.querySelectorAll('.' + checkboxClass);
            checkboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });

            updateBulkActionState();
        });

        // Handle individual checkboxes
        table.addEventListener('change', function (e) {
            if (e.target.classList.contains(checkboxClass)) {
                const checkboxes = table.querySelectorAll('.' + checkboxClass);
                const allChecked = Array.from(checkboxes).every(cb => cb.checked);

                selectAll.checked = allChecked;
                updateBulkActionState();
            }
        });
    }
}

/**
 * Update bulk action button state based on checkbox selection
 */
function updateBulkActionState() {
    const bulkActionButtons = document.querySelectorAll('.bulk-action-btn');
    const checkedBoxes = document.querySelectorAll('input[type="checkbox"].item-checkbox:checked');

    bulkActionButtons.forEach(button => {
        button.disabled = checkedBoxes.length === 0;
    });
}

/**
 * Show an alert message
 * @param {string} message - Message to display
 * @param {string} type - Alert type (success, danger, warning, info)
 * @param {boolean} dismissible - Whether the alert can be dismissed
 * @param {number} duration - Auto-hide duration in milliseconds (0 for no auto-hide)
 */
function showAlert(message, type = 'info', dismissible = true, duration = 5000) {
    const alertsContainer = document.getElementById('alerts-container');

    if (!alertsContainer) {
        console.error('Alerts container not found. Add a div with id="alerts-container" to your page.');
        return;
    }

    // Create alert element
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} ${dismissible ? 'alert-dismissible fade show' : ''}`;
    alert.role = 'alert';

    // Set message
    alert.innerHTML = message;

    // Add dismiss button if dismissible
    if (dismissible) {
        const closeButton = document.createElement('button');
        closeButton.type = 'button';
        closeButton.className = 'btn-close';
        closeButton.setAttribute('data-bs-dismiss', 'alert');
        closeButton.setAttribute('aria-label', 'Close');

        alert.appendChild(closeButton);
    }

    // Add to container
    alertsContainer.appendChild(alert);

    // Auto-hide after duration
    if (duration > 0) {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, duration);
    }
}

/**
 * Format currency based on locale
 * @param {number} amount - Amount to format
 * @param {string} currency - Currency code (default: USD)
 * @param {string} locale - Locale (default: en-US)
 * @returns {string} Formatted currency string
 */
function formatCurrency(amount, currency = 'USD', locale = 'en-US') {
    return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currency
    }).format(amount);
}

/**
 * Format date based on locale
 * @param {Date|string} date - Date to format
 * @param {string} locale - Locale (default: en-US)
 * @param {object} options - Intl.DateTimeFormat options
 * @returns {string} Formatted date string
 */
function formatDate(date, locale = 'en-US', options = { dateStyle: 'medium' }) {
    const dateObj = date instanceof Date ? date : new Date(date);
    return new Intl.DateTimeFormat(locale, options).format(dateObj);
}

/**
 * Debounce function to limit how often a function can be called
 * @param {Function} func - Function to debounce
 * @param {number} delay - Delay in milliseconds
 * @returns {Function} Debounced function
 */
function debounce(func, delay = 300) {
    let timerId;
    return function (...args) {
        clearTimeout(timerId);
        timerId = setTimeout(() => {
            func.apply(this, args);
        }, delay);
    };
}

/**
 * Load content from URL and insert into target element
 * @param {string} url - URL to load content from
 * @param {string|HTMLElement} target - Target element ID or element
 * @param {Function} callback - Callback function to run after content is loaded
 */
function loadContent(url, target, callback = null) {
    const targetElement = typeof target === 'string' ? document.getElementById(target) : target;

    if (!targetElement) {
        console.error('Target element not found:', target);
        return;
    }

    // Show loading indicator
    targetElement.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>';

    // Fetch content
    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.text();
        })
        .then(html => {
            targetElement.innerHTML = html;

            if (callback && typeof callback === 'function') {
                callback(html);
            }
        })
        .catch(error => {
            targetElement.innerHTML = `<div class="alert alert-danger">Error loading content: ${error.message}</div>`;
            console.error('Error loading content:', error);
        });
}

/**
 * Submit form via AJAX
 * @param {HTMLFormElement} form - Form element to submit
 * @param {Function} successCallback - Function to call on success
 * @param {Function} errorCallback - Function to call on error
 */
function submitFormAjax(form, successCallback = null, errorCallback = null) {
    // Validate form
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    // Create FormData object
    const formData = new FormData(form);

    // Disable form while submitting
    const submitBtn = form.querySelector('[type="submit"]');
    const originalBtnText = submitBtn ? submitBtn.innerHTML : '';

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Submitting...';
    }

    // Send request
    fetch(form.action, {
        method: form.method,
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (successCallback && typeof successCallback === 'function') {
                successCallback(data);
            } else {
                // Default success handling
                showAlert(data.message || 'Operation completed successfully', 'success');

                // Redirect if URL provided
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                }
            }
        })
        .catch(error => {
            if (errorCallback && typeof errorCallback === 'function') {
                errorCallback(error);
            } else {
                // Default error handling
                showAlert('Error: ' + error.message, 'danger');
            }
        })
        .finally(() => {
            // Re-enable form
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalBtnText;
            }
        });
}