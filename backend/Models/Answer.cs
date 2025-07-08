using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int QuestionnaireId { get; set; }
        public Questionnaire Questionnaire { get; set; } = null!;
    }
}
