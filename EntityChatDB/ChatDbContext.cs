using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EntityChatDB;

public partial class ChatDbContext : DbContext
{
    //private static string? DataSourse { get; set; }
    //private static string? NameDB { get; set; }
    public ChatDbContext()
    {
        //DataSourse = dataSourse;
        //NameDB = nameDB;
    }

    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseSqlServer($"Data Source={DataSourse};Initial Catalog={NameDB};Integrated Security=True;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImagesId).HasName("PK__Images__0E2E80BF63FF2937");

            entity.Property(e => e.Screen)
                .HasDefaultValueSql("(0x)")
                .HasColumnType("image");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C0C9CA9580BBB");

            entity.ToTable("Message");

            entity.Property(e => e.DataMessage)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Message1)
                .HasMaxLength(1)
                .HasColumnName("Message");

            entity.HasOne(d => d.UserFromNavigation).WithMany(p => p.MessageUserFromNavigations)
                .HasForeignKey(d => d.UserFrom)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFrom");

            entity.HasOne(d => d.UserToNavigation).WithMany(p => p.MessageUserToNavigations)
                .HasForeignKey(d => d.UserTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserTo");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C57CEE529");

            entity.HasIndex(e => e.ScreenId, "UQ__Users__0AB60FA45AAD0B0E").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F284560DC3E267").IsUnique();

            entity.Property(e => e.UserName)
                .HasMaxLength(30)
                .IsFixedLength();
            entity.Property(e => e.UserPassword).HasMaxLength(1);

            entity.HasOne(d => d.Screen).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.ScreenId)
                .HasConstraintName("FK_ScreenId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
