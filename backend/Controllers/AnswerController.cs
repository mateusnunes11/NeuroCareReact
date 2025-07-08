using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend;
using backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnswerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Answer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Answer>>> GetAnswers()
        {
            return await _context.Answers.Include(a => a.Questionnaire).ToListAsync();
        }

        // GET: api/Answer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Answer>> GetAnswer(int id)
        {
            var answer = await _context.Answers.Include(a => a.Questionnaire)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (answer == null)
                return NotFound();

            return answer;
        }

        // POST: api/Answer
        [HttpPost]
        public async Task<ActionResult<Answer>> PostAnswer(Answer answer)
        {
            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnswer), new { id = answer.Id }, answer);
        }

        // PUT: api/Answer/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnswer(int id, Answer answer)
        {
            if (id != answer.Id)
                return BadRequest();

            _context.Entry(answer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Answers.AnyAsync(a => a.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Answer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var answer = await _context.Answers.FindAsync(id);

            if (answer == null)
                return NotFound();

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
