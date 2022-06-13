using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ITW.PSM
{
    internal class MTWeight
    {
        #region Delegates

        public delegate void ErrorOnReadEventHandler();

        #endregion

        public string PresentWeight;
        private TcpClient _clientSocket = new TcpClient();
        private bool _connected;
        private string _iWtUnit = "Kgs";

        private object _oCapture;

        private string _oIp;

        private int _oPort;

        public string WtUnit
        {
            get { return _iWtUnit; }
            set { _iWtUnit = value; }
        }

        public object Weight
        {
            get
            {
                Capture();
                return _oCapture;
            }
            set { _oCapture = value; }
        }

        public string IPAddr
        {
            get { return _oIp; }
            set { _oIp = value; }
        }

        public int PortNum
        {
            get { return _oPort; }
            set { _oPort = value; }
        }

        public bool IsConnected
        {
            get { return _connected; }
        }

        public event ErrorOnReadEventHandler ErrorOnRead;

        private void Capture()
        {
            try
            {
                if (_clientSocket.Connected)
                {
                    _connected = true;
                    NetworkStream serverStream = _clientSocket.GetStream();

                    if (serverStream.CanWrite)
                    {
                        byte[] outStream = StrToByteArray("read wt0101 wt0103" + "\n");
                        serverStream.Write(outStream, 0, outStream.Length);

                        var inStream = new byte[_clientSocket.ReceiveBufferSize + 1];
                        serverStream.Read(inStream, 0, Convert.ToInt32(_clientSocket.ReceiveBufferSize));
                        string returndata = Encoding.ASCII.GetString(inStream);


                        string[] parts = Regex.Split(returndata, "G+");
                        if (parts.Length >1)
                        {
                            PresentWeight = parts[1].Substring(1, 7);
                        }


                        // Split on  separator
                      //  string[] parts = returndata.Split(',');
                        //foreach (string part in parts)
                        //{
                        //    PresentWeight = parts[3].Substring(2, 7);
                        //    //WtUnit = parts[2];
                        //    break;
                        //}
                        _oCapture = PresentWeight;
                    }
                    else
                    {
                        if (ErrorOnRead != null)
                        {
                            ErrorOnRead();
                        }
                    }
                }
                else
                {
                    _connected = false;
                    if (!_clientSocket.Connected)
                    {
                        Connect();
                    }
                }
            }
            catch (Exception ex)
            {
                //dblayer.tblEventLogInsert("Weighing Class Capture Error:" + ex.Message.ToString(), UPL.frmMain.Default.loginUser);

            }
        }

        public static byte[] StrToByteArray(string str)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        //StrToByteArray

        public bool Connect(string IPAddress, int PortNo)
        {
            IPAddr = IPAddress;
            PortNum = PortNo;
            return Connect();
        }

        public bool Connect()
        {
            try
            {
                _connected = false;
                _clientSocket = new TcpClient();
                if (!_clientSocket.Connected)
                {
                    _clientSocket.Connect(_oIp, _oPort);
                }

                if (_clientSocket.Connected)
                {
                    //--- Pass Credentials
                    NetworkStream serverStream = _clientSocket.GetStream();
                    if (serverStream.CanWrite)
                    {
                        //outStream = StrToByteArray("user admin" & vbCrLf)
                        byte[] outStream = StrToByteArray("user admin" + "\n");
                        serverStream.Write(outStream, 0, outStream.Length);
                        // Threading.Thread.Sleep(100)
                        _connected = true;
                    }
                }
                return _connected;
            }
            catch (Exception ex)
            {
                _clientSocket.Close();
                //MsgBox("Cannot open the socket to loadcells. Check connection!!!")
               // dblayer.tblEventLogInsert("Weighing Class Connect Error:" + ex.Message.ToString(), UPL.frmMain.Default.loginUser);
                string ex1 = ex.Message;
                return false;
            }
        }

        public void DisConnect()
        {
            try
            {
                _clientSocket.Close();
                _clientSocket = null;
            }
            catch (Exception ex)
            {
                //dblayer.tblEventLogInsert("Weighing Class Disconnect Error:" + ex.Message.ToString(), UPL.frmMain.Default.loginUser);
            }
        }
    }
}