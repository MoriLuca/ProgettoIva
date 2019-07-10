using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace exware.Data
{
    public class Db : DbContext
    {
        public Db(DbContextOptions<Db> options)
        : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;port=3306;database=exware;uid=root;password=0000");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Tag4Db>().Ignore(p => p.Value);
        }

        public DbSet<Tag4Db> Tags { get; set; }

        public class Tag4Db
        {
            public int Tag4DbId { get; set; }
            public DateTime Time { get; set; }
            public string Value { get; set; }
            public string Name { get; set; }
            public string Node { get; set; }
            public string Type { get; set; }

            public Tag4Db(){}
            public Tag4Db(LMOpcuaConnector.Data.Tag _tag)
            {
                Time = DateTime.Now;
                Value = _tag.Value.ToString();
                Name = _tag.Name;
                Node = _tag.NodeId.ToString();
                Type = _tag.Type.BuiltInType.ToString();
            }
        }
    }

}
