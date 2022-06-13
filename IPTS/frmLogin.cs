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
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            txtUsername.Text = string.Empty;
            txtUsername.Focus();
            txtPassword.Text = string.Empty;
            //IPTS_MINE_MASTER_GETDETAILS();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == string.Empty)
            {
                MessageBox.Show("Please enter username.!");
                txtUsername.Focus();
                return;
            }
            if (txtPassword.Text == string.Empty)
            {
                MessageBox.Show("Please enter password.!");
                txtPassword.Focus();
                return;
            }
            //if (cbMine.SelectedValue.ToString() == "0")
            //{
            //    MessageBox.Show("Please select mine.!");
            //    return;
            //}


            DataTable dtLogin = DBLayer.IPTS_USERMASTER_GETDETAILS(txtUsername.Text, "LOGIN");
            if (dtLogin != null)
            {
                if (dtLogin.Rows.Count > 0)
                {
                    if (dtLogin.Rows[0]["USER_LOGIN_PASSWORD"].ToString() != txtPassword.Text)
                    {
                        MessageBox.Show("Enter wrong password");
                        txtPassword.Text = string.Empty;
                        txtPassword.Focus();
                        return;
                    }
                    Helper.UserName = txtUsername.Text;
                    Helper.UserRole = dtLogin.Rows[0]["USER_ROLE"].ToString();
                    //Helper.Mine = cbMine.Text;
                    //Helper.MineCode = cbMine.SelectedValue.ToString();
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Enter wrong credentials");
                    Clear();
                    return;
                }
            }

        }

        private void IPTS_MINE_MASTER_GETDETAILS()
        {
            try
            {
                DataTable dt = DBLayer.IPTS_MINE_MASTER_GETDETAILS("", "", "HHT");
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.NewRow();
                        dr.ItemArray = new object[] { 0, "Select" };
                        dt.Rows.InsertAt(dr, 0);
                        cbMine.ValueMember = "MINE_ID";
                        cbMine.DisplayMember = "MINE_NAME";
                        cbMine.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals(Convert.ToChar(13)))
            {
                btnLogin_Click(sender, e);
            }
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtUsername.Text != string.Empty)
                {
                    txtPassword.Focus();
                }
                else
                {
                    txtUsername.Focus();
                }
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            //IPTS_MINE_MASTER_GETDETAILS();
        }
    }
}
