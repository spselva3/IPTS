using DragonFactory.TcpIPClient;
using LMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPTS
{
    public partial class frmManualERPUpdate : Form
    {
        ErrorLogs _error = new ErrorLogs();
        public string _connection = ConfigurationManager.ConnectionStrings["ConStrn"].ConnectionString;

        public frmManualERPUpdate()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            txtGrossWeight.Text = string.Empty;
            txtMineNumber.Text = string.Empty;
            txtTareWeight.Text = string.Empty;
            txtNetWeight.Text = string.Empty;
            txtTruckID.Text = string.Empty;
            txtTruckNumber.Text = string.Empty;
            txtShift.Text = string.Empty;
            //LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.CompleteGreen);
            //LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Constant, "");
            //LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, "");
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

        private void IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT_MANUAL(object TripID, object TruckID, object TransporterCode, object Transporter, object WeighBridgeNumber
          , object MaterialMovementCode, object TareWeight, object GrossWeight, object NetWeight, object UpdatedBy, object TruckNumber, object Shift, object MineNo, object TimeStamp)
        {
            try
            {
                DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT_MANUAL(TripID, TruckID, TransporterCode, Transporter, WeighBridgeNumber, MaterialMovementCode, TareWeight, GrossWeight, NetWeight, UpdatedBy, TruckNumber, Shift, MineNo, TimeStamp);
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private String GetShift()
        {
            return SQLHelper.Execute_Stored_Procedure("IPTS_SHIFTMASTER_GETDETAILS").Rows[0]["SHIFT"].ToString();
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
                    try
                    {
                        OracleBLLayer.InsertNewGrossWeight_Manual(91, int.Parse(txtMineNumber.Text), 8, txtTruckNumber.Text, txtGrossWeight.Text, txtTareWeight.Text, txtNetWeight.Text,
                            "0", "0", txtNetWeight.Text, Helper.UserName, txtShift.Text, 33, 1, dtpDate.Text, dtpTime.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                        _error.CONNECTION_NAME(_connection);
                        _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "only erp try catch", ex.Message.ToString(), "High", Helper.UserName);
                    }

                    DataTable dtID = DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS("", "", "", "", "", "ID");
                    if (dtID != null)
                    {
                        if (dtID.Rows.Count > 0)
                        {
                            DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATEFLAG(dtID.Rows[0]["ID"].ToString());
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

        private void Save()
        {
            try
            {
                if (txtTruckID.Text == string.Empty)
                {
                    MessageBox.Show("Please enter Truck ID");
                    txtTruckID.Focus();
                    return;
                }
                if (txtTruckNumber.Text == string.Empty)
                {
                    MessageBox.Show("Please enter Truck NUmber");
                    txtTruckNumber.Focus();
                    return;
                }
                if (txtMineNumber.Text == string.Empty)
                {
                    MessageBox.Show("Please enter Mine number");
                    txtMineNumber.Focus();
                    return;
                }
                if (txtGrossWeight.Text == string.Empty)
                {
                    MessageBox.Show("Please enter Gross weight");
                    txtGrossWeight.Focus();
                    return;
                }
                if (txtNetWeight.Text == string.Empty)
                {
                    MessageBox.Show("Please enter net weight");
                    txtNetWeight.Focus();
                    return;
                }
                if (decimal.Parse(txtGrossWeight.Text) <= decimal.Parse(txtTareWeight.Text))
                {
                    MessageBox.Show("Gross weight should be greater then the tare weight");
                    return;
                }

                DataTable dt = DBLayer.IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS(txtTruckID.Text, "", "", "TRUCKID");
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        //IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT(txtMineNumber.Text, txtTruckID.Text, dt.Rows[0]["TransporterCode"].ToString(), dt.Rows[0]["Transporter"].ToString(), 1, "L", txtTareWeight.Text, txtGrossWeight.Text, txtNetWeight.Text, Helper.UserName, txtTruckNumber.Text, GetShift(), txtMineNumber.Text);
                        IPTS_TRUCKGROSSWEIGHTDETAILS_INSERT_MANUAL(txtMineNumber.Text, txtTruckID.Text, dt.Rows[0]["TransporterCode"].ToString(), dt.Rows[0]["Transporter"].ToString(), 1, "L", txtTareWeight.Text, txtGrossWeight.Text, txtNetWeight.Text, Helper.UserName, txtTruckNumber.Text, txtShift.Text, txtMineNumber.Text, dtpDate.Text + " " + dtpTime.Text);
                        GROSSWEIGHT_INSERT_IN_ERP();
                        //LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.GreenTickMark);
                        //Thread.Sleep(2000);
                        //LedDisplaysList.SendToSiren(LedHelper.Siran.TwentySeconds);
                        MessageBox.Show("Data saved success");
                        Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                _error.CONNECTION_NAME(_connection);
                _error.INSERT_ERROR_LOG_IN_DATABASE("IPTS", "Truck Gross Weight", "Save", ex.Message.ToString(), "High", Helper.UserName);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void txtTruckID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtTruckID.Text != string.Empty)
                {
                    Add();
                }
            }
        }

        private void Add()
        {
            try
            {
                //LedDisplaysList.SendToBiColorBoard(LedHelper.BiColorBoardMessage.CompleteRed);
                //LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.TopLeft, LedHelper.MessageDisplayMode.Constant, txtTruckID.Text);
                DataTable _dtTare = DBLayer.IPTS_TRUCKTAREWEIGHTDETAILS_GETDETAILS(txtTruckID.Text, "", "", "TRUCKIDLASTTAREWEIGHT");
                if (_dtTare != null)
                {
                    if (_dtTare.Rows.Count > 0)
                    {
                        txtTruckNumber.Text = _dtTare.Rows[0]["TruckNumber"].ToString();
                        txtTareWeight.Text = _dtTare.Rows[0]["TareWeight"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void pbAdd_Click(object sender, EventArgs e)
        {
            if (txtTruckID.Text != string.Empty)
            {
                Add();
            }
        }

        private void txtGrossWeight_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtGrossWeight.Text != string.Empty)
                {
                    decimal _grossWt = Convert.ToDecimal(txtGrossWeight.Text);
                    txtGrossWeight.Text = _grossWt.ToString();
                    decimal _netWt = Convert.ToDecimal(txtGrossWeight.Text) - Convert.ToDecimal(txtTareWeight.Text);
                    txtNetWeight.Text = _netWt.ToString();

                    //LedDisplaysList.SendToLedDisplay(LedHelper.LedLocation.BottomLeft, LedHelper.MessageDisplayMode.Constant, txtGrossWeight.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }

        #region LedDisplay

        //LedHelper LedDisplaysList;
        //private void StartLedProcess()
        //{
        //    try
        //    {
        //        LedDisplaysList = new LedHelper();
        //        for (int i = 0; i < 1; i++)
        //        {
        //            LedDisplaysList.StartLedConnection(Helper.DisplayIP, Helper.DisplayPort.ToString(),
        //                                        (o, e) =>
        //                                        {
        //                                            ICustomClient _client = (ICustomClient)o;
        //                                            var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");

        //                                            if (h.IpAddress == Helper.DisplayIP)
        //                                            {

        //                                                if (i == 0)
        //                                                {

        //                                                }
        //                                                //**************Hardware status end***************

        //                                            }
        //                                        },
        //                                         (o, e) =>
        //                                         {
        //                                             ICustomClient _client = (ICustomClient)o;
        //                                             var h = _client.GetFieldValue<CustomClientTcpEndPoint>("_serverEndPoint");

        //                                             if (h.IpAddress == Helper.DisplayIP)
        //                                             {
        //                                             }
        //                                         });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //lblMessage.Text = ex.Message.ToString();
        //        throw;
        //    }
        //}

        //private void StopLedProcess()
        //{
        //    for (int i = 0; i < 1; i++)
        //    {
        //        LedDisplaysList.CloseLedConnection();
        //    }
        //}

        #endregion

        private void frmManualERPUpdate_Load(object sender, EventArgs e)
        {
            dtpDate.Format = DateTimePickerFormat.Custom;
            dtpDate.CustomFormat = "yyyy-MM-dd";
            dtpDate.MaxDate = DateTime.Now;
        }

        private void frmManualERPUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            //StopLedProcess();
        }
    }
}
