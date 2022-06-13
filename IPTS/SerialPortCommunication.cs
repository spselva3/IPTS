using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTS
{
    public delegate void OnSerialPortDataReceived(String Data);
    public class SerialPortCommunication
    {
        private OnSerialPortDataReceived _OnDataReceived;
        public OnSerialPortDataReceived OnDataReceived
        {
            set { _OnDataReceived = value; }
            get { return _OnDataReceived; }
        }
        SerialPort MySerialPort;
        public String PortName { get; set; }
        public int BaudRate { get; set; }
        public String ParityBit { get; set; }
        public int DataBits { get; set; }
        public String HandShake { get; set; }
        public SerialPortCommunication(String PortName, int BaudRate, String ParityBit, int DataBits, String HandShake, OnSerialPortDataReceived OnDataReceived)
        {
            this.PortName = PortName;
            this.BaudRate = BaudRate;
            this.ParityBit = ParityBit;
            this.DataBits = DataBits;
            this.HandShake = HandShake;
            this.OnDataReceived = OnDataReceived;

            MySerialPort = new SerialPort();
            MySerialPort.PortName = this.PortName;
            MySerialPort.BaudRate = this.BaudRate;
            MySerialPort.DataBits = this.DataBits;
            MySerialPort.Encoding = ASCIIEncoding.UTF8;
            MySerialPort.ReadTimeout = 3000;
            MySerialPort.RtsEnable = true;
            MySerialPort.DtrEnable = true;

            if (this.ParityBit.ToUpper().Trim() == "NONE")
                MySerialPort.Parity = Parity.None;
            else if (this.ParityBit.ToUpper().Trim() == "EVEN")
                MySerialPort.Parity = Parity.Even;
            else if (this.ParityBit.ToUpper().Trim() == "ODD")
                MySerialPort.Parity = Parity.Odd;
            else if (this.ParityBit.ToUpper().Trim() == "MARK")
                MySerialPort.Parity = Parity.Mark;
            else if (this.ParityBit.ToUpper().Trim() == "SPACE")
                MySerialPort.Parity = Parity.Space;

            if (this.HandShake.ToUpper().Trim() == "NONE")
                MySerialPort.Handshake = Handshake.None;
            else if (this.HandShake.ToUpper().Trim() == "XON/XOFF")
                MySerialPort.Handshake = Handshake.XOnXOff;
            else if (this.HandShake.ToUpper().Trim() == "RTS/CTS")
                MySerialPort.Handshake = Handshake.RequestToSend;

        }

        void MySerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                // string Data = sp.ReadExisting();
                string Data = sp.ReadLine();
                OnDataReceived(Data);
                MySerialPort.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                //Helper.WriteOnLog(ex.Message, Helper.LocationID);
            }
        }


        public Boolean OpenWithDataReceiveListener()
        {
            try
            {
                if (MySerialPort == null)
                    return false;

                if (!MySerialPort.IsOpen)
                {
                    MySerialPort.DataReceived += new SerialDataReceivedEventHandler(MySerialPort_DataReceived);
                    MySerialPort.Open();
                }

            }
            catch (Exception ex)
            {
                //Helper.WriteOnLog(ex.Message, Helper.LocationID);
            }
            return MySerialPort.IsOpen;
        }

        public Boolean OpenWithDataReceiveListener(String PortName, int BaudRate, String ParityBit, int DataBits, String HandShake)
        {
            if (!MySerialPort.IsOpen)
            {
                this.PortName = PortName;
                this.BaudRate = BaudRate;
                this.ParityBit = ParityBit;
                this.DataBits = DataBits;
                this.HandShake = HandShake;
                MySerialPort.PortName = this.PortName;
                MySerialPort.BaudRate = this.BaudRate;
                MySerialPort.DataBits = this.DataBits;


                if (this.ParityBit.ToUpper().Trim() == "NONE")
                    MySerialPort.Parity = Parity.None;
                else if (this.ParityBit.ToUpper().Trim() == "EVEN")
                    MySerialPort.Parity = Parity.Even;
                else if (this.ParityBit.ToUpper().Trim() == "ODD")
                    MySerialPort.Parity = Parity.Odd;
                else if (this.ParityBit.ToUpper().Trim() == "MARK")
                    MySerialPort.Parity = Parity.Mark;
                else if (this.ParityBit.ToUpper().Trim() == "SPACE")
                    MySerialPort.Parity = Parity.Space;

                if (this.HandShake.ToUpper().Trim() == "NONE")
                    MySerialPort.Handshake = Handshake.None;
                else if (this.HandShake.ToUpper().Trim() == "XON/XOFF")
                    MySerialPort.Handshake = Handshake.XOnXOff;
                else if (this.HandShake.ToUpper().Trim() == "RTS/CTS")
                    MySerialPort.Handshake = Handshake.RequestToSend;

                MySerialPort.DataReceived += new SerialDataReceivedEventHandler(MySerialPort_DataReceived);


                // MySerialPort.StopBits = StopBits.Two;
                MySerialPort.Open();
                // MySerialPort.DiscardInBuffer();
            }

            return MySerialPort.IsOpen;
        }

        public String GetData()
        {
            String Value = string.Empty;
            if (!MySerialPort.IsOpen)
                MySerialPort.Open();
            Value = MySerialPort.ReadLine();

            MySerialPort.Close();
            return Value;

        }

        public Boolean Stop()
        {
            if (MySerialPort == null)
                return true;
            if (MySerialPort.IsOpen)
            {
                MySerialPort.DataReceived -= new SerialDataReceivedEventHandler(MySerialPort_DataReceived);
                // System.Threading.Thread.Sleep(300);
                // MySerialPort.Close();
            }
            return !MySerialPort.IsOpen;
        }

        public void Close()
        {
            MySerialPort.Close();
        }
        public Boolean IsConnected()
        {
            return true;
        }
    }
}
