using Microsoft.EntityFrameworkCore;
using Models;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Request> Requests { get; set; }
        public DbSet<ReqCategory> Categories { get; set; }
        public DbSet<ReqStatus> Statuses { get; set; }
        public DbSet<ReqPriority> Priorities { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Response> Responses { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Request>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.ReqCategory)
                .WithMany()
                .HasForeignKey(r => r.ReqCategoryId);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.ReqPriority)
                .WithMany()
                .HasForeignKey(r => r.ReqPriorityId);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.ReqStatus)
                .WithMany()
                .HasForeignKey(r => r.ReqStatusId);



            modelBuilder.Entity<Response>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.Request)
                .WithOne(req => req.Response)
                .HasForeignKey<Response>(r => r.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.User)
                .WithMany(u => u.Responses)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.ResStatus)
                .WithMany()
                .HasForeignKey(r => r.ResStatusId);


            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

        }


    }
}
