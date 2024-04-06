using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin;
using webapi.DB.Abstractions;
using webapi.DB.Ef;
using webapi.Exceptions;
using webapi.Models;

namespace tests.Controllers_Tests.Admin
{
    public class LinkController_Test
    {
        [Fact]
        public async Task GetLink_ById_Success()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            linkRepositoryMock.Setup(x => x.GetById(It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(new LinkModel());

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.GetLink(1, null);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetLink_ByToken_Success()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
                .ReturnsAsync(new LinkModel());

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.GetLink(null, "F");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetLink_DbConnectionFailed()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.GetLink(null, "F");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetLink_LinkIsNull()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            linkRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
                .ReturnsAsync((LinkModel)null);

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.GetLink(null, "F");

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeLinks_Success()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            var sortMock = new Mock<ISorting>();
            linkRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
                .ReturnsAsync(new List<LinkModel>());

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, sortMock.Object, null);
            var result = await linkController.GetRangeLinks(null, null, null, true, false);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRangeLinks_DbConnectionFailed()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            var sortMock = new Mock<ISorting>();
            linkRepositoryMock.Setup(x => x.GetAll(It.IsAny<Func<IQueryable<LinkModel>, IQueryable<LinkModel>>>(), CancellationToken.None))
                .ThrowsAsync(new OperationCanceledException());

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, sortMock.Object, null);
            var result = await linkController.GetRangeLinks(null, null, null, true, false);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteLink_Success()
        {
            var id = 1;
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.DeleteLink(id);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(204, objectResult.StatusCode);
            linkRepositoryMock.Verify(x => x.Delete(id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteLink_EntityNotDeleted()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            linkRepositoryMock.Setup(x => x.Delete(It.IsAny<int>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.DeleteLink(1);

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteRangeLinks_Success()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            var ids = new List<int> { 1, 2, 3 };

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.DeleteRangeLinks(ids);

            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
            linkRepositoryMock.Verify(x => x.DeleteMany(ids, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteRangeLinks_EntityNotDeleted()
        {
            var linkRepositoryMock = new Mock<IRepository<LinkModel>>();
            linkRepositoryMock.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
                .ThrowsAsync(new EntityNotDeletedException());

            var linkController = new Admin_LinkController(linkRepositoryMock.Object, null, null);
            var result = await linkController.DeleteRangeLinks(new List<int> { 1 });

            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
