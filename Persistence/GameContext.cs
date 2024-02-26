using GOF.Entities;
using Microsoft.EntityFrameworkCore;

namespace GOF.Persistence
{
    public class GameContext : DbContext
    {
        public DbSet<Board> Boards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("GameofLife");
        }
    }
}
