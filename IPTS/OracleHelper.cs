using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Configuration;


namespace IPTS
{
    public static class OracleHelper
    {
        static OracleHelper()
        {

        }
        public static string _connectionstring = ConfigurationManager.AppSettings["OracleConnectionString"].ToString();//      "Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.20.203)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = XE)));User ID=KCPERP; Password=KCPERP;";// String.Empty;
        public static int _CommandTimeOut =60;
       
        public static OracleCommand CreateCommand()
        {
            try
            {
                OracleConnection connection = new OracleConnection();
                connection.ConnectionString = _connectionstring;
                OracleCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                return command;
            }
            catch (OracleException execption)
            {
                throw new System.ApplicationException(execption.Message);
            }

            catch (Exception exception)
            {
                throw new System.ApplicationException("Command Exception");
            }
        }

       
        public static OracleConnection CreateConnection()
        {

            try
            {
                OracleConnection con = new OracleConnection();
                con.ConnectionString = _connectionstring;
                return con;
            }
            catch (Exception exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }

        public static OracleParameter CreateParameter()
        {
            OracleParameter parameter = new OracleParameter();
            return parameter;
        }

        public static DataTable ExecuteReader(OracleCommand command)
        {
            DataTable table;
            try
            {
                command.CommandTimeout = _CommandTimeOut;
                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();
                OracleDataReader reader = command.ExecuteReader();
                table = new DataTable("MyTable");
                table.Load(reader);
            }
            catch (OracleException exception)
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

        public static int ExecuteNonQuery(string QueryString)
        {
            int AfectedRows = -1;

            using (OracleCommand command = OracleHelper.CreateCommand())
            {
                try
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = QueryString;
                    command.CommandTimeout = _CommandTimeOut;
                    command.Connection.Open();
                    using (var trans = command.Connection.BeginTransaction())
                    {
                        command.Transaction = trans;
                        AfectedRows = command.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                catch (OracleException exception)
                {
                    throw new ApplicationException(exception.Message);
                }
                finally
                {
                    command.Connection.Close();
                }
            }

            return AfectedRows;

        }
       

        public static DataSet ExecuteReader(OracleCommand command, string[] table_names)
        {
            DataSet ds;
            try
            {
                command.CommandTimeout = _CommandTimeOut;
                command.Connection.Open();
                OracleDataReader reader = command.ExecuteReader();
                ds = new DataSet();
                ds.Load(reader, LoadOption.OverwriteChanges, table_names);
            }
            catch (Exception ex)
            { throw new ApplicationException(ex.Message);; }
            finally
            { command.Connection.Close(); }
            return ds;
        }

        public static int ExecuteNonQuery(OracleCommand command)
        {
            int AfectedRows = -1;
            try
            {

                command.CommandTimeout = _CommandTimeOut;
                command.Connection.Open();
                using (var trans = command.Connection.BeginTransaction())
                {
                    command.Transaction = trans;
                    AfectedRows = command.ExecuteNonQuery();
                    trans.Commit();
                }

            }
            catch (OracleException exception)
            {
                throw new ApplicationException(exception.Message);
            }
            finally
            { command.Connection.Close(); }
            return AfectedRows;
        }

        public static string ExecuteScalar(OracleCommand command)
        {
            string value = "";
            try
            {
                command.CommandTimeout = _CommandTimeOut;
                command.Connection.Open();
                value = command.ExecuteScalar().ToString();
            }
            catch (OracleException exception)
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
                using (OracleCommand cmd = OracleHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = Procedure_Name;
                    
                  //  dt = Helper.ExecuteReader(cmd);  // Original...........
                    dt = OracleHelper.ExecuteDataSet(cmd).Tables[0];//Modified..........
                    return dt;
                }
            }
            catch (OracleException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }


        public static DataTable ExecuteQuery(string QueryString)
        {
            DataTable dt;
            try
            {
                using (OracleCommand cmd = OracleHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = QueryString;

                    //  dt = Helper.ExecuteReader(cmd);  // Original...........
                    dt = OracleHelper.ExecuteDataSet(cmd).Tables[0];//Modified..........
                    return dt;
                }
            }
            catch (OracleException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }
     
        public static DataTable Execute_Stored_Procedure(string Procedure_Name, List<OracleParameter> parameters)
        {
            DataTable dt;
            try
            {
                using (OracleCommand cmd = OracleHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = Procedure_Name;

                    foreach (OracleParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }

                   // dt = OracleHelper.ExecuteDataSet(cmd).Tables[0]; //modified form executeDataReader to ExecuteDataSet..........
                    dt = OracleHelper.ExecuteReader(cmd);
                    return dt;
                }
            }
            catch (OracleException exception)
            {
                throw new ApplicationException("Please Check Network Connection"+exception.Message);
            }
        }
      
        public static OracleParameter fn_CreateParameter(string ParameterName, OracleDbType type, object vale)
        {
            try
            {
                OracleParameter par = new OracleParameter();
                par.ParameterName = ParameterName;
              //  par.OracleType = type;
                par.OracleDbType = type;
               // par.DbType = type;
                par.Value = vale;
                return par;
            }
            catch (OracleException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }

        public static DataSet ExecuteDataSet(OracleCommand cmd)
        {
            try
            {
                DataSet ds = new DataSet();
                OracleDataAdapter adpt = new OracleDataAdapter(cmd);
                adpt.Fill(ds, "table");
                return ds;
            }
            catch (OracleException exception)
            {
                throw new ApplicationException(exception.Message);
            }

        }
    
        public static int Execute_Stored_Procedure_NonQuery(string Procedure_Name, List<OracleParameter> parameters)
        {
            try
            {
                using (OracleCommand cmd = OracleHelper.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = Procedure_Name;

                    foreach (OracleParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }

                    return OracleHelper.ExecuteNonQuery(cmd); //modified form executeDataReader to ExecuteDataSet..........
                }
            }
            catch (OracleException exception)
            {
                throw new ApplicationException(exception.Message);
            }
        }
    
    }
 
}
