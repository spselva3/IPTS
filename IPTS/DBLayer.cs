using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFactory.Utilities;
using DragonFactory;
using System.Configuration;
using System.Globalization;

namespace IPTS
{
    public class DBLayer
    {
        #region IPTS_USERMASTER

        public static DataTable IPTS_USERMASTER_GETDETAILS(object USER_LOGIN_NAME, object STATUS)
        {
            try
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(SQLHelper.fn_CreateParameter("@USER_LOGIN_NAME", SqlDbType.VarChar, USER_LOGIN_NAME));
                Parameters.Add(SQLHelper.fn_CreateParameter("@STATUS", SqlDbType.VarChar, STATUS));
                return SQLHelper.Execute_Stored_Procedure("IPTS_USERMASTER_GETDETAILS", Parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_HARDWARECONFIGURATION_GETDETAILS

        public static DataTable IPTS_HARDWARECONFIGURATION_GETDETAILS()
        {
            try
            {
                return SQLHelper.Execute_Stored_Procedure("IPTS_HARDWARECONFIGURATION_GETDETAILS");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_MINE_MASTER_GETDETAILS

        public static DataTable IPTS_MINE_MASTER_GETDETAILS(object MINE_ID, object MINE_NAME, object STATUS)
        {
            try
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(SQLHelper.fn_CreateParameter("@MINE_ID", SqlDbType.VarChar, MINE_ID));
                Parameters.Add(SQLHelper.fn_CreateParameter("@MINE_NAME", SqlDbType.VarChar, MINE_NAME));
                Parameters.Add(SQLHelper.fn_CreateParameter("@STATUS", SqlDbType.VarChar, STATUS));
                return SQLHelper.Execute_Stored_Procedure("IPTS_MINE_MASTER_GETDETAILS", Parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_TRUCKGROSSWEIGHTDETAILS_PRINT

        public static DataTable IPTS_TRUCKGROSSWEIGHTDETAILS_PRINT(object FROMDATE, object TODATE, object MINE)
        {
            try
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(SQLHelper.fn_CreateParameter("@MINE_ID", SqlDbType.VarChar, FROMDATE));
                Parameters.Add(SQLHelper.fn_CreateParameter("@MINE_NAME", SqlDbType.VarChar, TODATE));
                Parameters.Add(SQLHelper.fn_CreateParameter("@STATUS", SqlDbType.VarChar, MINE));
                return SQLHelper.Execute_Stored_Procedure("IPTS_TRUCKGROSSWEIGHTDETAILS_PRINT", Parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS

        public static DataTable IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS(object TruckID, object RFIDTagID, object TruckNumber, object STATUS)
        {
            try
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(SQLHelper.fn_CreateParameter("@TruckID", SqlDbType.VarChar, TruckID));
                Parameters.Add(SQLHelper.fn_CreateParameter("@RFIDTagID", SqlDbType.VarChar, RFIDTagID));
                Parameters.Add(SQLHelper.fn_CreateParameter("@TruckNumber", SqlDbType.VarChar, TruckNumber));
                Parameters.Add(SQLHelper.fn_CreateParameter("@STATUS", SqlDbType.VarChar, STATUS));
                return SQLHelper.Execute_Stored_Procedure("IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS", Parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS

        public static DataTable IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS(object TripID, object TruckID, object TruckNumber, object Shift
            , object MineNo, object STATUS)
        {
            try
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(SQLHelper.fn_CreateParameter("@TripID", SqlDbType.VarChar, TripID));
                Parameters.Add(SQLHelper.fn_CreateParameter("@TruckID", SqlDbType.VarChar, TruckID));
                Parameters.Add(SQLHelper.fn_CreateParameter("@TruckNumber", SqlDbType.VarChar, TruckNumber));
                Parameters.Add(SQLHelper.fn_CreateParameter("@Shift", SqlDbType.VarChar, Shift));
                Parameters.Add(SQLHelper.fn_CreateParameter("@MineNo", SqlDbType.VarChar, MineNo));
                Parameters.Add(SQLHelper.fn_CreateParameter("@STATUS", SqlDbType.VarChar, STATUS));
                return SQLHelper.Execute_Stored_Procedure("IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS", Parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS

        public static DataTable IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(object TruckID, object RFIDTagID, object TruckNumber, object STATUS)
        {
            try
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(SQLHelper.fn_CreateParameter("@TruckID", SqlDbType.VarChar, TruckID));
                Parameters.Add(SQLHelper.fn_CreateParameter("@RFIDTagID", SqlDbType.VarChar, RFIDTagID));
                Parameters.Add(SQLHelper.fn_CreateParameter("@TruckNumber", SqlDbType.VarChar, TruckNumber));
                Parameters.Add(SQLHelper.fn_CreateParameter("@STATUS", SqlDbType.VarChar, STATUS));
                return SQLHelper.Execute_Stored_Procedure("IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS", Parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT

        public static void IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT(object TripID, object TruckID, object TransporterCode, object Transporter, object WeighBridgeNumber
            , object MaterialMovementCode, object TareWeight, object GrossWeight, object NetWeight, object UpdatedBy, object TruckNumber, object Shift, object MineNo)
        {
            try
            {
                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT", CommandType.StoredProcedure, new List<SqlParameter>()
            {
                new SqlParameter("@TripID",TripID),
                new SqlParameter("@TruckID",TruckID),
                new SqlParameter("@TransporterCode",TransporterCode),
                new SqlParameter("@Transporter",Transporter),
                new SqlParameter("@WeighBridgeNumber",WeighBridgeNumber),
                new SqlParameter("@MaterialMovementCode",MaterialMovementCode),
                new SqlParameter("@TareWeight",TareWeight),
                new SqlParameter("@GrossWeight",GrossWeight),
                new SqlParameter("@NetWeight",NetWeight),
                new SqlParameter("@UpdatedBy",UpdatedBy),
                new SqlParameter("@TruckNumber",TruckNumber),
                new SqlParameter("@Shift",Shift),
                new SqlParameter("@MineNo",MineNo),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static void IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT_MANUAL(object TripID, object TruckID, object TransporterCode, object Transporter, object WeighBridgeNumber
           , object MaterialMovementCode, object TareWeight, object GrossWeight, object NetWeight, object UpdatedBy, object TruckNumber, object Shift, object MineNo, object TimeStamp)
        {
            try
            {
                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT_MANUAL", CommandType.StoredProcedure, new List<SqlParameter>()
            {
                new SqlParameter("@TripID",TripID),
                new SqlParameter("@TruckID",TruckID),
                new SqlParameter("@TransporterCode",TransporterCode),
                new SqlParameter("@Transporter",Transporter),
                new SqlParameter("@WeighBridgeNumber",WeighBridgeNumber),
                new SqlParameter("@MaterialMovementCode",MaterialMovementCode),
                new SqlParameter("@TareWeight",TareWeight),
                new SqlParameter("@GrossWeight",GrossWeight),
                new SqlParameter("@NetWeight",NetWeight),
                new SqlParameter("@UpdatedBy",UpdatedBy),
                new SqlParameter("@TruckNumber",TruckNumber),
                new SqlParameter("@Shift",Shift),
                new SqlParameter("@MineNo",MineNo),
                new SqlParameter("@TimeStamp",TimeStamp),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region IPTS_TRUCKTAREWEIGHTDETAILS_INSERT

        public static void IPTS_TRUCKTAREWEIGHTDETAILS_INSERT(object RFIDTagID, object TruckID, object Transporter, object TransporterCode,
    object TareWeight, object PreviousTareWeight, object UpdatedBy, object TruckNumber, object Remarks)
        {
            try
            {

                if (PreviousTareWeight.ToString() == "")
                {
                    PreviousTareWeight = DBNull.Value;
                }

                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("IPTS_TRUCKTAREWEIGHTDETAILS_INSERT", CommandType.StoredProcedure, new List<SqlParameter>()
            {
                new SqlParameter("@RFIDTagID",RFIDTagID),
                new SqlParameter("@TruckID",TruckID),
                new SqlParameter("@Transporter",Transporter),
                new SqlParameter("@TransporterCode",TransporterCode),
                new SqlParameter("@TareWeight",TareWeight),
                new SqlParameter("@PreviousTareWeight",PreviousTareWeight),
                new SqlParameter("@UpdatedBy",UpdatedBy),
                new SqlParameter("@TruckNumber",TruckNumber),
                new SqlParameter("@Remarks",Remarks),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATEFLAG

        public static void IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATEFLAG(object ID)
        {
            try
            {

                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATEFLAG", CommandType.StoredProcedure, new List<SqlParameter>()
            {
                new SqlParameter("@ID",ID),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATE_PRINT(object TripID)
        {
            try
            {

                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATE_PRINT", CommandType.StoredProcedure, new List<SqlParameter>()
            {
                new SqlParameter("@TripID",TripID),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_TRUCKREGISTRATIONMASTER_INSERT

        public static void IPTS_TRUCKREGISTRATIONMASTER_INSERT(object TruckID, object RFIDTagID, object TransporterCode, object Transporter,
            object IsActive, object UpdatedBy, object Make, object Model, object TruckNumber)
        {
            try
            {
                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("IPTS_TRUCKREGISTRATIONMASTER_INSERT", CommandType.StoredProcedure, new List<SqlParameter>()
            {
                new SqlParameter("@TruckID",TruckID),
                new SqlParameter("@RFIDTagID",RFIDTagID),
                new SqlParameter("@TransporterCode",TransporterCode),
                new SqlParameter("@Transporter",Transporter),
                new SqlParameter("@IsActive",IsActive),
                new SqlParameter("@UpdatedBy",UpdatedBy),
                new SqlParameter("@Make",Make),
                new SqlParameter("@Model",Model),
                new SqlParameter("@TruckNumber",TruckNumber),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region COMMON


        public static void spINSERTREADERLOG(object READER_IP, object READER_NAME, object WB_NO, object TAG_VALUE)
        {
            try
            {
                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("spINSERTREADERLOG", CommandType.StoredProcedure, new List<SqlParameter>()
            {
                new SqlParameter("@READER_IP",READER_IP),
                new SqlParameter("@READER_NAME",READER_NAME),
                new SqlParameter("@WB_NO",WB_NO),
                new SqlParameter("@TAG_VALUE",TAG_VALUE),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_HARDWARECONFIGURATION_UPDATE

        public static void IPTS_HARDWARECONFIGURATION_UPDATE(object Location, object PLCIPAddress, object PLCSlot, object PLCRack, object SensorPort1, object SensorPort2, object TrafficLightPortRed
            , object TrafficLightPortGreen, object RFIDReaderIP, object RFIDReaderPort, object UpdatedBy, object WeighBridgeIP,
            object WeighBridgePort, object HooterPort, object AutoSaveEnable, object AutoSaveInterval, object TruckGrossWeightThreshold
            , object TruckTareWeightFrequency, object TruckMinWt, object TruckMaxWt)
        {
            try
            {
                MSSQLHelper.SqlCon = ConfigurationManager.ConnectionStrings["ConStrn"].ToString();
                MSSQLHelper.ExecSqlDataSet("IPTS_HARDWARECONFIGURATION_UPDATE", CommandType.StoredProcedure, new List<SqlParameter>()
            {
              new SqlParameter("@Location",Location),
              new SqlParameter("@PLCIPAddress",PLCIPAddress),
              new SqlParameter("@PLCSlot",PLCSlot),
              new SqlParameter("@PLCRack",PLCRack),
              new SqlParameter("@SensorPort1",SensorPort1),
              new SqlParameter("@SensorPort2",SensorPort2),
              new SqlParameter("@TrafficLightPortRed",TrafficLightPortRed),
              new SqlParameter("@TrafficLightPortGreen",TrafficLightPortGreen),
              new SqlParameter("@RFIDReaderIP",RFIDReaderIP),
              new SqlParameter("@RFIDReaderPort",RFIDReaderPort),
              new SqlParameter("@UpdatedBy",UpdatedBy),
              new SqlParameter("@WeighBridgeIP",WeighBridgeIP),
              new SqlParameter("@WeighBridgePort",WeighBridgePort),
              new SqlParameter("@HooterPort",HooterPort),
              new SqlParameter("@AutoSaveEnable",AutoSaveEnable),
              new SqlParameter("@AutoSaveInterval",AutoSaveInterval),
              new SqlParameter("@TruckGrossWeightThreshold",TruckGrossWeightThreshold),
              new SqlParameter("@TruckTareWeightFrequency",TruckTareWeightFrequency),
              new SqlParameter("@TruckMinWt",TruckMinWt),
              new SqlParameter("@TruckMaxWt",TruckMaxWt),
            });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region IPTS_TRUCKGROSSWEIGHTDETAILS_MANUALPRINT

        public static DataTable IPTS_TRUCKGROSSWEIGHTDETAILS_MANUALPRINT(object FROMDATE, object TODATE, object MINE)
        {
            try
            {
                List<SqlParameter> Parameters = new List<SqlParameter>();
                Parameters.Add(SQLHelper.fn_CreateParameter("@FROMDATE", SqlDbType.VarChar, FROMDATE));
                Parameters.Add(SQLHelper.fn_CreateParameter("@TODATE", SqlDbType.VarChar, TODATE));
                Parameters.Add(SQLHelper.fn_CreateParameter("@MINE", SqlDbType.VarChar, MINE));
                return SQLHelper.Execute_Stored_Procedure("IPTS_TRUCKGROSSWEIGHTDETAILS_MANUALPRINT", Parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
