
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data
{

    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options) { }

        public DbSet<FileEntity> Files { get; set; }
    }

}