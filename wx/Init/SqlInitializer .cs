using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using wx.Context;

namespace wx.Init
{
    public class SqlInitializer : IDatabaseInitializer<SqlDbContext>
    {

        #region 继承 IDatabaseInitializer<SqlDbContext>接口,实现InitializeDatabase()方法
        public void InitializeDatabase(SqlDbContext context)
        {
            //判断数据库是否已经存在
            if (context.Database.Exists())
            {
                //数据库模式是否与模型兼容
                if (!context.Database.CompatibleWithModel(true))
                {
                    context.Database.Delete();
                }
            }
            context.Database.Create();
                
        }
        #endregion
        
    }
}