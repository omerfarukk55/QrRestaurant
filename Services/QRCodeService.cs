using QRCoder;
using System;
using System.Drawing;
using System.IO;

namespace RestaurantQRSystem.Services
{
    public class QRCodeService
    {
        public byte[] GenerateQRCode(string content, int pixelsPerModule = 20)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            // QRCode sınıfını doğru şekilde kullan
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            // Doğrudan byte array olarak al
            return qrCode.GetGraphic(pixelsPerModule);
        }
    }
}