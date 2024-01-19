namespace webapi.DTO
{
    public class NotifyDTO
    {
        public string message_header { get; set; }
        public string message { get; set; }
        public string priority { get; set; }
        public int receiver_id { get; set; }
    }
}
