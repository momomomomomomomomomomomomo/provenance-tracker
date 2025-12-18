using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using provenancetracker.Data;
using provenancetracker.Models.Entities;

namespace provenancetracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserManager<ApplicationUser> userManager) : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager = userManager;



        [HttpPut("participant-approve")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddParticipantRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (await _userManager.IsInRoleAsync(user, "Participant"))
            {

                return Ok(new { message = "User is already a Participant." });
            }


            var roleResult = await _userManager.AddToRoleAsync(user, "Participant");


            if (roleResult.Succeeded)
            {
                return Ok(new { message = "Participant role added." });
            }


            return BadRequest(new { message = "Failed to add role.", errors = roleResult.Errors });

        }
    }
}