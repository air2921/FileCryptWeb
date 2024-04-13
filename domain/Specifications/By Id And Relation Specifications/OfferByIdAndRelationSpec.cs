using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.By_Relation_Specifications
{
    public class OfferByIdAndRelationSpec : Specification<OfferModel>
    {
        public OfferByIdAndRelationSpec(int offerId, int userId, bool? sent)
        {
            OfferId = offerId;
            UserId = userId;
            Sent = sent;

            if (sent.HasValue)
            {
                if (sent.Equals(true))
                    Query.Where(x => x.offer_id.Equals(offerId) && x.sender_id.Equals(userId));
                else
                    Query.Where(x => x.offer_id.Equals(offerId) && x.receiver_id.Equals(userId));
            }
            else
                Query.Where(x => x.offer_id.Equals(offerId) && (x.receiver_id.Equals(userId) || x.sender_id.Equals(userId)));
        }

        public int OfferId { get; private set; }
        public int UserId { get; private set; }
        public bool? Sent { get; private set; }
    }
}
