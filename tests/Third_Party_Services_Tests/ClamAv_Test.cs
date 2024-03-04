using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using nClam;
using webapi.Third_Party_Services;

namespace tests.Third_Party_Services_Tests
{
    public class ClamAv_Test
    {
        [Fact]
        public async Task GetResultScan_CleanFile_ReturnsTrue()
        {
            var loggerMock = new Mock<ILogger<ClamAV>>();
            var clamSettingMock = new Mock<IClamSetting>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var cleanFileClamClientMock = new Mock<webapi.Third_Party_Services.IClamClient>();
            cleanFileClamClientMock
                .Setup(c => c.SendAndScanFileAsync(It.IsAny<Stream>(), ct))
                .ReturnsAsync(new ClamScanResult("ok"));
            clamSettingMock.Setup(c => c.SetClam()).Returns(cleanFileClamClientMock.Object);

            var clamAV = new ClamAV(loggerMock.Object, clamSettingMock.Object);

            var fileMock = new Mock<IFormFile>();
            var fileStreamMock = new MemoryStream(3 * 1024 * 1024);

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStreamMock);

            var result = await clamAV.GetResultScan(fileMock.Object, ct);

            Assert.True(result);
        }

        [Fact]
        public async Task GetResultScan_Exception_ReturnsFalse()
        {
            var loggerMock = new Mock<ILogger<ClamAV>>();
            var clamSettingMock = new Mock<IClamSetting>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var cleanFileClamClientMock = new Mock<webapi.Third_Party_Services.IClamClient>();
            cleanFileClamClientMock
                .Setup(c => c.SendAndScanFileAsync(It.IsAny<Stream>(), ct))
                .ThrowsAsync(new Exception("Test ex"));
            clamSettingMock.Setup(c => c.SetClam()).Returns(cleanFileClamClientMock.Object);

            var clamAV = new ClamAV(loggerMock.Object, clamSettingMock.Object);

            var fileMock = new Mock<IFormFile>();
            var fileStreamMock = new MemoryStream(3 * 1024 * 1024);

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStreamMock);

            var result = await clamAV.GetResultScan(fileMock.Object, ct);

            Assert.False(result);
        }

        [Fact]
        public async Task GetResultScan_InfectedFile_ReturnsFalse()
        {
            var loggerMock = new Mock<ILogger<ClamAV>>();
            var clamSettingMock = new Mock<IClamSetting>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var cleanFileClamClientMock = new Mock<webapi.Third_Party_Services.IClamClient>();
            cleanFileClamClientMock
                .Setup(c => c.SendAndScanFileAsync(It.IsAny<Stream>(), ct))
                .ReturnsAsync(new ClamScanResult("found"));
            clamSettingMock.Setup(c => c.SetClam()).Returns(cleanFileClamClientMock.Object);

            var clamAV = new ClamAV(loggerMock.Object, clamSettingMock.Object);

            var fileMock = new Mock<IFormFile>();
            var fileStreamMock = new MemoryStream(3 * 1024 * 1024);

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStreamMock);

            var result = await clamAV.GetResultScan(fileMock.Object, ct);

            Assert.False(result);
        }

        [Fact]
        public async Task GetResultScan_ClamAv_Error_ReturnsFalse()
        {
            var loggerMock = new Mock<ILogger<ClamAV>>();
            var clamSettingMock = new Mock<IClamSetting>();
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var cleanFileClamClientMock = new Mock<webapi.Third_Party_Services.IClamClient>();
            cleanFileClamClientMock
                .Setup(c => c.SendAndScanFileAsync(It.IsAny<Stream>(), ct))
                .ReturnsAsync(new ClamScanResult("error"));
            clamSettingMock.Setup(c => c.SetClam()).Returns(cleanFileClamClientMock.Object);

            var clamAV = new ClamAV(loggerMock.Object, clamSettingMock.Object);

            var fileMock = new Mock<IFormFile>();
            var fileStreamMock = new MemoryStream(3 * 1024 * 1024);

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStreamMock);

            var result = await clamAV.GetResultScan(fileMock.Object, ct);

            Assert.False(result);
        }
    }
}
