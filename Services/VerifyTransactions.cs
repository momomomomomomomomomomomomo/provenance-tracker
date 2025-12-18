using System;
using Microsoft.EntityFrameworkCore;
using provenancetracker.Data;

namespace provenancetracker.Services;

public class VerifyTransactions(ApplicationDbContext context)
{

    private readonly ApplicationDbContext _context = context;
    public async Task<bool> VerifyChainAsync(Guid productId)
    {
        var chain = await _context.Transactions
            .Where(t => t.ProductId == productId && t.ConfirmationStatus == "Confirmed")
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        if (chain.Count == 0) return true;


        string lastValidHash = "0000000000000000000000000000000000000000000000000000000000000000";

        foreach (var transaction in chain)
        {

            if (transaction.PreviousHash != lastValidHash)
            {
                return false;
            }
            string status = transaction.Status?.Trim() ?? "";
            string location = transaction.Location?.Trim() ?? "";
            string description = transaction.Description?.Trim() ?? "";

            string blockData = $"{transaction.ProductId}{transaction.ParticipantId}{transaction.Status}{transaction.CreatedAt.Ticks}{transaction.PreviousHash}{transaction.Location}{transaction.Description}";
            string recalculatedHash = HashTransaction.ComputeSha256Hash(blockData);
            Console.WriteLine($"id:{transaction.ParticipantId}////{blockData}");
            if (recalculatedHash != transaction.CurrentHash)
            {
                return false;
            }

            lastValidHash = transaction.CurrentHash;
        }

        return true;
    }

}
