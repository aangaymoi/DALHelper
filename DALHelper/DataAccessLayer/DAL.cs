using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Reflection;

[assembly: Obfuscation(Exclude = false, Feature = "preset(maximum);+anti ildasm;+anti tamper;-constants;-ctrl flow;+anti debug;+invalid metadata;-ref proxy;-resources;+rename(mode=letters,flatten=false);")]
namespace DataAccessLayer
{
    /// <summary> 
    /// Data access layer
    ///     One output parameter is supported, and the output parameter must be the last parameter, and everything be auto    
    /// </summary>
    public static class Base
    {
        // Access 2010, 2007
        // ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=.\DBNorthwind\Northwind.accdb;";
        // ProviderName = "System.Data.OleDb";

        // Oracle
        // ConnectionString = @"Provider=OraOLEDB.Oracle;Data Source=NTD_NEW;User Id=NTD_NEW;Password=ikkan;";
        // ProviderName = "System.Data.OleDb";

        // SqlServer
        // ConnectionString = @"Provider=SqlOLEDB;Server=.\SQLEXPRESS;Database=Id;Uid=sa;Pwd=123;";
        // "System.Data.OleDb";
        // System.Data.SqlClient
        
        private static DbProviderFactory _provider = null;
        private static String _connectionString = String.Empty;     // backup connection string
        private static String _providerName = String.Empty;         // backup provider name
        private static int _commandTimeout = 30;

        /// <summary> 
        /// CreateDataAdapter
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>DbDataAdapter</returns>
        private static DbDataAdapter CreateDataAdapter(this DbCommand command)
        {
            DbDataAdapter adapter = _provider.CreateDataAdapter();
            adapter.SelectCommand = command;

            return adapter;
        }

        /// <summary>
        /// AddParams
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>void</returns>
        private static void AddParams(this DbCommand command, object[] paramObj)
        {
            if (paramObj == null)
                return;

            command.Parameters.Clear();

            for (int idx = 0; idx < paramObj.Length; idx++)
            {
                DbParameter param = _provider.CreateParameter();
                param.ParameterName = String.Format("@A{0:d2}", idx);
                param.Value = paramObj[idx];
                                
                command.Parameters.Add(param);
            }
        }        

        /// <summary>
        /// CreateComm4nd
        /// </summary>
        /// <param name="conn">DbConnection</param>        
        /// <returns>DbCommand</returns>
        private static DbCommand CreateComm4nd(this DbConnection conn)
        {
            DbCommand command = _provider.CreateCommand();            
            command.Connection = conn;
            command.CommandTimeout = _commandTimeout;
            command.Connection.ConnectionString = _connectionString;

            command.Connection.Open();

            return command;
        }

        /// <summary>
        /// CreateComm4nd
        /// </summary>
        /// <param name="conn">DbConnection</param>
        /// <param name="transaction">DbTransaction</param>
        /// <returns>DbCommand</returns>
        private static DbCommand CreateComm4nd(this DbConnection conn, DbTransaction transaction)
        {
            DbCommand command = conn.CreateComm4nd();
            command.Transaction = transaction;
            return command;
        }

        /// <summary>
        /// Create command with query
        /// </summary>
        /// <param name="conn">DbConnection</param>
        /// <param name="query">string</param>
        /// <returns>DbCommand</returns>
        private static DbCommand CreateComm4nd(this DbConnection conn, string query, object[] paramObj)
        {
            DbCommand command = conn.CreateComm4nd();
            command.CommandText = query;
            command.AddParams(paramObj);
            return command;
        }

        /// <summary> 
        /// Using provider to CreateInstance
        /// </summary>
        /// <param name="providerName">string</param>
        /// <param name="connectionString">string</param>                
        /// <returns>void</returns>
        public static void CreateInstance(this string providerName, string connectionString, int commandTimeout)
        {
            if (_provider != null)
                return;

            _commandTimeout = commandTimeout;
            _providerName = providerName;
            _connectionString = connectionString;

            // create provider
            _provider = DbProviderFactories.GetFactory(_providerName);
        }

        /// <summary>
        /// public static void CreateInstance(this string connectionString, int commandTimeout)
        /// </summary>
        /// <param name="connectionString">string</param>
        /// <param name="commandTimeout">int</param>
        /// <returns></returns>
        public static void CreateInstance(this string connectionString, int commandTimeout)
        {
            "System.Data.OleDb".CreateInstance(connectionString, commandTimeout);
        }

