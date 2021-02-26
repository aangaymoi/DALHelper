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
    /// Auto mapping data into object T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Mapper<T> where T : class, new()
    {
        /// <summary> 
        /// Map
        /// </summary>
        /// <param name="record">IDataRecord</param>
        /// <returns>T</returns>
        public virtual T Map(IDataRecord record)
        {
            T entity = new T();

            /*Old*/
            //foreach (System.Reflection.PropertyInfo item in entity.GetType().GetProperties())
            //{                

            //    for (int i = 0; i < record.FieldCount; i++)
            //    {
            //        bool isValid = record.GetName(i).Equals(item.Name, StringComparison.OrdinalIgnoreCase);
            //        if (isValid)
            //        {
            //            if (record[item.Name] != DBNull.Value)
            //            {
            //                item.SetValue(entity, Convert.ChangeType(record[item.Name], item.PropertyType), null);
            //                break;
            //            }
            //        }
            //    }
            //}

            var properties = entity.GetType().GetProperties();
            for (int i = 0; i < record.FieldCount; i++)
            {
                var name = record.GetName(i);
                var item = properties.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                var isValid = item != null;
                if (isValid)
                {
                    var val = record[i];
                    if (val != DBNull.Value)
                    {
                        item.SetValue(entity, Convert.ChangeType(val, item.PropertyType), null);
                    }
                }
            }
            
            return entity;
        }
        
        /// <summary> 
        /// Get all collection
        /// </summary>
        /// <param name="reader">IDataReader</param>
        /// <returns>List<T></returns>
        public List<T> MapAll(IDataReader reader)
        {
            List<T> collection = new List<T>();
            while (reader.Read())
                collection.Add(Map(reader));
            reader.Close();

            return collection;
        }

        /// <summary> 
        /// Get object T
        /// </summary>
        /// <param name="reader">IDataReader</param>
        /// <returns>T</returns>
        public T MapFirst(IDataReader reader)
        {
            if (reader.Read())
                return Map(reader);

            return null;
        }
    }
}
