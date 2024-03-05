using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using webapi.Helpers;

namespace tests.Helpers_Tests
{
    public class FileManager_Test
    {
        private readonly string _dataDirectory;

        public FileManager_Test()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            _dataDirectory = Path.Combine(currentDirectory, "..", "..", "..", "..", "data");
        }

        [Fact]
        public void GetMimesFromCsvFile_ReturnsMimes_WhenCsvFileExists()
        {
            var filePath = Directory.GetFiles(_dataDirectory).FirstOrDefault();
            var loggerMock = new Mock<ILogger<FileManager>>();
            var env = new Mock<IWebHostEnvironment>();
            var fileManager = new FileManager(loggerMock.Object, env.Object);

            var result = fileManager.GetMimesFromCsvFile(filePath);

            Assert.NotNull(result);
            Assert.IsType<HashSet<string>>(result);
        }

        [Fact]
        public void GetMimesFromCsvFile_LogsException_WhenCsvFileDoesNotExist()
        {
            var filePath = "test.csv";
            var logger = new FakeLogger<FileManager>();
            var env = new Mock<IWebHostEnvironment>();
            var fileManager = new FileManager(logger, env.Object);

            var result = fileManager.GetMimesFromCsvFile(filePath);

            Assert.Equal(new HashSet<string>() { }, result);
            Assert.Single(logger.LoggedMessages);
        }

        [Fact]
        public void GetCsvFiles_SuccessGet()
        {
            var env = new Mock<IWebHostEnvironment>();
            var loggerMock = new Mock<ILogger<FileManager>>();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
            env.SetupGet(x => x.ContentRootPath).Returns(path);

            var fileManager = new FileManager(loggerMock.Object, env.Object);

            var result = fileManager.GetCsvFiles();

            Assert.Equal(Directory.GetFiles(_dataDirectory), result);
        }

        [Fact]
        public void TestGetFileSizeInMb_WithIFormFile()
        {
            var env = new Mock<IWebHostEnvironment>();
            var loggerMock = new Mock<ILogger<FileManager>>();

            var formFile = new Mock<IFormFile>();
            formFile.Setup(x => x.Length).Returns(1024);

            var fileManager = new FileManager(loggerMock.Object, env.Object);

            var result = fileManager.GetFileSizeInMb(formFile.Object);

            Assert.Equal(0.0009765625, result);
        }

        [Fact]
        public void TestGetFileSizeInMb_WithStringFilePath()
        {
            var env = new Mock<IWebHostEnvironment>();
            var loggerMock = new Mock<ILogger<FileManager>>();

            var filePath = "example.txt";
            var fileInfo = new FileInfo(filePath);
            fileInfo.Create().Close();
            fileInfo.Refresh();

            var fileManager = new FileManager(loggerMock.Object, env.Object);

            var result = fileManager.GetFileSizeInMb(filePath);

            Assert.Equal(0.0, result);
            fileInfo.Delete();
        }

        [Fact]
        public void TestGetFileSizeInMb_WithStringFilePath_ThrowsException()
        {
            var env = new Mock<IWebHostEnvironment>();
            var logger = new FakeLogger<FileManager>();
            var filePath = "example.txt";

            var fileManager = new FileManager(logger, env.Object);

            Assert.Throws<ArgumentException>(() => fileManager.GetFileSizeInMb(filePath));
            Assert.Single(logger.LoggedMessages);
        }

        [Fact]
        public void TestGetFileSizeInMb_WithUnsupportedFileType()
        {
            var env = new Mock<IWebHostEnvironment>();
            var loggerMock = new Mock<ILogger<FileManager>>();

            var unsupportedFile = new object();
            var fileManager = new FileManager(loggerMock.Object, env.Object);

            Assert.Throws<ArgumentException>(() => fileManager.GetFileSizeInMb(unsupportedFile));
        }
    }
}
