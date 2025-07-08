namespace backend.Models
{
    public class QuestionnaireRequest
    {
        public List<AnswerRequest> Answers { get; set; } = new();
    }

    public class AnswerRequest
    {
        public int Value { get; set; }
    }
}
