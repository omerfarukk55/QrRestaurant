document.addEventListener('DOMContentLoaded', function () {
    // Token'ı sayfadan al
    var token = $('input[name="__RequestVerificationToken"]').val();

    // Masa kartına tıklandığında modal açılması
    document.querySelectorAll('.table-card').forEach(card => {
        card.addEventListener('click', function () {
            const tableId = this.getAttribute('data-table-id');
            loadTableDetails(tableId);
        });
    });

    // Sipariş tamamla butonu (Modal açıldığında eklenir)
    document.getElementById('btnCompleteOrder').addEventListener('click', function () {
        const tableId = this.getAttribute('data-table-id');
        const orderId = this.getAttribute('data-order-id');

        if (tableId && orderId) {
            completeOrderAndClearTable(tableId, orderId);
        }
    });

    // Masa detaylarını yükleme fonksiyonu
    function loadTableDetails(tableId) {
        // Modalı aç
        var modalInstance = new bootstrap.Modal(document.getElementById('tableDetailModal'));
        modalInstance.show();

        fetch(`/Admin/Table/GetTableDetails/${tableId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Masa bilgileri alınamadı');
                }
                return response.json();
            })
            .then(data => {
                console.log("Masa data:", data); // Debug için

                // Modal başlığını güncelle
                document.getElementById('modalTableName').textContent = data.table.name;

                // Masa durumunu göster - isOccupied ile kontrol et, order'a değil
                const statusBadge = document.getElementById('modalTableStatus');
                if (data.table.isOccupied) {
                    statusBadge.textContent = 'Dolu';
                    statusBadge.className = 'badge bg-danger';
                } else {
                    statusBadge.textContent = 'Boş';
                    statusBadge.className = 'badge bg-success';
                }

                // Sipariş bilgilerini göster veya gizle
                const orderInfo = document.getElementById('modalOrderInfo');
                const orderItemsContainer = document.getElementById('orderItemsContainer');
                const completeOrderBtn = document.getElementById('btnCompleteOrder');

                if (data.order) {
                    // Sipariş bilgilerini göster
                    orderInfo.style.display = 'block';
                    orderItemsContainer.style.display = 'block';
                    document.getElementById('modalOrderId').textContent = data.order.id;

                    // Sipariş ürünlerini listele
                    const orderItemsList = document.getElementById('orderItemsList');
                    orderItemsList.innerHTML = '';

                    if (data.order.items && data.order.items.length > 0) {
                        let itemsHtml = '<div class="table-responsive"><table class="table table-sm">';
                        itemsHtml += '<thead><tr><th>Ürün</th><th class="text-center">Adet</th><th class="text-end">Fiyat</th></tr></thead><tbody>';

                        data.order.items.forEach(item => {
                            itemsHtml += `
                                <tr>
                                    <td>${item.productName}</td>
                                    <td class="text-center">${item.quantity}</td>
                                    <td class="text-end">${item.totalPrice.toFixed(2)}₺</td>
                                </tr>
                            `;
                        });

                        itemsHtml += '</tbody></table></div>';
                        orderItemsList.innerHTML = itemsHtml;
                    } else {
                        orderItemsList.innerHTML = '<div class="alert alert-info">Bu siparişte ürün bulunmuyor.</div>';
                    }

                    // Toplam tutarı göster
                    document.getElementById('modalTotalAmount').textContent = `${data.order.totalAmount.toFixed(2)}₺`;

                    // Tamamla butonunu aktifleştir ve veri ekle
                    completeOrderBtn.removeAttribute('disabled');
                    completeOrderBtn.setAttribute('data-table-id', tableId);
                    completeOrderBtn.setAttribute('data-order-id', data.order.id);
                } else {
                    // Sipariş yoksa ilgili alanları gizle
                    orderInfo.style.display = 'none';
                    orderItemsContainer.style.display = 'none';
                    document.getElementById('modalTotalAmount').textContent = '0.00₺';

                    // Tamamla butonunu devre dışı bırak
                    completeOrderBtn.setAttribute('disabled', 'disabled');
                    completeOrderBtn.removeAttribute('data-table-id');
                    completeOrderBtn.removeAttribute('data-order-id');
                }
            })
            .catch(error => {
                console.error('Masa bilgileri alınamadı:', error);
                alert('Masa bilgileri yüklenirken bir hata oluştu.');
            });
    }

    // Siparişi tamamla ve masayı temizle fonksiyonu
    function completeOrderAndClearTable(tableId, orderId) {
        if (!confirm('Bu masadaki siparişi tamamlamak ve masayı boşaltmak istediğinize emin misiniz?')) {
            return;
        }
    
        var token = $('input[name="__RequestVerificationToken"]').val();
    
        // Önce siparişi tamamla
        $.ajax({
            url: '/Admin/Table/CompleteOrder',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ TableId: tableId, OrderId: orderId }),
            headers: {
                'RequestVerificationToken': token
            },
            success: function (data) {
                if (data.success) {
                    // Sonra masayı temizle
                    $.ajax({
                        url: '/Admin/Table/ClearTable',
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ tableId: tableId }),
                        headers: {
                            'RequestVerificationToken': token
                        },
                        success: function (data) {
                            if (data.success) {
                                alert('Sipariş tamamlandı ve masa boşaltıldı.');
                                $('#tableDetailModal').modal('hide');
                                window.location.reload();
                            }
                        },
                        error: function (xhr) {
                            alert('Masa temizlenemedi: ' + xhr.responseText);
                        }
                    });
                } else {
                    alert(data.message || 'Sipariş tamamlanamadı');
                }
            },
            error: function (xhr) {
                alert('Sipariş tamamlanamadı: ' + xhr.responseText);
            }
        });
    }
});