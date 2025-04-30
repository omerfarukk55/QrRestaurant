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
// Add this to your site.js or a separate tables.js file
$(document).ready(function () {
    // Table detail modal handling
    $('#tableDetailModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var tableId = button.data('table-id');
        var tableName = button.data('table-name');
        var tableStatus = button.data('table-status');
        var tableAmount = button.data('table-amount');
        var orderId = button.data('table-orderid');

        var modal = $(this);
        modal.find('#modalTableName').text(tableName);
        modal.find('#modalTableStatus').text(tableStatus);

        // Clear previous content
        modal.find('#orderItemsList').empty();

        // Load table details via AJAX
        $.ajax({
            url: '/Admin/Table/GetTableDetail/' + tableId,
            type: 'GET',
            success: function (data) {
                // Display QR code
                $('#tableQrCode').attr('src',
                    'https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=' +
                    encodeURIComponent(data.table.QrCodeUrl));

                // If there's an active order
                if (data.order) {
                    $('#modalOrderId').text(data.order.Id);
                    $('#modalOrderInfo').show();
                    $('#modalTotalAmount').text(parseFloat(data.order.TotalAmount).toFixed(2) + ' ₺');

                    var itemsList = $('#orderItemsList');
                    itemsList.empty();

                    // Create table for order items
                    var table = $('<table class="table table-sm">');
                    var thead = $('<thead>').append('<tr><th>Ürün</th><th>Adet</th><th>Fiyat</th><th>Toplam</th></tr>');
                    var tbody = $('<tbody>');

                    $.each(data.order.Items, function (i, item) {
                        var row = $('<tr>');
                        row.append($('<td>').text(item.ProductName));
                        row.append($('<td>').text(item.Quantity));
                        row.append($('<td>').text(parseFloat(item.UnitPrice).toFixed(2) + ' ₺'));
                        row.append($('<td>').text(parseFloat(item.TotalPrice).toFixed(2) + ' ₺'));
                        tbody.append(row);
                    });

                    table.append(thead).append(tbody);
                    itemsList.append(table);

                    // Configure buttons based on order status
                    if (data.order.ReadyForPayment) {
                        $('#btnProcessPayment').show().data('order-id', data.order.Id);
                        $('#btnClearTable').hide();
                    } else {
                        $('#btnProcessPayment').hide();
                        $('#btnClearTable').hide();
                    }

                    if (data.order.Status === 'Completed') {
                        $('#btnProcessPayment').hide();
                        $('#btnClearTable').show();
                    }

                    $('#orderItemsContainer').show();
                } else {
                    // No active order
                    $('#modalOrderId').text('');
                    $('#modalOrderInfo').hide();
                    $('#orderItemsContainer').hide();

                    // Hide action buttons for empty table
                    $('#btnProcessPayment').hide();
                    $('#btnClearTable').hide();
                    $('#btnPrintReceipt').hide();
                }

                // Store table ID in modal for button actions
                modal.data('table-id', tableId);
            },
            error: function () {
                toastr.error('Masa detayları yüklenirken bir hata oluştu.');
            }
        });
    });

    // Process payment button click
    $('#btnProcessPayment').click(function () {
        var modal = $('#tableDetailModal');
        var tableId = modal.data('table-id');
        var orderId = $(this).data('order-id');

        // Show payment method selection modal
        $('#paymentMethodModal').modal('show');
        $('#paymentMethodModal').data('table-id', tableId);
        $('#paymentMethodModal').data('order-id', orderId);
    });

    // Confirm payment button click
    $('#confirmPayment').click(function () {
        var paymentModal = $('#paymentMethodModal');
        var tableId = paymentModal.data('table-id');
        var orderId = paymentModal.data('order-id');
        var paymentMethod = $('#paymentMethod').val();

        $(this).prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> İşleniyor...');

        // Process payment via AJAX
        $.ajax({
            url: '/Admin/Table/ProcessPayment',
            type: 'POST',
            data: {
                tableId: tableId,
                orderId: orderId,
                paymentMethod: paymentMethod
            },
            success: function (response) {
                if (response.success) {
                    $('#paymentMethodModal').modal('hide');
                    $('#tableDetailModal').modal('hide');

                    toastr.success('Ödeme başarıyla alındı');

                    // Reload the page after a short delay
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    toastr.error('Ödeme işlemi sırasında bir hata oluştu');
                    $('#confirmPayment').prop('disabled', false).html('Ödemeyi Tamamla');
                }
            },
            error: function () {
                toastr.error('Ödeme işlemi sırasında bir hata oluştu');
                $('#confirmPayment').prop('disabled', false).html('Ödemeyi Tamamla');
            }
        });
    });

    // Clear table button click
    $('#btnClearTable').click(function () {
        var modal = $('#tableDetailModal');
        var tableId = modal.data('table-id');

        $(this).prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> İşleniyor...');

        // Clear the table via AJAX
        $.ajax({
            url: '/Admin/Table/ClearTable',
            type: 'POST',
            data: { tableId: tableId },
            success: function (response) {
                if (response.success) {
                    $('#tableDetailModal').modal('hide');

                    toastr.success('Masa başarıyla boşaltıldı');

                    // Reload the page after a short delay
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    toastr.error('Masa boşaltılırken bir hata oluştu');
                    $('#btnClearTable').prop('disabled', false).html('Masayı Boşalt');
                }
            },
            error: function () {
                toastr.error('Masa boşaltılırken bir hata oluştu');
                $('#btnClearTable').prop('disabled', false).html('Masayı Boşalt');
            }
        });
    });

    // Print receipt button click
    $('#btnPrintReceipt').click(function () {
        var orderId = $('#modalOrderId').text();
        if (!orderId) return;

        // Open receipt in new window
        window.open('/Admin/Order/PrintReceipt/' + orderId, '_blank');
    });
});
$(document).ready(function () {
    // Masa kartına tıklanınca detay modalını aç
    $('.table-card').click(function () {
        var tableId = $(this).data('table-id');
        var tableName = $(this).data('table-name');
        var tableStatus = $(this).data('table-status');
        var tableAmount = $(this).data('table-amount');
        var orderId = $(this).data('table-orderid');

        openTableDetailModal(tableId, tableName, tableStatus, tableAmount, orderId);
    });

    // Masa detay modalı açma fonksiyonu
    function openTableDetailModal(tableId, tableName, tableStatus, tableAmount, orderId) {
        var modal = $('#tableDetailModal');

        // Temel bilgileri ayarla
        modal.find('#modalTableName').text(tableName);
        modal.find('#modalTableStatus').text(tableStatus).removeClass('bg-success bg-danger').addClass(tableStatus === 'Boş' ? 'bg-success' : 'bg-danger');

        // Sipariş bilgisi varsa göster
        if (orderId) {
            $('#modalOrderId').text(orderId);
            $('#modalOrderInfo').show();
            $('#btnProcessPayment').data('order-id', orderId);
        } else {
            $('#modalOrderInfo').hide();
            $('#orderItemsContainer').hide();
            $('#paymentStatusContainer').hide();
        }

        // Clear previous content
        modal.find('#orderItemsList').empty();

        // Load table details via AJAX
        $.ajax({
            url: '/Admin/Table/GetTableDetails/' + tableId,
            type: 'GET',
            success: function (data) {
                // Display QR code
                $('#tableQrCode').attr('src',
                    'https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=' +
                    encodeURIComponent(data.table.QrCodeUrl));

                // If there's an active order
                if (data.order) {
                    $('#modalOrderId').text(data.order.Id);
                    $('#modalOrderInfo').show();
                    $('#modalTotalAmount').text(parseFloat(data.order.TotalAmount).toFixed(2) + ' ₺');

                    var itemsList = $('#orderItemsList');
                    itemsList.empty();

                    // Create table for order items
                    var table = $('<table class="table table-sm">');
                    var thead = $('<thead>').append('<tr><th>Ürün</th><th>Adet</th><th>Fiyat</th><th>Toplam</th></tr>');
                    var tbody = $('<tbody>');

                    $.each(data.order.Items, function (i, item) {
                        var row = $('<tr>');
                        row.append($('<td>').text(item.ProductName));
                        row.append($('<td>').text(item.Quantity));
                        row.append($('<td>').text(parseFloat(item.UnitPrice).toFixed(2) + ' ₺'));
                        row.append($('<td>').text(parseFloat(item.TotalPrice).toFixed(2) + ' ₺'));
                        tbody.append(row);
                    });

                    table.append(thead).append(tbody);
                    itemsList.append(table);

                    // Update payment status UI
                    updatePaymentStatusUI(
                        data.order.PaymentStatus || 'Pending',
                        data.order.PaymentMethod,
                        data.order.PaymentDate
                    );

                    // Configure buttons based on order status and payment status
                    if (data.order.PaymentStatus === 'Completed') {
                        $('#btnProcessPayment').hide();
                        $('#btnRefundPayment').removeClass('d-none');
                        $('#btnClearTable').show();
                    } else {
                        $('#btnProcessPayment').show();
                        $('#btnRefundPayment').addClass('d-none');
                        $('#btnClearTable').hide();
                    }

                    if (data.order.Status === 'Completed') {
                        $('#btnProcessPayment').hide();
                        $('#btnClearTable').show();
                    }

                    $('#orderItemsContainer').show();
                    $('#paymentStatusContainer').show();
                } else {
                    // No active order
                    $('#modalOrderId').text('');
                    $('#modalOrderInfo').hide();
                    $('#orderItemsContainer').hide();
                    $('#paymentStatusContainer').hide();

                    // Hide action buttons for empty table
                    $('#btnProcessPayment').hide();
                    $('#btnRefundPayment').addClass('d-none');
                    $('#btnClearTable').hide();
                    $('#btnPrintReceipt').hide();
                }

                // Store table ID in modal for button actions
                modal.data('table-id', tableId);

                // Show the modal
                modal.modal('show');
            },
            error: function () {
                toastr.error('Masa detayları yüklenirken bir hata oluştu.');
            }
        });
    }

    // Payment status handling in the modal
    function updatePaymentStatusUI(status, paymentMethod, paymentDate) {
        $('#paymentStatusLoading').hide();

        let badgeClass = '';
        let statusText = '';

        // Set badge color and text based on payment status
        switch (status) {
            case 'Pending':
                badgeClass = 'bg-warning';
                statusText = 'Ödeme Bekliyor';
                break;
            case 'Processing':
                badgeClass = 'bg-info';
                statusText = 'İşleniyor';
                break;
            case 'Completed':
                badgeClass = 'bg-success';
                statusText = 'Ödenmiş';
                break;
            case 'Failed':
                badgeClass = 'bg-danger';
                statusText = 'Başarısız';
                break;
            case 'Refunded':
                badgeClass = 'bg-secondary';
                statusText = 'İade Edilmiş';
                break;
            default:
                badgeClass = 'bg-secondary';
                statusText = 'Bilinmiyor';
        }

        // Update the badge
        $('#paymentStatusBadge').removeClass('bg-warning bg-info bg-success bg-danger bg-secondary')
            .addClass(badgeClass)
            .text(statusText);

        // Show payment details if payment is completed
        if (status === 'Completed' && paymentMethod) {
            let formattedDate = paymentDate ? new Date(paymentDate).toLocaleString('tr-TR') : 'Belirtilmemiş';
            $('#paymentDetails').html(`
                <strong>Ödeme Yöntemi:</strong> ${paymentMethod}<br>
                <strong>Ödeme Tarihi:</strong> ${formattedDate}
            `).show();
        } else {
            $('#paymentDetails').hide();
        }
    }

    // Process payment button click
    $('#btnProcessPayment').click(function () {
        var modal = $('#tableDetailModal');
        var tableId = modal.data('table-id');
        var orderId = $(this).data('order-id');
        var totalAmount = parseFloat($('#modalTotalAmount').text().replace('₺', '').trim());

        // Show payment method selection modal
        $('#paymentMethodModal').modal('show');
        $('#paymentMethodModal').data('table-id', tableId);
        $('#paymentMethodModal').data('order-id', orderId);

        // Otomatik olarak toplam tutarı doldur
        if (!isNaN(totalAmount)) {
            $('#paidAmount').val(totalAmount.toFixed(2));
        }

        // Para üstü bölümünü başlangıçta gizle
        $('#changeAmount').hide();
    });

    // Nakit ödeme için para üstü hesaplama
    $('#paymentMethod, #paidAmount').on('change input', function () {
        var paymentMethod = $('#paymentMethod').val();
        var paidAmount = parseFloat($('#paidAmount').val());
        var orderTotal = parseFloat($('#modalTotalAmount').text().replace('₺', '').trim());

        if (paymentMethod === 'Nakit' && !isNaN(paidAmount) && !isNaN(orderTotal) && paidAmount >= orderTotal) {
            var change = paidAmount - orderTotal;
            $('#changeAmountValue').text(change.toFixed(2) + ' ₺');
            $('#changeAmount').show();
        } else {
            $('#changeAmount').hide();
        }
    });

    // Confirm payment button click
    $('#confirmPayment').click(function () {
        var paymentModal = $('#paymentMethodModal');
        var tableId = paymentModal.data('table-id');
        var orderId = paymentModal.data('order-id');
        var paymentMethod = $('#paymentMethod').val();
        var paidAmount = parseFloat($('#paidAmount').val());
        var paymentNotes = $('#paymentNotes').val();
        var completeOrder = $('#completeOrderCheck').is(':checked');

        if (isNaN(paidAmount) || paidAmount <= 0) {
            toastr.error('Lütfen geçerli bir ödeme tutarı girin');
            return;
        }

        $(this).prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> İşleniyor...');

        // Process payment via AJAX
        $.ajax({
            url: '/Admin/Payment/ProcessPayment',
            type: 'POST',
            data: {
                orderId: orderId,
                paymentMethod: paymentMethod,
                amount: paidAmount,
                notes: paymentNotes,
                completeOrder: completeOrder
            },
            success: function (response) {
                if (response.success) {
                    $('#paymentMethodModal').modal('hide');

                    toastr.success('Ödeme başarıyla alındı');

                    // Update payment status in the current table detail modal
                    updatePaymentStatusUI(
                        'Completed',
                        paymentMethod,
                        new Date().toISOString()
                    );

                    // Update buttons
                    $('#btnProcessPayment').hide();
                    $('#btnRefundPayment').removeClass('d-none');

                    if (completeOrder) {
                        // Reload the page after a delay to refresh all tables
                        setTimeout(function () {
                            location.reload();
                        }, 1500);
                    }
                } else {
                    toastr.error('Ödeme işlemi sırasında bir hata oluştu: ' + response.message);
                    $('#confirmPayment').prop('disabled', false).html('Ödemeyi Tamamla');
                }
            },
            error: function () {
                toastr.error('Ödeme işlemi sırasında bir hata oluştu');
                $('#confirmPayment').prop('disabled', false).html('Ödemeyi Tamamla');
            }
        });
    });

    // Refund button click
    $('#btnRefundPayment').click(function () {
        var orderId = $('#modalOrderId').text();
        $('#refundModal').modal('show');
        $('#refundModal').data('order-id', orderId);
    });

    // Confirm refund button click
    $('#confirmRefund').click(function () {
        var refundModal = $('#refundModal');
        var orderId = refundModal.data('order-id');
        var refundReason = $('#refundReason').val();

        if (!refundReason.trim()) {
            toastr.error('Lütfen iade nedeni belirtin');
            return;
        }

        $(this).prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> İşleniyor...');

        // Process refund via AJAX
        $.ajax({
            url: '/Admin/Payment/RefundPayment',
            type: 'POST',
            data: {
                paymentId: orderId,  // Bu orderId değil, ödeme ID'si olmalı, backend'de düzeltilmeli
                refundReason: refundReason
            },
            success: function (response) {
                if (response.success) {
                    $('#refundModal').modal('hide');

                    toastr.success('Ödeme başarıyla iade edildi');

                    // Update UI to show refunded status
                    updatePaymentStatusUI('Refunded', response.paymentMethod, response.paymentDate);
                    $('#btnRefundPayment').addClass('d-none');
                    $('#btnProcessPayment').show();

                    // Reload after a delay
                    setTimeout(function () {
                        location.reload();
                    }, 1500);
                } else {
                    toastr.error('İade işlemi sırasında bir hata oluştu: ' + response.message);
                    $('#confirmRefund').prop('disabled', false).html('İade Et');
                }
            },
            error: function () {
                toastr.error('İade işlemi sırasında bir hata oluştu');
                $('#confirmRefund').prop('disabled', false).html('İade Et');
            }
        });
    });

    // Clear table button click
    $('#btnClearTable').click(function () {
        var modal = $('#tableDetailModal');
        var tableId = modal.data('table-id');

        if (!confirm('Bu masayı boşaltmak istediğinizden emin misiniz?')) {
            return;
        }

        $(this).prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> İşleniyor...');

        // Clear the table via AJAX
        $.ajax({
            url: '/Admin/Table/ClearTable',
            type: 'POST',
            data: { tableId: tableId },
            success: function (response) {
                if (response.success) {
                    $('#tableDetailModal').modal('hide');

                    toastr.success('Masa başarıyla boşaltıldı');

                    // Reload the page after a short delay
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    toastr.error('Masa boşaltılırken bir hata oluştu');
                    $('#btnClearTable').prop('disabled', false).html('Masayı Boşalt');
                }
            },
            error: function () {
                toastr.error('Masa boşaltılırken bir hata oluştu');
                $('#btnClearTable').prop('disabled', false).html('Masayı Boşalt');
            }
        });
    });

    // Print receipt button click
    $('#btnPrintReceipt').click(function () {
        var orderId = $('#modalOrderId').text();
        if (!orderId) return;

        // Open receipt in new window
        window.open('/Admin/Payment/PrintReceipt/' + orderId, '_blank');
    });
});