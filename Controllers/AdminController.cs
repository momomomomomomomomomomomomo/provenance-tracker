using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using provenancetracker.Data;
using provenancetracker.Models.Entities;
using provenancetracker.Services;

namespace provenancetracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(ApplicationDbContext context, VerifyTransactions verifyTransactions, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly VerifyTransactions _verifyTransactions = verifyTransactions;
        private readonly UserManager<ApplicationUser> _userManager = userManager;






        [HttpGet("pending-transactions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingTransactions()
        {
            var pendingTransactions = await _context.Transactions
                .Where(t => t.ConfirmationStatus == "Pending")
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(pendingTransactions);
        }

        [HttpGet("pending-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingUsers()
        {
            //  Get ALL users in the "Participant" role
            var participants = await _userManager.GetUsersInRoleAsync("Participant");


            var pendingParticipants = participants
                .Where(u => !u.IsApproved)
                .ToList();

            return Ok(pendingParticipants);
        }


        [HttpPost("confirm-transaction/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmTransaction(Guid id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound("Transaction not found.");

            if (transaction.ConfirmationStatus == "Confirmed")
            {

                return BadRequest("Transaction already confirmed.");
            }

            // Verify chain integrity before confirming
            bool isChainValid = await _verifyTransactions.VerifyChainAsync(transaction.ProductId);
            if (!isChainValid)
            {
                return BadRequest("Chain integrity compromised. Cannot confirm transaction.");
            }

            transaction.ConfirmationStatus = "Confirmed";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Transaction confirmed", transaction });
        }

        [HttpPost("confirm-user/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmUser(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { Message = $"User with email {email} not found." });
            }

            user.IsApproved = true;

            var result = await _userManager.UpdateAsync(user);


            if (result.Succeeded)
            {
                return Ok(new { Message = "User approved successfully." });
            }

            // If the update failed (e.g., concurrency stamp issue, validation)
            return BadRequest(result.Errors);
        }

        [HttpPost("cancel-transaction/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelTransaction(Guid id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound("Transaction not found.");

            if (transaction.ConfirmationStatus == "Confirmed")
                return BadRequest("Cannot cancel a transaction that is already confirmed.");


            transaction.ConfirmationStatus = "Cancelled";



            await _context.SaveChangesAsync();

            return Ok(new { message = "Transaction cancelled", transaction });
        }
    }
}
