using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using nClam;
using services.ClamAv;

namespace tests.Services
{
    public class ClamAvTest
    {
        [Fact]
        public async Task GetResultScan_CleanFile_ReturnsTrue()
        {
            var loggerMock = new Mock<ILogger<ClamAV>>();
            var clamSettingMock = new Mock<IClamSetting>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var cleanFileClamClientMock = new Mock<services.ClamAv.IClamClient>();
            var stream = new MemoryStream(3 * 1024 * 1024);

            cleanFileClamClientMock
                .Setup(c => c.SendAndScanFileAsync(stream, ct))
                .ReturnsAsync(new ClamScanResult("ok"));
            clamSettingMock.Setup(c => c.SetClam(It.IsAny<string>(), It.IsAny<int>())).Returns(cleanFileClamClientMock.Object);

            var clamAV = new ClamAV(loggerMock.Object, clamSettingMock.Object)
            {
                ClamPort = 3310,
                ClamServer = "localhost"
            };

            var result = await clamAV.GetResultScan(stream, ct);

            Assert.True(result);
        }

        [Fact]
        public async Task GetResultScan_Exception_ReturnsFalse()
        {
            var loggerMock = new Mock<ILogger<ClamAV>>();
            var clamSettingMock = new Mock<IClamSetting>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var cleanFileClamClientMock = new Mock<services.ClamAv.IClamClient>();
            var stream = new MemoryStream(3 * 1024 * 1024);

            cleanFileClamClientMock
                .Setup(c => c.SendAndScanFileAsync(stream, ct))
                .ThrowsAsync(new Exception("Test ex"));
            clamSettingMock.Setup(c => c.SetClam(It.IsAny<string>(), It.IsAny<int>())).Returns(cleanFileClamClientMock.Object);

            var clamAV = new ClamAV(loggerMock.Object, clamSettingMock.Object)
            {
                ClamPort = 3310,
                ClamServer = "localhost"
            };

            var result = await clamAV.GetResultScan(stream, ct);

            Assert.False(result);
        }

        [Theory]
        [InlineData("found")]
        [InlineData("error")]
        public async Task GetResultScan_ClamAv_NotValid(string scanResult)
        {
            var loggerMock = new Mock<ILogger<ClamAV>>();
            var clamSettingMock = new Mock<IClamSetting>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var cleanFileClamClientMock = new Mock<services.ClamAv.IClamClient>();
            var stream = new MemoryStream(3 * 1024 * 1024);

            cleanFileClamClientMock
                .Setup(c => c.SendAndScanFileAsync(stream, ct))
                .ReturnsAsync(new ClamScanResult(scanResult));
            clamSettingMock.Setup(c => c.SetClam(It.IsAny<string>(), It.IsAny<int>())).Returns(cleanFileClamClientMock.Object);

            var clamAV = new ClamAV(loggerMock.Object, clamSettingMock.Object)
            {
                ClamPort = 3310,
                ClamServer = "localhost"
            };

            var result = await clamAV.GetResultScan(stream, ct);

            Assert.False(result);
        }
    }
}
