// E-Commerce 2tlob Client Interactions

document.addEventListener('DOMContentLoaded', function () {
    // Update live cart item count in navbar badge
    const cartBadge = document.getElementById('navbarCartBadge');
    if (cartBadge) {
        fetch('/Cart/Count')
            .then(res => res.json())
            .then(data => {
                if (data && data.count > 0) {
                    cartBadge.textContent = data.count;
                    cartBadge.classList.remove('d-none');
                } else {
                    cartBadge.classList.add('d-none');
                }
            })
            .catch(() => {
                // Ignore network errors on badge fetch
            });
    }

    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 5000);
    });

    // Image preview helper for product creation/edit
    const imageInput = document.querySelector('input[type="file"][name="ImageFile"]');
    const imagePreview = document.getElementById('imagePreviewContainer');
    if (imageInput && imagePreview) {
        imageInput.addEventListener('change', function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    imagePreview.innerHTML = `<img src="${e.target.result}" class="img-fluid rounded border shadow-sm" style="max-height: 200px;" alt="Selected Preview" />`;
                };
                reader.readAsDataURL(file);
            }
        });
    }
});
