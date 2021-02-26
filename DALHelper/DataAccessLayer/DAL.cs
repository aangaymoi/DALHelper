using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Reflection;

// [assembly: Obfuscation(Exclude = false, Feature = "preset(maximum);+anti ildasm;+anti tamper;-constants;-ctrl flow;-anti debug;+invalid metadata;-ref proxy;-resources;-rename(mode=letters,flatten=false);")]
namespace DataAccessLayer
{
    /// <summary> 
    /// Data access layer
    ///     One output parameter is supported, and the output parameter must be the last parameter, and everything be auto    
    /// </summary>
    public class DB : IDisposable
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
        
        private DbProviderFactory _provider = null;
        private String _connectionString = String.Empty;     // backup connection string
        private int _commandTimeout = 30;

        /// <summary> 
        /// createDataAdapter
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>DbDataAdapter</returns>
        private DbDataAdapter createDataAdapter(DbCommand command)
        {
            DbDataAdapter adapter = _provider.CreateDataAdapter();
            adapter.SelectCommand = command;

            return adapter;
        }

        /// <summary>
        /// addParams
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>void</returns>
        private void addParams(DbCommand command, object[] paramObj)
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
        /// createComm4nd
        /// </summary>
        /// <param name="conn">DbConnection</param>
        /// <param name="query">string</param>
        /// <returns>DbCommand</returns>
        private DbCommand createComm4nd(DbConnection conn, string query, object[] paramObj)
        {
            DbCommand command = _provider.CreateCommand();
            command.Connection = conn;

            command.CommandTimeout = _commandTimeout;
            command.Connection.ConnectionString = _connectionString;

            command.CommandText = query;

            this.addParams(command, paramObj);

            command.Connection.Open();
            return command;
        }

        /// <summary> 
        /// Using provider to CreateInstance
        /// </summary>
        /// <param name="providerName">string</param>
        /// <param name="connectionString">string</param>                
        /// <returns>void</returns>
        public DB(string providerName, string connectionString, int commandTimeout)
        {
            _commandTimeout = commandTimeout;
            _connectionString = connectionString;

            // create provider
            _provider = DbProviderFactories.GetFactory(providerName);
        }

        /// <summary>
        /// public static void CreateInstance(this string connectionString, int commandTimeout)
        /// </summary>
        /// <param name="connectionString">string</param>
        /// <param name="commandTimeout">int</param>
        /// <returns></returns>
        public DB(string connectionString, int commandTimeout) : this("System.Data.OleDb", connectionString, commandTimeout) {}

        /// <summary>
        /// public static void CreateInstance(this string connectionString)
        /// </summary>
        /// <param name="connectionString">string</param>
        /// <returns></returns>
        public DB(string connectionString) : this(connectionString, 120) { }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>object</returns>
        public object GetValue(string query, object[] paramObj)
        {
            object paramOut = new Guid();
            return GetValue(query, paramObj, ref paramOut);
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
        public object GetValue(string query, object[] paramObj, ref object paramOut)
        {
            object value = null;

            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = this.createComm4nd(conn, query, paramObj))
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
        public T GetData<T>(string query, object[] paramObj) where T : class, new()
        {
            object paramOut = new Guid();
            return this.GetData<T>(query, paramObj, ref paramOut);
        }

        /// <summary>
        /// GetData<DataSet/DataTable>
        /// </summary>
        /// <typeparam name="T">DataSet or DataTable</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object</param>
        /// <returns>DataSet or DataTable</returns>
        public T GetData<T>(string query, object[] paramObj, ref object paramOut) where T : class, new()
        {
            T dat = new T();

            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = this.createComm4nd(conn, query, paramObj))
                {
                    bool isOutput = !(paramOut is Guid);
                    if (isOutput) command.Parameters[command.Parameters.Count - 1].Direction = ParameterDirection.Output;

                    using (DbDataAdapter adapter = this.createDataAdapter(command))
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
        public List<T> GetObjects<T>(string query, object[] paramObj)
            where T : class, new()
        {
            return this.GetObjects<T>(query, paramObj, new Mapper<T>());
        }        

        /// <summary>
        /// public static List<T> GetObjects<T>(this string query, object[] paramObj, ref object paramOut) where T : class, new()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="paramObj"></param>
        /// <param name="paramOut"></param>
        /// <returns></returns>
        public List<T> GetObjects<T>(string query, object[] paramObj, ref object paramOut)
            where T : class, new()
        {
            return this.GetObjects<T>(query, paramObj, ref paramOut, new Mapper<T>());
        }

        /// <summary>
        /// GetObjects<T>
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <param name="map">Mapper<T></param>
        /// <returns>List<T></returns>
        public List<T> GetObjects<T>(string query, object[] paramObj, Mapper<T> map)
            where T : class, new()
        {
            object paramOut = new Guid();
            return this.GetObjects<T>(query, paramObj, ref paramOut, map);
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
        public List<T> GetObjects<T>(string query, object[] paramObj, ref object paramOut, Mapper<T> map)
            where T : class, new()
        {
            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = this.createComm4nd(conn, query, paramObj))
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
        public T GetObject<T>(string query, object[] paramObj)
            where T : class, new()
        {
            return this.GetObject<T>(query, paramObj, new Mapper<T>());
        }

        /// <summary>
        /// public static T GetObject<T>(this string query, object[] paramObj, Mapper<T> map) where T : class, new()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="paramObj"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public T GetObject<T>(string query, object[] paramObj, Mapper<T> map)
            where T : class, new()
        {
            object paramOut = new Guid();
            return this.GetObject<T>(query, paramObj, ref paramOut, map);
        }

        /// <summary>
        /// GetObject<T>
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <returns>T</returns>
        public T GetObject<T>(string query, object[] paramObj, ref object paramOut)
            where T : class, new()
        {
            return this.GetObject<T>(query, paramObj, ref paramOut, new Mapper<T>());
        }

        /// <summary>
        /// GetObject<T>
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="query">string</param>
        /// <param name="paramObj">object[]</param>
        /// <param name="map">Mapper<T></param>
        /// <returns>T</returns>
        public T GetObject<T>(string query, object[] paramObj, ref object paramOut, Mapper<T> map)
            where T : class, new()
        {
            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = this.createComm4nd(conn, query, paramObj))
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
        public int Execute(string query, object[] paramObj)
        {
            object paramOut = new Guid();
            return this.Execute(query, paramObj, ref paramOut);
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
        public int Execute(string query, object[] paramObj, ref object paramOut)
        {
            using (DbConnection conn = _provider.CreateConnection())
            {
                using (DbCommand command = this.createComm4nd(conn, query, paramObj))
                {
                    bool isOutput = !(paramOut is Guid);
                    if (isOutput) command.Parameters[command.Parameters.Count - 1].Direction = ParameterDirection.Output;

                    int idx = command.ExecuteNonQuery();
                    if (isOutput) paramOut = command.Parameters[command.Parameters.Count - 1].Value;

                    return idx;
                }
            }
        }

        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DB()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}