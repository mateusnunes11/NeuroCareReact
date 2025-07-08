using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using backend.Models;


public class User : IdentityUser
{

    public ICollection<Questionnaire> Questionnaires { get; set; } = new List<Questionnaire>();
}
