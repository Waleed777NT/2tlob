document.addEventListener('DOMContentLoaded', function () {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    document.querySelectorAll('.js-add-to-cart').forEach(btn => {
        btn.addEventListener('click', async () => {
            const productId = btn.dataset.productId;
            const originalHtml = btn.innerHTML;
            btn.disabled = true;

            try {
                const res = await fetch('/Cart/AddToCart', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
                    body: `productId=${productId}&quantity=1`
                });

                if (res.status === 401) {
                    window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
                    return;
                }
                if (!res.ok) throw new Error(`Request failed with status ${res.status}`);

                const data = await res.json();
                btn.innerHTML = '<i class="bi bi-check2 me-1"></i>Added!';
                setTimeout(() => { btn.innerHTML = originalHtml; btn.disabled = false; }, 1500);

                const cartBadge = document.querySelector('.js-cart-count');
                if (cartBadge && typeof data.cartCount === 'number') {
                    cartBadge.textContent = data.cartCount;
                    cartBadge.classList.remove('d-none');
                }
            } catch (err) {
                console.error(err);
                btn.innerHTML = '<i class="bi bi-exclamation-triangle me-1"></i>Failed';
                setTimeout(() => { btn.innerHTML = originalHtml; btn.disabled = false; }, 2000);
                alert('Could not add this item to your cart. Please try again.');
            } finally {
                if (document.body.contains(btn)) btn.disabled = false;
            }
        });
    });

    document.querySelectorAll('.js-toggle-wishlist').forEach(btn => {
        btn.addEventListener('click', async () => {
            const productId = btn.dataset.productId;
            btn.disabled = true;

            try {
                const res = await fetch('/Wishlist/Toggle', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
                    body: `productId=${productId}`
                });

                if (res.status === 401) {
                    window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
                    return;
                }
                if (!res.ok) throw new Error(`Request failed with status ${res.status}`);

                const data = await res.json();
                const icon = btn.querySelector('.js-wishlist-icon');
                const label = btn.querySelector('.js-wishlist-label');
                if (icon) icon.className = data.inWishlist ? 'bi bi-heart-fill me-1 js-wishlist-icon' : 'bi bi-heart me-1 js-wishlist-icon';
                if (label) label.textContent = data.inWishlist ? 'Remove from Wishlist' : 'Add to Wishlist';
                btn.classList.toggle('btn-outline-danger', data.inWishlist);
                btn.classList.toggle('btn-outline-secondary', !data.inWishlist);
            } catch (err) {
                console.error(err);
                alert('Could not update your wishlist. Please try again.');
            } finally {
                btn.disabled = false;
            }
        });
    });
});