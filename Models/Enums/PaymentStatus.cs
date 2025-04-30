namespace RestaurantQRSystem.Models.Enums
{
    public enum PaymentStatus
    {
        
            Pending,    // Ödeme bekliyor
            Processing, // İşleniyor
            Completed,  // Tamamlandı
            Failed,     // Başarısız
            Refunded    // İade edildi
        
    }
}
