using webapi.DB;
using webapi.Models;

namespace tests.Db_Tests.Sql_Tests
{
    public class Sorting_Test
    {
        [Fact]
        public void SortFiles_SortsQueryCorrectly()
        {
            var testData = new List<FileModel>
            {
                new FileModel { user_id = 1, operation_date = DateTime.Now.AddDays(-2), type = "Type1", file_mime_category = "Category1", file_mime = "Mime1" },
                new FileModel { user_id = 2, operation_date = DateTime.Now.AddDays(-1), type = "Type2", file_mime_category = "Category2", file_mime = "Mime2" },
                new FileModel { user_id = 1, operation_date = DateTime.Now, type = "Type3", file_mime_category = "Category1", file_mime = "Mime3" }
            }.AsQueryable();

            var mockSet = new Mock<IQueryable<FileModel>>();
            mockSet.As<IQueryable<FileModel>>().Setup(m => m.Provider).Returns(testData.Provider);
            mockSet.As<IQueryable<FileModel>>().Setup(m => m.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<FileModel>>().Setup(m => m.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<FileModel>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());

            var sorter = new Sorting();

            var sortedQuery = sorter.SortFiles(userId: 1, skip: 0, count: 10, byDesc: true, type: null, mime: null, category: "Category1");

            var result = sortedQuery(testData).ToList();
            Assert.Equal(2, result.Count);
            Assert.True(result.All(f => f.user_id == 1));
            Assert.True(result[0].operation_date > result[1].operation_date);
        }

        [Fact]
        public void SortNotifications_SortsQueryCorrectly()
        {
            var testData = new List<NotificationModel>
            {
                new NotificationModel { user_id = 1, send_time = DateTime.Now.AddDays(-2), priority = "High", is_checked = false },
                new NotificationModel { user_id = 2, send_time = DateTime.Now.AddDays(-1), priority = "Low", is_checked = true },
                new NotificationModel { user_id = 1, send_time = DateTime.Now, priority = "Medium", is_checked = true }
            }.AsQueryable();

            var sorter = new Sorting();

            var sortedQuery = sorter.SortNotifications(userId: 1, skip: 0, count: 10, byDesc: true, priority: null, isChecked: null);

            var result = sortedQuery(testData).ToList();
            Assert.Equal(2, result.Count);
            Assert.True(result.All(n => n.user_id == 1));
            Assert.True(result[0].send_time > result[1].send_time);
        }

        [Fact]
        public void SortOffers_SortsQueryCorrectly()
        {
            var testData = new List<OfferModel>
            {
                new OfferModel { sender_id = 1, receiver_id = 2, created_at = DateTime.Now.AddDays(-2), is_accepted = true, offer_type = "Type1" },
                new OfferModel { sender_id = 2, receiver_id = 1, created_at = DateTime.Now.AddDays(-1), is_accepted = false, offer_type = "Type2" },
                new OfferModel { sender_id = 1, receiver_id = 2, created_at = DateTime.Now, is_accepted = true, offer_type = "Type3" }
            }.AsQueryable();

            var sorter = new Sorting();

            var sortedQuery = sorter.SortOffers(userId: 1, skip: 0, count: 10, byDesc: true, sended: null, isAccepted: true, type: null);

            var result = sortedQuery(testData).ToList();
            Assert.Equal(2, result.Count);
            Assert.True(result.All(o => o.sender_id == 1 || o.receiver_id == 1));
            Assert.True(result[0].created_at > result[1].created_at);
            Assert.True(result.All(o => o.is_accepted));
        }

        [Fact]
        public void SortLinks_SortsQueryCorrectly()
        {
            var testData = new List<LinkModel>
            {
                new LinkModel { user_id = 1, created_at = DateTime.Now.AddDays(-2), expiry_date = DateTime.Now.AddDays(-1) },
                new LinkModel { user_id = 2, created_at = DateTime.Now.AddDays(-1), expiry_date = DateTime.Now.AddDays(1) },
                new LinkModel { user_id = 1, created_at = DateTime.Now, expiry_date = DateTime.Now.AddDays(2) }
            }.AsQueryable();

            var sorter = new Sorting();

            var sortedQuery = sorter.SortLinks(userId: 1, skip: 0, count: 10, byDesc: true, expired: null);

            var result = sortedQuery(testData).ToList();
            Assert.Equal(2, result.Count);
            Assert.True(result.All(l => l.user_id == 1));
            Assert.True(result[0].created_at > result[1].created_at);
        }
    }
}
