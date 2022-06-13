using DragonDesignSurface;
using DragonFactory;
using DragonFactory.TcpIPClient;
using DragonFactory.UserContols;
using DragonFactory.Utilities;
using DragonGraphicObjects;
using LMS;
using S7.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TraceTool;

namespace IPTS
{
    public partial class frmMaterialMovement : Form
    {
        ErrorLogs _error = new ErrorLogs();
        public string _connection = ConfigurationManager.ConnectionStrings["ConStrn"].ConnectionString;
        SerialPortCommunication WeighBridge;
        String WeighingScaleData = string.Empty;
        public static string _word = string.Empty;
        static WinTrace _printTrace = new WinTrace("PrintTrace", "PrintTrace");

        frmMain _Main;
        public frmMaterialMovement(frmMain Main)
        {
            InitializeComponent();
            _Main = Main;
            InitParams();
        }

        #region Initialize

        #region Trace
        private static readonly WinTrace _wt1Trace = new WinTrace("Weight1", "Weight1");
        private static readonly WinTrace _wt2Trace = new WinTrace("Weight2", "Weight2");
        private static readonly WinTrace _wt3Trace = new WinTrace("Weight3", "Weight3");
        private static readonly WinTrace _plcTrace = new WinTrace("PLCConn", "PLCConn");
        private static readonly WinTrace _plcReadTrace = new WinTrace("PLCRead", "PLCRead");

        #endregion

        private void InitParams()
        {
            _plcIp = Helper.PLCIP;
            _db1 = new DB1();
            _db1.DataChanged += _db1_DataChanged;
        }

        #region Device Count
        int noOfWeighing = 1;

        #endregion

        #endregion

        #region Log
        private void LogAsync(string msg, ListLogErrorLevels sd = ListLogErrorLevels.Verbose)
        {
            logList.InvokeEx(_ =>
            {
                logList.Log(sd, msg);

            });
        }
        #endregion

        #region Weight Captcher Through Serial Port

        private void UpdateData(String Data)
        {
            try
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new OnSerialPortDataReceived(UpdateData), Data);
                else
                {
                    if (Data.IndexOf("G") > 0)
                    {
                        Regex regexObj = new Regex(@"[^\d]");
                        lblGrossWeight.Text = regexObj.Replace(Data.Substring(0, Data.IndexOf("G")).Trim(), "");
                        decimal _grossWt = Convert.ToDecimal(lblGrossWeight.Text);
                        lblGrossWeight.Text = _grossWt.ToString();
                        decimal _netWt = Convert.ToDecimal(lblGrossWeight.Text) - Convert.ToDecimal(lblTareWeight.Text);
                        lblNetWeight.Text = _netWt.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                //Helper.WriteOnLog(ex.Message, Helper.LocationID);
            }
        }

        private void OnWeighingScaleDataReceived(String Data)
        {
            try
            {
                (new OnSerialPortDataReceived(UpdateData)).BeginInvoke(Data, null, null);
            }
            catch (Exception ex)
            {
                //Helper.WriteOnLog(ex.Message, Helper.LocationID);
            }
        }

