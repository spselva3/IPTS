using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Configuration;
using DragonFactory.Utilities;
using System.Threading;
using TraceTool;
namespace IPTS
{
    public class RfidHelper
    {
        #region Initialize

        WinTrace _errorTrace = new WinTrace("ChinaReader", "ChinaReader");
        //WinTrace _errorTraceLog = new WinTrace("ChinaReaderLog", "ChinaReaderLog");
        public RfidHelper()
        {
            //EPC_MultiTagInventory_timer = new System.Windows.Forms.Timer();
            //EPC_MultiTagInventory_timer.Interval = 50;
            //EPC_MultiTagInventory_timer.Tick += EPC_MultiTagInventory_timer_Tick;
        }

        RFID_StandardProtocol rfid_sp = new RFID_StandardProtocol();
        Socket sockClient = null;
        hComSocket CS;
        public int ConnectFlag = 0;
        public event Action<string, string> NewTagValueReceived;
        string _ip;
        string _port;

        CancellationTokenSource _cs;

        #endregion

        #region StartConnection

        public void StartConnection(string ip, string port)
        {
            try
            {
                _port = port;
                _ip = ip;
                int status = 0;

                _errorTrace.Send("StartConnection");
                status = ConnectReader(_ip, _port);
                _errorTrace.Send("StartConnection return " + status);
                if (0 == status)
                {
                    CS.hCom = -1;
                    CS.sockClient = sockClient;
                    ConnectFlag = 1;
                }
            }
            catch (Exception ex)
            {
                _errorTrace.SendValue("StartConnection " + ex.Message.ToString(), ex);
            }

        }

        int ConnectReader(string ip, string port)
        {
            int status = 0;
            string TempStrEnglish = "";

            status = SocketConnect(ip, port);

            switch (status)
            {
                case 0:
                    _errorTrace.Send("Connect success!");
                    TempStrEnglish = "Connect success!";

                    break;
                case -1:
                    _errorTrace.Send("Connect fail!");
                    TempStrEnglish = "Connect fail!";

                    break;
                case -2:
                    _errorTrace.Send("Connect parameter incomplete!");
                    TempStrEnglish = "Connect parameter incomplete!";

                    break;
                case -3:
                    _errorTrace.Send("Input IP Invalid!");
                    TempStrEnglish = "Input IP Invalid!";
                    break;
                case -4:
                    _errorTrace.Send("Error thrown!");
                    TempStrEnglish = "Error thrown!";

                    break;
            }

            if (TempStrEnglish != "Connect success!")
                return -1;
            return 0;
        }

        int SocketConnect(string ip, string port)
        {
            try
            {
                IPAddress ipaddr;
                if (ip == "" || port == "")
                {
                    return -2;
                }
                else if (!IPAddress.TryParse(ip, out ipaddr))
                {
                    return -3;
                }
                else
                {

                    if (rfid_sp.Socket_ConnectSocket(ref sockClient, ip, Convert.ToInt32(port)) == RFID_StandardProtocol.SUCCESS)
                        return 0;
                    else
                        return -1;
                }
            }
            catch (Exception ex)
            {

                _errorTrace.SendValue("SocketConnect" + ex.Message, ex);
                return -4;
            }

        }

        #endregion

        #region Core Read

        private void ReadFromReaderAync()
        {
            if (ConnectFlag == 1)
            {
                try
                {
                    ClearIDBuffer();
                    EPCMultiTagInventory();
                }
                catch (Exception ex)
                {
                    _errorTrace.SendValue(ex.Message, ex);
                    SocketDisConnect();
                    Thread.Sleep(20);
                    StartConnection(_ip, _port);
                    //StopIdentifying();
                }
            }
            else
            {
                try
                {
                    SocketDisConnect();
                    Thread.Sleep(20);
                    try
                    {
                        StartConnection(_ip, _port);
                    }
                    catch (Exception ex2)
                    {
                        _errorTrace.SendValue("Repeat Reconnect: " + ex2.Message, ex2);
                    }

                }
                catch (Exception ex)
                {
                    _errorTrace.SendValue("Repeat Disconnect: " + ex.Message, ex);
                }

            }
        }

        void ClearIDBuffer()
        {
            try
            {
                rfid_sp.BufferM_ClearBuffer(CS, 0xFF);
            }
            catch (Exception ex)
            {
                _errorTrace.SendValue("ClearIDBuffer " + ex.Message.ToString(), ex);
            }
        }

