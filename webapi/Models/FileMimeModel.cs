using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace webapi.Models
{
    [Table("allowedmime")]
    public class FileMimeModel
    {
        [Key]
        public int mime_id { get; set; }

        public string? mime_name { get; set; }
    }
}
