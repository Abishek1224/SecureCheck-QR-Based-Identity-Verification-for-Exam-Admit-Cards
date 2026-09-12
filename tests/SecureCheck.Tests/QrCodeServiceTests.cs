using SecureCheck.Infrastructure.Services;
using Xunit;

namespace SecureCheck.Tests;

public class QrCodeServiceTests
{
    [Fact]
    public void GenerateQrCode_ReturnsPngBytes()
    {
        // Arrange
        var service = new QrCodeService();
        var data = "https://localhost:5000/api/verify/test";

        // Act
        var result = service.GenerateQrCode(data);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // PNG files start with these bytes
        Assert.Equal(0x89, result[0]);
        Assert.Equal(0x50, result[1]);
        Assert.Equal(0x4E, result[2]);
        Assert.Equal(0x47, result[3]);
    }
}