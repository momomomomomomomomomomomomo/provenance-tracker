using System;

namespace provenancetracker.Models.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
}
