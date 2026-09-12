using QRCoder;
using SecureCheck.Core.Interfaces;

namespace SecureCheck.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCode(string data)
    {
        using var qrGenerator = new QRCodeGenerator();

        using var qrData = qrGenerator.CreateQrCode(
            data,
            QRCodeGenerator.ECCLevel.Q);

        var pngQrCode = new PngByteQRCode(qrData);

        return pngQrCode.GetGraphic(10);
    }
}