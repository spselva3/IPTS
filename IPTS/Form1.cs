using DragonFactory.TcpIPClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TraceTool;

namespace IPTS
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        #region Weighing Process
        List<ICustomClient> _weight1 = new List<ICustomClient>();
        List<string> CurrentWeight = new List<string>() { "", "", "" };
        static Regex _wtRegex = new Regex(@"\u0002 (\d+)\u0003");
        string _strWt = "";
        string _finalWt = "";

        private void StartWeighingProcess()
        {
            for (int i = 0; i < 1; i++)
            {
                _weight1.Add(CustomClientFactory.CreateClient(new CustomClientTcpEndPoint("192.168.13.230", 7730)));

                #region Connected
                _weight1[i].Connected += (o, e) =>
                {
                    ICustomClient _client = (ICustomClient)o;
                    var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");

                    if (h.IpAddress == "192.168.13.230" && h.TcpPort.ToString() == "7730")
                    {
                        //LogAsync("Weighing 1 with IP:" + h.IpAddress + ":" + h.TcpPort + " Connected");
                    }
                    //HANDLE THREAD
                };
                #endregion

                #region Disconnected
                _weight1[i].Disconnected += (o, e) =>
                {
                    ICustomClient _client = (ICustomClient)o;
                    var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");

                    if (h.IpAddress == "192.168.13.230" && h.TcpPort.ToString() == "7730")
                    {
                        // LogAsync("Weighing 1 with IP:" + h.IpAddress + ":" + h.TcpPort + " disconnected");
                    }
                    //HANDLE THREAD
                };
                #endregion

                #region Handling Weight

                _weight1[i].MessageReceived += (o, e) =>
                {
                    ICustomClient _client = (ICustomClient)o;
                    var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");
                    //_wt3Trace.SendValue("Weight from " + h.IpAddress, ((ClientCustomTextMessage)e.Message).Text);
                    if (h.IpAddress == "192.168.13.230" && h.TcpPort.ToString() == "7730")
                    {
                        try
                        {
                            _strWt = _strWt + ((ClientCustomTextMessage)e.Message).Text;

                            _finalWt = _strWt.Split('\r')[0].Replace("", "").Replace("      ", "").Replace("\r", "").Replace("\n", "").Replace("KG", "").Replace(" ", "");
                            if (_finalWt == "00" || _finalWt == "0" || _finalWt == "" || _finalWt == "K" || _finalWt == "G")
                            {
                                _strWt = "";
                            }

                            textBox1.Invoke((Action)(() => textBox1.Text = _finalWt));
                            //textBox1.Text = _finalWt;

                //            this.InvokeEx(_ =>
                //{
                //    textBox1.Text = _finalWt;
                //});



                            //if (_strWt.Length > 20)
                            //{
                            //    _wt3Trace.SendValue("Looped", _strWt);
                            //    var mat = _wtRegex.Matches(_strWt);
                            //    if (mat.Count > 0)
                            //    {
                            //        string we = int.Parse(mat[0].Groups[0].Value.Replace("\u0002", "").Replace("\u0003", "").Replace(" ", "")).ToString();
                            //        _wt3Trace.SendValue("Weight match", we);
                            //        CurrentWeight[0] = we;
                            //    }
                            //    _strWt = "";
                            //}
                            //else
                            //{
                            //    _strWt = _strWt + ((ClientCustomTextMessage)e.Message).Text;
                            //    _finalWt = _strWt.Split('\r')[0].Replace("   ", "").Replace("\r", "").Replace("\n", "").Replace("KG", "").Replace(" ", "");
                            //    txtNewTareWeight.Text = _finalWt;
                            //}
                        }
                        catch (Exception ex)
                        {

                            //_wt3Trace.SendValue("Error", ex);
                        }
                        //CurrentWeight[0] = ((ClientCustomTextMessage)e.Message).Text.Split('\r')[0].Replace("\r", "").Replace("\n", "");
                    }
                    //HANDLE THREAD
                };
                #endregion

                _weight1[i].PingAndDisconnectInterval = 5000;
                _weight1[i].ReconnectInterval = 5000;
                _weight1[i].TryToConnectContinuosly();
            }
        }

        private void StopWeighingProcess()
        {
            for (int i = 0; i < 1; i++)
            {
                _weight1[0].Disconnect();
            }

            //WeighBridge.Stop();
        }
        #endregion
       ITW.PSM. MTWeight objMTW = new ITW.PSM.MTWeight();
        private void Form1_Load(object sender, EventArgs e)
        {
           // StartWeighingProcess();
        }


        bool netwtstatus = false;
        private void NetWTConnect()
        {
            try
            {
                objMTW.IPAddr = "192.168.13.230";
                objMTW.PortNum = 7730;
                try
                {
                    if (objMTW.Connect() == true)
                    {
                        //txtnetwt.BackColor = Color.White;
                        //dblayer.tblEventlogInsert("Net Wt Connected");
                        netwtstatus = true;
                    }
                    else
                    {
                        //txtnetwt.BackColor = Color.Wheat;
                    }
                }
                catch (Exception)
                {
                    //dblayer.tblEventlogInsert("DisConnected Net Wt ");
                    netwtstatus = false;
                }

            }
            catch (Exception ex)
            {
               // dblayer.tblEventlogInsert("Net Wt Error: " + ex.Message.ToString());
                netwtstatus = false;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!objMTW.IsConnected)
            {

                if (netwtstatus == false)
                {
                    NetWTConnect();
                }

            }
            else
            {
                try
                {
                    textBox1.Text = objMTW.Weight.ToString();
                }
                catch (Exception ex)
                {
                    netwtstatus = false;
                    //dblayer.tblEventlogInsert("Net Weighing Capture Error:" + ex.Message.ToString());
                }

            }
        }
    }
}
