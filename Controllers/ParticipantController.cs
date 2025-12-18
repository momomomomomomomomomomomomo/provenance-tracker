using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using provenancetracker.Data;
using provenancetracker.Models;
using provenancetracker.Models.Entities;
using provenancetracker.Services;

namespace provenancetracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto transaction)
        {

            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null || !appUser.IsApproved)
            {
                return Unauthorized();
            }

            var product = await _context.Products.FindAsync(transaction.ProductId);
            if (product == null && !transaction.FirstTransaction)
            {
                return BadRequest("No Product found.");
            }
            if (product == null)
            {
                product = new Product
                {
                    Id = transaction.ProductId,
                    Description = transaction.Description,
                    Status = transaction.Status,
                };
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
            }





            //  Get the correct last transaction *for this product*
            var lastTransaction = await _context.Transactions
                .Where(t => t.ProductId == transaction.ProductId && t.ConfirmationStatus == "Confirmed")
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            var currentDate = DateTime.UtcNow;
            string status = transaction.Status?.Trim() ?? "";
            string location = transaction.Location?.Trim() ?? "";
            string description = transaction.Description?.Trim() ?? "";
            string prevHash = lastTransaction == null
                ? "0000000000000000000000000000000000000000000000000000000000000000"
                : lastTransaction.CurrentHash;
            string blockData = $"{transaction.ProductId}{appUser.Id}{transaction.Status}{currentDate.Ticks}{prevHash}{transaction.Location}{transaction.Description}";
            Console.WriteLine($"id:{appUser.Id}/////{blockData}");

            //  Create the new transaction
            Transaction newTransaction = new Transaction
            {
                ProductId = transaction.ProductId,
                ParticipantId = appUser.Id,
                Status = transaction.Status!,
                Description = transaction.Description!,
                Location = transaction.Location!,
                CreatedAt = currentDate,
                PreviousHash = prevHash,
                CurrentHash = HashTransaction.ComputeSha256Hash(blockData),
                ConfirmationStatus = "Pending"
            };

            await _context.Products
                    .Where(p => p.Id == transaction.ProductId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Description, transaction.Description)
                        .SetProperty(p => p.Status, transaction.Status)
                    );


            await _context.Transactions.AddAsync(newTransaction);
            await _context.SaveChangesAsync();

            return Ok(newTransaction);
        }


        [HttpGet]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetTransactions()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {

                    return NotFound("User not found for the provided token.");
                }

                var transactions = await _context.Transactions
                    .Where(t => t.ParticipantId == user.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();


                return Ok(transactions);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }
    }
}