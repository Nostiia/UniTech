using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniTech.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        public int Post_Id { get; set; }
        public string Topic { get; set; }
        public string Search_hucks { get; set; }
        public string Post_text { get; set; }

        public int TeacherId { get; set; }
        public User Teacher { get; set; }
    }

}
