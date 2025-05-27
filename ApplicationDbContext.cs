using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VoziBa.Models;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options): base(options)
    {
    }
    public DbSet<Korisnik> Korisnik { get; set; }
    public DbSet<Vozilo> Vozilo { get; set; }
    public DbSet<Recenzija> Recenzija { get; set; }
    public DbSet<Rezervacija> Rezervacija { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
        modelBuilder.Entity<Vozilo>().ToTable("Vozilo");
        modelBuilder.Entity<Rezervacija>().ToTable("Rezervacija");
        modelBuilder.Entity<Recenzija>().ToTable("Recenzija");
        base.OnModelCreating(modelBuilder);
    }
}