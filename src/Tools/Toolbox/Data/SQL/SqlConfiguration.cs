//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Data.SqlClient;
//using Toolbox.Tools;

//namespace Toolbox.Data;

///// <summary>
///// SQL configuration (immutable builder pattern)
///// </summary>
//[DebuggerDisplay("ConnectionString={ConnectionString}")]
//public record SqlConfiguration
//{
//    public SqlConfiguration(string connectionString)
//    {
//        connectionString.NotEmpty();
//        ConnectionString = connectionString;
//    }

//    public SqlConfiguration(string server, string databaseName)
//    {
//        server.NotEmpty();
//        databaseName.NotEmpty();

//        var builder = new SqlConnectionStringBuilder()
//        {
//            DataSource = server,
//            InitialCatalog = databaseName,
//            IntegratedSecurity = true
//        };

//        ConnectionString = builder.ToString();
//    }

//    public SqlConfiguration(string server, string databaseName, string userId, string password)
//    {
//        server.NotEmpty();
//        databaseName.NotEmpty();
//        userId.NotEmpty();

//        var builder = new SqlConnectionStringBuilder()
//        {
//            DataSource = server,
//            InitialCatalog = databaseName,
//            UserID = userId,
//            Password = password,
//        };

//        ConnectionString = builder.ToString();
//    }

//    public SqlConfiguration(SqlConnectionStringBuilder builder)
//    {
//        builder.NotNull();
//        ConnectionString = builder.ToString();
//    }

//    /// <summary>
//    /// SQL connection string
//    /// </summary>
//    public string ConnectionString { get; }

//    ///// <summary>
//    ///// Test the DB connection by executing "select 1"
//    ///// </summary>
//    ///// <returns>true if pass, false if not</returns>
//    //public bool TestConnection()
//    //{
//    //    try
//    //    {
//    //        using (var conn = new SqlConnection(ConnectionString))
//    //        using (var cmd = conn.CreateCommand())
//    //        {
//    //            cmd.CommandText = "select 1";
//    //            cmd.CommandType = CommandType.Text;

//    //            conn.Open();
//    //            cmd.ExecuteNonQuery();
//    //            return true;
//    //        }
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        ToolboxEventSource.Log.Error(context, $"Cannot open database - connection string: {ConnectionString}", ex);
//    //        return false;
//    //    }
//    //}
//}
