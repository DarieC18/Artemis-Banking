// auth.js - JavaScript para páginas de autenticación

document.addEventListener('DOMContentLoaded', function () {

    const passwordToggles = document.querySelectorAll('[data-password-toggle]');
    passwordToggles.forEach(toggle => {
        toggle.addEventListener('click', function () {
            const targetId = this.getAttribute('data-password-toggle');
            const input = document.getElementById(targetId);
            const icon = this.querySelector('i');

            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.remove('fa-eye');
                icon.classList.add('fa-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.remove('fa-eye-slash');
                icon.classList.add('fa-eye');
            }
        });
    });

    const authForms = document.querySelectorAll('.ab-auth-form');
    authForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitBtn = this.querySelector('[type="submit"]');
            if (submitBtn && !submitBtn.classList.contains('loading')) {
                submitBtn.classList.add('loading');
                submitBtn.disabled = true;

                const originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner"></span> Procesando...';

                setTimeout(() => {
                    if (document.querySelector('.ab-invalid-feedback')) {
                        submitBtn.classList.remove('loading');
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalText;
                    }
                }, 100);
            }
        });
    });

    const autoAlerts = document.querySelectorAll('[data-auto-dismiss]');
    autoAlerts.forEach(alert => {
        const delay = parseInt(alert.getAttribute('data-auto-dismiss')) || 5000;
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-10px)';
            setTimeout(() => alert.remove(), 500);
        }, delay);
    });

    const firstError = document.querySelector('.ab-invalid-feedback');
    if (firstError) {
        firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
});