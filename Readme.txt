

               <DALHelper> - What's new in the latest version
			   
     We are no more support this version, and we recommend using Install-Package PH.DALHelper
     We have already supported .NET Core.

     Version <2.0>
     This is handle query, excute from MSSQL, Oracle, Access just simple in one Library.
     The result can automatic map into your Model or your custom Mapper

     How to install using nuget package
     Install-Package DALHelper -Version 2.0.0
	 
     1. Using with Instance:
        new DB("Provider=sqlncli11;Server=sssssssssssssssss,NNNN;Database=AAAAAAAAAAAA;Uid=zzzzzzzzzz;Pwd=qqqqqqqqqqqqqqqqqqqqqqq;Connect Timeout=180;", 180);
   
     2. Using via DALHelper
        This is simple, quickly and easy to use
        
        var msqlConnection = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ToString();
        msqlConnection.CreateInstance(180);
        
        // Execute Store Procedure with param is ID and list of Categories
        var res = "sp_GETcategories ?".GetObjects(new object[] { id }, new CategoryMapper());
        
        // Excute query 		
        "sp_DELETECategory ?".Execute(new object[] { id });
        
        // Excute SQL query
        var res = "SELECT ID, NAME from Customers where ID=?".GetObjects(new object[] { id }, new CustomerMapper());
 
        // StoreProc with output, for paging, etc
        object total = null;
        string query = "sp_GETaaa ?, ?, ?, ?, ?, ? OUTPUT";
        var res = query.GetObjects<AAA>(new object[] { id, q, offset, pageSize, orderBy, 0 }, ref total, new AAAMapper());

     3. How to create CustomerMapper
        public class CustomerMapper : Mapper<Customer>
        {
            public override Customer Map(IDataRecord record)
            {
                var cus = new Customer();
                var idx = -1;
                
                cus.ID = record[++idx].ToString();
                cus.Name = record[++idx].ToString();        
                
                return cus;
            }
        }
	 

		
		

 
