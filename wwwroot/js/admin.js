// Admin panel JavaScript kodları
document.addEventListener('DOMContentLoaded', function () {
    // Sidebar toggle
    const menuToggle = document.getElementById('menu-toggle');
    if (menuToggle) {
        menuToggle.addEventListener('click', function (e) {
            e.preventDefault();
            document.getElementById('wrapper').classList.toggle('toggled');
        });
    }
    if ("Notification" in window && Notification.permission !== "granted" && Notification.permission !== "denied") {
        Notification.requestPermission();
    }

    // Bildirim sistemi başlatma
    initializeNotifications();
});

// Bildirim sistemi
function initializeNotifications() {
    // SignalR bağlantısı burada kurulacak
   
    // Şimdilik sadece arayüz işlevselliği

    // Örnek bildirim ekleme
    const notificationDropdown = document.querySelector('.notification-dropdown');
    if (notificationDropdown) {
        const emptyNotification = notificationDropdown.querySelector('.notification-empty');

        // Gerçek uygulamada bu veriler SignalR veya API'den gelecek
        const sampleNotifications = [
            { id: 1, message: 'Yeni sipariş alındı: #1001', time: '5 dakika önce', isRead: false },
            { id: 2, message: 'Masa 3 ödeme bekliyor', time: '15 dakika önce', isRead: true }
        ];

        if (sampleNotifications.length > 0 && emptyNotification) {
            emptyNotification.style.display = 'none';

            sampleNotifications.forEach(notification => {
                const notificationItem = document.createElement('li');
                notificationItem.className = `notification-item ${notification.isRead ? '' : 'unread'}`;
                notificationItem.innerHTML = `
                    <div class="d-flex justify-content-between">
                        <div>${notification.message}</div>
                        <div class="notification-time">${notification.time}</div>
                    </div>
                `;
                notificationDropdown.insertBefore(notificationItem, emptyNotification);
            });

            // Bildirim sayısını güncelle
            const unreadCount = sampleNotifications.filter(n => !n.isRead).length;
            const badge = document.querySelector('#notificationDropdown .badge');
            if (badge) {
                badge.textContent = unreadCount;
                badge.style.display = unreadCount > 0 ? 'block' : 'none';
            }
        }
    }
}

// Tablo arama işlevi
function initializeTableSearch() {
    const searchInput = document.getElementById('table-search');
    if (searchInput) {
        searchInput.addEventListener('keyup', function () {
            const searchValue = this.value.toLowerCase();
            const tableRows = document.querySelectorAll('table tbody tr');

            tableRows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(searchValue) ? '' : 'none';
            });
        });
    }
}

// Grafik oluşturma (Chart.js kullanılacak)
function initializeCharts() {
    // Satış grafiği örneği
    const salesChartCanvas = document.getElementById('salesChart');
    if (salesChartCanvas) {
        const salesChart = new Chart(salesChartCanvas, {
            type: 'line',
            data: {
                labels: ['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi', 'Pazar'],
                datasets: [{
                    label: 'Haftalık Satışlar',
                    data: [12, 19, 3, 5, 2, 3, 7],
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    // Kategori grafiği örneği
    const categoryChartCanvas = document.getElementById('categoryChart');
    if (categoryChartCanvas) {
        const categoryChart = new Chart(categoryChartCanvas, {
            type: 'doughnut',
            data: {
                labels: ['Ana Yemekler', 'İçecekler', 'Tatlılar', 'Başlangıçlar'],
                datasets: [{
                    label: 'Kategori Satışları',
                    data: [35, 25, 20, 20],
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.2)',
                        'rgba(54, 162, 235, 0.2)',
                        'rgba(255, 206, 86, 0.2)',
                        'rgba(75, 192, 192, 0.2)'
                    ],
                    borderColor: [
                        'rgba(255, 99, 132, 1)',
                        'rgba(54, 162, 235, 1)',
                        'rgba(255, 206, 86, 1)',
                        'rgba(75, 192, 192, 1)'
                    ],
                    borderWidth: 1
                }]
            }
        });
    }
}