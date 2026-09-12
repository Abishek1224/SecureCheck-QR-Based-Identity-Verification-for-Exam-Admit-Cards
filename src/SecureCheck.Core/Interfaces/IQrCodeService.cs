namespace SecureCheck.Core.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string data);
}