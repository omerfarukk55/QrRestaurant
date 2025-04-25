// Sepeti LocalStorage'da sakla
function getCart() {
    return JSON.parse(localStorage.getItem('cart')) || [];
}

function saveCart(cart) {
    localStorage.setItem('cart', JSON.stringify(cart));
}

function addToCart(productId, productName, productPrice) {
    let cart = getCart();
    let item = cart.find(x => x.id === productId);
    if (item) {
        item.quantity++;
    } else {
        cart.push({
            id: productId,
            name: productName,
            price: productPrice,
            quantity: 1
        });
    }
    saveCart(cart);
    updateCartBadge();
    showCartToast(productName + " sepete eklendi!");
}

function removeFromCart(productId) {
    let cart = getCart().filter(x => x.id !== productId);
    saveCart(cart);
    updateCartBadge();
    updateCartSummary();
}

// Karttaki ürün miktarını artır/azalt (opsiyonel)
function changeCartQuantity(productId, delta) {
    let cart = getCart();
    let item = cart.find(x => x.id === productId);
    if (!item) return;
    item.quantity += delta;
    if (item.quantity <= 0) {
        cart = cart.filter(x => x.id !== productId);
    }
    saveCart(cart);
    updateCartBadge();
    updateCartSummary();
}

// Sepetteki eleman sayısını güncelle
function updateCartBadge() {
    let cart = getCart();
    let totalCount = cart.reduce((acc, x) => acc + x.quantity, 0);
    let badge = document.querySelector('.cart-badge');
    if (badge) {
        badge.textContent = totalCount;
        badge.style.display = totalCount > 0 ? 'inline-block' : 'none';
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
}

// Sepet Modalını göster
function showCart() {
    updateCartSummary();
    var modal = new bootstrap.Modal(document.getElementById('cartModal'));
    modal.show();
}

// Toast mesajı göster (kısa onay)
function showCartToast(msg) {
    if (window.bootstrap && document.body) {
        var toastDiv = document.createElement('div');
        toastDiv.className = 'toast align-items-center text-bg-success border-0 position-fixed bottom-0 end-0 m-3';
        toastDiv.setAttribute('role', 'alert');
        toastDiv.innerHTML = `
            <div class='d-flex'>
                <div class='toast-body'>${msg}</div>
                <button type='button' class='btn-close btn-close-white me-2 m-auto' data-bs-dismiss='toast' aria-label='Kapat'></button>
            </div>
        `;
        document.body.appendChild(toastDiv);
        var toast = new bootstrap.Toast(toastDiv, { delay: 1500 });
        toast.show();
        toastDiv.addEventListener('hidden.bs.toast', () => toastDiv.remove());
    } else {
        alert(msg);
    }
}

// Sayfa yüklendiğinde cart badge güncelle
document.addEventListener('DOMContentLoaded', function () {
    updateCartBadge();
    // Sepete ekle butonları için event
    document.querySelectorAll('.add-to-cart').forEach(btn => {
        btn.addEventListener('click', function () {
            let id = this.getAttribute('data-product-id');
            let name = this.getAttribute('data-product-name');
            let price = parseFloat(this.getAttribute('data-product-price'));
            addToCart(id, name, price);
        });
    });
});
function clearCart() {
    localStorage.removeItem('cart');
    updateCartBadge();
    updateCartSummary();
}
// Genel site JavaScript kodları
document.addEventListener('DOMContentLoaded', function () {
    // Sepet işlemleri
    initializeCart();
});

// Sepet başlatma
function initializeCart() {
    updateCartBadge();
    // Sepete ekleme butonları için event listener document.querySelectorAll('.add-to-cart').forEach(button => {
    button.addEventListener('click', function (e) {
        e.preventDefault();
        const productId = this.getAttribute('data-product-id');
        const productName = this.getAttribute('data-product-name');
        const productPrice = parseFloat(this.getAttribute('data-product-price'));

        addToCart(productId, productName, productPrice);
    });
}

// Sepete ürün ekleme
function addToCart(productId, productName, productPrice) {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    // Ürün sepette var mı kontrol et 
    const existingProductIndex = cart.findIndex(item => item.id === productId);
    if (existingProductIndex > -1) {
        cart[existingProductIndex].quantity += 1;
    }

    else {
        // Ürünü sepete ekle 
        cart.push({
        id: productId,
        name: productName,
        price: productPrice,
        quantity: 1
    });
}

// Sepeti güncelle
localStorage.setItem('cart', JSON.stringify(cart));
updateCartBadge();

// Bildirim göster
showNotification(`${productName} sepete eklendi.`);
}

// Sepet badge'ini güncelleme
function updateCartBadge() {
    const cart = JSON.parse(localStorage.getItem('cart')) || [];
    const totalItems = cart.reduce((total, item) => total + item.quantity, 0);
    const badge = document.querySelector('.cart-badge');
    if (badge) {
        badge.textContent = totalItems;
        badge.style.display = totalItems > 0 ? 'block' : 'none';
    }

}

// Bildirim gösterme
function showNotification(message) {
    const notification = document.createElement('div');
    notification.className = 'toast align-items-center text-white bg-success border-0';
    notification.setAttribute('role', 'alert');
    notification.setAttribute('aria-live', 'assertive');
    notification.setAttribute('aria-atomic', 'true');
    notification.innerHTML = ` <div class="d-flex"> <div class="toast-body">${message}
</div >
<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close" > </button >
</div >
`;
    const toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        const container = document.createElement('div');
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(container);
        container.appendChild(notification);
    }

    else {
        toastContainer.appendChild(notification);
    }

    const toast = new bootstrap.Toast(notification);
    toast.show();
}
