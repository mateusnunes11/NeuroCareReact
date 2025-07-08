using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnaireController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuestionnaireController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Questionnaire>>> GetQuestionnaires()
        {
            return await _context.Questionnaires
                .Include(q => q.User)
                .Include(q => q.Answers)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Questionnaire>> GetQuestionnaire(int id)
        {
            var questionnaire = await _context.Questionnaires
                .Include(q => q.User)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (questionnaire == null)
                return NotFound();

            return questionnaire;
        }

        // DTOs para receber o formato enviado pelo frontend
        public class QuestionnaireRequest
        {
            public List<AnswerRequest> Answers { get; set; } = new();
        }

        public class AnswerRequest
        {
            public int Value { get; set; }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PostQuestionnaire([FromBody] QuestionnaireRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (request.Answers == null || request.Answers.Count != 20)
                return BadRequest(new { error = "Você deve enviar exatamente 20 respostas." });

            var questionnaire = new Questionnaire
            {
                UserId = userId,
                Answers = request.Answers.Select(a => new Answer { Value = a.Value }).ToList()
            };

            _context.Questionnaires.Add(questionnaire);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Questionário salvo com sucesso." });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutQuestionnaire(int id, Questionnaire questionnaire)
        {
            if (id != questionnaire.Id)
                return BadRequest();

            _context.Entry(questionnaire).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Questionnaires.AnyAsync(q => q.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestionnaire(int id)
        {
            var questionnaire = await _context.Questionnaires.FindAsync(id);

            if (questionnaire == null)
                return NotFound();

            _context.Questionnaires.Remove(questionnaire);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("me/metrics")]
        [Authorize]
        public async Task<IActionResult> GetMyMetrics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var questionnaires = await _context.Questionnaires
                .Where(q => q.UserId == userId)
                .Include(q => q.Answers)
                .ToListAsync();

            if (!questionnaires.Any())
                return Ok(new { count = 0, average = 0, max = 0, history = new int[0] });

            var scores = questionnaires
                .Select(q => q.Answers.Count(a => a.Value == 1))
                .ToList();

            return Ok(new
            {
                count = questionnaires.Count,
                average = scores.Average(),
                max = scores.Max(),
                history = scores
            });
        }

        [HttpGet("global/metrics")]
        public async Task<IActionResult> GetGlobalMetrics()
        {
            var questionnaires = await _context.Questionnaires
                .Include(q => q.Answers)
                .ToListAsync();

            if (!questionnaires.Any())
                return Ok(new { average = 0, percentageRisk = 0, total = 0, distribution = new Dictionary<int, int>() });

            var scores = questionnaires
                .Select(q => q.Answers.Count(a => a.Value == 1))
                .ToList();

            var average = scores.Any() ? scores.Average() : 0;
            var percentageRisk = scores.Count > 0 ? 100.0 * scores.Count(s => s >= 7) / scores.Count() : 0;
            var distribution = scores.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());

            return Ok(new
            {
                average,
                percentageRisk,
                total = scores.Count(),
                distribution
            });
        }
    }
}