        /// <summary>
        /// public static void CreateInstance(this string connectionString)
        /// </summary>
        /// <param name="connectionString">string</param>
        /// <returns></returns>
        public static void CreateInstance(this string connectionString)
        {
            connectionString.CreateInstance(120);
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>object</returns>
        public static object GetValue(this string query, object[] paramObj)
        {
            object paramOut = new Guid();
            return query.GetValue(paramObj, ref paramOut);
        }

        /// <summary>
        /// public static object GetValue(this string query, object[] paramObj, ref object paramOut)  
        ///     object x = null;
        ///     bool isOk = "svcUpdateSomething ?, ?, ? OUTPUT".GetValue(new object[] { id, version, pathFormatted, desc, 0 }, ref x).ToLong() == 0;
        ///     int botId = x.ToInt();
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramObj"></param>
        /// <param name="paramOut"></param>
        /// <returns></returns>
        public static object GetValue(this string query, object[] paramObj, ref object paramOut)
        {
            object value = null;

            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = conn.CreateComm4nd(query, paramObj))
                {
                    bool isOutput = !(paramOut is Guid);
                    if (isOutput) command.Parameters[command.Parameters.Count - 1].Direction = ParameterDirection.Output;

                    value = command.ExecuteScalar();
                    if (isOutput) paramOut = command.Parameters[command.Parameters.Count - 1].Value;
                }
            }

            return value;
        }

        /// <summary>
        /// public static T GetData<T>(this string query, object[] paramObj) where T : class, new()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="paramObj"></param>
        /// <returns></returns>
        public static T GetData<T>(this string query, object[] paramObj) where T : class, new()
        {
            object paramOut = new Guid();
            return query.GetData<T>(paramObj, ref paramOut);
        }

