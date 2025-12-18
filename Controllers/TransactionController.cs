using Microsoft.AspNetCore.Authorization;
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
    public class TransactionController(ApplicationDbContext context, VerifyTransactions verifyTransactions, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly VerifyTransactions _verifyTransactions = verifyTransactions;

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductChain(Guid productId)
        {
            var transactions = await _context.Transactions
              .Where(t => t.ProductId == productId && t.ConfirmationStatus == "Confirmed")
              .OrderBy(t => t.CreatedAt)
              .ToListAsync();

            bool isChainValid = await _verifyTransactions.VerifyChainAsync(productId);

            return Ok(new
            {
                transactions,
                chainValid = isChainValid,
            });
        }


    }
}