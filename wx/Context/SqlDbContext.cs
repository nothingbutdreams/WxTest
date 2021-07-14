using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using wx.Migrations;
using wx.Models;

namespace wx.Context
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext()
            : base("name=conSql")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SqlDbContext, Configuration>("conSql"));
        }

        #region 数据库相关表 新增表需要在此添加对应关系 

        public virtual DbSet<WxUser> WxUser { get; set; }

        public virtual DbSet<UserAuth> UserAuth { get; set; }
        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //阻止表名复数形式
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}