using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Inkton.Nester;
using Codesearch;
using Codesearch.Model;

namespace Codesearch.Database
{
    public class QueryContext : DbContext
    {
        public QueryContext (DbContextOptions<QueryContext> options)
            : base(options)
        {
        }

        public DbSet<SearchQuery> Queries { get; set; }
        public DbSet<SearchResult> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SearchQuery>()
                .HasKey(query => query.Id);
            modelBuilder.Entity<SearchResult>()
                .HasKey(result => result.Id);
        }
    }

    public static class QueryContextFactory
    {
        public static QueryContext Create(Runtime runtime)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueryContext>();

            string connectionString = string.Format(@"Server={0};database={1};uid={2};pwd={3};",
                                    runtime.MySQL.Host,
                                    runtime.MySQL.Resource,
                                    runtime.MySQL.User,
                                    runtime.MySQL.Password);

            optionsBuilder.UseMySql(connectionString);
            var context = new QueryContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
