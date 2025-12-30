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
        public DbSet<ReqType> ReqTypes { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Response> Responses { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Report> Reports { get; set; }

        public DbSet<RequestHistory> RequestHistories { get; set; }

        public DbSet<Comment> Comments { get; set; }



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
                .HasOne(r => r.ReqType)
                .WithMany()
                .HasForeignKey(r => r.ReqTypeId);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.ReqPriority)
                .WithMany()
                .HasForeignKey(r => r.ReqPriorityId);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.ReqStatus)
                .WithMany()
                .HasForeignKey(r => r.ReqStatusId);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Executor)
                .WithMany()
                .HasForeignKey(r => r.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);




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


            // Report Entity Configuration
            modelBuilder.Entity<Report>()
                .HasKey(r => r.Id);

            // Report -> User (Sender)
            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Report -> ReqCategory
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReqCategory)
                .WithMany()
                .HasForeignKey(r => r.ReqCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Report -> ReqStatus
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReqStatus)
                .WithMany()
                .HasForeignKey(r => r.ReqStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Report -> ReqType
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReqType)
                .WithMany()
                .HasForeignKey(r => r.ReqTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Report -> ReqPriority
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReqPriority)
                .WithMany()
                .HasForeignKey(r => r.ReqPriorityId)
                .OnDelete(DeleteBehavior.Restrict);
            // Report -> Executor (User)
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Executor)
                .WithMany()
                .HasForeignKey(r => r.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Report -> Request (Optional relationship)
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Request)
                .WithMany()
                .HasForeignKey(r => r.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestHistory>()
                    .HasKey(rh => rh.Id);

            modelBuilder.Entity<RequestHistory>()
                    .HasOne(rh => rh.Request)
                    .WithMany()
                    .HasForeignKey(rh => rh.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RequestHistory>()
                .HasOne(rh => rh.User)
                .WithMany()
                .HasForeignKey(rh => rh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasMany(r => r.Comments)
                .WithOne(c => c.Request)
                .HasForeignKey(c => c.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

        }



    }
}