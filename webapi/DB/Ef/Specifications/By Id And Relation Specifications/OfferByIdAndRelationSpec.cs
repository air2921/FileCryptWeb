using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class OfferByIdAndRelationSpec : Specification<OfferModel>
    {
        public OfferByIdAndRelationSpec(int offerId, int userId, bool? sent)
        {
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
    }
}