        void EPCMultiTagInventory()
        {
            //_errorTraceLog.Send("Trying to read");
            int TagCount = 0;
            int CntGot = 0;
            int GetCount = 0;
            int i, j = 0;
            int status;
            string EPC = "";
            BufferData[] Data = new BufferData[1024];
            for (int index = 0; index < Data.Length; index++)
            {
                Data[index].Data = new byte[512];
            }

            status = rfid_sp.GEN2_MultiTagInventory(CS, ref TagCount, 0xFF);
            if (0x00 == status)
            {
                while (CntGot < TagCount)
                {
                    status = rfid_sp.BufferM_GetTagData(CS, ref GetCount, Data, 0xFF);
                    if (0x00 == status)
                    {
                        if (GetCount <= 0)
                        {
                            break;
                        }

                        for (i = 0; i < GetCount; i++)
                        {
                            EPC = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}{10:X2}{11:X2}",
                                Data[i].Data[0], Data[i].Data[1], Data[i].Data[2], Data[i].Data[3],
                                Data[i].Data[4], Data[i].Data[5], Data[i].Data[6], Data[i].Data[7],
                                Data[i].Data[8], Data[i].Data[9], Data[i].Data[10], Data[i].Data[11]);
                            //_errorTraceLog.Send(EPC);
                            UpdateReaderReadValue = EPC;
                        }
                    }
                    CntGot += GetCount;
                }
            }
            //_errorTraceLog.Send("Completed reading");
        }

        #endregion

        #region Stop Connection
        public void CloseConnection()
        {
            int status = 0;
            status = DisConnectReader();

            if (0 == status)
            {
                ConnectFlag = 0;
            }
            //EPC_MultiTagInventory_timer.Enabled = false;

            if (_cs != null)
                _cs.Cancel();
        }

        int DisConnectReader()
        {
            int status = 0;
            string TempStrEnglish = "";

            status = SocketDisConnect();

            switch (status)
            {
                case 0:
                    TempStrEnglish = "Close success!";
                    break;
                case -1:
                    TempStrEnglish = "Close fail!";
                    break;
                case -2:
                    TempStrEnglish = "Handle unavailable!";
                    break;
            }

            if (TempStrEnglish != "Close success!")
                return -1;
            return 0;
        }

        int SocketDisConnect()
        {
            if (sockClient != null)
            {
                if (rfid_sp.Socket_CloseSocket(sockClient) == RFID_StandardProtocol.SUCCESS)
                {
                    CS.sockClient = null;
                    sockClient = null;
                    rfid_sp.Opened = false;
                    rfid_sp = null;
                    rfid_sp = new RFID_StandardProtocol();
                    _errorTrace.Send("Successfully closed the socket!");
                    ConnectFlag = 0;
                    return 0;
                }
                else
                {
                    _errorTrace.Send("Error while closing socket");
                    return -1;
                }
            }
            else
            {
                _errorTrace.Send("Socket already null");
                return -2;
            }
        }
        #endregion

        #region Start - Stop Process
        public void StartIdendifying()
        {
            //EPC_MultiTagInventory_timer.Enabled = true;
            _cs = new CancellationTokenSource();

            DragonFactory.PeriodicTaskFactory.Start(ReadFromReaderAync, 70, synchronous: true,
                cancelToken: _cs.Token,
                periodicTaskCreationOptions: TaskCreationOptions.LongRunning);
        }

        public void StopIdentifying()
        {
            //EPC_MultiTagInventory_timer.Enabled = false;
            if (_cs != null)
                _cs.Cancel();
        }
        #endregion

        #region Update Current Tag Value

        string _lastReadValue = "";

        protected virtual void OnNewTagValueReceived(string newV, string oldV)
        {
            var handler = NewTagValueReceived;
            if (handler != null)
                handler(newV, oldV);
        }

        public string UpdateReaderReadValue
        {
            get
            {
                return _lastReadValue;
            }
            set
            {
                if (_lastReadValue != value)
                {
                    try
                    {
                        _errorTrace.Send("Reading: Tag Changed: ", value);
                        OnNewTagValueReceived(value, _lastReadValue);
                    }
                    catch (Exception ex)
                    {
                        _errorTrace.SendValue("UpdateReaderReadValue " + ex.Message.ToString(), ex);
                    }
                }
                _lastReadValue = value;
            }
        }

        #endregion

        #region Antenna Properties
        public class AntennaProperties
        {
            public bool Presence { get; set; }
            public int PowerIndBm { get; set; }
        }

        public List<AntennaProperties> AntennaStatus = new List<AntennaProperties>() { 
        new AntennaProperties(),
        new AntennaProperties(),
        new AntennaProperties(),
        new AntennaProperties()
        };
        public void UpdateAntennaPresence()
        {
            string TempStrEnglish = "";
            int Workant = 0;
            int antStatus = 0;

            if (0x00 == rfid_sp.Config_GetAntenna(CS, ref Workant, ref antStatus, 0xFF))
            {
                TempStrEnglish = "antenna work state query success";
                if ((Workant & (1 << 0)) != 0)
                    AntennaStatus[0].Presence = true;
                else
                    AntennaStatus[0].Presence = false;
                if ((Workant & (1 << 1)) != 0)
                    AntennaStatus[1].Presence = true;
                else
                    AntennaStatus[1].Presence = false;
                if ((Workant & (1 << 2)) != 0)
                    AntennaStatus[2].Presence = true;
                else
                    AntennaStatus[2].Presence = false;
                if ((Workant & (1 << 3)) != 0)
                    AntennaStatus[3].Presence = true;
                else
                    AntennaStatus[3].Presence = false;
            }
            else
            {
                TempStrEnglish = "antenna work state query failed";
            }
        }

        public void UpdateAntennaPower()
        {
            UpdateAntennaPresence();

            string TempStrEnglish = "";
            byte[] aPwr = new byte[4];

            if (0x00 == rfid_sp.Config_GetRfPower(CS, aPwr, 0xFF))
            {
                TempStrEnglish = "antenna power query success";

                AntennaStatus[0].PowerIndBm = (int)aPwr[0];
                AntennaStatus[1].PowerIndBm = (int)aPwr[1];
                AntennaStatus[2].PowerIndBm = (int)aPwr[2];
                AntennaStatus[3].PowerIndBm = (int)aPwr[3];
            }
            else
            {
                TempStrEnglish = "antenna power query failed";
            }

        }

        public void SetAntennaPower(string p1, string p2, string p3, string p4)
        {
            string TempStrEnglish = "";
            byte[] aPwr = new byte[4];

            int m_anten1_dbm = 0;
            int m_anten2_dbm = 0;
            int m_anten3_dbm = 0;
            int m_anten4_dbm = 0;

            m_anten1_dbm = Convert.ToInt32(p1);
            m_anten2_dbm = Convert.ToInt32(p2);
            m_anten3_dbm = Convert.ToInt32(p3);
            m_anten4_dbm = Convert.ToInt32(p4);

            m_anten1_dbm = (m_anten1_dbm < 0) ? 0 : m_anten1_dbm;
            m_anten1_dbm = (m_anten1_dbm > 32) ? 32 : m_anten1_dbm;

            m_anten2_dbm = (m_anten2_dbm < 0) ? 0 : m_anten2_dbm;
            m_anten2_dbm = (m_anten2_dbm > 32) ? 32 : m_anten2_dbm;

            m_anten3_dbm = (m_anten3_dbm < 0) ? 0 : m_anten3_dbm;
            m_anten3_dbm = (m_anten3_dbm > 32) ? 32 : m_anten3_dbm;

            m_anten4_dbm = (m_anten4_dbm < 0) ? 0 : m_anten4_dbm;
            m_anten4_dbm = (m_anten4_dbm > 32) ? 32 : m_anten4_dbm;

            aPwr[0] = (byte)((m_anten1_dbm) & 0xFF);
            aPwr[1] = (byte)((m_anten2_dbm) & 0xFF);
            aPwr[2] = (byte)((m_anten3_dbm) & 0xFF);
            aPwr[3] = (byte)((m_anten4_dbm) & 0xFF);
            if (0x00 == rfid_sp.Config_SetRfPower(CS, aPwr, 0xFF))
            {
                TempStrEnglish = "antenna power set success";
            }
            else
            {
                TempStrEnglish = "antenna power set failed";
            }
            UpdateAntennaPower();
        }

        public void SetAntennaPresence(bool a1, bool a2, bool a3, bool a4)
        {
            string TempStrEnglish = "";
            int[] ant = new int[4];
            int Workant = 0;
            if (a1)
                ant[0] = 1;
            if (a2)
                ant[1] = 2;
            if (a3)
                ant[2] = 4;
            if (a4)
                ant[3] = 8;
            Workant = ant[0] + ant[1] + ant[2] + ant[3];
            if (0x00 == rfid_sp.Config_SetAntenna(CS, Workant, 0xFF))
            {
                TempStrEnglish = "antenna work state set success";
            }
            else
            {
                TempStrEnglish = "antenna work state set failed";
            }
            UpdateAntennaPresence();
        }
        #endregion
    }
}
