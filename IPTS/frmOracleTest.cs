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
    public partial class frmOracleTest : Form
    {
        DBLayer _dblayer = new DBLayer();

        public frmOracleTest()
        {
            InitializeComponent();
        }

        private String GetShift()
        {
            return SQLHelper.Execute_Stored_Procedure("IPTS_SHIFTMASTER_GETDETAILS").Rows[0]["SHIFT"].ToString();
        }

        private void IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS(object TripID, object TruckID, object TruckNumber, object Shift, object MineNo, object STATUS)
        {
            try
            {
                DataTable dt = DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS(TripID, TruckID, TruckNumber, Shift, MineNo, STATUS);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (STATUS.ToString() == "ALL")
                        {
                            dgvTruckGrossWeight.DataSource = dt;
                        }
                        if (STATUS.ToString() == "TRUCKID")
                        {
                            txtGrossWeight.Text = dt.Rows[0]["GrossWeight"].ToString();
                            txtMineNumber.Text = dt.Rows[0]["MineNo"].ToString();
                            txtNetWeight.Text = dt.Rows[0]["NetWeight"].ToString();
                            txtTareWeight.Text = dt.Rows[0]["TareWeight"].ToString();
                            txtTransportCOde.Text = dt.Rows[0]["TransporterCode"].ToString();
                            txtTransporter.Text = dt.Rows[0]["Transporter"].ToString();
                            txtTruckID.Text = dt.Rows[0]["TruckID"].ToString();
                            txtTruckNumber.Text = dt.Rows[0]["TruckNumber"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBox1.Text = OracleBLLayer.GetString(91, int.Parse(txtMineNumber.Text), 8, txtTruckNumber.Text, txtGrossWeight.Text, txtTareWeight.Text, txtNetWeight.Text,
                    "0", "0", txtNetWeight.Text, Helper.UserName, GetShift(), 33, 1);
                OracleBLLayer.InsertNewGrossWeight(91, int.Parse(txtMineNumber.Text), 8, txtTruckNumber.Text, txtGrossWeight.Text, txtTareWeight.Text, txtNetWeight.Text,
                    "0", "0", txtNetWeight.Text, Helper.UserName, GetShift(), 33, 1);
                MessageBox.Show("ERP Updated Successful.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgvTruckGrossWeight_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgvTruckGrossWeight.Rows[e.RowIndex];
                txtTruckID.Text = row.Cells["TruckID"].Value.ToString();
                IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS("", txtTruckID.Text, "", "", "", "TRUCKID");
            }
        }

        private void frmOracleTest_Load(object sender, EventArgs e)
        {
            IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS("", "", "", "", "", "ALL");
        }
    }
}
