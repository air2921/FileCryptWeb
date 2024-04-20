using domain.Models;

namespace application.DTO.Inner
{
    public class AcceptOfferDTO
    {
        public string KeyName { get; set; }
        public int StorageId { get; set; }
        public OfferModel Offer { get; set; }
    }
}
