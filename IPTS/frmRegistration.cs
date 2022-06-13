using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPTS
{
    public partial class frmRegistration : Form
    {
        frmMain _Main;
        public frmRegistration(frmMain Main)
        {
            InitializeComponent();
            _Main = Main;
        }

        private void frmRegistration_Load(object sender, EventArgs e)
        {
            try
            {
                if (_Main._reader.ConnectFlag == 1)
                {
                    _Main._reader.NewTagValueReceived += _reader_NewTagValueReceived;
                    //_Main.lblLocationXReader.BackColor = Color.Green;
                }
                else
                {
                    //_Main.lblLocationXReader.BackColor = Color.Red;
                }
                IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS("", "", "", "ALL");
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
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void pbBack_Click(object sender, EventArgs e)
        {
            this.Close();
            _Main.PanelVisuable(true);
        }

        private void IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS(object TruckID, object RFIDTagID, object TruckNumber, object STATUS)
        {
            DataTable dt = DBLayer.IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS(TruckID, RFIDTagID, TruckNumber, STATUS);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    if (STATUS.ToString() == "ALL")
                    {
                        dgvTruckRegistration.DataSource = dt;
                        SetFontAndColors();
                    }
                    if (STATUS.ToString() == "TRUCKID")
                    {
                        if (txtTruckID.Text != string.Empty)
                        {
                            txtMake.Text = dt.Rows[0]["Make"].ToString();
                            txtModel.Text = dt.Rows[0]["Model"].ToString();
                            txtRFIDValue.Text = dt.Rows[0]["RFIDTagID"].ToString();
                            txtTransporter.Text = dt.Rows[0]["Transporter"].ToString();
                            txtTransportCOde.Text = dt.Rows[0]["TransporterCode"].ToString();
                            txtTruckNumber.Text = dt.Rows[0]["TruckNumber"].ToString();
                            int _regActive;
                            _regActive = Convert.ToInt32(dt.Rows[0]["IsActive"]);
                            if (_regActive == 1)
                            {
                                ckStatus.Checked = true;
                            }
                            else if (_regActive == 0)
                            {
                                ckStatus.Checked = false;
                            }
                        }
                    }
                    if (STATUS.ToString() == "RFID")
                    {

                    }
                }
            }
        }

        private void Validation()
        {
            if (txtMake.Text == string.Empty)
            {
                MessageBox.Show("Please enter Make");
                txtMake.Focus();
                return;
            }
            if (txtModel.Text == string.Empty)
            {
                MessageBox.Show("Please enter Model");
                txtModel.Focus();
                return;
            }
            if (txtRFIDValue.Text == string.Empty)
            {
                MessageBox.Show("Please scan rfid tag");
                txtRFIDValue.Focus();
                return;
            }
            if (txtTransportCOde.Text == string.Empty)
            {
                MessageBox.Show("Please enter Transporter code");
                txtTransportCOde.Focus();
                return;
            }
            if (txtTransporter.Text == string.Empty)
            {
                MessageBox.Show("Please enter Transporter name");
                txtTransporter.Focus();
                return;
            }
            if (txtTruckID.Text == string.Empty)
            {
                MessageBox.Show("Please enter Truck ID");
                txtTruckID.Focus();
                return;
            }

        }

        private void pbSave_Click(object sender, EventArgs e)
        {
            try
            {
                Validation();
                int _status;
                if (ckStatus.Checked == true)
                {
                    _status = 1;
                }
                else
                {
                    _status = 0;
                }

                DBLayer.IPTS_TRUCKREGISTRATIONMASTER_INSERT(txtTruckID.Text.Trim(), txtRFIDValue.Text.Trim(), txtTransportCOde.Text.Trim(), txtTransporter.Text.Trim(), _status, Helper.UserName, txtMake.Text.Trim(), txtModel.Text.Trim(), txtTruckNumber.Text.Trim());
                MessageBox.Show("Truck Registration successfully");
                IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS("", "", "", "ALL");
                Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void SetFontAndColors()
        {
            dgvTruckRegistration.EnableHeadersVisualStyles = false;
            dgvTruckRegistration.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Calibri", 13, FontStyle.Bold);//Century Gothic
            dgvTruckRegistration.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvTruckRegistration.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvTruckRegistration.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            this.dgvTruckRegistration.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black;
            this.dgvTruckRegistration.DefaultCellStyle.Font = new System.Drawing.Font("Calibri", 12);
            this.dgvTruckRegistration.DefaultCellStyle.ForeColor = Color.Black;
            this.dgvTruckRegistration.DefaultCellStyle.BackColor = Color.White;
            this.dgvTruckRegistration.DefaultCellStyle.SelectionForeColor = Color.Black;
            this.dgvTruckRegistration.DefaultCellStyle.SelectionBackColor = Color.White;//FromArgb(41, 128, 185);
        }

        private void Clear()
        {
            txtMake.Text = string.Empty;
            txtModel.Text = string.Empty;
            txtRFIDValue.Text = string.Empty;
            txtTransportCOde.Text = string.Empty;
            txtTransporter.Text = string.Empty;
            txtTruckNumber.Text = string.Empty;
            txtTruckID.Text = string.Empty;
            //ckStatus.Checked = false;
            txtTruckID.Enabled = true;
            _Main._reader.UpdateReaderReadValue = string.Empty;
        }


        private void pbCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void dgvTruckRegistration_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgvTruckRegistration.Rows[e.RowIndex];
                txtTruckID.Text = row.Cells["TruckID"].Value.ToString();
                //txtTruckID.Enabled = false;
                IPTS_TRUCKREGISTRATIONMASTER_GETDETAILS(txtTruckID.Text.Trim(), "", "", "TRUCKID");
            }
        }

        private void frmRegistration_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_Main._reader.ConnectFlag == 1)
                {
                    _Main._reader.NewTagValueReceived -= _reader_NewTagValueReceived;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void pbResetRfidValue_Click(object sender, EventArgs e)
        {
            txtRFIDValue.Text = string.Empty;
            //_Main._reader.UpdateReaderReadValue = string.Empty;
            _Main._reader.UpdateReaderReadValue = string.Empty;
        }
    }
}
