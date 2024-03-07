namespace webapi.DTO
{
    public class UpdateStorageDTO
    {
        public string? storage_name { get; set; }
        public bool? encrypt { get; set; }
        public int? access_code { get; set; }
    }
}
