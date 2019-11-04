using Microsoft.EntityFrameworkCore;

namespace CashDesk
{
    class CashDeskContext : DbContext
    {
        public DbSet<Member> Members { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<Deposit> Deposits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseInMemoryDatabase("CashDesk");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>()
                .HasIndex(m => m.LastName)
                .IsUnique();

            modelBuilder.Entity<Member>()
                .HasMany(m => m.Memberships)            // N        Member.MemberShips
                .WithOne(m => m.Member)                 // 1        MemberShip.Member
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Membership>()
                .HasMany(m => m.Deposits)               // N        Membership.Deposits
                .WithOne(m => m.Membership)             // 1        Deposits.MemberShip
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
