// Sepeti LocalStorage'dan alma
function getCart() {
    try {
        const storedCart = localStorage.getItem('cart');
        const cart = storedCart ? JSON.parse(storedCart) : [];
        return Array.isArray(cart) ? cart : [];
    } catch (e) {
        console.error("Sepet verisi bozuk:", e);
        return []; // Hata durumunda yeni bir sepet oluştur
    }
}

// Sepeti LocalStorage'a kaydetme
function saveCart(cart) {
    localStorage.setItem('cart', JSON.stringify(cart));
}

// Sepete ürün ekleme
function addToCart(productId, productName, productPrice) {
    try {
        // Geçerlilik kontrolü
        if (!productId || !productName || productPrice === undefined || productPrice === null) {
            console.error("Eksik ürün bilgisi: ID, ad ve fiyat gereklidir.");
            return false;
        }

        let cart = getCart();
        const existingProductIndex = cart.findIndex(item => String(item.id) === String(productId));

        if (existingProductIndex > -1) {
            // Var olan ürünün miktarını artır
            cart[existingProductIndex].quantity += 1;
        } else {
            // Ürünü sepete ekle
            cart.push({
                id: productId,
                name: productName,
                price: parseFloat(productPrice), // Sayı olduğundan emin olalım
                quantity: 1
            });
        }

        // Sepeti kaydet
        saveCart(cart);

        // Arayüzü güncelle
        updateCartBadge();

        // Bildirim göster
        showNotification(`${productName} sepete eklendi.`, 'success');
        return true;
    } catch (error) {
        console.error("Sepete ekleme işlemi başarısız:", error);
        showNotification("Ürün sepete eklenemedi. Lütfen tekrar deneyin.", "error");
        return false;
    }
}

// Sepetten ürün çıkarma
function removeFromCart(productId) {
    let cart = getCart().filter(x => String(x.id) !== String(productId));
    saveCart(cart);
    updateCartBadge();
    updateCartSummary();
}

// Karttaki ürün miktarını artır/azalt
function changeCartQuantity(productId, delta) {
    let cart = getCart();
    let item = cart.find(x => String(x.id) === String(productId));
    if (!item) return;

    item.quantity += delta;

    if (item.quantity <= 0) {
        cart = cart.filter(x => String(x.id) !== String(productId));
    }

    saveCart(cart);
    updateCartBadge();
    updateCartSummary();
}

// Sepeti temizleme
function clearCart() {
    localStorage.removeItem('cart');
    updateCartBadge();
    updateCartSummary();
}

// Sepetteki eleman sayısını güncelle
function updateCartBadge() {
    const cart = getCart();
    const totalItems = cart.reduce((total, item) => total + item.quantity, 0);
    const badge = document.querySelector('.cart-badge');
    if (badge) {
        badge.textContent = totalItems;
        badge.style.display = totalItems > 0 ? 'block' : 'none';
    }
}

// Sepet modalında içeriği güncelle
function updateCartSummary() {
    let cart = getCart();
    let summary = cart.length === 0
        ? "<div class='alert alert-info'>Sepetiniz boş.</div>"
        : "<ul class='list-group mb-3'>" + cart.map(x =>
            `<li class='list-group-item d-flex justify-content-between align-items-center'>
                <span>
                    <b>${x.name}</b><br>
                    <small class="text-muted">${x.price.toFixed(2)}₺ x ${x.quantity}</small>
                </span>
                <span>
                    <button class='btn btn-sm btn-secondary' onclick='changeCartQuantity("${x.id}", -1)'>-</button>
                    <span class='mx-2'>${x.quantity}</span>
                    <button class='btn btn-sm btn-secondary' onclick='changeCartQuantity("${x.id}", 1)'>+</button>
                    <button class='btn btn-sm btn-danger ms-2' onclick='removeFromCart("${x.id}")'>Sil</button>
                </span>
            </li>`
        ).join('') +
        "</ul>" +
        `<div class='text-end'>Toplam: <b>${cart.reduce((acc, x) => acc + x.price * x.quantity, 0).toFixed(2)}₺</b></div>`;

    let cartSummary = document.getElementById('cartSummary');
    if (cartSummary) cartSummary.innerHTML = summary;
    document.getElementById('completeOrderBtn').addEventListener('click', function () {
        const tableId = localStorage.getItem('tableId');
        if (!tableId) {
            alert('Masa bilgisi bulunamadı! QR kodu tekrar okutun.');
            return;
        }

        console.log('Siparişi tamamlama URL:', `/Order/Create?tableId=${tableId}`);
        window.location.href = `/Order/Create?tableId=${tableId}`;
    });
}

// Sepet Modalını göster
function showCart() {
    updateCartSummary();
    var modal = new bootstrap.Modal(document.getElementById('cartModal'));
    modal.show();
}

// Bildirim gösterme (birleştirilmiş fonksiyon)
function showNotification(message, type = 'success') {
    if (window.bootstrap && document.body) {
        const notification = document.createElement('div');
        notification.className = `toast align-items-center text-white bg-${type} border-0`;
        notification.setAttribute('role', 'alert');
        notification.setAttribute('aria-live', 'assertive');
        notification.setAttribute('aria-atomic', 'true');
        notification.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            document.body.appendChild(toastContainer);
        }

        toastContainer.appendChild(notification);
        const toast = new bootstrap.Toast(notification, { delay: 3000 });
        toast.show();

        notification.addEventListener('hidden.bs.toast', () => notification.remove());
    } else {
        alert(message);
    }
}

// Sayfa yüklendiğinde sepet işlemlerini başlat
document.addEventListener('DOMContentLoaded', function () {
    initializeCart();
});

// Sepet başlatma
function initializeCart() {
    updateCartBadge();

    // Sepete ekleme butonları için event listener
    document.querySelectorAll('.add-to-cart').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const productId = this.getAttribute('data-product-id');
            const productName = this.getAttribute('data-product-name');
            const productPrice = parseFloat(this.getAttribute('data-product-price'));

            addToCart(productId, productName, productPrice);
        });
    });
}