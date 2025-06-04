using System.ComponentModel.DataAnnotations;

namespace UniTech.ViewsModels
{
    public class PostViewModel
    {
        [Required]
        public string Topic { get; set; }

        [Required]
        public string Search_hucks { get; set; }

        [Required]
        public string Post_text { get; set; }
    }
}
