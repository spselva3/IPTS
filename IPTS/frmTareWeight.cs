using DragonFactory;
using DragonFactory.TcpIPClient;
using DragonFactory.UserContols;
using DragonFactory.Utilities;
using LMS;
using S7.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TraceTool;

namespace IPTS
{
    public partial class frmTareWeight : Form
    {
        ErrorLogs _error = new ErrorLogs();
        public string _connection = ConfigurationManager.ConnectionStrings["ConStrn"].ConnectionString;
        SerialPortCommunication WeighBridge;
        String WeighingScaleData = string.Empty;

        frmMain _Main;
        public frmTareWeight(frmMain Main)
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

        //int noOfRfidReaders = 1;
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
                        txtNewTareWeight.Text = regexObj.Replace(Data.Substring(0, Data.IndexOf("G")).Trim(), "");
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

        private void WeightCaptcher()
        {
            try
            {
                txtNewTareWeight.Text = string.Empty;

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
                txtNewTareWeight.Text = regexObj.Replace(Data.Substring(0, Data.IndexOf("G")).Trim(), "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Weighing Process
        List<ICustomClient> _weight1 = new List<ICustomClient>();
        List<string> CurrentWeight = new List<string>() { "", "", "" };
        static Regex _wtRegex = new Regex(@"\u0002 (\d+)\u0003");
        string _strWt = "";
        string _finalWt = "";
        private void StartWeighingProcess()
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
                            if (_strWt.IndexOf("G") > 0)
                            {
                                Regex regexObj = new Regex(@"[^\d]");
                                _finalWt = regexObj.Replace(_strWt.Substring(0, _strWt.IndexOf("G")).Trim(), "");
                                txtNewTareWeight.Invoke((Action)(() => txtNewTareWeight.Text = _finalWt));
                                _strWt = "";
                            }
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

        private void StopWeighingProcess()
        {
            for (int i = 0; i < noOfWeighing; i++)
            {
                _weight1[0].Disconnect();
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
                                                        //**************Hardware status end***************

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

        #region Time Delays
        int Start_Weighing_Delay = Helper.AutoSaveInterval;
        int Traffic_Light_CloseDelay = 5;
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
                            //_plc.Write("DB1.DBX0.6", true);
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
                                //pbPLCConnected.Visible = true;
                                //pbPLCDisconnected.Visible = false;
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
                    //pbPLCDisconnected.Visible = true;
                    //pbPLCConnected.Visible = false;
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

            txtNewTareWeight.InvokeEx(_ =>
            {

                if (e.PropertyName == "X_0_0_Weight_1_Entry_Sensor")
                {

                    if (e.NewValue == true)
                    {
                        picFirstSensor.BackColor = Color.Red;
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
                        LogAsync("Invalid logic. Required Logic: _isWeighingAlreadyInProgress && _goingOutStarted. Current: " + _isWeighingAlreadyInProgress + _goingOutStarted, ListLogErrorLevels.Critical);
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

        private void ScanForProperLogic()
        {
            if (_rfidValidScanDone
                               && !_db1.X_0_0_Weight_1_Entry_Sensor
                               && !_db1.X_0_1_Weight_1_Exitt_Sensor
                               && decimal.Parse(_finalWt) > Helper.TruckMinWt * 1000
                               && !_isWeighingAlreadyInProgress)
            {
                _isWeighingAlreadyInProgress = true;
                //START WEIGHING AFTER TIME DELAY
                _weightToken = new CancellationTokenSource();
                PeriodicTaskFactory.Start(() =>
                {
                    Take_Weight();
                }, maxIterations: 1, delayInMilliseconds: Start_Weighing_Delay * 1000, cancelToken: _weightToken.Token);
                //WeighingLogicList[0].MoveToNextStep();
                //UPDATE TIMER
                int dis = Helper.AutoSaveInterval;
                _timerToken = new CancellationTokenSource();
                PeriodicTaskFactory.Start(() =>
                {
                    txtTruckID.InvokeEx(_ =>
                    {
                        if (txtTruckID.Text != string.Empty)
                        {
                            decimal _newTareWt = Convert.ToDecimal(_finalWt);
                            txtNewTareWeight.Text = _newTareWt.ToString();
                        }
                    });
                    LogAsync("Count down:" + dis);
                },
                maxIterations: Start_Weighing_Delay, intervalInMilliseconds: 1000, cancelToken: _timerToken.Token);
            }
        }

        #region Traffic Light
        private void TrafficLightStatusChange(bool greenLight, bool redLight)
        {

            _plc.Write("DB1.DBX0.6", greenLight);  //6 is gree 
            _plc.Write("DB1.DBX0.7", redLight);//7 is red

            LogAsync("Traffic Light Green:" + greenLight + ". Red:" + redLight, ListLogErrorLevels.Critical);

            if (!_db1.X_0_1_Weight_1_Exitt_Sensor)
            {
                //LogAsync("Traffic Light Red close event is called when Sensor 2 is ON. Event Aborted!", ListLogErrorLevels.Critical);
            }
            else
            {
                LogAsync("Traffic Light Red on", ListLogErrorLevels.Critical);
            }
        }
        #endregion

        #region Weight Update
        private void updateWeightToDB()
        {
            try
            {

                if (txtTruckID.Text != string.Empty)
                {
                    if (Convert.ToDecimal(txtNewTareWeight.Text) < Helper.TruckMinWt * 1000)
                    {
                        lblMessage.Text = "Truck Weight " + (txtNewTareWeight.Text) + " is less than the Minimum Weight.";
                        lblMessage.ForeColor = Color.Red;
                        return;
                    }
                    else if (Convert.ToDecimal(txtNewTareWeight.Text) > Helper.TruckMaxWt * 1000)
                    {
                        lblMessage.Text = "Truck Weight " + (txtNewTareWeight.Text) + " is more than the Maximum Weight.";
                        lblMessage.ForeColor = Color.Red;
                        return;
                    }
                    else
                    {
                        if (ckAutoSave.Checked == true)
                        {
                            Save();
                        }
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

            LogAsync("Weight Taken for Station Weight Recorded = " + txtNewTareWeight.Text);
            txtNewTareWeight.InvokeEx(_ =>
            {
                updateWeightToDB();
            });
            LogAsync("Traffic Light on", ListLogErrorLevels.Critical);
        }
        #endregion

        #region Reset Logic
        private void ResetLogicStatus()
        {
            try
            {
                //newly written
                _isWeighingAlreadyInProgress = false;
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

        private void SetFontAndColors()
        {
            dgvTruckTareWeight.EnableHeadersVisualStyles = false;
            dgvTruckTareWeight.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Calibri", 13, FontStyle.Bold);//Century Gothic
            dgvTruckTareWeight.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvTruckTareWeight.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvTruckTareWeight.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            this.dgvTruckTareWeight.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black;
            this.dgvTruckTareWeight.DefaultCellStyle.Font = new System.Drawing.Font("Calibri", 12);
            this.dgvTruckTareWeight.DefaultCellStyle.ForeColor = Color.Black;
            this.dgvTruckTareWeight.DefaultCellStyle.BackColor = Color.White;
            this.dgvTruckTareWeight.DefaultCellStyle.SelectionForeColor = Color.Black;
            this.dgvTruckTareWeight.DefaultCellStyle.SelectionBackColor = Color.White;//FromArgb(41, 128, 185);
        }

        private void IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(object TruckID, object RFIDTagID, object TruckNumber, object STATUS)
        {
            DataTable dt = DBLayer.IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(TruckID, RFIDTagID, TruckNumber, STATUS);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    if (STATUS.ToString() == "ALL")
                    {
                        dgvTruckTareWeight.DataSource = dt;
                        SetFontAndColors();
                    }
                    if (STATUS.ToString() == "TRUCKIDLASTTAREWEIGHT")
                    {
                        txtPreviousTareWeight.Text = dt.Rows[0]["TareWeight"].ToString();
                    }
                    if (STATUS.ToString() == "TRUCKID")
                    {
                        dgvTruckTareWeight.DataSource = dt;
                        SetFontAndColors();
                    }
                    if (STATUS.ToString() == "RFID")
                    {
                        if (txtRFIDValue.Text != string.Empty)
                        {
                            txtMake.Text = dt.Rows[0]["Make"].ToString();
                            txtModel.Text = dt.Rows[0]["Model"].ToString();
                            txtTransporter.Text = dt.Rows[0]["Transporter"].ToString();
                            txtTransportCOde.Text = dt.Rows[0]["TransporterCode"].ToString();
                            txtTruckNumber.Text = dt.Rows[0]["TruckNumber"].ToString();
                            txtTruckID.Text = dt.Rows[0]["TruckID"].ToString();
                            IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(txtTruckID.Text, "", "", "TRUCKIDLASTTAREWEIGHT");
                            int _regActive;
                            _regActive = Convert.ToInt32(dt.Rows[0]["IsActive"]);
                            if (_regActive == 1)
                            {
                                ckStatus.Checked = true;
                                txtStatus.Text = "Active";
                            }
                            else if (_regActive == 0)
                            {
                                ckStatus.Checked = false;
                                txtStatus.Text = "In Active";
                            }

                        }
                    }
                }
            }
        }

        private void IPTS_TRUCKTAREWEIGHTDETAILS_INSERT(object RFIDTagID, object TruckID, object Transporter, object TransporterCode,
    object TareWeight, object PreviousTareWeight, object UpdatedBy, object TruckNumber, object Remarks)
        {
            try
            {
                if (txtTruckID.Text != string.Empty && txtNewTareWeight.Text != string.Empty)
                {
                    DBLayer.IPTS_TRUCKTAREWEIGHTDETAILS_INSERT(RFIDTagID, TruckID, Transporter, TransporterCode, TareWeight, PreviousTareWeight, UpdatedBy, TruckNumber, Remarks);
                    lblMessage.Text = "Tare Update successfully.!";
                }
                else
                {
                    MessageBox.Show("Truck id or Weight are empty");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Save()
        {
            try
            {
                IPTS_TRUCKTAREWEIGHTDETAILS_INSERT(txtRFIDValue.Text.Trim(), txtTruckID.Text.Trim(), txtTransporter.Text.Trim(), txtTransportCOde.Text.Trim(), txtNewTareWeight.Text.Trim(), txtPreviousTareWeight.Text.Trim(), Helper.UserName, txtTruckNumber.Text.Trim(), txtRemarks.Text);
                try
                {
                    decimal _tareweight = 0;
                    _tareweight = decimal.Parse(txtNewTareWeight.Text);
                    OracleBLLayer.UpdateLastUpdatedWeight(txtTruckNumber.Text, Helper.UserName);
                    OracleBLLayer.UpdateNewTareWeight(txtTransportCOde.Text.Trim(), txtTruckID.Text.Trim(), txtTruckNumber.Text, _tareweight.ToString(), Helper.UserName, "33");
                    lblMessage.Text = "Truck Tare Weight Updated In ERP.";
                    lblMessage.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(txtTruckID.Text, "", "", "TRUCKID");
                Clear();
            }
            catch (Exception ex)
            {

            }
        }

        private void pbSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Clear()
        {
            txtMake.Text = string.Empty;
            txtModel.Text = string.Empty;
            txtNewTareWeight.Text = string.Empty;
            txtPreviousTareWeight.Text = string.Empty;
            txtRemarks.Text = string.Empty;
            txtRFIDValue.Text = string.Empty;
            txtTransportCOde.Text = string.Empty;
            txtTransporter.Text = string.Empty;
            txtTruckID.Text = string.Empty;
            txtTruckNumber.Text = string.Empty;
            ckStatus.Checked = false;
            txtStatus.Text = string.Empty;
            //_Main._reader.UpdateReaderReadValue = string.Empty;
            _Main._reader.UpdateReaderReadValue = string.Empty;
        }

        private void pbCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void frmTareWeight_Load(object sender, EventArgs e)
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
                StartPlcProcess();
                StartLedProcess();
            }
            catch (Exception ex)
            {

            }
        }

        void _reader_NewTagValueReceived(string arg1, string arg2)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    if (txtRFIDValue.Text == string.Empty)
                    {
                        txtRFIDValue.Text = arg1;
                        IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS("", txtRFIDValue.Text.Trim(), "", "RFID");
                        IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(txtTruckID.Text, "", "", "TRUCKID");
                        _rfidValidScanDone = true;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void dgvTruckTareWeight_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgvTruckTareWeight.Rows[e.RowIndex];
                txtTruckID.Text = row.Cells["TruckID"].Value.ToString();
                txtTruckID.Enabled = false;
                IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(txtTruckID.Text.Trim(), "", "", "TRUCKID");
            }
        }

        private void frmTareWeight_FormClosing(object sender, FormClosingEventArgs e)
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

        private void frmTareWeight_Shown(object sender, EventArgs e)
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

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
    }


}
