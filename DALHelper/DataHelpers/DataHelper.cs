using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;

/*[assembly: Obfuscation(Exclude = false, Feature = "strong name key:Helpers.pfx")]*/
/*[assembly: Obfuscation(Exclude = false, Feature = "strong name key password:f0a3cb75b4e57dcc3835d79ab1886888")]*/
/*+rename(mode=letters,flatten=false);*/
// [assembly: Obfuscation(Exclude = false, Feature = "preset(maximum);+anti ildasm;+anti tamper;-constants;-ctrl flow;+anti debug;+invalid metadata;-ref proxy;-resources;+rename(mode=letters,flatten=false);")]
namespace Helpers
{
    public static class DataHelper
    {
        /// <summary>
        /// public static byte[] ToBytes(this IDataRecord record, int idx)
        ///     remarks: In case larges bytes put GetBytes in while (bytesRead < length)
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>byte[]</returns>
        public static byte[] ToBytes(this IDataRecord record, int idx)
        {
            return record[idx] as byte[];
            //if (record.IsDBNull(idx))
            //    return new byte[0];

            //long length = record.GetBytes(idx, 0, null, 0, 0);
            //byte[] buffer = new byte[length];
            //record.GetBytes(idx, 0, buffer, 0, (int)length);

            //return buffer;
        }

        /// <summary>
        /// public static Guid ToUniqueIdentifier(this IDataRecord record, int idx)
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>Guid</returns>
        public static Guid ToUniqueIdentifier(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return Guid.Empty;

            return record.GetGuid(idx);
        }

        /// <summary>
        /// ToInt
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>int</returns>
        public static int ToInt(this object obj)
        {
            int ret = 0;
            int.TryParse(obj.ToString(), out ret);

            return ret;
        }

        /// <summary>
        /// ToLong
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>long</returns>
        public static long ToLong(this object obj)
        {
            long ret = 0;
            long.TryParse(obj.ToString(), out ret);

            return ret;
        }

        /// <summary>
        /// ToLong
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>long</returns>
        public static long ToLong(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new long();

            return record.GetInt64(idx);
        }

        /// <summary>
        /// ToInt
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>int</returns>
        public static int ToInt(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new int();

            return record.GetInt32(idx);
        }

        /// <summary>
        /// ToSmallInt
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>short</returns>
        public static short ToSmallInt(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new short();

            return record.GetInt16(idx);
        }

        /// <summary>
        /// public static byte ToTinyInt(this IDataRecord record, int idx)
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>byte</returns>
        public static byte ToTinyInt(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new byte();

            return record.GetByte(idx);
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>string</returns>
        public static string ToString(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return String.Empty;

            return record.GetString(idx);
        }

        /// <summary>
        /// public static string ToDate(this IDataRecord record, int idx)
        /// [*] Note:
        ///     SQL DataType    C# DataType
        ///     Date            DateTime
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns></returns>
        public static DateTime ToDate(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new DateTime();

            return DateTime.Parse(record.GetString(idx));
        }

        /// <summary>
        /// ToDateTime
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new DateTime();

            return record.GetDateTime(idx);
        }

        public static DateTime? ToDateTimeObject(this IDataRecord record, int idx)
        {
            return record[idx] as DateTime?;
        }

        /// <summary>
        /// ToBool
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>bool</returns>
        public static bool ToBool(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new bool();

            return record.GetBoolean(idx);
        }

        /// <summary>
        /// public static Decimal ToDecimal(this IDataRecord record, int idx)
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <param name="idx">int</param>
        /// <returns>Decimal</returns>
        public static Decimal ToDecimal(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new Decimal();

            return record.GetDecimal(idx);
        }

        public static float ToFloat(this IDataRecord record, int idx)
        {
            if (record.IsDBNull(idx))
                return new float();

            return (float)record.GetDouble(idx);
        }
    }
}