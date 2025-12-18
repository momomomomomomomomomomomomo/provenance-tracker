using System;

namespace provenancetracker.Models.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ParticipantId { get; set; }

    public string Description { get; set; }
    public string Status { get; set; }

    public string PreviousHash { get; set; }
    public string CurrentHash { get; set; }
    public string Location { get; set; }
    public string ConfirmationStatus { get; set; }

    public DateTime CreatedAt { get; set; }


}
