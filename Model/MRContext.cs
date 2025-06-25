// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers

using Microsoft.EntityFrameworkCore;



namespace MediaRecycler;

/// <summary>
/// DB context used for Media Recycler application.
/// </summary>
public partial class MRContext : DbContext
{
    public MRContext()
    {
    }

    public MRContext(DbContextOptions<MRContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TargetLink> TargetLinks { get; set; }

    public virtual DbSet<PostPage> PostPages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\kcrow\\Documents\\Data\\Solutions\\MediaRecycler\\MediaRecycler\\Model\\Database1.mdf;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TargetLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Table__3214EC070FFF6B39");

            entity.Property(e => e.Link).HasMaxLength(150);
            entity.Property(e => e.PostId).HasMaxLength(50);
        });

        modelBuilder.Entity<PostPage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostPage__3214EC07B0094615");

            entity.Property(e => e.Link).HasMaxLength(150);
            entity.Property(e => e.PostId).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
