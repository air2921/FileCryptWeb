using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class OffersSortSpec : Specification<OfferModel>
    {
        public OffersSortSpec(int? userId, int skip, int count, bool byDesc, bool? sent, bool? isAccepted, string? type)
        {
            UserId = userId;
            SkipCount = skip;
            Count = count;
            ByDesc = byDesc;
            Sent = sent;
            IsAccepted = isAccepted;
            Type = type;

            if (byDesc)
                Query.OrderByDescending(o => o.created_at);
            else
                Query.OrderBy(o => o.created_at);

            if (userId.HasValue)
            {
                if (sent.HasValue)
                {
                    if (sent.Equals(true))
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

        public int? UserId { get; private set; }
        public int SkipCount { get; private set; }
        public int Count { get; private set; }
        public bool ByDesc { get; private set; }
        public bool? Sent { get; private set; }
        public bool? IsAccepted { get; private set; }
        public string? Type { get; private set; }
    }
}
