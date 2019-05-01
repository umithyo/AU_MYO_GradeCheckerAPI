using GradeCheckerAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GradeCheckerAPI.Data
{
    public class APIContext:DbContext
    {
        public static string ConnectionString { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ConnectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Seen> SeenClasses { get; set; }
    }
}
