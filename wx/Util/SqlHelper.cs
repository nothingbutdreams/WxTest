using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace wx.Util
{
    public class SqlHelper
    {
        //数据库连接字符串
        public static readonly string sqlConnectionString = ConfigurationManager.ConnectionStrings["conSql"].ConnectionString;


        // 使用哈希表存储缓存后的参数
        private static Hashtable parmCache =
            Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 执行不返回的结果的SQL语句，依赖于特定的数据库连接和使用的提供者参数
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, 
        ///  "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">为SqlConnection指定一个有效的连接字符串</param>
        /// <param name="commandType">命令类型(stored procedure, text等等)</param>
        /// <param name="commandText">存储过程名或T-SQL命令</param>
        /// <param name="commandParameters">SqlParamters数组用于执行命令</param>
        /// <returns>返回一个整型值得到影响的行数</returns>
        public static int ExecuteNonQuery(string cmdText,
            CommandType cmdType, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();  //创建SqlCommand对象实例
            //根据传入的连接字符串构建SqlConnection对象
            using (SqlConnection conn = new SqlConnection(sqlConnectionString))
            {
                //根据传入的SqlParameter以及cmdType和cmdText构建SqlCommand命令
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();  //执行SQL并返回受影响的行数
                cmd.Parameters.Clear();           //清除参数集合
                return val;                       //返回受影响的行数
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) using an existing SQL Transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">an existing sql transaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(string cmdText,
            CommandType cmdType,params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();                        //实例化SqlCommand对象
            SqlConnection conn = new SqlConnection(sqlConnectionString); //根据连接字符串实例化SqlConnection
            try
            {
                //根据传入的SqlParameter数组及相关SQL参数设置SqlCommand属性
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                //执行连接在执行完毕后关闭连接
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();   //清除参数列表
                return rdr;               //返回SqlDataReader对象
            }
            catch
            {
                conn.Close();             //在出现异常时即时关闭连接
                throw;                    //重新抛出异常
            }
        }

        public static DataTable ExecuteTable(string cmdText,
            CommandType cmdType, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand(); //实例化SqlCommand对象
            //根据指定的连接字符串创建SqlConnection对象
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            DataSet ds = null;
            try
            {
                //构建SqlCommand对象的相关属性
                PrepareCommand(cmd, conn, null, cmdType,
                    cmdText, commandParameters);
                //创建SqlDataAdapter实例，传入SqlCommand对象
                SqlDataAdapter dp = new SqlDataAdapter(cmd);
                ds = new DataSet();
                dp.Fill(ds);  //执行SQL语句并填充DataSet
            }
            catch
            {
                conn.Close();  //在出现异常时关闭连接
                throw;         //抛出异常
            }
            finally
            {
                if (conn != null) //无论是否出现异常，在离开时都要关闭连接
                    conn.Close();
            }
            //返回DataSet的第1个表
            return (ds.Tables[0] != null) ? ds.Tables[0] : null;
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string cmdText, CommandType cmdType, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 添加参数数组到缓存
        /// </summary>
        /// <param name="cacheKey">参数缓存的键</param>
        /// <param name="cmdParms">要被缓存的SqlParamters数组</param>
        public static void CacheParameters(string cacheKey,
            params SqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;  //添加到哈希表
        }
        /// <summary>
        /// 获取缓存的参数数组
        /// </summary>
        /// <param name="cacheKey">用来查找缓存的参数的键</param>
        /// <returns>缓存的SqlParamters数组</returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms =
                (SqlParameter[])parmCache[cacheKey];  //获取Sql参数数组
            //如果当前缓存中并不存在参数数组，则返回null
            if (cachedParms == null)
                return null;
            //实例化新的SqlParameter数组
            SqlParameter[] clonedParms =
                new SqlParameter[cachedParms.Length];
            //将cachedParm表中缓存的参数赋给SqlParameter数组
            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();
            return clonedParms;   //返回SqlCommand数组实例
        }

        /// <summary>
        /// Prepare a command for execution
        /// </summary>
        /// <param name="cmd">SqlCommand object</param>
        /// <param name="conn">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn,
            SqlTransaction trans, CommandType cmdType,
            string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)  //判断连接是否打开
                conn.Open();                         //打开连接
            cmd.Connection = conn;                   //赋给SqlCommand连接对象
            cmd.CommandText = cmdText;               //指定连接文本
            if (trans != null)
                cmd.Transaction = trans;            //是否开启事务
            cmd.CommandType = cmdType;              //指定命令类型
            if (cmdParms != null)
            {  //循环SqlParameter数组，添加到参数集合
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

    }
}