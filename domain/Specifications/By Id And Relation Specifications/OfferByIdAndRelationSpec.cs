using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.By_Relation_Specifications
{
    public class OfferByIdAndRelationSpec : Specification<OfferModel>
    {
        public OfferByIdAndRelationSpec(int offerId, int userId)
        {
            OfferId = offerId;
            UserId = userId;

            Query.Where(x => x.offer_id.Equals(offerId) && (x.receiver_id.Equals(userId) || x.sender_id.Equals(userId)));
        }

        public int OfferId { get; private set; }
        public int UserId { get; private set; }
    }
}
