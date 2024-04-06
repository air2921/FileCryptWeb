using webapi.Models;

namespace webapi.DB.Abstractions
{
    public interface ISorting
    {
        Func<IQueryable<OfferModel>, IQueryable<OfferModel>> SortOffers(int? userId, int? skip, int? count, bool byDesc, bool? sended, bool? isAccepted, string? type);
        Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>> SortNotifications(int? userId, int? skip, int? count, bool byDesc, string? priority, bool? isChecked);
        Func<IQueryable<FileModel>, IQueryable<FileModel>> SortFiles(int? userId, int? skip, int? count, bool byDesc, string? type, string? mime, string? category);
        Func<IQueryable<LinkModel>, IQueryable<LinkModel>> SortLinks(int? userId, int? skip, int? count, bool byDesc, bool? expired);
    }
}
