using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Reflection;

// [assembly: Obfuscation(Exclude = false, Feature = "preset(maximum);+anti ildasm;+anti tamper;-constants;-ctrl flow;+anti debug;+invalid metadata;-ref proxy;-resources;+rename(mode=letters,flatten=false);")]
namespace DataAccessLayer
{
    /// <summary> 
    /// Data access layer
    ///     One output parameter is supported, and the output parameter must be the last parameter, and everything be auto    
    /// </summary>
    public static class DALHelper
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

        private static DB _db;

        #region -- Create Instance --

        /// <summary> 
        /// Using provider to CreateInstance
        /// </summary>
        /// <param name="providerName">string</param>
        /// <param name="connectionString">string</param>                
        /// <returns>void</returns>
        public static void CreateInstance(this string providerName, string connectionString, int commandTimeout)
        {
            _db = new DB(providerName, connectionString, commandTimeout);
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

        #endregion

        #region -- GetValue --

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
            return _db.GetValue(query, paramObj, ref paramOut);
        }

        #endregion

        #region -- GetData --

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
            return _db.GetData<T>(query, paramObj, ref paramOut);
        }

        #endregion

        #region -- GetObjects --

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
            return _db.GetObjects<T>(query, paramObj, ref paramOut, map);
        }

        #endregion

        #region -- GetObject --

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
            return _db.GetObject<T>(query, paramObj, ref paramOut, map);
        }

        #endregion

        #region -- Execute --

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
            return _db.Execute(query, paramObj, ref paramOut);
        }

        #endregion
    }
}