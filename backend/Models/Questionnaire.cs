using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace backend.Models
{
    public class Questionnaire
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
