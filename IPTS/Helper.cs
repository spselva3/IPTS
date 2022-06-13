using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTS
{
    public static class Helper
    {
        #region Common
        public static String DevisionName = string.Empty;
        public static String DevisionCode = "33";
        public static String MateriMovementType = string.Empty;
        public static String MateriMovementCode = string.Empty;
        public static String RM_CODE = "91";
        public static String RM_NAME = "LIMESTONE";
        public static String Mine = string.Empty;
        public static String MineCode = string.Empty;
        public static string UserName = string.Empty;
        public static string UserRole = string.Empty;
        public static string RegistrationRfidValidation = string.Empty;

        public static String BigQuerry = string.Empty;

        #endregion

        #region Hardware
        public static String PLCIP = string.Empty;
        public static int PLCPort = 0;
        public static int SensorPort1 = 0;
        public static int SensorPort2 = 0;
        public static int TrafficLightPortGreen = 0;
        public static int TrafficLightPortRed = 0;
        public static int HooterPort = 0;
        public static String RFIDReaderIP = string.Empty;
        public static int RFIDReaderPort = 0;
        public static String WeighBridgeIP = string.Empty;
        public static int WeighBridgePort = 0;
        public static String DisplayIP = string.Empty;
        public static int DisplayPort = 0;

        public static bool AutoSave = false;
        public static decimal TruckMinWt = 0;
        public static decimal TruckMaxWt = 0;
        public static int TareWeightFrequencedays = 0;
        public static int TruckGrossWeightThreshold = 0;
        public static int AutoSaveInterval = 0;
        public static int TrafficLightClose = 3;

        #endregion


        #region Weigh Serial Port

        public static string GetSerialPortName()
        {
            return ConfigurationManager.AppSettings["WSPortName"].ToString().Trim();
        }

        public static int GetSerialPortBaudRate()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["WSBaudRate"].ToString().Trim());
        }

        public static string GetSerialPortParityBit()
        {
            return ConfigurationManager.AppSettings["WSParityBit"].ToString().Trim();
        }

        public static int GetSerialPortDataBits()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["WSDataBits"].ToString().Trim());
        }

        public static string GetWeighingScaleOperationMode()
        {
            return ConfigurationManager.AppSettings["OperationMode"].ToString().Trim();
        }

        public static string GetSerialPortHandShake()
        {
            return ConfigurationManager.AppSettings["WSHandShake"].ToString().Trim();
        }

        #endregion



    }
}
