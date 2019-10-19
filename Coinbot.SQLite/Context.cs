using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Coinbot.SQLite.Models;

namespace Coinbot.SQLite
{
    public class CoinbotContext : DbContext
    {
        private readonly string _connString;

        public DbSet<Order> Orders { get; set; }
        public DbSet<Tick> Ticks { get; set; }
        public CoinbotContext()
        {
            // added parameterless ctor for design time
            _connString = "Data Source=Coinbot.db";
        }

        public CoinbotContext(string connString)
        {
            _connString = connString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasIndex(b => new { b.SellId, b.BuyId, b.Stock, b.SessionId});

            modelBuilder.Entity<Tick>()
                .HasIndex(b => new { b.BaseCoin, b.TargetCoin, b.Stock, b.SessionId });
        }
    }
}
