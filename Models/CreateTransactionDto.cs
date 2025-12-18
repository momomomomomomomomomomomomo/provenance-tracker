using System;

namespace provenancetracker.Models;

public class CreateTransactionDto
{
    public Guid ProductId { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public bool FirstTransaction { get; set; }
}
