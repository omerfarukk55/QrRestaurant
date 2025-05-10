using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantQRSystem.Models.Enums
{
    /// <summary>
    /// Sipariş durumlarını tanımlayan enum.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Sipariş oluşturuldu ve henüz işleme alınmadı
        /// </summary>
        [Display(Name = "Alındı")]
        Received = 1,

        /// <summary>
        /// Sipariş mutfakta hazırlanıyor
        /// </summary>
        [Display(Name = "Hazırlanıyor")]
        Preparing = 2,

        /// <summary>
        /// Sipariş hazır, servis için bekliyor
        /// </summary>
        [Display(Name = "Hazır")]
        Ready = 3,

        /// <summary>
        /// Sipariş müşteriye teslim edildi
        /// </summary>
        [Display(Name = "Teslim Edildi")]
        Delivered = 4,

        /// <summary>
        /// Sipariş iptal edildi
        /// </summary>
        [Display(Name = "İptal Edildi")]
        Cancelled = 5,

        /// <summary>
        /// Sipariş reddedildi (stok eksikliği, vb. nedenlerle)
        /// </summary>
        [Display(Name = "Reddedildi")]
        Rejected = 6,

        /// <summary>
        /// Ödeme tamamlandı
        /// </summary>
        [Display(Name = "Ödendi")]
        Paid = 7,
        Completed = 8
    }
}