        /// <summary>
        /// GetData<DataSet/DataTable>
        /// </summary>
        /// <typeparam name="T">DataSet or DataTable</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object</param>
        /// <returns>DataSet or DataTable</returns>
        public static T GetData<T>(this string query, object[] paramObj, ref object paramOut) where T : class, new()
        {
            T dat = new T();

            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = conn.CreateComm4nd(query, paramObj))
                {
                    bool isOutput = !(paramOut is Guid);
                    if (isOutput) command.Parameters[command.Parameters.Count - 1].Direction = ParameterDirection.Output;

                    using (DbDataAdapter adapter = command.CreateDataAdapter())
                    {
                        if (dat is DataTable)
                            adapter.Fill(dat as DataTable);
                        else adapter.Fill(dat as DataSet);
                    }
                    if (isOutput) paramOut = command.Parameters[command.Parameters.Count - 1].Value;                    
                }
            }

            return dat;
        }

        /// <summary>
        /// GetObjects<T>
        /// Example:
        /// -------------------------------------------------
        ///     public class B
        ///     {
        ///         public string Alias { get; set; }
        ///         public int Class { get; set; }
        ///     }
        ///     string query = "Select Alias, Class from A where Id = ?";
        ///     List<B> bb = query.GetObjects<B>(new object[] { 42 });
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>List<T></returns>
        public static List<T> GetObjects<T>(this string query, object[] paramObj)
            where T : class, new()
        {
            return query.GetObjects<T>(paramObj, new Mapper<T>());
        }        

        /// <summary>
        /// public static List<T> GetObjects<T>(this string query, object[] paramObj, ref object paramOut) where T : class, new()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="paramObj"></param>
        /// <param name="paramOut"></param>
        /// <returns></returns>
        public static List<T> GetObjects<T>(this string query, object[] paramObj, ref object paramOut)
            where T : class, new()
        {
            return query.GetObjects<T>(paramObj, ref paramOut, new Mapper<T>());
        }

        /// <summary>
        /// GetObjects<T>
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <param name="map">Mapper<T></param>
        /// <returns>List<T></returns>
        public static List<T> GetObjects<T>(this string query, object[] paramObj, Mapper<T> map)
            where T : class, new()
        {
            object paramOut = new Guid();
            return query.GetObjects<T>(paramObj, ref paramOut, map);
        }

        /// <summary>
        /// GetObjects<T>: This object just support one param out for dict procedure with get List<T>
        /// Example
        ///     TestProcParamout ?, ?, ? OUTPUT
        ///     object paramOut = null;
        ///     List<TestProcParamout> resultList = query.GetObjects<TestProcParamout>(new object[] { 1, 100, 0 }, ref paramOut, new TestProcParamoutMapper());
        /// Note: Just for programmer
        ///     For param out in dict procedure using with ExecuteReader 
        ///     so DbDataReader must be close before get parameter out.
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <param name="paramOut">ref object</param>
        /// <param name="map">Mapper<T></param>
        /// <returns>List<T></returns>
        public static List<T> GetObjects<T>(this string query, object[] paramObj, ref object paramOut, Mapper<T> map)
            where T : class, new()
        {
            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = conn.CreateComm4nd(query, paramObj))
                {
                    bool isOutput = !(paramOut is Guid);
                    if (isOutput) command.Parameters[command.Parameters.Count - 1].Direction = ParameterDirection.Output;

                    List<T> resultList = map.MapAll(command.ExecuteReader(CommandBehavior.CloseConnection));                    
                    if (isOutput) paramOut = command.Parameters[command.Parameters.Count - 1].Value;

                    return resultList;
                }
            }
        }

        /// <summary>
        /// public static T GetObject<T>(this string query, object[] paramObj) where T : class, new()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="paramObj"></param>
        /// <returns></returns>
        public static T GetObject<T>(this string query, object[] paramObj)
            where T : class, new()
        {
            return query.GetObject<T>(paramObj, new Mapper<T>());
        }

        /// <summary>
        /// public static T GetObject<T>(this string query, object[] paramObj, Mapper<T> map) where T : class, new()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="paramObj"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static T GetObject<T>(this string query, object[] paramObj, Mapper<T> map)
            where T : class, new()
        {
            object paramOut = new Guid();
            return query.GetObject<T>(paramObj, ref paramOut, map);
        }

        /// <summary>
        /// GetObject<T>
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>T</returns>
        public static T GetObject<T>(this string query, object[] paramObj, ref object paramOut)
            where T : class, new()
        {
            return query.GetObject<T>(paramObj, ref paramOut, new Mapper<T>());
        }

        /// <summary>
        /// GetObject<T>
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <param name="map">Mapper<T></param>
        /// <returns>T</returns>
        public static T GetObject<T>(this string query, object[] paramObj, ref object paramOut, Mapper<T> map)
            where T : class, new()
        {
            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = conn.CreateComm4nd(query, paramObj))
                {
                    bool isOutput = !(paramOut is Guid);
                    if (isOutput) command.Parameters[command.Parameters.Count - 1].Direction = ParameterDirection.Output;

                    T one = map.MapFirst(command.ExecuteReader(CommandBehavior.CloseConnection));                    
                    if (isOutput) paramOut = command.Parameters[command.Parameters.Count - 1].Value;

                    return one;
                }
            }
        }

        /// <summary>
        /// Execute
        /// Example:
        /// -------------------------------------------------
        ///     string query = "update A set Alias=? where ID=?";
        ///     query.Execute(new object[] { "xxx", 42 });
        /// </summary>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>int</returns>
        public static int Execute(this string query, object[] paramObj)
        {
            object paramOut = new Guid();
            return query.Execute(paramObj, ref paramOut);
        }

        /// <summary>
        /// Execute (_tokens procedure with parameter out)
        /// Example:
        ///     object paramOut = 0;    
        ///     string query = "GetUserId ?, ?, ? OUTPUT";                
        ///     query.Execute(new object[] { uName, pwd.ToMD5Sha1(), 0 }, paramOut);
        /// </summary>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <param name="paramOut">object</param>
        public static int Execute(this string query, object[] paramObj, ref object paramOut)
        {
            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = conn.CreateComm4nd(query, paramObj))
                {
                    bool isOutput = !(paramOut is Guid);
                    if (isOutput) command.Parameters[command.Parameters.Count - 1].Direction = ParameterDirection.Output;

                    int idx = command.ExecuteNonQuery();
                    if (isOutput) paramOut = command.Parameters[command.Parameters.Count - 1].Value;

                    return idx;
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public static void Dispose()
        {            
        }
    }
}