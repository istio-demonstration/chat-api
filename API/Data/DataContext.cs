using System;
using System.Linq;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
  public class DataContext : IdentityDbContext<AppUser,AppRole,int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>,
       IdentityUserToken<int>>
  {
    public DataContext(DbContextOptions options) : base(options)
    {

    }
    //public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    public DbSet<Message> Message { get; set; }

    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // sqlite doesn't support DateTimeOffset
       
            base.OnModelCreating(modelBuilder);
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType
                                                                                   == typeof(decimal));
                    foreach (var property in properties)
                    {
                        modelBuilder.Entity(entityType.Name).Property(property.Name).HasConversion<double>(); // cuz sqlite doesn't support double column type,if you orderby decimal,it will throw an error
                    }
                    // sqlite doesn't support DateTimeOffset
                    var dateTimeProperties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset));
                    foreach (var property in dateTimeProperties)
                    {
                        modelBuilder.Entity(entityType.Name).Property(property.Name).HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }

            modelBuilder.Entity<UserLike>().HasKey(k => new { k.SourceUserId,  k.LikeUserId });
            modelBuilder.Entity<UserLike>().HasOne(s => s.SourceUser)
                                           .WithMany(l => l.LikedUsers)
                                           .HasForeignKey(s => s.SourceUserId)
                                           .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLike>().HasOne(s => s.LikedUser)
                                           .WithMany(l => l.LikedByUsers)
                                           .HasForeignKey(s => s.LikeUserId)
                                           .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>().HasOne(u => u.Recipient)
                                          .WithMany(m => m.MessagesReceived)
                                          .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>().HasOne(u => u.Sender)
                                          .WithMany(m => m.MessagesSent)
                                          .OnDelete(DeleteBehavior.Restrict);

            // AppUser
            modelBuilder.Entity<AppUser>().HasMany(ur => ur.UserRoles).WithOne(u => u.AppUser)
                .HasForeignKey(ur => ur.UserId).IsRequired();
            // AppRole
            modelBuilder.Entity<AppRole>().HasMany(ur => ur.UserRoles).WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId).IsRequired();

            //Group
            modelBuilder.Entity<Group>().ToTable("Groups");
            //Connection
            modelBuilder.Entity<Connection>().ToTable("Connections");

        }
    }
}