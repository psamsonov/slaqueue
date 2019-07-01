using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SLAQueue.Models;
using SLAQueue.Services;

namespace SLAQueue.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult<IEnumerable<User>> Get()
        {
            return Ok(UserService.GetUsers());
        }

        [HttpPost]
        public ActionResult<Guid> CreateUser([FromBody]string slaClass)
        {
            if (Enum.TryParse(slaClass, out SLAClass parsedClass))
            {
                return Ok(UserService.CreateUser(parsedClass));
            }

            return BadRequest();
        }
    }
}