        private void frmMaterialMovement_Shown(object sender, EventArgs e)
        {
            try
            {
                if (ConfigurationManager.AppSettings["WBConnected"].ToString() == "SerialPort")
                {
                    WeighBridge = new SerialPortCommunication(Helper.GetSerialPortName(), Helper.GetSerialPortBaudRate(), Helper.GetSerialPortParityBit(),
                        Helper.GetSerialPortDataBits(), Helper.GetSerialPortHandShake(), new OnSerialPortDataReceived(OnWeighingScaleDataReceived));
                    if (Helper.GetWeighingScaleOperationMode().ToUpper().Trim() == "DEDICATEDPORT")
                    {
                        WeighBridge.OpenWithDataReceiveListener();
                        pbWBConnected.Visible = true;
                        pbWBDisconnected.Visible = false;
                    }
                }
                if (ConfigurationManager.AppSettings["WBConnected"].ToString() == "Enthernet")
                {
                    StartWeighingProcess();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void WeightCaptcher()
        {
            try
            {
                lblGrossWeight.Text = "00";

                String Data = string.Empty;
                while (Data.IndexOf("G") < 0)
                {
                    try
                    {
                        Data = WeighBridge.GetData();
                        System.Threading.Thread.Sleep(200);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                Regex regexObj = new Regex(@"[^\d]");
                lblGrossWeight.Text = regexObj.Replace(Data.Substring(0, Data.IndexOf("G")).Trim(), "");
                decimal _grossWt = Convert.ToDecimal(lblGrossWeight.Text);
                lblGrossWeight.Text = _grossWt.ToString();
                decimal _netWt = Convert.ToDecimal(lblGrossWeight.Text) - Convert.ToDecimal(lblTareWeight.Text);
                lblNetWeight.Text = _netWt.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region LedDisplay

        LedHelper LedDisplaysList;
        private void StartLedProcess()
        {
            try
            {
                LedDisplaysList = new LedHelper();
                for (int i = 0; i < 1; i++)
                {
                    LedDisplaysList.StartLedConnection(Helper.DisplayIP, Helper.DisplayPort.ToString(),
                                                (o, e) =>
                                                {
                                                    ICustomClient _client = (ICustomClient)o;
                                                    var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");
                                                    if (h.IpAddress == Helper.DisplayIP)
                                                    {
                                                        if (i == 0)
                                                        {
                                                        }
                                                    }
                                                },
                                                 (o, e) =>
                                                 {
                                                     ICustomClient _client = (ICustomClient)o;
                                                     var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");
                                                     if (h.IpAddress == Helper.DisplayIP)
                                                     {
                                                     }
                                                 });
                }
            }
            catch (Exception ex)
            {
                //lblMessage.Text = ex.Message.ToString();
                throw;
            }
        }

        private void StopLedProcess()
        {
            for (int i = 0; i < 1; i++)
            {
                LedDisplaysList.CloseLedConnection();
            }
        }

        #endregion

        #region Weighing Process
        List<ICustomClient> _weight1 = new List<ICustomClient>();
        static Regex _wtRegex = new Regex(@" \d+KG");
        string _strWt = "";
        string _finalWt = "";

        private void StartWeighingProcess()
        {
            Ping _pWeight = new Ping();
            PingReply _rWeight;
            _rWeight = _pWeight.Send(Helper.WeighBridgeIP, 500);
            if (_rWeight.Status == IPStatus.Success)
            {
                for (int i = 0; i < noOfWeighing; i++)
                {
                    _weight1.Add(CustomClientFactory.CreateClient(new CustomClientTcpEndPoint(Helper.WeighBridgeIP, Helper.WeighBridgePort)));

                    #region Connected
                    _weight1[i].Connected += (o, e) =>
                    {
                        ICustomClient _client = (ICustomClient)o;
                        var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");

                        if (h.IpAddress == Helper.WeighBridgeIP && h.TcpPort.ToString() == Helper.WeighBridgePort.ToString())
                        {
                            pbWBConnected.Visible = true;
                            pbWBDisconnected.Visible = false;
                            LogAsync("Weighing 1 with IP:" + h.IpAddress + ":" + h.TcpPort + " Connected");
                        }
                        //HANDLE THREAD
                    };
                    #endregion

                    #region Disconnected
                    _weight1[i].Disconnected += (o, e) =>
                    {
                        ICustomClient _client = (ICustomClient)o;
                        var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");

                        if (h.IpAddress == Helper.WeighBridgeIP && h.TcpPort.ToString() == Helper.WeighBridgePort.ToString())
                        {
                            LogAsync("Weighing 1 with IP:" + h.IpAddress + ":" + h.TcpPort + " disconnected");
                        }
                        //HANDLE THREAD
                    };
                    #endregion

                    #region Handling Weight

                    _weight1[i].MessageReceived += (o, e) =>
                    {
                        ICustomClient _client = (ICustomClient)o;
                        var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");
                        if (h.IpAddress == Helper.WeighBridgeIP && h.TcpPort.ToString() == Helper.WeighBridgePort.ToString())
                        {
                            try
                            {
                                _strWt = _strWt + ((ClientCustomTextMessage)e.Message).Text;
                                var mt = _wtRegex.Matches(_strWt);
                                if (mt.Count > 1)
                                {
                                    _finalWt = mt[0].Value.Replace("KG", "").Replace(" ", "");
                                    lblGrossWeight.Invoke((Action)(() => lblGrossWeight.Text = _finalWt));
                                    _strWt = "";
                                }
                                //if (_strWt.IndexOf("G") > 0)
                                //{
                                //    //Regex regexObj = new Regex(@"[^\d]");
                                //    _finalWt = regexObj.Replace(_strWt.Substring(0, _strWt.IndexOf("G")).Trim(), "");
                                //    lblGrossWeight.Invoke((Action)(() => lblGrossWeight.Text = _finalWt));
                                //    _strWt = "";
                                //}
                            }
                            catch (Exception ex)
                            {

                                _wt3Trace.SendValue("Error", ex);
                            }
                        }
                    };
                    #endregion

                    _weight1[i].PingAndDisconnectInterval = 5000;
                    _weight1[i].ReconnectInterval = 5000;
                    _weight1[i].TryToConnectContinuosly();
                }
            }
        }

        private void StopWeighingProcess()
        {
            for (int i = 0; i < noOfWeighing; i++)
            {
                _weight1[0].Disconnect();
            }
        }

        #endregion

        #region Time Delays
        int Start_Weighing_Delay = Helper.AutoSaveInterval;
        int Traffic_Light_CloseDelay = Helper.TrafficLightPortGreen;
        #endregion

        #region PLC Connect Disconnect

        #region PLC - Init
        CancellationTokenSource _plcConnectionCheckToken;
        CancellationTokenSource _plcReadToken;
        CancellationTokenSource _formClosingToken = new CancellationTokenSource();
        CancellationTokenSource _weightToken = new CancellationTokenSource();
        CancellationTokenSource _timerToken = new CancellationTokenSource();
        CancellationTokenSource _trafficLight = new CancellationTokenSource();

        private static string _plcIp = Helper.PLCIP;
        Plc _plc;
        DB1 _db1;
        #endregion

        public void StartPlcProcess()
        {
            _plcConnectionCheckToken = new CancellationTokenSource();
            PeriodicTaskFactory.Start(connectToPlc,
                                        intervalInMilliseconds: 5000,
                                        delayInMilliseconds: 500,
                                        synchronous: true,
                                        cancelToken: _plcConnectionCheckToken.Token,
                                        periodicTaskCreationOptions: TaskCreationOptions.LongRunning);
        }

        public void StopPlcProcess()
        {
            if (_plcReadToken != null)
            {
                _plcReadToken.Cancel();
            }
            Thread.Sleep(25);

            if (_plcConnectionCheckToken != null)
            {
                _plcConnectionCheckToken.Cancel();
            }
            Thread.Sleep(25);

            if (_plc != null)
            {
                _plc.Close();
                _plc = null;
                LogAsync("PLC disconnected");
            }
        }

        private void connectToPlc()
        {
            _plcTrace.Debug.Send("Connect to Plc Event Called");
            try
            {
                if (GeneralUtils.PingIp(_plcIp, 1000))
                {
                    if (_plc == null)
                    {
                        //create a new plc connection
                        _plcTrace.Send("Trying to connect to PLC with IP" + _plcIp);
                        _plc = new Plc(CpuType.S71200, _plcIp, 0, 0);
                        _plc.Open();
                        if (_plc.IsConnected)
                        {
                            LogAsync("PLC disconnected");
                            _plcTrace.Send("PLC Connected. IP:" + _plcIp);
                            //_plc.Write("DB1.DBX0.0", true);
                            //_plc.Write("DB1.DBX0.7", false);//thiru
                            //TrafficLightStatusChange(true, false); after enable
                            //PLC CONNECTED. DATACHANGE
                            _plcReadToken = new CancellationTokenSource();
                            PeriodicTaskFactory.Start(UpdateDBs,
                                                            intervalInMilliseconds: 300,
                                                            delayInMilliseconds: 300,
                                                            synchronous: true,
                                                            cancelToken: _plcReadToken.Token,
                                                            periodicTaskCreationOptions: TaskCreationOptions.LongRunning);
                            LogAsync("PLC Connected", ListLogErrorLevels.Info);
                            Invoke(new Action(() =>
                            {
                                pbPLCConnected.Visible = true;
                                pbPLCDisconnected.Visible = false;
                            }));
                        }

                    }
                    else if (_plc != null && _plc.IsConnected)
                    {
                        _plcTrace.Debug.Send("PLC already connected");
                    }
                    else
                    {
                        _plcTrace.Send("PLC.IsConnected property failed for IP:" + _plcIp);
                        disconnectPlc();
                    }
                }
                else
                {
                    _plcTrace.Send("PLC Ping failed for IP:" + _plcIp);
                    disconnectPlc();
                }

            }
            catch (Exception ex)
            {
                LogAsync("Error in PLC Connection: " + ex.Message, ListLogErrorLevels.Error);
                _plcTrace.SendValue("Error in ConnectToPlc", ex);
            }
        }

        private void disconnectPlc()
        {
            if (_plcReadToken != null)
            {
                _plcReadToken.Cancel();
            }
            Thread.Sleep(25);



            if (_plc != null)
            {
                _plc.Close();
                _plc = null;
                LogAsync("PLC disconnected");
                Invoke(new Action(() =>
                {
                    pbPLCDisconnected.Visible = true;
                    pbPLCConnected.Visible = false;
                }));
            }
            else
            {
                LogAsync("PLC disconnected");
                Invoke(new Action(() =>
                {
                    pbPLCDisconnected.Visible = true;
                    pbPLCConnected.Visible = false;
                }));
            }
        }

        #endregion

        #region DB Datachanged

        private void UpdateDBs()
        {
            _plcReadTrace.Debug.Send("PLC Db udpate event called");
            try
            {
                if (_plc != null && _plc.IsConnected)
                {
                    _plc.ReadClass(_db1, 1);
                }
            }
            catch (Exception ex)
            {
                _plcReadTrace.SendValue("Error in UpdateDBs", ex);
            }
        }

        void _db1_DataChanged(object sender, DataChangedEventArgs e)
        {
            LogAsync("PLC Datachange: " + e.PropertyName + " :: " + e.OldValue + " --> " + e.NewValue);

            #region Checking Sensor

            lblTareWeight.InvokeEx(_ =>
            {

                if (e.PropertyName == "X_0_0_Weight_1_Entry_Sensor")
                {
                    if (e.NewValue == true)
                    {
                        picFirstSensor.BackColor = Color.Red;
                        _weightToken.Cancel();
                        _timerToken.Cancel();
                        _isWeighingAlreadyInProgress = false;
                    }
                    else
                    {
                        picFirstSensor.BackColor = Color.Green;
                    }
                }
                else if (e.PropertyName == "X_0_1_Weight_1_Exitt_Sensor")
                {
                    if (e.NewValue == true)
                    {
                        picSecondSensor.BackColor = Color.Red;
                        if (_weighttakingprocess == false)
                        {
                            _weightToken.Cancel();
                            _timerToken.Cancel();
                            _isWeighingAlreadyInProgress = false;
                        }
                    }
                    else
                    {
                        picSecondSensor.BackColor = Color.Green;
                    }
                }
            });

            #endregion

            #region Station 1


            if (e.PropertyName == "X_0_0_Weight_1_Entry_Sensor")
            {
                if (e.NewValue == true)
                {
                    TrafficLightStatusChange(false, true);
                }
                else
                {

                }
            }
            else if (e.PropertyName == "X_0_1_Weight_1_Exitt_Sensor")
            {
                if (e.NewValue == true)
                {
                    _goingOutStarted = true;
                }
                else
                {
                    #region False
                    if (_isWeighingAlreadyInProgress && _goingOutStarted)
                    {
                        _trafficLight = new CancellationTokenSource();
                        PeriodicTaskFactory.Start(() =>
                        {
                            TrafficLightStatusChange(true, false);
                            ResetLogicStatus();
                        }, maxIterations: 1, delayInMilliseconds: Traffic_Light_CloseDelay * 1000, cancelToken: _trafficLight.Token);
                    }
                    else
                    {
                        //LogAsync("Invalid logic. Required Logic: _isWeighingAlreadyInProgress && _goingOutStarted. Current: " + _isWeighingAlreadyInProgress + _goingOutStarted, ListLogErrorLevels.Critical);
                    }
                    #endregion
                }
            }


            #endregion
        }

        #endregion

        bool _rfidValidScanDone = false;
        bool _isWeighingAlreadyInProgress = false;
        bool _goingOutStarted = false;

        public void RestScanningProperLogic()
        {
            _rfidValidScanDone = false;
            _isWeighingAlreadyInProgress = false;
            _goingOutStarted = false;
        }
        DateTime _lastGrossWtReceivedDateTime = DateTime.Now;
        // bool _isWeightOk = false;
        private void ScanForProperLogic()
        {
            try
            {
                if (lblGrossWeight.Text != string.Empty)
                {
                    //if ((DateTime.Now - _lastGrossWtReceivedDateTime).TotalSeconds > 10)
                    //{
                    //    _isWeightOk = false;
                    //    lblGrossWeight.Text = "00";
                    //    _lastGrossWtReceivedDateTime = DateTime.Now;
                    //    LogAsync("Weight not received in last 10 seconds");
                    //    _error.CONNECTION_NAME(_connection);
                    //    _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "ScanForProperLogic", "Weight not received", "Weight not received in last 10 seconds", "High", "WB1 User");
                    //    //send error
                    //}
                    //else
                    //{
                    //    _isWeightOk = true;
                    //}

                    if (_rfidValidScanDone
                                       && !_db1.X_0_0_Weight_1_Entry_Sensor
                                       && !_db1.X_0_1_Weight_1_Exitt_Sensor
                                       && decimal.Parse(lblGrossWeight.Text) > Helper.TruckMinWt * 1000
                                       && !_isWeighingAlreadyInProgress)
                    {
                        _isWeighingAlreadyInProgress = true;
                        //START WEIGHING AFTER TIME DELAY
                        _weightToken = new CancellationTokenSource();
                        PeriodicTaskFactory.Start(() =>
                        {
                            Take_Weight();
                        }, maxIterations: 1, delayInMilliseconds: Start_Weighing_Delay * 1000, cancelToken: _weightToken.Token);
                        //UPDATE TIMER

                        int dis = Start_Weighing_Delay;
                        _timerToken = new CancellationTokenSource();
                        PeriodicTaskFactory.Start(() =>
                        {
                            dis--;
                            lblAutoSave.InvokeEx(_ =>
                            {
                                lblAutoSave.Text = dis.ToString();
                                if (lblTruckNumber.Text != string.Empty)
                                {
                                    decimal _grossWt = Convert.ToDecimal(lblGrossWeight.Text);
                                    lblGrossWeight.Text = _grossWt.ToString();
                                    decimal _netWt = Convert.ToDecimal(lblGrossWeight.Text) - Convert.ToDecimal(lblTareWeight.Text);
                                    lblNetWeight.Text = _netWt.ToString();
                                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, lblGrossWeight.Text);
                                }
                            });
                            LogAsync("Count down:" + dis);
                        },
                        maxIterations: Start_Weighing_Delay, intervalInMilliseconds: 1000, cancelToken: _timerToken.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "ScanForProperLogic", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        #region Traffic Light
        private void TrafficLightStatusChange(bool greenLight, bool redLight)
        {

            LogAsync("Traffic Light Green:" + greenLight + ". Red:" + redLight, ListLogErrorLevels.Critical);

            if (!_db1.X_0_1_Weight_1_Exitt_Sensor)
            {
                //LogAsync("Traffic Light Red close event is called when Sensor 2 is ON. Event Aborted!", ListLogErrorLevels.Critical);
            }
            else
            {
                LogAsync("Traffic Light Red on", ListLogErrorLevels.Critical);
                pcLEDSignFullRed.InvokeEx(_ =>
                {
                    LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.CompleteGreen);
                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Constant, "");
                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, "");
                });
            }
        }
        #endregion

        #region Weight Update
        private void updateWeightToDB()
        {
            try
            {

                if (lblTruckNumber.Text != string.Empty)
                {
                    if (Convert.ToDecimal(lblGrossWeight.Text) < Helper.TruckMinWt * 1000)
                    {
                        lblMessage.Text = "Truck Weight " + (lblGrossWeight.Text) + " is less than the Minimum Weight.";
                        lblMessage.ForeColor = Color.Red;
                        return;
                    }
                    else if (Convert.ToDecimal(lblGrossWeight.Text) > Helper.TruckMaxWt * 1000)
                    {
                        lblMessage.Text = "Truck Weight " + (lblGrossWeight.Text) + " is more than the Maximum Weight.";
                        lblMessage.ForeColor = Color.Red;
                        return;
                    }
                    else
                    {
                        Save();
                    }
                }

            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message.ToString();
                lblMessage.ForeColor = Color.Red;
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "updateWeightToDB", ex.Message.ToString(), "High", Helper.UserName);
            }
        }
        #endregion

        #region Take Weight
        private void Take_Weight()
        {
            LogAsync("Weight Taken for Station Weight Recorded = " + lblGrossWeight.Text);
            lblGrossWeight.InvokeEx(_ =>
            {
                updateWeightToDB();
            });
            //_plc.Write("DB1.DBX0.6", true);
            //TrafficLightStatusChange(true, false);
            LogAsync("Traffic Light on", ListLogErrorLevels.Critical);
            pcLEDSignOk.InvokeEx(_ =>
            {
                pcLEDSignFullGreen.Visible = false;
                pcLEDSignFullRed.Visible = false;
                pcLEDSignOk.Visible = true;
                pcLEDSignCross.Visible = false;
                LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.GreenTickMark);
                Thread.Sleep(2000);
                LedDisplaysList.SendToSiren(LedHelper.Siran.TenSeconds);
            });

        }
        #endregion

        #region Reset Logic
        private void ResetLogicStatus()
        {
            try
            {
                //newly written
                _isWeighingAlreadyInProgress = false;
                _weighttakingprocess = false;
                _goingOutStarted = false;
                _rfidValidScanDone = false;
                _weightToken.Cancel();
                _timerToken.Cancel();
                _trafficLight.Cancel();
                Invoke(new Action(() =>
                {
                    //thI Clear();
                    lblMessage.Text = string.Empty;

                    lblMessage.ForeColor = Color.Black;
                    pcLEDSignFullGreen.Visible = true;
                    pcLEDSignFullRed.Visible = false;
                    pcLEDSignOk.Visible = false;
                    pcLEDSignCross.Visible = false;
                    lblGrossWeight.Text = "0";

                    LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.CompleteGreen);
                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Constant, "");
                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, "");

                }));
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message.ToString();
                lblMessage.ForeColor = Color.Red;
            }
        }
        #endregion

        private void pbBack_Click(object sender, EventArgs e)
        {
            this.Close();
            _Main.PanelVisuable(true);
        }

        private void Clear()
        {
            try
            {
                lblAutoSave.Text = Helper.AutoSaveInterval.ToString();
                _Main._reader.UpdateReaderReadValue = string.Empty;
                txtRFIDTagValue.Text = string.Empty;
                picFirstSensor.BackColor = Color.Green;
                picSecondSensor.BackColor = Color.Green;
                picWeighBridge.Visible = false;
                lblMessage.Text = string.Empty;
                lblMessage.ForeColor = Color.Black;
                GrossWeightClear();
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message.ToString();
                lblMessage.ForeColor = Color.Red;

            }
        }

        private void GrossWeightClear()
        {
            lblTruckID.Text = string.Empty;
            lblTruckNumber.Text = string.Empty;
            lblTareWeight.Text = string.Empty;
            lblNetWeight.Text = string.Empty;
            lblMineNumber.Text = string.Empty;
            lblMaterial.Text = string.Empty;
            lblGrossWeight.Text = "0";
        }

        public bool GetTruckNumber()
        {
            try
            {
                #region Check Truck
                DataTable _dtTruck = DBLayer.IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS("", txtRFIDTagValue.Text, "", "RFID");
                if (_dtTruck.Rows.Count > 0)
                {
                    #region Truck Gross Weight to Gross weight Time Diffrence
                    DataTable dtTimeDiff = DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS("", _dtTruck.Rows[0]["TruckID"].ToString(), "", "", "", "TRUCKLASTGROSSWEIGHT");
                    if (dtTimeDiff.Rows.Count > 0)
                    {
                        if (int.Parse(dtTimeDiff.Rows[0]["DiffTime"].ToString()) > Helper.TruckGrossWeightThreshold)
                        {
                            #region Tare Weight Capatcher or not
                            DataTable _dtTare = DBLayer.IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(_dtTruck.Rows[0]["TruckID"].ToString(), "", "", "TRUCKID");
                            if (_dtTare.Rows.Count > 0)
                            {
                                #region Tare Weight days Frequency
                                DataTable _dtTruckNo = DBLayer.IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS("", txtRFIDTagValue.Text, "", "TAREWEIGHTDAYDIFFERENCE");
                                if (_dtTruckNo.Rows.Count > 0)
                                {
                                    int _daysDiff = Helper.TareWeightFrequencedays;
                                    if (Convert.ToInt32(_dtTruckNo.Rows[0]["DAYDIFF"]) <= _daysDiff)
                                    {
                                        if (_dtTruck.Rows[0]["Mine"].ToString() != "")
                                        {
                                            lblTruckID.Text = _dtTruck.Rows[0]["TruckID"].ToString();
                                            lblMineNumber.Text = _dtTruck.Rows[0]["Mine"].ToString();
                                            lblTruckNumber.Text = _dtTruck.Rows[0]["TruckNumber"].ToString();
                                            lblTareWeight.Text = _dtTruckNo.Rows[0]["TareWeight"].ToString();
                                            lblMaterial.Text = _dtTruck.Rows[0]["Material"].ToString();
                                            LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.CompleteRed);
                                            LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Constant, lblTruckID.Text);
                                            pcLEDSignFullGreen.Visible = false;
                                            pcLEDSignFullRed.Visible = true;
                                            pcLEDSignOk.Visible = false;
                                            pcLEDSignCross.Visible = false;
                                            return true;
                                        }
                                        else
                                        {
                                            lblMessage.Text = "Truck not associated.";
                                            lblMessage.ForeColor = Color.Red;
                                            _error.CONNECTION_NAME(_connection);
                                            _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber", "Truck No : " + _dtTruck.Rows[0]["TruckNumber"].ToString() + " not associated.", "High", Helper.UserName);
                                            _Main._reader.UpdateReaderReadValue = string.Empty;
                                            txtRFIDTagValue.Text = string.Empty;
                                            RestScanningProperLogic();
                                            Clear();
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        lblMessage.Text = "Truck Tare-Weight is not captured as per the Frequency : Truck ID " + _dtTruckNo.Rows[0]["TruckID"].ToString() + "  (" + _dtTruckNo.Rows[0]["DAYDIFF"].ToString() + " days left.)";
                                        lblMessage.ForeColor = Color.Red;
                                        _error.CONNECTION_NAME(_connection);
                                        _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber", "Truck Tare-Weight is not captured as per the Frequency : Truck ID " + _dtTruckNo.Rows[0]["TruckID"].ToString() + "", "High", Helper.UserName);
                                        _Main._reader.UpdateReaderReadValue = string.Empty;
                                        txtRFIDTagValue.Text = string.Empty;
                                        RestScanningProperLogic();
                                        Clear();
                                        return false;
                                    }
                                }
                                else
                                {
                                    lblMessage.Text = "Invalid Truck";
                                    lblMessage.ForeColor = Color.Red;
                                    _error.CONNECTION_NAME(_connection);
                                    _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber - spGetTruckNoandTareWeight", "Invalid Truck", "High", Helper.UserName);
                                    _Main._reader.UpdateReaderReadValue = string.Empty;
                                    txtRFIDTagValue.Text = string.Empty;
                                    RestScanningProperLogic();
                                    LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.RedCrossMark);
                                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Running, "Invalid Truck");
                                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, "");
                                    Clear();

                                    return false;
                                }
                                #endregion
                            }
                            else
                            {
                                lblMessage.Text = "Truck Tare-Weight is not captured. Please take Tare-Weight first.";
                                lblMessage.ForeColor = Color.Red;
                                _error.CONNECTION_NAME(_connection);
                                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber - spGetTruckTareWeight", "Truck Tare-Weight is not captured. Please take Tare-Weight first.", "High", Helper.UserName);
                                _Main._reader.UpdateReaderReadValue = string.Empty;
                                txtRFIDTagValue.Text = string.Empty;
                                RestScanningProperLogic();
                                Clear();
                                return false;
                            }
                            #endregion
                        }
                        else
                        {
                            lblMessage.Text = "Time span of " + Helper.TruckGrossWeightThreshold + " minutes.";
                            lblMessage.ForeColor = Color.Red;
                            _error.CONNECTION_NAME(_connection);
                            _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber - spGetTruckTareWeight", "Truck No : " + _dtTruck.Rows[0]["TruckNumber"].ToString() + " Time span of 10 minutes.", "High", Helper.UserName);
                            _Main._reader.UpdateReaderReadValue = string.Empty;
                            txtRFIDTagValue.Text = string.Empty;
                            RestScanningProperLogic();
                            Clear();
                            return false;
                        }
                    }
                    else
                    {
                        DataTable _dtTare = DBLayer.IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(_dtTruck.Rows[0]["TruckID"].ToString(), "", "", "TRUCKID");
                        if (_dtTare.Rows.Count > 0)
                        {
                            DataTable _dtTruckNo = DBLayer.IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS("", txtRFIDTagValue.Text, "", "TAREWEIGHTDAYDIFFERENCE");
                            if (_dtTruckNo.Rows.Count > 0)
                            {
                                int _daysDiff = Helper.TareWeightFrequencedays;
                                if (Convert.ToInt32(_dtTruckNo.Rows[0]["DAYDIFF"]) <= _daysDiff)
                                {
                                    if (_dtTruck.Rows[0]["Mine"].ToString() != "")
                                    {
                                        lblTruckID.Text = _dtTruck.Rows[0]["TruckID"].ToString();
                                        lblMineNumber.Text = _dtTruck.Rows[0]["Mine"].ToString();
                                        lblTruckNumber.Text = _dtTruck.Rows[0]["TruckNumber"].ToString();
                                        lblTareWeight.Text = _dtTruckNo.Rows[0]["TareWeight"].ToString();
                                        lblMaterial.Text = _dtTruck.Rows[0]["Material"].ToString();
                                        LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.CompleteRed);
                                        LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Constant, lblTruckID.Text);
                                        pcLEDSignFullGreen.Visible = false;
                                        pcLEDSignFullRed.Visible = true;
                                        pcLEDSignOk.Visible = false;
                                        pcLEDSignCross.Visible = false;
                                        return true;
                                    }
                                    else
                                    {
                                        lblMessage.Text = "Truck not associated.";
                                        lblMessage.ForeColor = Color.Red;
                                        _error.CONNECTION_NAME(_connection);
                                        _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber", "Truck No : " + _dtTruck.Rows[0]["TruckNumber"].ToString() + " not associated.", "High", Helper.UserName);
                                        _Main._reader.UpdateReaderReadValue = string.Empty;
                                        txtRFIDTagValue.Text = string.Empty;
                                        RestScanningProperLogic();
                                        Clear();
                                        LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.RedCrossMark);
                                        LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Running, "Truck not associated.");
                                        LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, "");
                                        return false;
                                    }
                                }
                                else
                                {
                                    lblMessage.Text = "Truck Tare-Weight is not captured as per the Frequency : Truck ID " + _dtTruckNo.Rows[0]["TruckID"].ToString() + "  (" + _dtTruckNo.Rows[0]["DAYDIFF"].ToString() + " days left.)";
                                    lblMessage.ForeColor = Color.Red;
                                    _error.CONNECTION_NAME(_connection);
                                    _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber", "Truck Tare-Weight is not captured as per the Frequency : Truck ID " + _dtTruckNo.Rows[0]["TruckID"].ToString() + "  (" + _dtTruckNo.Rows[0]["DAYDIFF"].ToString() + " days left.)", "High", Helper.UserName);
                                    _Main._reader.UpdateReaderReadValue = string.Empty;
                                    txtRFIDTagValue.Text = string.Empty;
                                    RestScanningProperLogic();
                                    Clear();
                                    return false;
                                }
                            }
                            else
                            {
                                lblMessage.Text = "Invalid Truck";
                                lblMessage.ForeColor = Color.Red;
                                _error.CONNECTION_NAME(_connection);
                                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber - spGetTruckNoandTareWeight", "Invalid Truck", "High", Helper.UserName);
                                _Main._reader.UpdateReaderReadValue = string.Empty;
                                txtRFIDTagValue.Text = string.Empty;
                                RestScanningProperLogic();
                                Clear();
                                LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.RedCrossMark);
                                LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Running, "Invalid Truck");
                                LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, "");
                                return false;
                            }
                        }
                        else
                        {
                            lblMessage.Text = "Truck Tare-Weight is not captured. Please take Tare-Weight first.";
                            lblMessage.ForeColor = Color.Red;
                            _error.CONNECTION_NAME(_connection);
                            _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber - spGetTruckTareWeight", "Truck No : " + _dtTruck.Rows[0]["TruckNumber"].ToString() + " Tare-Weight is not captured. Please take Tare-Weight first.", "High", Helper.UserName);
                            _Main._reader.UpdateReaderReadValue = string.Empty;
                            txtRFIDTagValue.Text = string.Empty;
                            RestScanningProperLogic();
                            Clear();
                            return false;
                        }
                    }
                    #endregion
                }
                else
                {
                    lblMessage.Text = "Invalid Truck";
                    lblMessage.ForeColor = Color.Red;
                    _error.CONNECTION_NAME(_connection);
                    _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber - spGetTruck", "Invalid Truck", "High", Helper.UserName);
                    _Main._reader.UpdateReaderReadValue = string.Empty;
                    txtRFIDTagValue.Text = string.Empty;
                    RestScanningProperLogic();
                    Clear();
                    LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.RedCrossMark);
                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Running, "Invalid Truck");
                    LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, "");
                    return false;
                }

                #endregion
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message.ToString();
                lblMessage.ForeColor = Color.Red;
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetTruckNumber", ex.Message.ToString(), "High", Helper.UserName);
                RestScanningProperLogic();
                Clear();
                return false;
            }
        }

        private void PopulatedLime()
        {
            Helper.MateriMovementType = "Lime Stone";
            Helper.MateriMovementCode = "L";
            lblWeighmentOperation.Text = "Weighment Operation : " + Helper.MateriMovementType;

            pbLimestone.Size = new System.Drawing.Size(275, 255);
            pbLimestone.Location = new System.Drawing.Point(95, 110);

            pbTareWeight.Size = new System.Drawing.Size(265, 230);
            pbTareWeight.Location = new System.Drawing.Point(396, 124);

        }

        private void PopulatedTare()
        {
            Helper.MateriMovementType = "Tare Wight";
            Helper.MateriMovementCode = "T";
            lblWeighmentOperation.Text = "Weighment Operation : " + Helper.MateriMovementType;

            pbLimestone.Size = new System.Drawing.Size(265, 230);
            pbLimestone.Location = new System.Drawing.Point(110, 122);

            pbTareWeight.Size = new System.Drawing.Size(275, 255);
            pbTareWeight.Location = new System.Drawing.Point(396, 110);

        }

        private void pbLimestone_Click(object sender, EventArgs e)
        {
            PopulatedLime();
        }

        frmTareWeight TareWeight;
        private void pbTareWeight_Click(object sender, EventArgs e)
        {
            //PopulatedTare();
            this.Close();
            if (TareWeight == null)
            {
                TareWeight = new frmTareWeight(_Main);
                TareWeight.FormClosed += new FormClosedEventHandler(TareWeight_FormClosed);
                TareWeight.MdiParent = _Main;
                this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                TareWeight.Dock = DockStyle.Fill;
                TareWeight.Show();
            }
            else
            {
                TareWeight.Activate();
            }
        }

        void TareWeight_FormClosed(object sender, FormClosedEventArgs e)
        {
            TareWeight = null;
        }

        private void frmMaterialMovement_Load(object sender, EventArgs e)
        {
            try
            {
                if (_Main._reader.ConnectFlag == 1)
                {
                    _Main._reader.NewTagValueReceived += _reader_NewTagValueReceived;
                }
                else
                {

                }

                if (Helper.AutoSave == true)
                {
                    pbSave.Visible = false;
                }
                else
                {
                    pbSave.Visible = true;
                }
                pcLEDSignFullGreen.Visible = true;
                pcLEDSignFullRed.Visible = false;
                pcLEDSignOk.Visible = false;
                pcLEDSignCross.Visible = false;
                lblAutoSave.Text = Helper.AutoSaveInterval.ToString();
                StartPlcProcess();
                StartLedProcess();
                PopulatedLime();
                PrintFormLoad();
                if (Helper.UserRole == "Super Admin")
                {
                    pbERPManualUpdate.Visible = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void PrintFormLoad()
        {
            try
            {
                LoadDatFile(@"printformat.dat");
                dpsDesignCtrl.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        void _reader_NewTagValueReceived(string arg1, string arg2)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    if (txtRFIDTagValue.Text == string.Empty)
                    {
                        txtRFIDTagValue.Text = arg1;
                        if (txtRFIDTagValue.Text != string.Empty)
                        {
                            if (pcLEDSignOk.Visible != true)
                            {
                                bool retVal = false;
                                txtRFIDTagValue.InvokeEx(__ =>
                                {
                                    retVal = GetTruckNumber();
                                });
                                if (retVal)
                                {
                                    picWeighBridge.Visible = true;
                                    pcLEDSignFullGreen.Visible = false;
                                    pcLEDSignFullRed.Visible = true;
                                    pcLEDSignOk.Visible = false;
                                    pcLEDSignCross.Visible = false;
                                    _rfidValidScanDone = true;
                                }
                                else
                                {
                                    LogAsync("GetTruckNumber Faild");
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "_reader_NewTagValueReceived", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private void pbCancel_Click(object sender, EventArgs e)
        {
            ResetLogicStatus();
            Clear();
            _Main._reader.UpdateReaderReadValue = "";
        }

        private readonly DragonPageDesignCtrl dpsDesignCtrl = new DragonPageDesignCtrl();

        public void LoadDatFile(string filename)
        {
            // load objects from a binary file
            _printTrace.Error.Send("Started");
            try
            {
                this.InvokeEx(_ =>
                {
                    Enabled = false;
                });

                #region Main Logic

                WelcomePage welcomePage;
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    BinaryFormatter myBinarySerializer = new BinaryFormatter();
                    welcomePage = (WelcomePage)myBinarySerializer.Deserialize(fs);
                }

                _printTrace.Error.Send("Deserialized");

                // for (int i = 0; i < welcomePage.PageParametersForSavingList.Count; i++)
                for (int i = 0; i < 1; i++)
                {
                    _printTrace.Error.Send("Page started");

                    dpsDesignCtrl.Name = welcomePage.PageParametersForSavingList[i].PageName;
                    dpsDesignCtrl.SetPrinterParameters(welcomePage.PageParametersForSavingList[i]);
                    _printTrace.Error.Send("Parameters1 set");
                    dpsDesignCtrl.designSurfaceCtrl.SetAllObjects(welcomePage.GraphicObjectCollectionList[i], welcomePage.DesignPageDisplayPropertiesForSavingList[i]);
                    dpsDesignCtrl.SetPrinterParameters(welcomePage.PageParametersForSavingList[i]);
                    _printTrace.Error.Send("Graphic Object Collection set");
                    _printTrace.Error.Send("Page completed");
                }

                this.InvokeEx(_ =>
                {
                    Enabled = true;
                    Activate();
                });
                _printTrace.Error.Send("completed");
                #endregion

            }
            catch (Exception ex)
            {
                throw new ApplicationException("File failed to load.", ex);
            }
        }

        public void PrintDatFile()
        {
            // load objects from a binary file
            _printTrace.Error.Send("Started");
            try
            {
                foreach (var dr in dpsDesignCtrl.designSurfaceCtrl.DrawingObjects)
                {
                    if (dr.GetType() == typeof(TextGraphic))
                    {
                        if (dr.DataLink == "VARDATE")
                        {
                            dr.DataLink = "";
                        }
                        if (dr.DataLink == "VARTIPPER")
                        {
                            dr.DataLink = "TIPPER";
                        }
                        if (dr.DataLink == "VARBYNAME")
                        {
                            dr.DataLink = cbByName.Text;
                        }
                        if (dr.DataLink == "VARCLARK")
                        {
                            dr.DataLink = "Clerk";
                        }
                        if (dr.DataLink == "VARVEHICLENO")
                        {
                            dr.DataLink = lblTruckNumber.Text;
                        }
                        if (dr.DataLink == "VARCRSOPT")
                        {
                            dr.DataLink = "Crusher Operator";
                        }
                        if (dr.DataLink == "VARTONAME")
                        {
                            dr.DataLink = cbToName.Text;
                        }
                        if (dr.DataLink == "VARADDRESS")
                        {
                            if (lblMineNumber.Text == "88")
                            {
                                dr.DataLink = "Mandadi RF\nKCP - Manadai Mine (34.98 Ha)\nCrusher Point\nPolepalli\nDurgi(Mdl)\nGuntur(Dist)";
                            }
                            if (lblMineNumber.Text == "90")
                            {
                                dr.DataLink = "Mandadi RF\nKCP - Deposit-II (24.32 Ha)\nCrusher Point\nPolepalli\nDurgi(Mdl)\nGuntur(Dist)";
                            }
                            if (lblMineNumber.Text == "91")
                            {
                                dr.DataLink = "Mandadi RF\nKCP - Deposit-I (24.32 Ha)\nCrusher Point\nPolepalli\nDurgi(Mdl)\nGuntur(Dist)";
                            }
                        }

                        if (dr.DataLink == "VARWEIGHT")
                        {
                            dr.DataLink = (decimal.Parse(lblNetWeight.Text) / 1000).ToString();
                        }
                        if (dr.DataLink == "VARPLACE")
                        {
                            dr.DataLink = "Macherla";
                        }
                        if (dr.DataLink == "VARPLACEDATE")
                        {
                            dr.DataLink = System.DateTime.Now.ToString("dd-MM-yyyy");
                        }

                        if (dr.DataLink == "VARISSUEDPERMITTIME")
                        {
                            dr.DataLink = System.DateTime.Now.ToString("HH:mm");
                        }
                        if (dr.DataLink == "VARISSUEDPERMIDATE")
                        {
                            dr.DataLink = System.DateTime.Now.ToString("dd-MM-yyyy");
                        }
                        if (dr.DataLink == "VARVALIDPERMITTIME")
                        {
                            dr.DataLink = System.DateTime.Now.AddMinutes(3).ToString("HH:mm");
                        }
                        if (dr.DataLink == "VARVALIDPERMIDATE")
                        {
                            dr.DataLink = System.DateTime.Now.ToString("dd-MM-yyyy");
                        }
                        if (dr.DataLink == "VARWEIGHTWORD")
                        {
                            dr.DataLink = _word;
                        }
                        if (dr.DataLink == "VARDESTINATION")
                        {
                            if (lblMineNumber.Text == "88")
                            {
                                dr.DataLink = "Compartment No : 82\nMandadi RF, Block-II\n\n\n\n\nThe K.C.P Limited\nMandadi\nVeldurthy\nGuntur\nMines to Crusher";
                            }
                            if (lblMineNumber.Text == "90")
                            {
                                dr.DataLink = "Compartment No : 82\nMandadi RF, Block-II\n\n\n\n\nThe K.C.P Limited\nPolepalli(V)\nDurgi(M)\nGuntur\nDeposit-II Mines to Crusher";
                            }
                            if (lblMineNumber.Text == "91")
                            {
                                dr.DataLink = "Compartment No : 82\nMandadi RF, Block-II\n\n\n\n\nThe K.C.P Limited\nMandadi(V)\nVeldurthy(M)\nGuntur(Dist)\nDeposit-I Mines to Crusher";
                            }
                        }
                    }
                }

                dpsDesignCtrl.PrintCurrentPage();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("File failed to load.", ex);
            }
        }

        #region Convert Value to WORD

        private static String ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }

        private static String tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }

        private static String ConvertWholeNumber(String Number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX  
                bool isDone = false;//test if already translated  
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))  
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric  
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping  
                    String place = "";//digit grouping name:hundres,thousand,etc...  
                    switch (numDigits)
                    {
                        case 1://ones' range  

                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range  
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range  
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range  
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range  
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range  
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...  
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)  
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros  
                        //if (beginsZero) word = " and " + word.Trim();  
                    }
                    //ignore digit grouping names  
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private static String ConvertToWords(String numb)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "", pointStr1 = "", _pointsFirst = "", _pointsSecond = "";
            String endStr = "";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    _pointsFirst = points.Substring(0, 1);
                    _pointsSecond = points.Substring(1, 2);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = "ton and ";// just to separate whole numbers from points/cents  
                        endStr = "kgs " + endStr;//Cents  
                        pointStr = ones(_pointsFirst);
                        pointStr1 = ConvertWholeNumber(_pointsSecond).Trim();
                    }
                }
                val = String.Format("{0} {1}{2} {3}", ConvertWholeNumber(wholeNo).Trim(), andStr, pointStr + " " + pointStr1, endStr);
                _word = val;
            }
            catch { }
            return val;
        }

        private static String ConvertDecimals(String number)
        {
            String cd = "", digit = "", engOne = "";
            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ones(digit);
                }
                cd += " " + engOne;
            }
            return cd;
        }

        #endregion

        private String GetShift()
        {
            return SQLHelper.Execute_Stored_Procedure("IPTS_SHIFTMASTER_GETDETAILS").Rows[0]["SHIFT"].ToString();
        }

        private void IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT(object TripID, object TruckID, object TransporterCode, object Transporter, object WeighBridgeNumber
            , object MaterialMovementCode, object TareWeight, object GrossWeight, object NetWeight, object UpdatedBy, object TruckNumber, object Shift, object MineNo)
        {
            try
            {
                DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT(TripID, TruckID, TransporterCode, Transporter, WeighBridgeNumber, MaterialMovementCode, TareWeight, GrossWeight, NetWeight, UpdatedBy, TruckNumber, Shift, MineNo);
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private void GROSSWEIGHT_INSERT_IN_ERP()
        {
            try
            {
                Ping _p = new Ping();
                PingReply _r;
                _r = _p.Send("192.168.13.26", 2000);
                if (_r.Status == IPStatus.Success)
                {
                    int _matCode = 0;
                    if (lblMaterial.Text == "OVERBURDEN")
                    {
                        _matCode = 95;
                    }
                    else
                    {
                        _matCode = 91;
                    }

                    richTextBox1.Text = OracleBLLayer.GetString(_matCode, int.Parse(lblMineNumber.Text), 8, lblTruckNumber.Text, lblGrossWeight.Text, lblTareWeight.Text, lblNetWeight.Text,
                        "0", "0", lblNetWeight.Text, Helper.UserName, GetShift(), 33, 1);
                    OracleBLLayer.InsertNewGrossWeight(_matCode, int.Parse(lblMineNumber.Text), 8, lblTruckNumber.Text, lblGrossWeight.Text, lblTareWeight.Text, lblNetWeight.Text,
                        "0", "0", lblNetWeight.Text, Helper.UserName, GetShift(), 33, 1);

                    DataTable dtID = DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS("", "", "", "", "", "ID");
                    if (dtID != null)
                    {
                        if (dtID.Rows.Count > 0)
                        {
                            DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATEFLAG(dtID.Rows[0]["ID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GROSSWEIGHT_INSERT_IN_ERP", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        bool _weighttakingprocess = false;
        private void Save()
        {
            try
            {
                if (lblNetWeight.Text == "")
                {
                    MessageBox.Show("Gross or net weight not captured.");
                    return;
                }

                if (lblTruckID.Text != string.Empty)
                {
                    DataTable dt = DBLayer.IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS(lblTruckID.Text, "", "", "TRUCKID");
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            string _materialCode = string.Empty;
                            if (lblMaterial.Text == "OVERBURDEN")
                            {
                                _materialCode = "OB";
                            }
                            else
                            {
                                _materialCode = "L";
                            }

                            IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT(lblMineNumber.Text, lblTruckID.Text, dt.Rows[0]["TransporterCode"].ToString(), dt.Rows[0]["Transporter"].ToString(), 1, _materialCode, lblTareWeight.Text, lblGrossWeight.Text, lblNetWeight.Text, Helper.UserName, lblTruckNumber.Text, GetShift(), lblMineNumber.Text);
                            GROSSWEIGHT_INSERT_IN_ERP();
                            lblMessage.Text = "Data saved success";
                            _weighttakingprocess = true;
                            if (chkInstantChallen.Checked == true)
                            {
                                try
                                {
                                    string number = (decimal.Parse(lblNetWeight.Text) / 1000).ToString();
                                    //number = Convert.ToDouble(number).ToString();

                                    if (number.Contains("-"))
                                    {
                                        //isNegative = "Minus ";
                                        number = number.Substring(1, number.Length - 1);
                                    }
                                    if (number == "0")
                                    {
                                    }
                                    else
                                    {
                                        ConvertToWords(number);
                                    }
                                    if (lblMineNumber.Text != "89")
                                    {
                                        for (int z = 0; z < 3; z++)
                                        {
                                            PrintDatFile();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                dpsDesignCtrl.Controls.Clear();
                                PrintFormLoad();
                            }
                            Clear();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Truck id or Weight empty");
                }
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "Save", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private void pbSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void frmMaterialMovement_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                disconnectPlc();
                StopLedProcess();
                StopWeighingProcess();

                if (_Main._reader.ConnectFlag == 1)
                {
                    _Main._reader.NewTagValueReceived -= _reader_NewTagValueReceived;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void tmrCheckWt_Tick(object sender, EventArgs e)
        {
            ScanForProperLogic();
        }

        private void pbChallanPrint_Click(object sender, EventArgs e)
        {
            try
            {
                frmChallanPrint _print = new frmChallanPrint();
                _print.ShowDialog();
            }
            catch (Exception ex)
            {

            }
        }

        //private async void pbERPManualUpdate_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //if (_Main._reader.ConnectFlag == 1)
        //        //{
        //        //    _Main._reader.NewTagValueReceived -= _reader_NewTagValueReceived;
        //        //}
        //        //ResetLogicStatus();
        //        //if (_Main._reader.ConnectFlag == 1)
        //        //{
        //        //    _Main._reader.NewTagValueReceived -= _reader_NewTagValueReceived;
        //        //}
        //        StopLedProcess();
        //        frmManualERPUpdate _manual = new frmManualERPUpdate();
        //        _manual.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}


        private async void pbERPManualUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                await AuthorizeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.Message);
            }
        }

        #region Show Manual
        private frmManualERPUpdate _frmManualErpUpdate = null;
        private async Task AuthorizeAsync()
        {
            await Task.Run((() =>
           {
               if (_frmManualErpUpdate != null)
               {
                   MessageBox.Show("Already a Manual update window is open.Please check");
                   return;
               }

               if (_frmManualErpUpdate == null)
               {
                   _frmManualErpUpdate = new frmManualERPUpdate();
                   _frmManualErpUpdate.FormClosing += FrmManualErpUpdateFormClosing;
               }
               _frmManualErpUpdate.ShowDialog();

           }));
        }
        private void FrmManualErpUpdateFormClosing(object sender, FormClosingEventArgs e)
        {
            _frmManualErpUpdate = null;
        }
        #endregion

        private void chkInstantChallen_CheckedChanged(object sender, EventArgs e)
        {
            if (chkInstantChallen.Checked == true)
            {
                pnlInstantNames.Visible = true;
                cbByName.Text = "P.Rammohan Rao";
                cbToName.Text = "M.Murali Krishna";
            }
            else
            {
                pnlInstantNames.Visible = false;
            }
        }

        private void btnInstatntOK_Click(object sender, EventArgs e)
        {
            if (cbByName.SelectedIndex == -1)
            {
                MessageBox.Show("Please select by issued name");
                return;
            }
            if (cbToName.SelectedIndex == -1)
            {
                MessageBox.Show("Please select to issued name");
                return;
            }
            pnlInstantNames.Visible = false;
        }
    }

    #region Support Class
    public static class RefrelctionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field.GetValue(obj);
        }

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }

    public enum WeighingLogicSteps
    {
        FRESH_00 = 0,
        RFID_CLERANCE_05 = 05,
        FIRST_SENSOR_ON_10 = 10,
        FIRST_SENSOR_OFF_20 = 20,
        WEIGHING_IN_PROCESS_30 = 30,
        WEIGHING_OVER_40 = 40,
        BOOM_ON_50 = 50,
        SECOND_SENSOR_ON_60 = 60,
        SECOND_SENSOR_OFF_70 = 70,
        BOOM_OFF_80 = 80,
    }

    public class WeighingLogic
    {
        public WeighingLogicSteps CurrentStep = WeighingLogicSteps.FRESH_00;
        public event Action<WeighingLogicSteps, WeighingLogicSteps> LogicChanged;
        public void MoveToNextStep()
        {
            WeighingLogicSteps _rt = CurrentStep;
            CurrentStep = CurrentStep.Next();
            if (LogicChanged != null)
                LogicChanged(_rt, CurrentStep);
        }
        public void LogicReset()
        {
            CurrentStep = WeighingLogicSteps.FRESH_00;
        }
    }

    #endregion
}
