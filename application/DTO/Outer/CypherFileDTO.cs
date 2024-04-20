namespace application.DTO.Outer
{
    public class CypherFileDTO
    {
        public int UserId { get; set; }
        public int KeyId { get; set; }
        public int StorageId { get; set; }
        public string Code { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream Content { get; set; }
        public string Operation { get; set; }
    }
}
