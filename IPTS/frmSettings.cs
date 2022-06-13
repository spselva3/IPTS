using LMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPTS
{
    public partial class frmSettings : Form
    {
        ErrorLogs _error = new ErrorLogs();
        public string _connection = ConfigurationManager.ConnectionStrings["ConStrn"].ConnectionString;
        frmMain _Main;
        public frmSettings(frmMain Main)
        {
            InitializeComponent();
            _Main = Main;
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            try
            {
                GetHardwareDetails();
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Setting", "frmSettings_Load", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private void GetHardwareDetails()
        {
            try
            {
                txtHooterPort.Text = Helper.HooterPort.ToString();
                txtPLCIPAddress.Text = Helper.PLCIP;
                txtPLCPort.Text = Helper.PLCPort.ToString();
                txtRFIDReaderIP.Text = Helper.RFIDReaderIP;
                txtRFIDReaderPort.Text = Helper.RFIDReaderPort.ToString();
                txtSensorPort1.Text = Helper.SensorPort1.ToString();
                txtSensorPort2.Text = Helper.SensorPort2.ToString();
                txtTrafficLightPortGreen.Text = Helper.TrafficLightPortGreen.ToString();
                txtTrafficLightPortRed.Text = Helper.TrafficLightPortRed.ToString();
                txtWeighBridgeIP.Text = Helper.WeighBridgeIP;
                txtWeighBridgePort.Text = Helper.WeighBridgePort.ToString();
                numericUpDownAutoSaveTimeInterval.Value = Helper.AutoSaveInterval;
                numericUpDownTareWeighFrequecny.Value = Helper.TareWeightFrequencedays;
                numericUpDownTareWtLimitMax.Value = Helper.TruckMaxWt;
                numericUpDownTareWtLimitMin.Value = Helper.TruckMinWt;
                nudTimeSpan.Value = Helper.TruckGrossWeightThreshold;

                //RegRFIDReaderIP
                if (Helper.AutoSave == true)
                {
                    chkAutoSaveGrossWeight.Checked = true;
                }
                else
                {
                    chkAutoSaveGrossWeight.Checked = false;
                }
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Settings", "GetHardwareDetails", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private void pbBack_Click(object sender, EventArgs e)
        {
            this.Close();
            _Main.PanelVisuable(true);
        }

        private void pbSave_Click(object sender, EventArgs e)
        {
            try
            {
                int _checked;
                if (chkAutoSaveGrossWeight.Checked == true)
                {
                    _checked = 1;
                }
                else
                {
                    _checked = 0;
                }
                DBLayer.IPTS_HARDWARECONFIGURATION_UPDATE(txtLocation.Text, txtPLCIPAddress.Text, txtPLCPort.Text, 0, txtSensorPort1.Text, txtSensorPort2.Text, txtTrafficLightPortRed.Text, txtTrafficLightPortGreen.Text, txtRFIDReaderIP.Text, txtRFIDReaderPort.Text, Helper.UserName, txtWeighBridgeIP.Text, txtWeighBridgePort.Text, txtHooterPort.Text, _checked, numericUpDownAutoSaveTimeInterval.Value, nudTimeSpan.Value, numericUpDownTareWeighFrequecny.Value, numericUpDownTareWtLimitMin.Value, numericUpDownTareWtLimitMax.Value);
                if (MessageBox.Show("Setting updated successful\nDo you want to close Application?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Settings", "pbSave_Click", ex.Message.ToString(), "High", Helper.UserName);
            }
        }
    }
}
