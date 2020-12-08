using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Models;
using NGTI_Calender.Data;

namespace NGTI_Calender.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        //End point
        [HttpPost("get-details-from-google")]
        public async Task<IActionResult> GetUserDataFromBigG([FromBody] Person user)
        {
            if (ModelState.IsValid)
            {
                bool newUser = true;
                foreach(var item in _context.Person)
                {
                    if(item.EMail == user.EMail)
                    {
                        newUser = false;
                    }
                }
                if (newUser)
                {
                    _context.Add(user);
                    _context.SaveChanges();
                }
                foreach (var item in _context.Person)
                {
                    if (user.EMail == item.EMail)
                    {
                        user.PersonId = item.PersonId;
                    }
                }
                return Ok(user);
            }
            return BadRequest(ModelState);
        }

        //Get
        [HttpGet("get-personid")]
        public string Get(string email)
        {
            foreach(var item in _context.Person)
            {
                if(email == item.EMail)
                {
                    return item.PersonId.ToString();
                }
            }
            return "";
        }
    }
}
