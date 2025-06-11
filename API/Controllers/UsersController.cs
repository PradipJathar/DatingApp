using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext db;
        public UsersController(DataContext dataContext)
        {
            db = dataContext;
        }


        [HttpGet]
        public ActionResult<IEnumerable<AppUser>> GetUsers()
        {
            return db.Users.ToList();
        }


        [HttpGet("{id}")]
        public ActionResult<AppUser> GetUser(int id)
        {
            return db.Users.Find(id);
        }
    }
}
