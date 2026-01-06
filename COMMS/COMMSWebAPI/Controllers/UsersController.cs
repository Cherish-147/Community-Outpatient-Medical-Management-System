using COMMSWebAPI.Models;
using COMMSWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMMSWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(MyDbContext db, IAuthService iauthService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetAllUser()//get all user
        {
            var result = await db.Users.ToListAsync();
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUserById(int id)//by UserId
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return NotFound();
            return Ok(user);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, Users users)
        {
            if (id != users.UserID) return BadRequest();
            db.Entry(users).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!db.Users.Any(u => u.UserID == id)) return NotFound(ex.ToString()); throw;
            }
            return Ok();

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Users>> CreateUser(Users users)
        {
            users.UserID = 0;//because Identity
            string hashedInputPassword = iauthService.HashPassword(users.Password);//Hash
            users.Password = hashedInputPassword;
            db.Users.Add(users); await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserById), new { id = users.UserID }, users);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await db.Users.FindAsync(id); if (user == null) return NotFound();
            db.Users.Remove(user); await db.SaveChangesAsync();
            return Ok();
        }
    }
}
