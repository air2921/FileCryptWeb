using webapi.Models;

namespace webapi.DB
{
    public interface ISorting
    {
        Func<IQueryable<OfferModel>, IQueryable<OfferModel>> SortOffers(int? userId, int? skip, int? count, bool byDesc, bool? sended, bool? isAccepted, string? type);
        Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>> SortNotifications(int? userId, int? skip, int? count, bool byDesc, string? priority, bool? isChecked);
        Func<IQueryable<FileModel>, IQueryable<FileModel>> SortFiles(int? userId, int? skip, int? count, bool byDesc, string? type, string? mime, string? category);
        Func<IQueryable<LinkModel>, IQueryable<LinkModel>> SortLinks(int? userId, int? skip, int? count, bool byDesc, bool? expired);
    }

    public class Sorting : ISorting
    {
        public Func<IQueryable<FileModel>, IQueryable<FileModel>> SortFiles(int? userId, int? skip, int? count, bool byDesc, string? type, string? mime, string? category)
        {
            IQueryable<FileModel> sortedQuery;

            Func<IQueryable<FileModel>, IQueryable<FileModel>> sortDelegate = query =>
            {
                sortedQuery = byDesc ? query.OrderByDescending(f => f.operation_date) : query.OrderBy(f => f.operation_date);

                if (userId.HasValue)
                    sortedQuery = sortedQuery.Where(f => f.user_id.Equals(userId));

                if (!string.IsNullOrWhiteSpace(type))
                    sortedQuery = sortedQuery.Where(f => f.type.Equals(type));

                if (!string.IsNullOrWhiteSpace(category))
                    sortedQuery = sortedQuery.Where(f => f.file_mime_category.Equals(category));

                if (!string.IsNullOrWhiteSpace(mime))
                    sortedQuery = sortedQuery.Where(f => f.file_mime.Equals(mime));

                if (skip.HasValue && count.HasValue)
                    sortedQuery = sortedQuery.Skip(skip.Value).Take(count.Value);

                return sortedQuery;
            };

            return sortDelegate;
        }

        public Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>> SortNotifications(int? userId, int? skip, int? count, bool byDesc, string? priority, bool? isChecked)
        {
            IQueryable<NotificationModel> sortedQuery;

            Func<IQueryable<NotificationModel>, IQueryable<NotificationModel>> sortDelegate = query =>
            {
                sortedQuery = byDesc ? query.OrderByDescending(n => n.send_time) : query.OrderBy(n => n.send_time);

                if (userId.HasValue)
                    sortedQuery = sortedQuery.Where(n => n.user_id.Equals(userId.Value));

                if (!string.IsNullOrWhiteSpace(priority))
                    sortedQuery = sortedQuery.Where(n => n.priority.Equals(priority));

                if (isChecked.HasValue)
                    sortedQuery = sortedQuery.Where(o => o.is_checked.Equals(isChecked));

                if (skip.HasValue && count.HasValue)
                    sortedQuery = sortedQuery.Skip(skip.Value).Take(count.Value);

                return sortedQuery;
            };

            return sortDelegate;
        }

        public Func<IQueryable<OfferModel>, IQueryable<OfferModel>> SortOffers(int? userId, int? skip, int? count, bool byDesc, bool? sended, bool? isAccepted, string? type)
        {
            IQueryable<OfferModel> sortedQuery;

            Func<IQueryable<OfferModel>, IQueryable<OfferModel>> sortDelegate = query =>
            {
                sortedQuery = byDesc ? query.OrderByDescending(o => o.created_at) : query.OrderBy(o => o.created_at);

                if (userId.HasValue)
                {
                    if (sended.HasValue)
                    {
                        if (sended.Equals(true))
                        {
                            sortedQuery = sortedQuery.Where(o => o.sender_id.Equals(userId.Value));
                        }
                        else
                        {
                            sortedQuery = sortedQuery.Where(o => o.receiver_id.Equals(userId.Value));
                        }
                    }
                    else
                    {
                        sortedQuery = sortedQuery.Where(o => o.sender_id.Equals(userId.Value) || o.receiver_id.Equals(userId.Value));
                    }
                }

                if (isAccepted.HasValue)
                    sortedQuery = sortedQuery.Where(o => o.is_accepted.Equals(isAccepted));

                if (!string.IsNullOrWhiteSpace(type))
                    sortedQuery = sortedQuery.Where(o => o.offer_type.Equals(type));

                if (skip.HasValue && count.HasValue)
                    sortedQuery = sortedQuery.Skip(skip.Value).Take(count.Value);

                return sortedQuery;
            };

            return sortDelegate;
        }

        public Func<IQueryable<LinkModel>, IQueryable<LinkModel>> SortLinks(int? userId, int? skip, int? count, bool byDesc, bool? expired)
        {
            IQueryable<LinkModel> sortedQuery;

            Func<IQueryable<LinkModel>, IQueryable<LinkModel>> sortDelegate = query =>
            {
                sortedQuery = byDesc ? query.OrderByDescending(l => l.created_at) : query.OrderBy(l => l.created_at);

                if (userId.HasValue)
                    sortedQuery = sortedQuery.Where(l => l.user_id.Equals(userId.Value));

                if (expired.HasValue)
                    sortedQuery = expired.Equals(true) ? sortedQuery.Where(l => l.expiry_date < DateTime.UtcNow) : sortedQuery.Where(l => l.expiry_date > DateTime.UtcNow);

                if (skip.HasValue && count.HasValue)
                    sortedQuery = sortedQuery.Skip(skip.Value).Take(count.Value);

                return sortedQuery;
            };

            return sortDelegate;
        }
    }
}
