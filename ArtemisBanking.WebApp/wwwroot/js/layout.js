/* ============================================
   ARTEMIS BANKING - Layout JavaScript
   ============================================ */

(function () {
    'use strict';

    // ===== SIDEBAR FUNCTIONALITY =====
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const mainContent = document.getElementById('mainContent');
    const navbar = document.getElementById('navbar');
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const sidebarOverlay = document.getElementById('sidebarOverlay');

    // Toggle sidebar en desktop
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('sidebar-collapsed');
            navbar.classList.toggle('sidebar-collapsed');

            // Guardar preferencia en localStorage
            const isCollapsed = sidebar.classList.contains('collapsed');
            localStorage.setItem('artemis-sidebar-collapsed', isCollapsed);
        });

        // Restaurar estado del sidebar desde localStorage
        const savedState = localStorage.getItem('artemis-sidebar-collapsed');
        if (savedState === 'true') {
            sidebar.classList.add('collapsed');
            mainContent.classList.add('sidebar-collapsed');
            navbar.classList.add('sidebar-collapsed');
        }
    }

    // Toggle sidebar en mobile
    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function () {
            this.classList.toggle('active');
            sidebar.classList.toggle('show');
            sidebarOverlay.classList.toggle('show');
            document.body.style.overflow = sidebar.classList.contains('show') ? 'hidden' : '';
        });
    }

    // Cerrar sidebar al hacer click en overlay
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function () {
            sidebar.classList.remove('show');
            this.classList.remove('show');
            mobileMenuToggle.classList.remove('active');
            document.body.style.overflow = '';
        });
    }

    // ===== SIDEBAR SUBMENU FUNCTIONALITY =====
    const sidebarMenuItems = document.querySelectorAll('.ab-sidebar-menu-item');

    sidebarMenuItems.forEach(item => {
        const link = item.querySelector('.ab-sidebar-menu-link');
        const submenu = item.querySelector('.ab-sidebar-submenu');

        if (submenu && link) {
            link.addEventListener('click', function (e) {
                // Solo prevenir default si tiene submenu
                e.preventDefault();

                // Cerrar otros submenus
                sidebarMenuItems.forEach(otherItem => {
                    if (otherItem !== item) {
                        otherItem.classList.remove('show');
                    }
                });

                // Toggle este submenu
                item.classList.toggle('show');
            });
        }
    });

    // ===== NAVBAR DROPDOWN FUNCTIONALITY =====
    const dropdowns = document.querySelectorAll('.ab-nav-item.dropdown, .dropdown');

    dropdowns.forEach(dropdown => {
        const trigger = dropdown.querySelector('.ab-nav-link, .ab-nav-user');
        const menu = dropdown.querySelector('.ab-dropdown-menu');

        if (trigger && menu) {
            trigger.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                // Cerrar otros dropdowns
                dropdowns.forEach(otherDropdown => {
                    if (otherDropdown !== dropdown) {
                        otherDropdown.classList.remove('show');
                    }
                });

                // Toggle este dropdown
                dropdown.classList.toggle('show');
            });
        }
    });

    // Cerrar dropdowns al hacer click fuera
    document.addEventListener('click', function (e) {
        dropdowns.forEach(dropdown => {
            if (!dropdown.contains(e.target)) {
                dropdown.classList.remove('show');
            }
        });
    });

    // ===== MOBILE MENU FUNCTIONALITY (Cliente) =====
    const clientMenu = document.getElementById('clientMenu');
    if (clientMenu && mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function () {
            this.classList.toggle('active');
            clientMenu.classList.toggle('show');
        });
    }

    // ===== SCROLL BEHAVIOR =====
    let lastScrollTop = 0;
    window.addEventListener('scroll', function () {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        // Ocultar navbar al hacer scroll down, mostrar al hacer scroll up
        if (scrollTop > lastScrollTop && scrollTop > 100) {
            navbar.style.transform = 'translateY(-100%)';
        } else {
            navbar.style.transform = 'translateY(0)';
        }

        lastScrollTop = scrollTop;
    });

    // ===== FORM VALIDATION STYLING =====
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');
        }, false);
    });

    // ===== TOOLTIPS INITIALIZATION (si se usa Bootstrap) =====
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }

    // ===== AUTO-DISMISS ALERTS =====
    const alerts = document.querySelectorAll('.ab-alert[data-auto-dismiss]');
    alerts.forEach(alert => {
        const delay = parseInt(alert.dataset.autoDismiss) || 5000;
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transform = 'translateX(100%)';
            setTimeout(() => alert.remove(), 300);
        }, delay);
    });

    // ===== MONEY FORMAT INPUTS =====
    const moneyInputs = document.querySelectorAll('input[type="number"][data-money]');
    moneyInputs.forEach(input => {
        input.addEventListener('blur', function () {
            if (this.value) {
                const value = parseFloat(this.value);
                if (!isNaN(value)) {
                    this.value = value.toFixed(2);
                }
            }
        });
    });

    // ===== CARD NUMBER FORMAT =====
    const cardInputs = document.querySelectorAll('input[data-card-number]');
    cardInputs.forEach(input => {
        input.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\s+/g, '');
            let formattedValue = value.match(/.{1,4}/g)?.join(' ') || value;
            e.target.value = formattedValue;
        });
    });

    // ===== CONFIRMATION DIALOGS =====
    const confirmButtons = document.querySelectorAll('[data-confirm]');
    confirmButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const message = this.dataset.confirm || '¿Está seguro de realizar esta acción?';
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

    // ===== LOADING BUTTON STATES =====
    const loadingForms = document.querySelectorAll('form[data-loading]');
    loadingForms.forEach(form => {
        form.addEventListener('submit', function () {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Procesando...';
            }
        });
    });

    // ===== COPY TO CLIPBOARD =====
    const copyButtons = document.querySelectorAll('[data-copy]');
    copyButtons.forEach(button => {
        button.addEventListener('click', function () {
            const textToCopy = this.dataset.copy;
            navigator.clipboard.writeText(textToCopy).then(() => {
                // Mostrar feedback
                const originalText = this.innerHTML;
                this.innerHTML = '<i class="fas fa-check"></i> ¡Copiado!';
                setTimeout(() => {
                    this.innerHTML = originalText;
                }, 2000);
            });
        });
    });

    // ===== SMOOTH SCROLL =====
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href !== '') {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });

    // ===== RESPONSIVE TABLE WRAPPER =====
    const tables = document.querySelectorAll('table:not(.ab-table-responsive)');
    tables.forEach(table => {
        if (!table.parentElement.classList.contains('table-responsive')) {
            const wrapper = document.createElement('div');
            wrapper.className = 'table-responsive';
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        }
    });

    // ===== LAZY LOAD IMAGES =====
    if ('IntersectionObserver' in window) {
        const lazyImages = document.querySelectorAll('img[data-src]');
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    observer.unobserve(img);
                }
            });
        });

        lazyImages.forEach(img => imageObserver.observe(img));
    }

    // ===== PASSWORD TOGGLE VISIBILITY =====
    const passwordToggles = document.querySelectorAll('[data-password-toggle]');
    passwordToggles.forEach(toggle => {
        toggle.addEventListener('click', function () {
            const targetId = this.dataset.passwordToggle;
            const input = document.getElementById(targetId);
            if (input) {
                const type = input.type === 'password' ? 'text' : 'password';
                input.type = type;

                const icon = this.querySelector('i');
                if (icon) {
                    icon.classList.toggle('fa-eye');
                    icon.classList.toggle('fa-eye-slash');
                }
            }
        });
    });

    // ===== ACCOUNT NUMBER MASK =====
    function maskAccountNumber(number) {
        if (!number) return '';
        const str = number.toString();
        if (str.length <= 4) return str;
        return '•••• ' + str.slice(-4);
    }

    const maskedNumbers = document.querySelectorAll('[data-mask-account]');
    maskedNumbers.forEach(element => {
        const number = element.dataset.maskAccount;
        element.textContent = maskAccountNumber(number);
    });

    // ===== CONSOLE WELCOME MESSAGE =====
    console.log('%c🏦 Artemis Banking', 'color: #1B3B6F; font-size: 24px; font-weight: bold;');
    console.log('%cSistema de Banca en Línea v2.0', 'color: #00BFA5; font-size: 14px;');
    console.log('%c© 2025 ITLA - Todos los derechos reservados', 'color: #6C757D; font-size: 12px;');

})();