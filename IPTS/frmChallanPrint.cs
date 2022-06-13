using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DragonFactory.Utilities;
using TraceTool;
using DragonDesignSurface;
using DragonGraphicObjects;

namespace IPTS
{
    public partial class frmChallanPrint : Form
    {
        public static string _word = string.Empty;
        static WinTrace _printTrace = new WinTrace("PrintTrace", "PrintTrace");

        public frmChallanPrint()
        {
            InitializeComponent();
        }

        string _tripID = string.Empty;
        private void dgvTruckGrossWeight_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgvTruckGrossWeight.Rows[e.RowIndex];
                _tripID = row.Cells["TripID"].Value.ToString();
                IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS(_tripID, "", "", "", "", "CHALLANPRINT");
            }
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
                        if (STATUS.ToString() == "PRINT")
                        {
                            dgvTruckGrossWeight.DataSource = dt;
                        }
                        if (STATUS.ToString() == "CHALLANPRINT")
                        {
                            txtTruckNumber.Text = dt.Rows[0]["TruckNumber"].ToString();
                            ttxMineNumber.Text = dt.Rows[0]["MineNo"].ToString();
                            txtDate.Text = dt.Rows[0]["DT"].ToString();
                            txtTime.Text = dt.Rows[0]["TM"].ToString();
                            txtvalidtime.Text = dt.Rows[0]["TM1"].ToString();
                            decimal _netwt = decimal.Parse(dt.Rows[0]["NetWeight"].ToString()) / 1000;
                            txtNetWeight.Text = _netwt.ToString();
                        }
                    }
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

        private void frmChallanPrint_Load(object sender, EventArgs e)
        {
            IPTS_TRUCKGROSSWEIGHTDETAILS_GETDETAILS("", "", "", "", "", "PRINT");
            dtpFrom.Format = DateTimePickerFormat.Custom;
            dtpTo.Format = DateTimePickerFormat.Custom;
            dtpFrom.CustomFormat = "dd-MM-yyyy";
            dtpFrom.MaxDate = DateTime.Now;
            dtpTo.CustomFormat = "dd-MM-yyyy";
            dtpTo.MaxDate = DateTime.Now;
            IPTS_MINE_MASTER_GETDETAILS();
            PrintFormLoad();
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
                        dr.ItemArray = new object[] { 0, "Select Mine" };
                        dt.Rows.InsertAt(dr, 0);
                        cbMine.DisplayMember = "MINE_NAME";
                        cbMine.ValueMember = "MINE_ID";
                        cbMine.DataSource = dt;

                    }
                }
            }
            catch (Exception ex)
            {

            }
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
                            dr.DataLink = txtTruckNumber.Text;
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
                            if (ttxMineNumber.Text == "88")
                            {
                                dr.DataLink = "Mandadi RF\nKCP - Manadai Mine (34.98 Ha)\nCrusher Point\nPolepalli\nDurgi(Mdl)\nGuntur(Dist)";
                            }
                            if (ttxMineNumber.Text == "90")
                            {
                                dr.DataLink = "Mandadi RF\nKCP - Deposit-II (24.32 Ha)\nCrusher Point\nPolepalli\nDurgi(Mdl)\nGuntur(Dist)";
                            }
                            if (ttxMineNumber.Text == "91")
                            {
                                dr.DataLink = "Mandadi RF\nKCP - Deposit-I (24.32 Ha)\nCrusher Point\nPolepalli\nDurgi(Mdl)\nGuntur(Dist)";
                            }
                        }
                        if (dr.DataLink == "VARWEIGHT")
                        {
                            dr.DataLink = txtNetWeight.Text;
                        }
                        if (dr.DataLink == "VARPLACE")
                        {
                            dr.DataLink = "Macherla";
                        }
                        if (dr.DataLink == "VARPLACEDATE")
                        {
                            dr.DataLink = txtDate.Text;
                        }
                        if (dr.DataLink == "VARISSUEDPERMITTIME")
                        {
                            dr.DataLink = txtTime.Text;
                        }
                        if (dr.DataLink == "VARISSUEDPERMIDATE")
                        {
                            dr.DataLink = txtDate.Text;
                        }
                        if (dr.DataLink == "VARVALIDPERMITTIME")
                        {
                            dr.DataLink = txtvalidtime.Text;
                        }
                        if (dr.DataLink == "VARVALIDPERMIDATE")
                        {
                            dr.DataLink = txtDate.Text;
                        }
                        if (dr.DataLink == "VARWEIGHTWORD")
                        {
                            dr.DataLink = _word;
                        }
                        if (dr.DataLink == "VARDESTINATION")
                        {
                            if (ttxMineNumber.Text == "88")
                            {
                                dr.DataLink = "Compartment No : 82\nMandadi RF, Block-II\n\n\n\n\nThe K.C.P Limited\nMandadi\nVeldurthy\nGuntur\nMines to Crusher";
                            }
                            if (ttxMineNumber.Text == "90")
                            {
                                dr.DataLink = "Compartment No : 82\nMandadi RF, Block-II\n\n\n\n\nThe K.C.P Limited\nPolepalli(V)\nDurgi(M)\nGuntur\nDeposit-II Mines to Crusher";
                            }
                            if (ttxMineNumber.Text == "91")
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

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                string number = txtNetWeight.Text;
                if (number.Contains("-"))
                {
                    number = number.Substring(1, number.Length - 1);
                }
                if (number == "0")
                {
                }
                else
                {
                    ConvertToWords(number);
                }
                for (int z = 0; z < 3; z++)
                {
                    PrintDatFile();
                }
                try
                {
                    DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_UPDATE_PRINT(_tripID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                dpsDesignCtrl.Controls.Clear();
                PrintFormLoad();
                Clear();
                //IPTS_TRUCKGROSSWEIGHTDETAILS_MANUALPRINT(Convert.ToDateTime(dtpFrom.Text).ToString("yyyy-MM-dd"), Convert.ToDateTime(dtpTo.Text).ToString("yyyy-MM-dd"), cbMine.SelectedValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void IPTS_TRUCKGROSSWEIGHTDETAILS_MANUALPRINT(object FROMDATE, object TODATE, object MINE)
        {
            try
            {
                DataTable dt = DBLayer.IPTS_TRUCKGROSSWEIGHTDETAILS_MANUALPRINT(FROMDATE, TODATE, MINE);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        dgvTruckGrossWeight.DataSource = dt;
                    }
                    else
                    {
                        dgvTruckGrossWeight.DataSource = null;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Clear()
        {
            txtNetWeight.Text = string.Empty;
            txtTruckNumber.Text = string.Empty;
            txtDate.Text = string.Empty;
            txtTime.Text = string.Empty;
            txtvalidtime.Text = string.Empty;
            ttxMineNumber.Text = string.Empty;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void btnGetDetails_Click(object sender, EventArgs e)
        {
            IPTS_TRUCKGROSSWEIGHTDETAILS_MANUALPRINT(Convert.ToDateTime(dtpFrom.Text).ToString("yyyy-MM-dd"), Convert.ToDateTime(dtpTo.Text).ToString("yyyy-MM-dd"), cbMine.SelectedValue);
        }
    }
}
