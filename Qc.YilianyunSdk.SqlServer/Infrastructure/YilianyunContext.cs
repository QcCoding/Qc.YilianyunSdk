using Microsoft.EntityFrameworkCore;
using Qc.YilianyunSdk.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk.SqlServer.Infrastructure
{
    public class YilianyunContext : DbContext
    {
        private readonly string _connectionString;

        public YilianyunContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public DbSet<AccessTokenOutputModel> AccessTokenOutputModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessTokenOutputModel>()
                .ToTable("tbAccessTokenOutput")
                .HasKey(a => a.Machine_Code);
        }
    }
}
