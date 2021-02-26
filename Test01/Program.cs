using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;

namespace Test01
{
    public class Customer
    {
        public string ID { get; set; }
     
        public string CompanyName { get; set; }
        
        public string ContactName { get; set; }
    }

    public class CustomerMapper : Mapper<Customer>
    {
        public override Customer Map(IDataRecord record)
        {
            var cus = new Customer();
            var idx = -1;

            cus.ID = record[++idx].ToString();
            cus.CompanyName = record[++idx].ToString();
            cus.ContactName = record[++idx].ToString();

            return cus;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Using DALHelper just create at first time
            @"Provider=SqlOLEDB;Server=127.0.0.1,1433\SQLEXPRESS;Database=Northwind;Uid=sa;Pwd=<AAAAAAAAAAAAAAAAAAA>;".CreateInstance(180);

            // and using this any where you wanna query
            // Using Auto Mapper (default)
            var customers1 = "SELECT TOP (1000) [CustomerID] as ID,[CompanyName] CompanyName, [ContactName] ContactName FROM [Northwind].[dbo].[Customers]"
                .GetObjects<Customer>(null);

            var customers2 = "SELECT TOP (1000) [CustomerID], [CompanyName], [ContactName] FROM [Northwind].[dbo].[Customers]".GetObjects<Customer>(null, new CustomerMapper());
        }
    }
}
