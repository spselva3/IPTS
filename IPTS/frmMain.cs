using DragonFactory.TcpIPClient;
using LMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPTS
{
    public partial class frmMain : Form
    {
        ErrorLogs _error = new ErrorLogs();
        public string _connection = ConfigurationManager.ConnectionStrings["ConStrn"].ConnectionString;
        public RfidHelper _reader = new RfidHelper();

        public frmMain()
        {
            InitializeComponent();
            timer1.Start();
        }

        #region Check Ping, Connect For RFID

        public void Connecting_Readers(string IPAddress, string Port)
        {
            try
            {
                _reader = new RfidHelper();
                Ping _pingreader = new Ping();
                PingReply _replyreader;
                _replyreader = _pingreader.Send(IPAddress, 500);
                if (_replyreader.Status == IPStatus.Success)
                {
                    _reader.StartConnection(IPAddress, Port);
                    if (_reader.ConnectFlag == 1)
                    {
                        _reader.StartIdendifying();
                        picRFIDGreen.Visible = true;
                        picRFIDRed.Visible = false;
                    }
                }
                else
                {
                    //MessageBox.Show("" + IPAddress + " Reader not connected.!\n\nPlease Check");
                    picRFIDGreen.Visible = false;
                    picRFIDRed.Visible = true;
                    _error.CONNECTION_NAME(_connection);
                    _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "frmMain : Connecting_Readers", "Reader check ping status", "" + IPAddress + " Reader not connected.!\n\nPlease Check", "High", Helper.UserName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        #endregion

        private void pbLogout_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void PanelVisuable(bool _view)
        {
            pnlMenu.Visible = _view;
        }

        frmMaterialMovement MaterialMovement;
        private void pbMaterialMovement_Click(object sender, EventArgs e)
        {
            pnlMenu.Visible = false;
            if (TareWeight != null)
            {
                TareWeight.Close();
            }
            if (Configuration != null)
            {
                Configuration.Close();
            }
            if (Settings != null)
            {
                Settings.Close();
            }
            if (Registration != null)
            {
                Registration.Close();
            }
            if (MaterialMovement == null)
            {
                MaterialMovement = new frmMaterialMovement(this);
                MaterialMovement.FormClosed += new FormClosedEventHandler(MaterialMovement_FormClosed);
                MaterialMovement.MdiParent = this;
                this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                MaterialMovement.Dock = DockStyle.Fill;
                MaterialMovement.Show();
            }
            else
            {
                MaterialMovement.Activate();
            }
        }

        void MaterialMovement_FormClosed(object sender, FormClosedEventArgs e)
        {
            MaterialMovement = null;
        }

        frmTareWeight TareWeight;
        private void pbTareWeight_Click(object sender, EventArgs e)
        {
            pnlMenu.Visible = false;
            if (Configuration != null)
            {
                Configuration.Close();
            }
            if (MaterialMovement != null)
            {
                MaterialMovement.Close();
            }
            if (Settings != null)
            {
                Settings.Close();
            }
            if (Registration != null)
            {
                Registration.Close();
            }
            if (TareWeight == null)
            {
                TareWeight = new frmTareWeight(this);
                TareWeight.FormClosed += new FormClosedEventHandler(TareWeight_FormClosed);
                TareWeight.MdiParent = this;
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

        MainForm Configuration;
        private void pbConfiguration_Click(object sender, EventArgs e)
        {
            pnlMenu.Visible = false;
            try
            {
                if (_reader.ConnectFlag == 1)
                {
                    _reader.StopIdentifying();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            if (MaterialMovement != null)
            {
                MaterialMovement.Close();
            }
            if (TareWeight != null)
            {
                TareWeight.Close();
            }
            if (Settings != null)
            {
                Settings.Close();
            }
            if (Registration != null)
            {
                Registration.Close();
            }
            if (Configuration == null)
            {
                Configuration = new MainForm(this);
                Configuration.FormClosed += new FormClosedEventHandler(Configuration_FormClosed);
                Configuration.MdiParent = this;
                this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                Configuration.Dock = DockStyle.Fill;
                Configuration.Show();
            }
            else
            {
                Configuration.Activate();
            }
        }

        void Configuration_FormClosed(object sender, FormClosedEventArgs e)
        {
            Configuration = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            this.lblDatetime.Text = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
        }

        private void IPTS_HARDWARECONFIGURATION_GETDETAILS()
        {
            try
            {
                DataTable _dtHardware = DBLayer.IPTS_HARDWARECONFIGURATION_GETDETAILS();
                if (_dtHardware != null)
                {
                    if (_dtHardware.Rows.Count > 0)
                    {
                        Helper.PLCIP = _dtHardware.Rows[0]["PLCIPAddress"].ToString();
                        Helper.PLCPort = Convert.ToInt32(_dtHardware.Rows[0]["PLCSlot"]);
                        Helper.SensorPort1 = Convert.ToInt32(_dtHardware.Rows[0]["SensorPort1"]);
                        Helper.SensorPort2 = Convert.ToInt32(_dtHardware.Rows[0]["SensorPort2"]);
                        Helper.TrafficLightPortGreen = Convert.ToInt32(_dtHardware.Rows[0]["TrafficLightPortGreen"]);
                        Helper.TrafficLightPortRed = Convert.ToInt32(_dtHardware.Rows[0]["TrafficLightPortRed"]);
                        Helper.HooterPort = Convert.ToInt32(_dtHardware.Rows[0]["HooterPort"]);
                        Helper.RFIDReaderIP = _dtHardware.Rows[0]["RFIDReaderIP"].ToString();
                        Helper.RFIDReaderPort = Convert.ToInt32(_dtHardware.Rows[0]["RFIDReaderPort"]);
                        Helper.WeighBridgeIP = _dtHardware.Rows[0]["WeighBridgeIP"].ToString();
                        Helper.WeighBridgePort = Convert.ToInt32(_dtHardware.Rows[0]["WeighBridgePort"]);
                        Helper.TruckGrossWeightThreshold = Convert.ToInt32(_dtHardware.Rows[0]["TruckGrossWeightThreshold"]);
                        Helper.TareWeightFrequencedays = Convert.ToInt32(_dtHardware.Rows[0]["TruckTareWeightFrequency"]);
                        Helper.AutoSaveInterval = Convert.ToInt32(_dtHardware.Rows[0]["AutoSaveInterval"]);
                        Helper.TruckMinWt = Convert.ToDecimal(_dtHardware.Rows[0]["TruckMinWt"]);
                        Helper.TruckMaxWt = Convert.ToDecimal(_dtHardware.Rows[0]["TruckMaxWt"]);
                        Helper.AutoSave = Convert.ToBoolean(_dtHardware.Rows[0]["AutoSaveEnable"]);
                        Helper.DisplayIP = _dtHardware.Rows[0]["LedDisplay"].ToString();
                        Helper.DisplayPort = Convert.ToInt32(_dtHardware.Rows[0]["LedDisplayPort"]);
                    }
                }
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "GetHardwareDetails", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            lblVersion.Text = "Version " + fvi.FileVersion;


            IPTS_HARDWARECONFIGURATION_GETDETAILS();
            lblUser.Text = "User : " + Helper.UserName;
            try
            {
                lblUser.Text = "User : " + Helper.UserName;
                Connecting_Readers(Helper.RFIDReaderIP, Helper.RFIDReaderPort.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        frmSettings Settings;
        private void pbSettings_Click(object sender, EventArgs e)
        {
            pnlMenu.Visible = false;
            if (MaterialMovement != null)
            {
                MaterialMovement.Close();
            }
            if (TareWeight != null)
            {
                TareWeight.Close();
            }
            if (Configuration != null)
            {
                Configuration.Close();
            }
            if (Registration != null)
            {
                Registration.Close();
            }
            if (Configuration == null)
            {
                Settings = new frmSettings(this);
                Settings.FormClosed += new FormClosedEventHandler(Settings_FormClosed);
                Settings.MdiParent = this;
                this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                Settings.Dock = DockStyle.Fill;
                Settings.Show();
            }
            else
            {
                Settings.Activate();
            }
        }

        void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings = null;
        }

        frmRegistration Registration;
        private void pbRegistration_Click(object sender, EventArgs e)
        {
            pnlMenu.Visible = false;
            if (TareWeight != null)
            {
                TareWeight.Close();
            }
            if (Configuration != null)
            {
                Configuration.Close();
            }
            if (Settings != null)
            {
                Settings.Close();
            }
            if (MaterialMovement != null)
            {
                MaterialMovement.Close();
            }
            if (MaterialMovement == null)
            {
                Registration = new frmRegistration(this);
                Registration.FormClosed += new FormClosedEventHandler(Registration_FormClosed);
                Registration.MdiParent = this;
                this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                Registration.Dock = DockStyle.Fill;
                Registration.Show();
            }
            else
            {
                Registration.Activate();
            }
        }

        void Registration_FormClosed(object sender, FormClosedEventArgs e)
        {
            Registration = null;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_reader.ConnectFlag == 1)
                {
                    _reader.StopIdentifying();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
