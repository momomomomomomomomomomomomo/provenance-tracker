using System;
using Microsoft.AspNetCore.Identity;

namespace provenancetracker.Models.Entities;


public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsApproved { get; set; } = false;
}