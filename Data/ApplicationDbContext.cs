using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using provenancetracker.Models.Entities;

namespace provenancetracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<
    ApplicationUser,
    IdentityRole<Guid>,
    Guid
>(options)
{
    public DbSet<Product> Products { get; set; }

    public DbSet<Transaction> Transactions { get; set; }
}
