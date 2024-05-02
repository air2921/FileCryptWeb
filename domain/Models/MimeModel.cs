using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace domain.Models
{
    [Table("forbidden_mimes")]
    public class MimeModel
    {
        [Key]
        public int mime_id { get; set; }

        public string mime_name { get; set; }
    }
}
