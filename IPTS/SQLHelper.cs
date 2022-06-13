using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;

namespace IPTS
{
    public static class SQLHelper
    {
        static SQLHelper()
        {

        }
        public static string _connectionstring = ConfigurationManager.ConnectionStrings["ConStrn"].ConnectionString;
        public static int _CommandTimeOut =60;// Convert.ToInt32(ConfigurationManager.AppSettings["CommandTimeOut"].ToString());
        public static SqlCommand CreateCommand()
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = _connectionstring;
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                return command;
            }
            catch (SqlException execption)
            {
                throw new System.ApplicationException(execption.Message);
            }
           
                
        }

        //public static SqlConnection CreateConnection()
        //{
        //    SqlProviderFactory factory = SqlProviderFactories.GetFactory(_providername);
        //    SqlConnection connection = factory.CreateConnection();
        //    connection.ConnectionString = _connectionstring;
        //    return connection;
           
        //}
        public static SqlConnection CreateConnection()
        {

            try
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = _connectionstring;
                return con;
            }
            catch (Exception exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }

        //public static SqlConnection CreateConnection(bool IsTransactionNeeded)
        //{
        //    SqlProviderFactory factory = SqlProviderFactories.GetFactory(_providername);
        //    SqlConnection connection = factory.CreateConnection();
        //    connection.ConnectionString = _connectionstring;
        //    if (IsTransactionNeeded)
        //    {
        //        connection.be
        //    }
        //    return connection;
        //}

        //public static SqlCommand CreateCommand(bool IsTractionNeeded)
        //{

        //    SqlProviderFactory factory = SqlProviderFactories.GetFactory(_providername);
        //    SqlConnection connection = factory.CreateConnection();
        //    connection.ConnectionString = _connectionstring;
        //    SqlCommand command = connection.CreateCommand();
        //    command.CommandType = CommandType.StoredProcedure;
        //    if (connection.State != ConnectionState.Open)
        //        connection.Open();
        //    if (IsTractionNeeded)
        //    {
        //        command.Transaction = connection.BeginTransaction();
        //    }
        //    return command;

        //}


       // Create a Parameter
        public static SqlParameter CreateParameter()
        {
            SqlParameter parameter = new SqlParameter();
            return parameter;
        }

        public static DataTable ExecuteReader(SqlCommand command)
        {
            DataTable table;
            try
            {
                command.CommandTimeout = _CommandTimeOut;
                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                table = new DataTable();
                table.Load(reader);
            }
            catch (SqlException exception)
            {
                throw new ApplicationException(exception.Message);
            }
            catch (Exception exception)
            {
                throw new ApplicationException(exception.Message);
            }
            finally
            {
                command.Connection.Close();
            }
            return table;
        }

        public static DataSet ExecuteReader(SqlCommand command, string[] table_names)
        {
            DataSet ds;
            try
            {
                command.CommandTimeout = _CommandTimeOut;
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                ds = new DataSet();
                ds.Load(reader, LoadOption.OverwriteChanges, table_names);
            }
            catch (Exception ex)
            { throw new ApplicationException(ex.Message);; }
            finally
            { command.Connection.Close(); }
            return ds;
        }

        public static int ExecuteNonQuery(SqlCommand command)
        {
            int AfectedRows = -1;
            try
            {
                command.CommandTimeout = _CommandTimeOut;
                command.Connection.Open();
                AfectedRows = command.ExecuteNonQuery();
            }
            catch (SqlException exception)
            { 
                throw new ApplicationException(exception.Message);
            }
            finally
            { command.Connection.Close(); }
            return AfectedRows;
        }

        public static string ExecuteScalar(SqlCommand command)
        {
            string value = "";
            try
            {
                command.CommandTimeout = _CommandTimeOut;
                command.Connection.Open();
                value = command.ExecuteScalar().ToString();
            }
            catch (SqlException exception)
            { 
                throw new ApplicationException(exception.Message);
            }
            finally
            { command.Connection.Close(); }
            return value;
        }

        public static DataTable Execute_Stored_Procedure(string Procedure_Name)
        {
            DataTable dt;
            try
            {
                using (SqlCommand cmd = SQLHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = Procedure_Name;
                    
                  //  dt = Helper.ExecuteReader(cmd);  // Original...........
                    dt = SQLHelper.ExecuteDataSet(cmd).Tables[0];//Modified..........
                    return dt;
                }
            }
            catch (SqlException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }

        //public static DataTable Execute_Stored_Procedure(string Procedure_Name, List<SqlParameter> parameters)
        //{
        //    DataTable dt;
        //    try
        //    {
        //        using (SqlCommand cmd = Helper.CreateCommand())
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.CommandText = Procedure_Name;
                   
        //            foreach (SqlParameter p in parameters)
        //            {
        //                cmd.Parameters.Add(p);
        //            }

        //            dt = Helper.ExecuteReader(cmd);
        //            return dt;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public static DataTable Execute_Stored_Procedure(string Procedure_Name, List<SqlParameter> parameters)
        {
            DataTable dt;
            try
            {
                using (SqlCommand cmd = SQLHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = Procedure_Name;

                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }

                   // dt = SQLHelper.ExecuteDataSet(cmd).Tables[0]; //modified form executeDataReader to ExecuteDataSet..........
                    dt = SQLHelper.ExecuteReader(cmd);
                    return dt;
                }
            }
            catch (SqlException exception)
            {
                throw new ApplicationException("Please Check Network Connection"+exception.Message);
            }
        }
      
        public static SqlParameter fn_CreateParameter(string ParameterName, SqlDbType type, object vale)
        {
            try
            {
                SqlParameter par = new SqlParameter();
                par.ParameterName = ParameterName;
                par.SqlDbType = type;
                par.Value = vale;

                return par;
            }
            catch (SqlException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }

        public static DataSet ExecuteDataSet(SqlCommand cmd)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                adpt.Fill(ds, "table");
                return ds;
            }
            catch (SqlException exception)
            {
                throw new ApplicationException(exception.Message);
            }

        }
    
        public static int Execute_Stored_Procedure_NonQuery(string Procedure_Name, List<SqlParameter> parameters)
        {
            try
            {
                using (SqlCommand cmd = SQLHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = Procedure_Name;

                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }

                    return SQLHelper.ExecuteNonQuery(cmd); //modified form executeDataReader to ExecuteDataSet..........
                }
            }
            catch (SqlException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }

        public static int Execute_Stored_Procedure_NonQuery(string Procedure_Name)
        {
            try
            {
                using (SqlCommand cmd = SQLHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = Procedure_Name;
                    return SQLHelper.ExecuteNonQuery(cmd); //modified form executeDataReader to ExecuteDataSet..........
                }
            }
            catch (SqlException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }
        public static DataTable GetTables()
        {
            DataTable schema = new DataTable("Schema");
            using (SqlConnection connection = new SqlConnection(_connectionstring))
            {
                connection.Open();
                schema = connection.GetSchema("Tables");
                connection.Close();
            }
            return schema;
        }

        public static DataTable GetAuditTables()
        {
            DataTable dtAuditTables = new DataTable("Audit Tables");
            SqlCommand cmd = CreateCommand();
            cmd.CommandText = "Select name FROM sys.Tables where name like 'AUDIT%'";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtAuditTables);
            return dtAuditTables;
        }
    }
 
}
