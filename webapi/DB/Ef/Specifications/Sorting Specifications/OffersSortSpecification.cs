using Ardalis.Specification;
using webapi.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace webapi.DB.Ef.Specifications.Sorting_Specifications
{
    public class OffersSortSpecification : Specification<OfferModel>
    {
        public OffersSortSpecification(int? userId, int skip, int count, bool byDesc, bool? sended, bool? isAccepted, string? type)
        {
            if (byDesc)
                Query.OrderByDescending(o => o.created_at);
            else
                Query.OrderBy(o => o.created_at);

            if (userId.HasValue)
            {
                if (sended.HasValue)
                {
                    if (sended.Equals(true))
                        Query.Where(o => o.sender_id.Equals(userId.Value));
                    else
                        Query.Where(o => o.receiver_id.Equals(userId.Value));
                }
                else
                    Query.Where(o => o.sender_id.Equals(userId.Value) || o.receiver_id.Equals(userId.Value));
            }

            if (isAccepted.HasValue)
                Query.Where(o => o.is_accepted.Equals(isAccepted));

            if (!string.IsNullOrWhiteSpace(type))
                Query.Where(o => o.offer_type.Equals(type));

            Query.Skip(skip).Take(count);
        }
    }
}
