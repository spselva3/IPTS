using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using LMS;
using System.Configuration;
namespace IPTS
{
    public static class OracleBLLayer
    {
        public static String UpdateNewTareWeight(String TransporterCode, String TruckID, String TruckNumber, String TareWeight, String UserName, String DiversionCode)
        {
            String Query = @"Insert Into mines_vehicles(TRPT_CODE,VEH_SHORT_ID,VEHICLE_NO,FROM_DATE,TARE_WEIGHT,CREATED_BY,CREATED_DATE,DIVISION_CODE) 
                              values({0},{1},'{2}',TO_CHAR(SYSDATE, 'DD-MON-YY'),{3},'{4}',TO_CHAR(SYSDATE, 'DD-MON-YY'),{5})";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Query, TransporterCode, TruckID, TruckNumber, TareWeight, UserName, DiversionCode);
            OracleHelper.ExecuteNonQuery(sb.ToString());
            return sb.ToString();
        }

        public static String UpdateLastUpdatedWeight(String TruckNumber, String UserName)
        {
            String Query = "Update mines_vehicles set TO_DATE=TO_CHAR(SYSDATE-1, 'DD-MON-YY') , LAST_UPDATED_BY='{0}',LAST_UPDATED_DATE=TO_CHAR(SYSDATE, 'DD-MON-YY')  where VEHICLE_NO='{1}' AND TO_DATE Is NULL";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Query, UserName, TruckNumber);
            OracleHelper.ExecuteNonQuery(sb.ToString());
            return sb.ToString();
        }

        public static String GetString(int MaterialCode, int SupplierCode, int TransporterCode, String VehicleNo, String GrossQty, String TareWt,
             String Act_wt, String Deduction_percent, String Deduction_Qty, String NetWt, String User, String Shift, int DiversionCode, int WBNumber)
        {
            String Query = @"Insert Into mines_WEIGHMENTS( RSLIP_NO,DSLIP_NO ,SLIP_DATE ,RM_CODE ,SUPP_CODE ,TRPT_CODE ,VEH_NO  , GROSS_QTY,TARE_QTY ,ACT_QTY  , DED_PER , DED_QTY, 
NET_QTY,WB_IN_DT,WB_IN_TM,WB_OUT_DT,WB_OUT_TM,WB_OPTR ,WB_SHIFT  , DIVISION_CODE,WB_NO)  select (select NVL(max(TO_NUMBER(RSLIP_NO)),0) + 1 from mines_WEIGHMENTS),{0} ,TO_CHAR(SYSDATE, 'DD-MON-YY'),{1},{2},{3},'{4}',{5},{6},{7},{8},{9},{10},TO_CHAR(SYSDATE, 'DD-MON-YY'),TO_CHAR(SYSTIMESTAMP, 'HH24:MI:SS') ,TO_CHAR(SYSDATE, 'DD-MON-YY'),TO_CHAR(SYSTIMESTAMP, 'HH24:MI:SS') ,'{11}','{12}',{13},{14} from dual";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Query, GetTodaysMax(VehicleNo), MaterialCode, SupplierCode, TransporterCode, VehicleNo, GrossQty, TareWt, Act_wt, Deduction_percent, Deduction_Qty,
                NetWt, User, Shift, DiversionCode, WBNumber);
            return sb.ToString();
        }

        public static int GetTodaysMax(String VehicleNo)
        {
            String query = "select NVL(max(TO_NUMBER(DSLIP_NO)),0) + 1 from mines_WEIGHMENTS  where SLIP_DATE = TO_CHAR(SYSDATE, 'DD-MON-YY')";
            int Serial = 1;
            DataTable dt = OracleHelper.ExecuteQuery(query);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    Serial = Convert.ToInt32(dt.Rows[0][0]);
                }
                else
                {
                    Serial = 1;
                }
            }
            return Serial;
        }

        public static String InsertNewGrossWeight(int MaterialCode, int SupplierCode, int TransporterCode, String VehicleNo, String GrossQty, String TareWt,
           String Act_wt, String Deduction_percent, String Deduction_Qty, String NetWt, String User, String Shift, int DiversionCode, int WBNumber)
        {
            String Query = @"Insert Into mines_WEIGHMENTS( RSLIP_NO,DSLIP_NO ,SLIP_DATE ,RM_CODE ,SUPP_CODE ,TRPT_CODE ,VEH_NO  , GROSS_QTY,TARE_QTY ,ACT_QTY  , DED_PER , DED_QTY, 
NET_QTY,WB_IN_DT,WB_IN_TM,WB_OUT_DT,WB_OUT_TM,WB_OPTR ,WB_SHIFT  , DIVISION_CODE,WB_NO) select (select NVL(max(TO_NUMBER(RSLIP_NO)),0) + 1 from mines_WEIGHMENTS),{0} ,TO_CHAR(SYSDATE, 'DD-MON-YY'),{1},{2},{3},'{4}',{5},{6},{7},{8},{9},{10},TO_CHAR(SYSDATE, 'DD-MON-YY'),TO_CHAR(SYSTIMESTAMP, 'HH24:MI:SS') ,TO_CHAR(SYSDATE, 'DD-MON-YY'),TO_CHAR(SYSTIMESTAMP, 'HH24:MI:SS') ,'{11}','{12}',{13},{14} from dual";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Query, GetTodaysMax(VehicleNo), MaterialCode, SupplierCode, TransporterCode, VehicleNo, GrossQty, TareWt, Act_wt, Deduction_percent, Deduction_Qty,
                NetWt, User, Shift, DiversionCode, WBNumber);
            OracleHelper.ExecuteNonQuery(sb.ToString());
            return sb.ToString();
        }


        public static String InsertNewGrossWeight_Manual(int MaterialCode, int SupplierCode, int TransporterCode, String VehicleNo, String GrossQty, String TareWt,
           String Act_wt, String Deduction_percent, String Deduction_Qty, String NetWt, String User, String Shift, int DiversionCode, int WBNumber, String Date, String Time)
        {
            String Query = @"Insert Into mines_WEIGHMENTS( RSLIP_NO,DSLIP_NO ,SLIP_DATE ,RM_CODE ,SUPP_CODE ,TRPT_CODE ,VEH_NO  , GROSS_QTY,TARE_QTY ,ACT_QTY  , DED_PER , DED_QTY, 
NET_QTY,WB_IN_DT,WB_IN_TM,WB_OUT_DT,WB_OUT_TM,WB_OPTR ,WB_SHIFT , DIVISION_CODE,WB_NO) select (select NVL(max(TO_NUMBER(RSLIP_NO)),0) + 1 from mines_WEIGHMENTS),{0} ,'" + Convert.ToDateTime(Date).ToString("dd-MMM-yy") + "',{1},{2},{3},'{4}','{5}','{6}','{7}',{8},{9},'{10}','" + Convert.ToDateTime(Date).ToString("dd-MMM-yy") + "','" + Convert.ToDateTime(Time).ToString("HH:mm:ss") + "' ,'" + Convert.ToDateTime(Date).ToString("dd-MMM-yy") + "','" + Convert.ToDateTime(Time).ToString("HH:mm:ss") + "','{11}','{12}',{13},{14} from dual";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Query, 1, MaterialCode, SupplierCode, TransporterCode, VehicleNo, GrossQty, TareWt, Act_wt, Deduction_percent, Deduction_Qty,
                NetWt, User, Shift, DiversionCode, WBNumber);
            OracleHelper.ExecuteNonQuery(sb.ToString());
            return sb.ToString();
        }
    }
}
