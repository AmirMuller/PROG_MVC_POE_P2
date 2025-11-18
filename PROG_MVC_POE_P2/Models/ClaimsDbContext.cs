using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PROG_MVC_POE_P2.Models;

public partial class ClaimsDbContext : DbContext
{
    public ClaimsDbContext()
    {
    }

    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Claim> Claim { get; set; }

    public virtual DbSet<ClaimReviewView> ClaimReviewView { get; set; }

    public virtual DbSet<Lecturer> Lecturer { get; set; }

    public virtual DbSet<Payment> Payment { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=dbClaims;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.ClaimId).HasName("PK__Claim__EF2E139B1FD068B7");

            entity.Property(e => e.ClaimTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(100);

            entity.HasOne(d => d.Lecturer).WithMany(p => p.Claim)
                .HasForeignKey(d => d.LecturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Claim_Lecturer");

            entity.HasOne(d => d.Pay).WithMany(p => p.Claim)
                .HasForeignKey(d => d.PayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Claim_Payment");
        });

        modelBuilder.Entity<ClaimReviewView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ClaimReviewView");

            entity.Property(e => e.ClaimTime).HasColumnType("datetime");
            entity.Property(e => e.LecturerName).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(100);
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(e => e.LecturerId).HasName("PK__Lecturer__5A78B93D0EE15C2E");

            entity.Property(e => e.Faculty).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Position).HasMaxLength(255);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PayId).HasName("PK__Payment__EE8FCECF49309A95");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
