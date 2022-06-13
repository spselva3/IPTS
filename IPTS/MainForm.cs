#define _DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using DLL;

namespace IPTS
{
    public partial class MainForm : Form
    {
        frmMain _Main;
        //全局变量(global variable)
        hComSocket CS;                  //通信句柄结构体(connect handle struct)

        int hCom = -1;			        //串口句柄(comm handle)

        Socket sockClient = null;		    //网口句柄(TCP/IP handle)

        int ConnectFlag = 0;		    //连接标志位(connect falg)
        int Language = 0;
        int ResetFlag = 0;
        int OperTime6B = 0;
        int DisplayCnt6B = 0;

        int OperTime = 0;
        int DisplayCnt = 0;
        string OperationType = "";
        string RStr = "", EStr = "", TStr = "", UStr = "";

        RFID_StandardProtocol rfid_sp = new RFID_StandardProtocol();
        public static int Freqnum = 0;
        public static int[] Freqpoints = new int[124];

        BufferData[] Data = new BufferData[256];

        /*******************************工具函数***************************************/
        //将IP地址型字符串转换为十六进制BYTE
        public static void DecimalstrToByte(byte[] Buf, string Str)
        {
            int len = Str.Length;
            int sum = 0;
            int j = 0;
            int flag = 0;
            int Num = 0;
            for (int i = 0; i < len; i++)
            {
                if (Str[i] == '.')
                {
                    sum = (int)(sum / Math.Pow(10.0, (3 - Num)));
                    Buf[j] = (byte)((sum) & 0x00FF);
                    j++;
                    sum = 0;
                    flag = 0;
                    i++;
                    Num = 0;
                }
                switch (Str[i])
                {
                    case '0':
                        sum += (int)(0 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '1':
                        sum += (int)(1 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '2':
                        sum += (int)(2 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '3':
                        sum += (int)(3 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '4':
                        sum += (int)(4 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '5':
                        sum += (int)(5 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '6':
                        sum += (int)(6 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '7':
                        sum += (int)(7 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '8':
                        sum += (int)(8 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                    case '9':
                        sum += (int)(9 * Math.Pow(10.0, 2 - flag));
                        Num++;
                        break;
                }
                flag++;
            }
            sum = (int)(sum / Math.Pow(10.0, (3 - Num)));
            Buf[j] = (byte)((sum) & 0x00FF);
        }


        //将16进制的BYTE转换为十进制的字符串
        public static string IntToInt_Str(int value)
        {
            string str = string.Format("{0:D}", value);
            return str;
        }

        //将十六进制BYTE转换为IP地址型字符串
        public static void ByteToDecimalstr(ref string ToStr, byte[] Buf)
        {
            for (int i = 0; i < 4; i++)
            {
                ToStr = ToStr + IntToInt_Str((int)Buf[i]);
                if (3 != i)
                    ToStr = ToStr + ".";
            }
        }

        public MainForm(frmMain Main)
        {
            InitializeComponent();
            _Main = Main;
            tabControl2.SelectedIndex = 0;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //SPNumcomboBox.DataSource = SerialPort.GetPortNames();
            //SPNumcomboBox.SelectedIndex = 0;
            //SPBaudcomboBox.SelectedIndex = 9;

            try
            {
                SPNumcomboBox.DataSource = SerialPort.GetPortNames();
            }
            catch (System.Exception ex)
            {
                ListBoxAdd(ex.Message);
            }

            try
            {
                SPNumcomboBox.SelectedIndex = 0;
                SPBaudcomboBox.SelectedIndex = 5;
            }
            catch (System.Exception excp)
            {
                //if (Language == 0)
                //    ListBoxAdd("串口不可使用，请使用网口连接!");
                //else
                //{
                //    ListBoxAdd("Serial port cannot be used, ");
                //    ListBoxAdd("please connect with Ethernet port");
                //}
            }




            IPtextBox.Text = "192.168.13.226";
            InternetPorttextBox.Text = Convert.ToString(100);

            Connectbutton.Enabled = true;
            Disconnectbutton.Enabled = false;

            ReadLanguageFile(ref Language);
            if (Language == 0)
            {
                Language_comboBox.SelectedIndex = 1;
                LanguageChinese();
            }
            if (Language == 1)
            {
                Language_comboBox.SelectedIndex = 1;
                LanguageEnglish();
            }

            InitTab();

            //系统配置
            IO1_radioButton.Checked = true;
            timer1.Enabled = true;

            //ISO18000-6B
            MULTITAGIDENTIFY_radioButton.Checked = true;
            SINGLEREAD_radioButton.Checked = true;
            for (int index = 0; index < 301; index++)
            {
                TAGADDR_comboBox.Items.Add(index);
            }
            TAGADDR_comboBox.SelectedIndex = 8;

            //EPC GEN2
            SECREAD_radioButton.Checked = true;
            MULTITAG_INVENTORY_radioButton.Checked = true;

            EPC_STARTADDR_comboBox.SelectedIndex = 0;
            EPC_CNTNUM_comboBox.SelectedIndex = 0;
            USER_STARTADDR_comboBox.SelectedIndex = 0;
            USER_CNTNUM_comboBox.SelectedIndex = 0;
            TID_STARTADDR_comboBox.SelectedIndex = 0;
            TID_CNTNUM_comboBox.SelectedIndex = 0;
            RESERVE_STARTADDR_comboBox.SelectedIndex = 0;
            RESERVE_CNTNUM_comboBox.SelectedIndex = 0;
            ACCESSPASSWORD_SINGLE_textBox.Text = "00000000";

            SECREAD_radioButton_CheckedChanged(sender, e);
            MULTITAG_INVENTORY_radioButton_CheckedChanged(sender, e);
        }

        private void InitTab()
        {
            if (Language == 0)
            {
                SYSCONFIG_tabPage.Text = "系统配置";
                ISO180006B_tabPage.Text = "ISO18000-6B";
                EPCGEN2_tabPage.Text = "EPC GEN2";
            }
            else
            {
                SYSCONFIG_tabPage.Text = "System Configuration";
                ISO180006B_tabPage.Text = "ISO18000-6B";
                EPCGEN2_tabPage.Text = "EPC GEN2";
            }
        }

        public static void ReadLanguageFile(ref int LanguageFlag)
        {
            string path = "./Language.dat";
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            using (StreamReader sr = File.OpenText(path))
            {
                string s = sr.ReadLine();
                LanguageFlag = Convert.ToInt32(s, 10);
                sr.Close();
            }
        }

        public static void WriteLanguageFile(int LanguageFlag)
        {
            string path = "./Language.dat";
            using (StreamWriter sw = File.CreateText(path))
            {
                string s = Convert.ToString(LanguageFlag);
                sw.WriteLine(s);
                sw.Close();
            }
        }

        private void LanguageChinese()
        {
            tabPage1.Text = "网口";
            tabPage2.Text = "串口";

            label1.Text = "IP:";
            label2.Text = "端口:";
            label3.Text = "串口号:";
            label4.Text = "波特率:";

            Connectbutton.Text = "连接";
            Disconnectbutton.Text = "断开";
            groupBox1.Text = "连接方式";
            groupBox2.Text = "操作信息";
            CLRSYSMSG_button.Text = "清空";

            //系统配置
            groupBox3.Text = "天线工作状态";
            SETANTENNA_button.Text = "设置";
            GETANTENNA_button.Text = "查询";

            groupBox4.Text = "功率";
            SETRFPWR_button.Text = "设置";
            GETRFPWR_button.Text = "查询";
            SETINTERNET_button.Text = "设置";
            GETINTERNET_button.Text = "查询";

            groupBox5.Text = "网络";
            IPPORT_label.Text = "端口:";
            GATEWAY_label.Text = "网关:";
            MASK_label.Text = "掩码:";

            groupBox6.Text = "频率-频域";
            CHINA_radioButton.Text = "中国";
            NORTHAMERICA_radioButton.Text = "北美";
            EUROPE_radioButton.Text = "欧洲";
            OTHERS_radioButton.Text = "其他";
            SETFREQ_button.Text = "设置";
            GETFREQ_button.Text = "查询";

            groupBox7.Text = "可编程IO口";
            label12.Text = "I/O端口:";
            label13.Text = "输出电平:";
            LOWLEVEL_radioButton.Text = "0 低电平";
            HIGHLEVEL_radioButton.Text = "1 高电平";
            SETOUTPORT_button.Text = "设置";
            GETOUTPUT_button.Text = "查询";

            groupBox8.Text = "载波抵消策略";
            MODE_label.Text = "策略:";
            SINGLETAG_radioButton.Text = "单卡快读";
            MULTITAG_radioButton.Text = "多卡模式";
            CURRENTMODE_label.Text = "当前策略:";
            SETSFTM_button.Text = "设置";
            GETSFTM_button.Text = "查询";

            groupBox9.Text = "模式-接口";
            CMD_radioButton.Text = "命令模式";
            ANIMATION_radioButton.Text = "自动模式";
            TRIGGER_radioButton.Text = "触发模式";
            WEIGAN_checkBox.Text = "韦根";
            WEIGANSTYLE_label.Text = "类型";
            IP_checkBox.Text = "网口";
            RELAY_checkBox.Text = "继电器";
            SETINTERFACE_button.Text = "设置";
            GETINTERFACE_button.Text = "查询";

            CERTIFICATION_button.Text = "标签认证";
            RESET_button.Text = "读写器复位";

            //ISO18000-6B
            TAGIDETIFY_groupBox.Text = "标签操作";
            MULTITAGIDENTIFY_radioButton.Text = "多标签识别";
            MULTITAGREAD_radioButton.Text = "多标签读取";
            READADDR_label.Text = "读取地址";
            CLEAR_button.Text = "清空";
            ISO_TIME_label.Text = "时间";
            ISO_NUM_label.Text = "数量";
            STOP_button.Text = "停止";
            UNIQUETAG_groupBox.Text = "单标签";
            SINGLEREAD_radioButton.Text = "读取";
            SINGLEWRITE_radioButton.Text = "写入";
            SINGLELOCK_radioButton.Text = "锁定";
            ACCESSADDR_label.Text = "访问地址:";
            ACCESSID_label.Text = "访问ID:";
            RESULT_groupBox.Text = "结果";
            VALUE_label.Text = "数据:";
            CLEARRESULT_button.Text = "清空";

            //EPC GEN2
            MULTITAG_groupBox.Text = "多标签操作";
            SHOWDATA_groupBox.Text = "数据显示";

            SELECTMEMBANK_label.Text = "数据区域:";
            WRITEMEMBANK_label.Text = "写入区域:";
            STARTADDR_label.Text = "起始地址:";
            WRITEVALUE_label.Text = "写入数据:";
            WORDCNTNUM_label.Text = "读取字数:";
            WRITEWORDADDR_label.Text = "起始地址:";
            WRITEWORDNUM_label.Text = "写入字数:";

            TIMES_label.Text = "时间";
            NUM_label.Text = "数量";
            SPEED_label.Text = "速率";
            UNIQUETAG_groupBox.Text = "单标签操作";
            MEMBANK_SINGLE_label.Text = "访问区域:";
            STARTADDR_SINGLE_label.Text = "起始地址:";

            LENGTH_SINGLE_label.Text = "数据长度:";
            LOCKLEVEL_SINGLE_label.Text = "锁定等级:";
            ACCESSPASSWORD_SINGLE_label.Text = "访问密码:";
            MATCH_SINGLE_label.Text = "匹配度:";
            VALUE_SINGLE_label.Text = "数据:";

            MULTITAG_INVENTORY_radioButton.Text = "多标签查询";
            MULTITAG_READ_radioButton.Text = "多标签读取";
            MULTITAG_WRITE_radioButton.Text = "多标签写入";
            SECREAD_radioButton.Text = "安全读";
            SECWRITE_radioButton.Text = "安全写";
            SECLOCK_radioButton.Text = "安全锁";
            KILLTAG_radioButton.Text = "销毁标签";
            CONFIGTAG_radioButton.Text = "筛选配置";
            EPC_START_button.Text = "开始";
            EPC_STOP_button.Text = "停止";
            ONCEINVENTORY_button.Text = "单次盘询";
            CLEARSHOWDATA_button.Text = "清空";
            EXECUTE_button.Text = "执行";
        }

        private void LanguageEnglish()
        {
            tabPage1.Text = "Internet";
            tabPage2.Text = "SerialPort";

            label1.Text = "IP:";
            label2.Text = "Port:";
            label3.Text = "ComNo:";
            label4.Text = "Baud:";

            Connectbutton.Text = "Connect";
            Disconnectbutton.Text = "Disconnect";
            groupBox1.Text = "Connect mode";
            groupBox2.Text = "Operation Message";
            CLRSYSMSG_button.Text = "Clear";

            //系统配置
            groupBox3.Text = "Antenna work state";
            SETANTENNA_button.Text = "Set";
            GETANTENNA_button.Text = "Query";

            groupBox4.Text = "Powert";
            SETRFPWR_button.Text = "Set";
            GETRFPWR_button.Text = "Query";
            SETINTERNET_button.Text = "Set";
            GETINTERNET_button.Text = "Query";

            groupBox5.Text = "Internet";
            IPPORT_label.Text = "Port:";
            GATEWAY_label.Text = "Gate:";
            MASK_label.Text = "Mask:";

            groupBox6.Text = "Frequency-Frequency domain";
            CHINA_radioButton.Text = "China";
            NORTHAMERICA_radioButton.Text = "North America";
            EUROPE_radioButton.Text = "Europe";
            OTHERS_radioButton.Text = "Others";
            SETFREQ_button.Text = "Set";
            GETFREQ_button.Text = "Query";

            groupBox7.Text = "Programmable IO port";
            label12.Text = "I/O port:";
            label13.Text = "Output level:";
            LOWLEVEL_radioButton.Text = "0 Low level";
            HIGHLEVEL_radioButton.Text = "1 High level";
            SETOUTPORT_button.Text = "Set";
            GETOUTPUT_button.Text = "Query";

            groupBox8.Text = "Carrier-Nulling strategy";
            MODE_label.Text = "Strategy:";
            SINGLETAG_radioButton.Text = "Single tag";
            MULTITAG_radioButton.Text = "Multi tag";
            CURRENTMODE_label.Text = "Current strategy:";
            SETSFTM_button.Text = "Set";
            GETSFTM_button.Text = "Query";

            groupBox9.Text = "Model-Interface";
            CMD_radioButton.Text = "Command";
            ANIMATION_radioButton.Text = "Timing";
            TRIGGER_radioButton.Text = "Trigger";
            WEIGAN_checkBox.Text = "Weigan";
            WEIGANSTYLE_label.Text = "Style";
            IP_checkBox.Text = "IP";
            RELAY_checkBox.Text = "Relay";
            RELAY_radioButton.Text = "Relay";
            SETINTERFACE_button.Text = "Set";
            GETINTERFACE_button.Text = "Query";

            CERTIFICATION_button.Text = "Certification";
            RESET_button.Text = "Reset";

            //ISO18000-6B
            TAGIDETIFY_groupBox.Text = "TagIdentify";
            MULTITAGIDENTIFY_radioButton.Text = "MultiTagIdentify";
            MULTITAGREAD_radioButton.Text = "MultiTagRead";
            READADDR_label.Text = "ReadAddr";
            CLEAR_button.Text = "Clear";
            ISO_TIME_label.Text = "Time";
            ISO_NUM_label.Text = "Num";
            STOP_button.Text = "Stop";
            UNIQUETAG_groupBox.Text = "UniqueTag";
            SINGLEREAD_radioButton.Text = "Read";
            SINGLEWRITE_radioButton.Text = "Write";
            SINGLELOCK_radioButton.Text = "Lock";
            ACCESSADDR_label.Text = "Access Addr:";
            ACCESSID_label.Text = "Access ID:";
            RESULT_groupBox.Text = "Result";
            VALUE_label.Text = "Value:";
            CLEARRESULT_button.Text = "Clear";

            //EPC GEN2
            MULTITAG_groupBox.Text = "MultiTagIdentify";
            SHOWDATA_groupBox.Text = "ShowData";

            SELECTMEMBANK_label.Text = "DMembank:";
            WRITEMEMBANK_label.Text = "WMembank:";
            STARTADDR_label.Text = "StartAddr:";
            WRITEVALUE_label.Text = "WriteValue:";
            WORDCNTNUM_label.Text = "ReadWordCount:";
            WRITEWORDADDR_label.Text = "StartAddr:";
            WRITEWORDNUM_label.Text = "WriteWordCount:";

            TIMES_label.Text = "Time";
            NUM_label.Text = "Num";
            SPEED_label.Text = "Speed";
            UNIQUETAG_groupBox.Text = "Unique Tag Operation";
            MEMBANK_SINGLE_label.Text = "Access Membank:";
            STARTADDR_SINGLE_label.Text = "StartAddr:";

            LENGTH_SINGLE_label.Text = "Vlength:";
            LOCKLEVEL_SINGLE_label.Text = "LockLevel:";
            ACCESSPASSWORD_SINGLE_label.Text = "Access Password:";
            MATCH_SINGLE_label.Text = "Match:";
            VALUE_SINGLE_label.Text = "Value:";

            MULTITAG_INVENTORY_radioButton.Text = "MultiTagIdentify";
            MULTITAG_READ_radioButton.Text = "MultiTagRead";
            MULTITAG_WRITE_radioButton.Text = "MultiTagWrite";
            SECREAD_radioButton.Text = "Secure Read";
            SECWRITE_radioButton.Text = "Secure Write";
            SECLOCK_radioButton.Text = "Secure Lock";
            KILLTAG_radioButton.Text = "Kill Tag";
            CONFIGTAG_radioButton.Text = "Select Configutation";
            EPC_START_button.Text = "Start";
            EPC_STOP_button.Text = "Stop";
            ONCEINVENTORY_button.Text = "Once Inventory";
            CLEARSHOWDATA_button.Text = "Clear";
            EXECUTE_button.Text = "Execute";
        }

        void CheckRelay()
        {
            if (RELAY_radioButton.Checked)
            {
                if (0 == Language)
                {
                    LOWLEVEL_radioButton.Text = "关";
                    HIGHLEVEL_radioButton.Text = "开";
                }
                else
                {
                    LOWLEVEL_radioButton.Text = "Close";
                    HIGHLEVEL_radioButton.Text = "Open";
                }
            }
            else
            {
                if (0 == Language)
                {
                    LOWLEVEL_radioButton.Text = "0 低电平";
                    HIGHLEVEL_radioButton.Text = "1 高电平";
                }
                else
                {
                    LOWLEVEL_radioButton.Text = "0 Low level";
                    HIGHLEVEL_radioButton.Text = "1 High level";
                }
            }
        }

        private int SocketConnect()
        {
            IPAddress ipaddr;
            if (IPtextBox.Text == "" || InternetPorttextBox.Text == "")
            {
                return -2;			//输入信息不完整(input info incomplete)
            }
            else if (!IPAddress.TryParse(IPtextBox.Text, out ipaddr))
            {
                return -3;
            }
            else
            {

                if (rfid_sp.Socket_ConnectSocket(ref sockClient,
                                                 IPtextBox.Text,
                                                 Convert.ToInt32(InternetPorttextBox.Text))
                     == RFID_StandardProtocol.SUCCESS)
                    return 0;		//连接成功(connect success)
                else
                    return -1;		//连接失败(connect fail)
            }
        }

        private int SocketDisConnect()
        {
            if (sockClient != null)
            {
                if (rfid_sp.Socket_CloseSocket(sockClient) == RFID_StandardProtocol.SUCCESS)
                {
                    CS.sockClient = null;
                    return 0;		//关闭成功(close success)
                }
                else
                    return -1;		//关闭失败(close fail)
            }
            else
            {
                return -2;			//句柄无效(handle unavailable)
            }
        }

        private int ComConnect()
        {
            int SnIndex = -1;
            int BnIndex = -1;
            SnIndex = SPNumcomboBox.SelectedIndex;
            BnIndex = SPBaudcomboBox.SelectedIndex;

            if (SnIndex == -1 || BnIndex == -1)
            {
                return -2;		//输入信息不完整(input info incomplete)
            }
            else
            {
                if (rfid_sp.Serial_OpenSeries(ref hCom,
                                             SPNumcomboBox.Text,
                                             Convert.ToInt32(SPBaudcomboBox.Text))
                    == RFID_StandardProtocol.SUCCESS)
                    return 0;		//连接成功(connect success)
                else
                    return -1;		//连接失败(connect fail)
            }
        }

        private int ComDisConnect()
        {
            if (hCom != -1)
            {

                if (rfid_sp.Serial_CloseSeries(hCom) == RFID_StandardProtocol.SUCCESS)
                {
                    CS.hCom = -1;
                    return 0;	//关闭成功(close success)
                }
                else
                    return -1;	//关闭失败(close fail)
            }
            else
            {
                return -2;		//句柄无效(handle unavailable)
            }
        }

        delegate void ListBoxAddCallback(string Str);
        private void ListBoxAdd(string Str)
        {
            if (SYSMSG_listBox.InvokeRequired)
            {
                ListBoxAddCallback d = new ListBoxAddCallback(ListBoxAdd);
                Invoke(d, Str);
            }
            else
            {
                SYSMSG_listBox.Items.Add(Str);
                SYSMSG_listBox.TopIndex = SYSMSG_listBox.Items.Count - 1;
            }
        }

        delegate void SetVersionTextCallback(string str);
        private void SetVersionText(string str)
        {
            if (VERSION_label.InvokeRequired)
            {
                SetVersionTextCallback d = new SetVersionTextCallback(SetVersionText);
                Invoke(d, str);
            }
            else
            {
                VERSION_label.Text = str;
            }
        }

        delegate void SetSerialNoTextCallback(string str);
        private void SetSerialNoText(string str)
        {
            if (SERIALNO_label.InvokeRequired)
            {
                SetSerialNoTextCallback d = new SetSerialNoTextCallback(SetSerialNoText);
                Invoke(d, str);
            }
            else
            {
                SERIALNO_label.Text = str;
            }
        }

        delegate void SetTextCallback(string str);
        private void SetText(string str)
        {
            if (VERSION_label.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Invoke(d, str);
            }
            else
            {
                VERSION_label.Text = str;
            }

            if (SERIALNO_label.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Invoke(d, str);
            }
            else
            {
                SERIALNO_label.Text = str;
            }
        }

        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <param name="ReaderNum"></param>
        private void GetVersion(int ReaderNum)
        {
            byte major = 0;
            byte minor = 0;
            string TempStr = "";
            string TempStrEnglish = "";
            byte ReaderAddr = (byte)(ReaderNum & 0xFF);
            //int errcod = rfid_sp.Config_GetLocatorVersion(CS, ref major, ref minor, ReaderAddr);
            //ListBoxAdd("GetVersion errcod = " + errcod);
            if (0x00 == rfid_sp.Config_GetLocatorVersion(CS, ref major, ref minor, ReaderAddr))
            {
                string Major = string.Format("{0:D2}", (int)major); ;
                string Minor = string.Format("{0:D2}", (int)minor); ;
                TempStr = "V" + Major + "." + Minor;

                if (0 == Language)
                    ListBoxAdd("读写器固件版本号为: " + TempStr);
                else
                    ListBoxAdd("Reader firmware version: " + TempStr);
                SetVersionText(TempStr);
                //SetText(TempStr);
            }
            else
            {
                TempStr = "获取版本号失败!";
                TempStrEnglish = "Get version fail!";
                if (0 == Language)
                    ListBoxAdd(TempStr);
                else
                    ListBoxAdd(TempStrEnglish);
            }
        }

        /// <summary>
        /// 获取序列号
        /// </summary>
        /// <param name="ReaderNum"></param>
        private void GetSerialNum(int ReaderNum)
        {
            string TempStr = "";
            string TempStrEnglish = "";

            byte StartAddr = 0x10;
            byte PLen = 6;
            byte[] GData = new byte[6];
            byte ReaderAddr = (byte)(ReaderNum & 0xFF);
            //int errcod = rfid_sp.Parameter_GetReader(CS, StartAddr, PLen, GData, ReaderAddr);
            //ListBoxAdd("GetSerialNum errcod = " + errcod);
            if (0x00 == rfid_sp.Parameter_GetReader(CS, StartAddr, PLen, GData, ReaderAddr))
            {
                TempStr = ByteToHexStr(GData, PLen);
                if (0 == Language)
                    ListBoxAdd("读写器序列号为: " + TempStr);
                else
                    ListBoxAdd("Reader serial No.: " + TempStr);
                SetSerialNoText(TempStr);
                //SetText(TempStr);
            }
            else
            {
                TempStr = "获取序列号号失败!";
                TempStrEnglish = "Get serial No. fail!";
                if (0 == Language)
                    ListBoxAdd(TempStr);
                else
                    ListBoxAdd(TempStrEnglish);
            }
        }

        public static string ByteToHexStr(byte[] byte_arr, int arr_len)
        {
            string hexstr = "";
            for (int i = 0; i < arr_len; i++)
            {
                char hex1;
                char hex2;
                int value = byte_arr[i];
                int v1 = value / 16;
                int v2 = value % 16;
                //将商转换为字母(change consult to letter)
                if (v1 >= 0 && v1 <= 9)
                {
                    hex1 = (char)(48 + v1);
                }
                else
                {
                    hex1 = (char)(55 + v1);
                }
                //将余数转成字母(change remainder to letter)
                if (v2 >= 0 && v2 <= 9)
                {
                    hex2 = (char)(48 + v2);
                }
                else
                {
                    hex2 = (char)(55 + v2);
                }
                //将字母连成一串(make letter a string)
                hexstr = hexstr + hex1 + hex2;
            }
            return hexstr;
        }

        /// <summary>
        /// 16进制字符串转字节数组
        /// </summary>
        /// <param name="byteT">目标数组</param>
        /// <param name="str">源字符串</param>
        public static void HexStrToByte(byte[] byteT, string str)
        {
            string tmp = "";
            for (int i = 0; i < str.Length / 2; i++)
            {
                tmp = str.Substring(i * 2, 2);
                byteT[i] = Convert.ToByte(tmp, 16);
            }
        }

        private void CLRSYSMSG_button_Click(object sender, EventArgs e)
        {
            SYSMSG_listBox.Items.Clear();
        }

        private void SWITCH_button_Click(object sender, EventArgs e)
        {
            int Languagef = 0;
            DialogResult select = 0;
            if (Language_comboBox.SelectedIndex == 0)
                Languagef = 0;
            else if (Language_comboBox.SelectedIndex == 1)
                Languagef = 1;
            WriteLanguageFile(Languagef);
            if (0 == Language)
                select = MessageBox.Show("重新启动应用程序?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            else
                select = MessageBox.Show("Restart the application?", "Reminder", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (select == DialogResult.Yes)			//关闭应用程序
            {
                Application.Exit();
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                return;
            }
        }

        private void GetVersionThread()
        {
            GetVersion(255);
        }

        private void GetSerialNumThread()
        {
            GetSerialNum(255);
            Thread GVT = new Thread(new ThreadStart(GetVersionThread));
            GVT.Start();
            GVT.Join();
        }

        private int ConnectReader()
        {
            int status = 0;
            string TempStr = "";
            string TempStrEnglish = "";

            if (tabControl1.SelectedIndex == 1)
            {
                status = ComConnect();
            }
            if (tabControl1.SelectedIndex == 0)
            {
                status = SocketConnect();
            }

            switch (status)
            {
                case 0:
                    TempStr = "连接成功!";
                    TempStrEnglish = "Connect success!";
                    Connectbutton.Enabled = false;
                    Disconnectbutton.Enabled = true;
                    break;
                case -1:
                    TempStr = "连接失败!";
                    TempStrEnglish = "Connect fail!";
                    Connectbutton.Enabled = true;
                    Disconnectbutton.Enabled = false;
                    break;
                case -2:
                    TempStr = "连接参数不完整!";
                    TempStrEnglish = "Connect parameter incomplete!";
                    Connectbutton.Enabled = true;
                    Disconnectbutton.Enabled = false;
                    break;
                case -3:
                    TempStr = "输入IP地址无效!";
                    TempStrEnglish = "Input IP Invalid!";
                    Connectbutton.Enabled = true;
                    Disconnectbutton.Enabled = false;
                    break;
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
            if (TempStr != "连接成功!" || TempStrEnglish != "Connect success!")
                return -1;
            return 0;
        }

        private int DisConnectReader(object sender, EventArgs e)
        {
            int status = 0;
            string TempStr = "";
            string TempStrEnglish = "";

            if (tabControl1.SelectedIndex == 1)
            {
                status = ComDisConnect();
            }
            if (tabControl1.SelectedIndex == 0)
            {
                status = SocketDisConnect();
            }

            switch (status)
            {
                case 0:
                    TempStr = "关闭成功!";
                    TempStrEnglish = "Close success!";
                    break;
                case -1:
                    TempStr = "关闭失败!";
                    TempStrEnglish = "Close fail!";
                    break;
                case -2:
                    TempStr = "关闭句柄无效!";
                    TempStrEnglish = "Handle unavailable!";
                    break;
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
            if (TempStr != "关闭成功!" || TempStrEnglish != "Close success!")
                return -1;
            return 0;
        }

        private void Connectbutton_Click(object sender, EventArgs e)
        {
            int status = 0;

            status = ConnectReader();
            if (0 == status)
            {
                if (tabControl1.SelectedIndex == 1)
                {
                    CS.hCom = hCom;
                    CS.sockClient = null;
                }
                if (tabControl1.SelectedIndex == 0)
                {
                    CS.hCom = -1;
                    CS.sockClient = sockClient;
                }
                Connectbutton.Enabled = false;
                Disconnectbutton.Enabled = true;

                ConnectFlag = 1;		//连接标志位(connect flag)

                GetVersion(255);
                GetSerialNum(255);
                GETANTENNA_button_Click(sender, e);
                GETRFPWR_button_Click(sender, e);
                GETFREQ_button_Click(sender, e);
                GETINTERNET_button_Click(sender, e);
                GETSFTM_button_Click(sender, e);
                GETOUTPUT_button_Click(sender, e);
                GETINTERFACE_button_Click(sender, e);
            }
        }

        private void Disconnectbutton_Click(object sender, EventArgs e)
        {
            int status = 0;
            status = DisConnectReader(sender, e);

            if (0 == status)
            {
                Connectbutton.Enabled = true;
                Disconnectbutton.Enabled = false;
                ConnectFlag = 0;		//连接标志位(connect falg)
            }
        }

        /*系统配置*/
        /// <summary>
        /// 设置天线工作状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETANTENNA_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            int[] ant = new int[4];
            int Workant = 0;
            if (ANT1_checkBox.Checked)
                ant[0] = 1;
            if (ANT2_checkBox.Checked)
                ant[1] = 2;
            if (ANT3_checkBox.Checked)
                ant[2] = 4;
            if (ANT4_checkBox.Checked)
                ant[3] = 8;
            Workant = ant[0] + ant[1] + ant[2] + ant[3];
            if (0x00 == rfid_sp.Config_SetAntenna(CS, Workant, 0xFF))
            {
                TempStr = "天线工作状态设置成功!";
                TempStrEnglish = "antenna work state set success";
            }
            else
            {
                TempStr = "天线工作状态设置失败!";
                TempStrEnglish = "antenna work state set failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 查询天线工作状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETANTENNA_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            int Workant = 0;
            int antStatus = 0;

            if (0x00 == rfid_sp.Config_GetAntenna(CS, ref Workant, ref antStatus, 0xFF))
            {
                TempStr = "天线工作状态查询成功!";
                TempStrEnglish = "antenna work state query success";
                if ((Workant & (1 << 0)) != 0)
                    ANT1_checkBox.Checked = true;
                else
                    ANT1_checkBox.Checked = false;
                if ((Workant & (1 << 1)) != 0)
                    ANT2_checkBox.Checked = true;
                else
                    ANT2_checkBox.Checked = false;
                if ((Workant & (1 << 2)) != 0)
                    ANT3_checkBox.Checked = true;
                else
                    ANT3_checkBox.Checked = false;
                if ((Workant & (1 << 3)) != 0)
                    ANT4_checkBox.Checked = true;
                else
                    ANT4_checkBox.Checked = false;
                //#if _DEBUG
                if ((antStatus & (1 << 0)) != 0)
                {
                    //Image img = new Bitmap(@"E:\项目文档\2014-9-3\C#版-2014-9-15\C#版\位图\Single.bmp");
                    Image img = new Bitmap(@"Single.bmp");
                    ANT1_pictureBox.Image = img;
                }
                else
                {
                    Image img = new Bitmap(@"NoSingle.bmp");
                    ANT1_pictureBox.Image = img;
                }
                if ((antStatus & (1 << 1)) != 0)
                {
                    Image img = new Bitmap(@"Single.bmp");
                    ANT2_pictureBox.Image = img;
                }
                else
                {
                    Image img = new Bitmap(@"NoSingle.bmp");
                    ANT2_pictureBox.Image = img;
                }
                if ((antStatus & (1 << 2)) != 0)
                {
                    Image img = new Bitmap(@"Single.bmp");
                    ANT3_pictureBox.Image = img;
                }
                else
                {
                    Image img = new Bitmap(@"NoSingle.bmp");
                    ANT3_pictureBox.Image = img;
                }
                if ((antStatus & (1 << 3)) != 0)
                {
                    Image img = new Bitmap(@"Single.bmp");
                    ANT4_pictureBox.Image = img;
                }
                else
                {
                    Image img = new Bitmap(@"NoSingle.bmp");
                    ANT4_pictureBox.Image = img;
                }
                //#endif
            }
            else
            {
                TempStr = "天线工作状态查询失败!";
                TempStrEnglish = "antenna work state query failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETRFPWR_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte[] aPwr = new byte[4];

            int m_anten1_dbm = 0;
            int m_anten2_dbm = 0;
            int m_anten3_dbm = 0;
            int m_anten4_dbm = 0;

            m_anten1_dbm = Convert.ToInt32(ANT1_textBox.Text);
            m_anten2_dbm = Convert.ToInt32(ANT2_textBox.Text);
            m_anten3_dbm = Convert.ToInt32(ANT3_textBox.Text);
            m_anten4_dbm = Convert.ToInt32(ANT4_textBox.Text);

            m_anten1_dbm = (m_anten1_dbm < 0) ? 0 : m_anten1_dbm;
            m_anten1_dbm = (m_anten1_dbm > 32) ? 32 : m_anten1_dbm;

            m_anten2_dbm = (m_anten2_dbm < 0) ? 0 : m_anten2_dbm;
            m_anten2_dbm = (m_anten2_dbm > 32) ? 32 : m_anten2_dbm;

            m_anten3_dbm = (m_anten3_dbm < 0) ? 0 : m_anten3_dbm;
            m_anten3_dbm = (m_anten3_dbm > 32) ? 32 : m_anten3_dbm;

            m_anten4_dbm = (m_anten4_dbm < 0) ? 0 : m_anten4_dbm;
            m_anten4_dbm = (m_anten4_dbm > 32) ? 32 : m_anten4_dbm;

            aPwr[0] = (byte)((m_anten1_dbm) & 0xFF);
            aPwr[1] = (byte)((m_anten2_dbm) & 0xFF);
            aPwr[2] = (byte)((m_anten3_dbm) & 0xFF);
            aPwr[3] = (byte)((m_anten4_dbm) & 0xFF);
            if (0x00 == rfid_sp.Config_SetRfPower(CS, aPwr, 0xFF))
            {
                TempStr = "天线功率设置成功!";
                TempStrEnglish = "antenna power set success";
            }
            else
            {
                TempStr = "天线功率设置失败!";
                TempStrEnglish = "antenna power set failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 查询功率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETRFPWR_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte[] aPwr = new byte[4];

            if (0x00 == rfid_sp.Config_GetRfPower(CS, aPwr, 0xFF))
            {
                TempStr = "天线功率查询成功!";
                TempStrEnglish = "antenna power query success";

                ANT1_textBox.Text = string.Format("{0:D}", (int)aPwr[0]);
                ANT2_textBox.Text = string.Format("{0:D}", (int)aPwr[1]);
                ANT3_textBox.Text = string.Format("{0:D}", (int)aPwr[2]);
                ANT4_textBox.Text = string.Format("{0:D}", (int)aPwr[3]);
            }
            else
            {
                TempStr = "天线功率查询失败!";
                TempStrEnglish = "antenna power query failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 设置网口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETINTERNET_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            string IPAddr = "";
            string MaskCode = "";
            string GateWay = "";
            int InternetPort = 0;
            byte[] IPAddrbuf = new byte[4];
            byte[] MaskCodebuf = new byte[4];
            byte[] GateWaybuf = new byte[4];
            byte[] InternetPortbuf = new byte[2];

            IPAddr = IPADDR_textBox.Text;
            MaskCode = MASK_textBox.Text;
            GateWay = GATEWAY_textBox.Text;
            try
            {
                InternetPort = Convert.ToInt32(IPPORT_textBox.Text);
            }
            catch (System.Exception excp)
            {
                InternetPort = 0;
            }


            DecimalstrToByte(IPAddrbuf, IPAddr);
            DecimalstrToByte(MaskCodebuf, MaskCode);
            DecimalstrToByte(GateWaybuf, GateWay);
            InternetPortbuf[0] = (byte)((InternetPort) >> 8);
            InternetPortbuf[1] = (byte)((InternetPort) & 0xFF);

            if (0x00 == rfid_sp.Config_SetIntrnetAccess(CS, IPAddrbuf, MaskCodebuf, GateWaybuf, InternetPortbuf, 0xFF))
            {
                TempStr = "设置读写器网口参数成功!";
                TempStrEnglish = "Reader ethernet set success!";
                RESET_button_Click(sender, e);
            }
            else
            {
                TempStr = "设置读写器网口参数失败!";
                TempStrEnglish = "Reader ethernet set fail!";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 查询网口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETINTERNET_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            string IPAddr = "";
            string MaskCode = "";
            string GateWay = "";
            int InternetPort = 0;
            byte[] IPAddrbuf = new byte[4];
            byte[] MaskCodebuf = new byte[4];
            byte[] GateWaybuf = new byte[4];
            byte[] InternetPortbuf = new byte[2];

            if (0x00 == rfid_sp.Config_GetIntrnetAccess(CS, IPAddrbuf, MaskCodebuf, GateWaybuf, InternetPortbuf, 0xFF))
            {
                TempStr = "查询读写器网口参数成功!";
                TempStrEnglish = "Reader ethernet query success!";
                ByteToDecimalstr(ref IPAddr, IPAddrbuf);
                ByteToDecimalstr(ref MaskCode, MaskCodebuf);
                ByteToDecimalstr(ref GateWay, GateWaybuf);
                InternetPort = (InternetPortbuf[0] << 8) + (int)InternetPortbuf[1];
                IPADDR_textBox.Text = IPAddr;
                MASK_textBox.Text = MaskCode;
                GATEWAY_textBox.Text = GateWay;
                IPPORT_textBox.Text = Convert.ToString(InternetPort);
            }
            else
            {
                TempStr = "查询读写器网口参数失败!";
                TempStrEnglish = "Reader ethernet query fail!";
            }

            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 设置读写器射频参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETFREQ_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            if (CHINA_radioButton.Checked)
            {
                Freqnum = 0;
                Array.Clear(Freqpoints, 0, 124);
                Freqpoints[0] = 0;
            }
            if (NORTHAMERICA_radioButton.Checked)
            {
                Freqnum = 0;
                Array.Clear(Freqpoints, 0, 124);
                Freqpoints[0] = 1;
            }
            if (EUROPE_radioButton.Checked)
            {
                Freqnum = 0;
                Array.Clear(Freqpoints, 0, 124);
                Freqpoints[0] = 2;
            }

            if (0x00 == rfid_sp.Config_SetFreqPoint(CS, Freqnum, Freqpoints, 0xFF))
            {
                TempStr = "天线频率率设置成功!";
                TempStrEnglish = "antenna frequency set success";
            }
            else
            {
                TempStr = "天线频率设置失败!";
                TempStrEnglish = "antenna frequency set failed";
            }

            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 查询读写器射频参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETFREQ_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            if (0x00 == rfid_sp.Config_GetFreqPoint(CS, ref Freqnum, Freqpoints, 0xFF))
            {
                TempStr = "天线频率查询成功!";
                TempStrEnglish = "antenna frequency query success";
                if (0 == Freqnum)
                {
                    if (0 == Freqpoints[0])
                        CHINA_radioButton.Checked = true;
                    else
                        CHINA_radioButton.Checked = false;

                    if (1 == Freqpoints[0])
                        NORTHAMERICA_radioButton.Checked = true;
                    else
                        NORTHAMERICA_radioButton.Checked = false;

                    if (2 == Freqpoints[0])
                        EUROPE_radioButton.Checked = true;
                    else
                        EUROPE_radioButton.Checked = false;

                    OTHERS_radioButton.Checked = false;
                }
                else
                {
                    CHINA_radioButton.Checked = false;
                    NORTHAMERICA_radioButton.Checked = false;
                    EUROPE_radioButton.Checked = false;
                    OTHERS_radioButton.Checked = true;
                    //FreqPointsForm freqPoints = new FreqPointsForm();
                    //freqPoints.ShowDialog();
                }
            }
            else
            {
                TempStr = "天线频率查询失败!";
                TempStrEnglish = "antenna frequency query failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        private void OTHERS_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            //FreqPointsForm freqPoints = new FreqPointsForm();
            //freqPoints.ShowDialog();
        }

        private void OTHERS_radioButton_MouseClick(object sender, MouseEventArgs e)
        {
            //FreqPointsForm freqPoints = new FreqPointsForm();
            //freqPoints.ShowDialog();  
        }

        /// <summary>
        /// 设置可编程IO口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETOUTPORT_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte Num = 0x00;
            byte Level = 0x00;
            if (IO1_radioButton.Checked)
                Num = 0x00;
            if (IO2_radioButton.Checked)
                Num = 0x01;
            if (RELAY_radioButton.Checked)
                Num = 0x02;
            if (LOWLEVEL_radioButton.Checked)
                Level = 0x00;
            if (HIGHLEVEL_radioButton.Checked)
                Level = 0x01;
            if (!LOWLEVEL_radioButton.Checked && !HIGHLEVEL_radioButton.Checked)
                Level = 0x01;
            if (0x00 == rfid_sp.Config_SetOutPort(CS, Num, Level, 0xFF))
            {
                TempStr = "设置读写器设置可编程IO口成功!";
                TempStrEnglish = "set reader outport IO port success";
            }
            else
            {
                TempStr = "设置读写器设置可编程IO口失败!";
                TempStrEnglish = "set reader outport IO port failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 查询可编程IO口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETOUTPUT_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte Num = 0x00;
            byte Level = 0x00;
            if (IO1_radioButton.Checked)
                Num = 0x00;
            if (IO2_radioButton.Checked)
                Num = 0x01;
            if (RELAY_radioButton.Checked)
                Num = 0x02;

            if (0x00 == rfid_sp.Config_GetInPort(CS, Num, ref Level, 0xFF))
            {
                TempStr = "查询读写器设置可编程IO口成功!";
                TempStrEnglish = "query reader set outport IO port success";
            }
            else
            {
                TempStr = "查询读写器设置可编程IO口失败!";
                TempStrEnglish = "query reader set outport IO port failed";
            }
            if (0x00 == Level)
            {
                LOWLEVEL_radioButton.Checked = true;
                HIGHLEVEL_radioButton.Checked = false;
            }
            if (0x01 == Level)
            {
                LOWLEVEL_radioButton.Checked = false;
                HIGHLEVEL_radioButton.Checked = true;
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 设置读写器载波抵消策略
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETSFTM_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte Mode = 0x00;
            if (SINGLETAG_radioButton.Checked)
            {
                Mode = 0x00;
                if (0x00 == rfid_sp.Config_SetSingleFastTagMode(CS, Mode, 0xFF))
                {
                    TempStr = "设置读写器载波抵消策略成功!";
                    TempStrEnglish = "set single fast tag mode success";
                    CURRENTMODE_textBox.Text = "单卡快速模式";
                    CURRENTMODE_textBox.Text = "single tag fast mode";

                }
                else
                {
                    TempStr = "设置读写器载波抵消策略失败!";
                    TempStrEnglish = "set single fast tag mode failed";
                }
            }
            if (MULTITAG_radioButton.Checked)
            {
                Mode = 0x01;
                if (0x00 == rfid_sp.Config_SetSingleFastTagMode(CS, Mode, 0xFF))
                {
                    TempStr = "设置读写器载波抵消策略成功!";
                    TempStrEnglish = "set reader single fast tag mode success";
                    CURRENTMODE_textBox.Text = "多卡模式";
                    CURRENTMODE_textBox.Text = "multitag mode";
                }
                else
                {
                    TempStr = "设置读写器载波抵消策略失败!";
                    TempStrEnglish = "set reader single fast tag mode failed";
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 查询读写器载波抵消策略
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETSFTM_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte Mode = 0x00;
            if (0x00 == rfid_sp.Config_GetSingleFastTagMode(CS, ref Mode, 0xFF))
            {
                TempStr = "查询读写器载波抵消策略成功!";
                TempStrEnglish = "query reader single fast tag mode success";
                if (0x00 == Mode)
                {
                    SINGLETAG_radioButton.Checked = true;
                    if (0 == Language)
                        CURRENTMODE_textBox.Text = "单卡快读模式";
                    else
                        CURRENTMODE_textBox.Text = "single tag fast mode";
                }
                else
                {
                    MULTITAG_radioButton.Checked = true;
                    if (0 == Language)
                        CURRENTMODE_textBox.Text = "多卡模式";
                    else
                        CURRENTMODE_textBox.Text = "multitag mode";
                }
            }
            else
            {
                TempStr = "查询读写器载波抵消策略失败!";
                TempStrEnglish = "query reader single fast tag mode failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 设置模式-接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SETINTERFACE_button_Click(object sender, EventArgs e)
        {
            string Text = SETINTERFACE_button.Text;
            WorkModel(255, Text);
            OutInterface(255, Text);
        }

        /// <summary>
        /// 查询模式-接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETINTERFACE_button_Click(object sender, EventArgs e)
        {
            string Text = GETINTERFACE_button.Text;
            WorkModel(255, Text);
            OutInterface(255, Text);
        }

        /*****************************设置或查询工作模式***************************************/
        int WorkModel(int ReaderNum, string Text)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte ReaderAddr = (byte)(ReaderNum & 0xFF);
            byte PAddr = 0x90;
            int PLen = 1;
            int workmodel = 0;
            byte[] PSetData = new byte[1];
            byte[] PRefData = new byte[1024];

            if (Text == "设置" || Text == "Set")
            {
                if (CMD_radioButton.Checked)
                    workmodel = 0;
                if (ANIMATION_radioButton.Checked)
                    workmodel = 1;
                if (TRIGGER_radioButton.Checked)
                    workmodel = 2;
                PSetData[0] = (byte)(workmodel & 0xFF);
                if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                {
                    TempStr = "工作模式设置成功!";
                    TempStrEnglish = "Work model set success!";
                    if (TRIGGER_radioButton.Checked)
                        TriggerTime(255, Text);
                }
                else
                {
                    TempStr = "工作模式设置失败!";
                    TempStrEnglish = "Work model set fail!";
                }
            }
            else if (Text == "查询" || Text == "Query")
            {
                if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                {
                    workmodel = (int)PRefData[0];
                    if (1 == workmodel)
                    {
                        CMD_radioButton.Checked = false;
                        ANIMATION_radioButton.Checked = true;
                        TRIGGER_radioButton.Checked = false;
                    }
                    else if (2 == workmodel)
                    {
                        CMD_radioButton.Checked = false;
                        ANIMATION_radioButton.Checked = false;
                        TRIGGER_radioButton.Checked = true;
                    }
                    else
                    {
                        CMD_radioButton.Checked = true;
                        ANIMATION_radioButton.Checked = false;
                        TRIGGER_radioButton.Checked = false;
                    }
                    TempStr = "查询工作模式成功!";
                    TempStrEnglish = "Work model query success!";
                }
                else
                {
                    TempStr = "查询工作模式失败!";
                    TempStrEnglish = "Work model query fail!";
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
            return 0;
        }

        /********************************设置或查询接口***************************************/
        int OutInterface(int ReaderNum, string Text)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte ReaderAddr = (byte)(ReaderNum & 0xFF);
            byte PAddr = 0x00;
            int PLen = 1;
            byte[] PSetData = new byte[1];
            byte[] PRefData = new byte[1024];

            int OutPort = 0;
            if (Text == "设置" || Text == "Set")
            {
                if (RS485_checkBox.Checked)
                    OutPort = OutPort + 1;
                if (WEIGAN_checkBox.Checked)
                    OutPort = OutPort + 2;
                if (RS232_checkBox.Checked)
                    OutPort = OutPort + 4;
                if (IP_checkBox.Checked)
                    OutPort = OutPort + 8;
                if (RELAY_checkBox.Checked)
                    OutPort = OutPort + 16;
                PAddr = 0x97;
                PSetData[0] = (byte)(OutPort & 0xFF);
                if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                {
                    TempStr = "设置输出接口成功!";
                    TempStrEnglish = "OutInterface set success!";
                }
                else
                {
                    TempStr = "设置输出接口失败!";
                    TempStrEnglish = "OutInterface set fail!";
                }
                if (0 == Language)
                    ListBoxAdd(TempStr);
                else
                    ListBoxAdd(TempStrEnglish);

                if (WEIGAN_checkBox.Checked)
                {
                    WeigenStyle(ReaderNum, Text);
                }
                if (RELAY_checkBox.Checked)
                {
                    RelayDelayTime(ReaderNum, Text);
                }
            }
            else if (Text == "查询" || Text == "Query")
            {
                PAddr = 0x97;
                if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                {
                    OutPort = (int)PRefData[0];
                    if ((OutPort & (1 << 0)) != 0)
                        RS485_checkBox.Checked = true;
                    else
                        RS485_checkBox.Checked = false;
                    if ((OutPort & (1 << 1)) != 0)
                        WEIGAN_checkBox.Checked = true;
                    else
                        WEIGAN_checkBox.Checked = false;
                    if ((OutPort & (1 << 2)) != 0)
                        RS232_checkBox.Checked = true;
                    else
                        RS232_checkBox.Checked = false;
                    if ((OutPort & (1 << 3)) != 0)
                        IP_checkBox.Checked = true;
                    else
                        IP_checkBox.Checked = false;
                    if ((OutPort & (1 << 4)) != 0)
                        RELAY_checkBox.Checked = true;
                    else
                        RELAY_checkBox.Checked = false;
                    TempStr = "查询输出接口成功!";
                    TempStrEnglish = "OutInterface query success!";
                    if (0 == Language)
                        ListBoxAdd(TempStr);
                    else
                        ListBoxAdd(TempStrEnglish);
                    if ((OutPort & (1 << 1)) != 0)
                        WeigenStyle(ReaderNum, Text);
                    if ((OutPort & (1 << 4)) != 0)
                        RelayDelayTime(ReaderNum, Text);
                }
                else
                {
                    TempStr = "查询输出接口失败!";
                    TempStrEnglish = "OutInterface query fail!";
                    if (0 == Language)
                        ListBoxAdd(TempStr);
                    else
                        ListBoxAdd(TempStrEnglish);
                }
            }
            return 0;
        }

        /********************************设置触发时间***************************************/
        int TriggerTime(int ReaderNum, string Text)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte ReaderAddr = (byte)(ReaderNum & 0xFF);
            byte PAddr = 0x92;
            int PLen = 1;
            int triggertime = 0;
            byte[] PSetData = new byte[1];
            byte[] PRefData = new byte[1024];
            triggertime = Convert.ToInt32(TRIGGERTIME_textBox.Text);
            if (TRIGGERTIME_textBox.Text == "" || triggertime == 0)
            {
                TempStr = "触发时间不能为空或者0!";
                TempStrEnglish = "Trigger time doesn't null or zero!";
            }
            else
            {
                PSetData[0] = (byte)(triggertime & 0xFF);
                if (Text == "设置" || Text == "Set")
                {
                    if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                    {
                        TempStr = "设置触发时间成功!";
                        TempStrEnglish = "Trigger time set success!";
                    }
                    else
                    {
                        TempStr = "设置触发时间失败!";
                        TempStrEnglish = "Trigger time set fail!";
                    }
                }
                else if (Text == "查询" || Text == "Query")
                {
                    if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                    {
                        triggertime = (int)PRefData[0];
                        TRIGGERTIME_textBox.Text = triggertime.ToString();
                        TempStr = "查询触发时间成功!";
                        TempStrEnglish = "Trigger time query success!";
                    }
                    else
                    {
                        TempStr = "查询触发时间失败!";
                        TempStrEnglish = "Trigger time query fail!";
                    }
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
            return 0;
        }

        /********************************设置韦根口类型***************************************/
        void WeigenStyle(int ReaderNum, string Text)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte ReaderAddr = (byte)(ReaderNum & 0xFF);
            byte PAddr = 0x00;
            int PLen = 1;
            byte[] PSetData = new byte[1];
            byte[] PRefData = new byte[1024];
            int Weigen = 0;
            if (Text == "设置" || Text == "Set")
            {
                Weigen = WEIGANSTYLE_comboBox.SelectedIndex;
                PSetData[0] = (byte)(Weigen & 0xFF);
                PAddr = 0x98;
                if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                {
                    TempStr = "设置韦根类型成功!";
                    TempStrEnglish = "Weigen style set success!";
                }
                else
                {
                    TempStr = "设置韦根类型失败!";
                    TempStrEnglish = "Weigen style set fail!";
                }
            }
            else if (Text == "查询" || Text == "Query")
            {
                PAddr = 0x98;
                if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                {
                    Weigen = (int)PRefData[0];
                    WEIGANSTYLE_comboBox.SelectedIndex = Weigen;
                    TempStr = "查询韦根类型成功!";
                    TempStrEnglish = "Weigen style query success!";
                }
                else
                {
                    TempStr = "查询韦根类型失败!";
                    TempStrEnglish = "Weigen style query fail!";
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /********************************设置继电器时间***************************************/
        void RelayDelayTime(int ReaderNum, string Text)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            byte ReaderAddr = (byte)(ReaderNum & 0xFF);
            byte PAddr = 0x00;
            int PLen = 1;
            byte[] PSetData = new byte[1];
            byte[] PRefData = new byte[1024];
            int Relay = 0;
            if (Text == "设置" || Text == "Set")
            {
                Relay = Convert.ToInt32(DELAYTIME_textBox.Text);
                if (DELAYTIME_textBox.Text == "" || Relay == 0)
                {
                    TempStr = "继电器保持时间不能为空或者0!";
                    TempStrEnglish = "Relay delaytime doesn't null or zero!";
                }
                else
                {
                    PSetData[0] = (byte)(Relay & 0xFF);
                    PAddr = 0x99;
                    if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                    {
                        TempStr = "设置继电器保持时间成功!";
                        TempStrEnglish = "Relay delaytime set success!";
                    }
                    else
                    {
                        TempStr = "设置继电器保持时间失败!";
                        TempStrEnglish = "Relay delaytime set fail!";
                    }
                }
            }
            else if (Text == "查询" || Text == "Query")
            {
                PAddr = 0x99;
                if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                {
                    Relay = (int)PRefData[0];
                    DELAYTIME_textBox.Text = Relay.ToString();
                    TempStr = "查询继电器保持时间成功!";
                    TempStrEnglish = "Relay delaytime query success!";
                }
                else
                {
                    TempStr = "查询继电器保持时间成功!";
                    TempStrEnglish = "Relay delaytime query success!";
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        void InterfaceRadioCheck()
        {
            if (WEIGAN_checkBox.Checked)
            {
                WEIGANSTYLE_comboBox.Visible = true;
                WEIGANSTYLE_label.Visible = true;
            }
            else
            {
                WEIGANSTYLE_comboBox.Visible = false;
                WEIGANSTYLE_label.Visible = false;
            }
            if (RELAY_checkBox.Checked)
            {
                DELAYTIME_textBox.Visible = true;
                DELAYTIME_label.Visible = true;
            }
            else
            {
                DELAYTIME_textBox.Visible = false;
                DELAYTIME_label.Visible = false;
            }
        }

        void WorkModelRadioCheck()
        {
            if (TRIGGER_radioButton.Checked)
            {
                TRIGGERTIME_textBox.Visible = true;
                TRIGGERTIME_label.Visible = true;
            }
            else
            {
                TRIGGERTIME_textBox.Visible = false;
                TRIGGERTIME_label.Visible = false;
            }
        }

        //CertificationForm ctf = new CertificationForm();
        private void CERTIFICATION_button_Click(object sender, EventArgs e)
        {
            //ctf.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            InterfaceRadioCheck();
            WorkModelRadioCheck();
            CheckRelay();
        }

        /// <summary>
        /// 复位读写器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RESET_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            if (0x00 == rfid_sp.Config_ResetReader(CS, 0xFF))
            {
                TempStr = "读写器复位成功!";
                TempStrEnglish = "reset reader success";
                DisConnectReader(sender, e);
                Connectbutton.Enabled = true;
                Disconnectbutton.Enabled = false;
            }
            else
            {
                TempStr = "读写器复位失败!";
                TempStrEnglish = "reset reader failed";
                Connectbutton.Enabled = false;
                Disconnectbutton.Enabled = true;
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /*ISO18000-6B*/
        void InitTagDataListView()
        {
            TAGDATA_listView.Columns.Clear();
            TAGDATA_listView.Items.Clear();
            if (Language == 0)
            {
                TAGDATA_listView.Columns.Add("序号", 127, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("ISO18000-6B识别ID", 406, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("次数", 78, HorizontalAlignment.Center);
            }
            else
            {
                TAGDATA_listView.Columns.Add("No.", 127, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("ISO18000-6B Identify ID", 406, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("Times", 78, HorizontalAlignment.Center);
            }
        }

        private void MULTITAGIDENTIFY_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            READADDR_comboBox.Visible = false;
            READADDR_label.Visible = false;
            if (0 == Language)
                START_button.Text = "开始识别";
            else
                START_button.Text = "StartIdentify";

            InitTagDataListView();
        }

        private void MULTITAGREAD_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            READADDR_comboBox.Visible = true;
            READADDR_label.Visible = true;
            for (int i = 0; i < 17; i++)
            {
                READADDR_comboBox.Items.Add(i);
            }
            READADDR_comboBox.SelectedIndex = 0;
            TAGDATA_listView.Items.Clear();
            TAGDATA_listView.Columns.Clear();
            if (Language == 0)
            {
                START_button.Text = "开始读取";
                TAGDATA_listView.Columns.Add("序号", 127, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("ISO18000-6B读取数据", 406, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("次数", 78, HorizontalAlignment.Center);
            }
            else
            {
                START_button.Text = "Start Read";
                TAGDATA_listView.Columns.Add("No.", 127, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("ISO18000-6B Read Data", 406, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("Times", 78, HorizontalAlignment.Center);
            }
        }

        private void SINGLEREAD_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Language == 0)
                OPERATION_button.Text = "读取";
            else
                OPERATION_button.Text = "Read";
            QUERYLOCK_button.Visible = false;
            UID_checkBox.Enabled = true;
            RESULT_listBox.Items.Clear();
        }

        private void SINGLEWRITE_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Language == 0)
                OPERATION_button.Text = "写入";
            else
                OPERATION_button.Text = "Write";
            QUERYLOCK_button.Visible = false;
            UID_checkBox.Enabled = true;
            RESULT_listBox.Items.Clear();
        }

        private void SINGLELOCK_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Language == 0)
            {
                OPERATION_button.Text = "锁定";
                QUERYLOCK_button.Text = "查询";
            }
            else
            {
                OPERATION_button.Text = "Lock";
                QUERYLOCK_button.Text = "Query";
            }
            QUERYLOCK_button.Visible = true;
            UID_checkBox.Enabled = false;
            UID_checkBox.Checked = false;
            RESULT_listBox.Items.Clear();
        }

        private void START_button_Click(object sender, EventArgs e)
        {
            TAGDATA_listView.Items.Clear();
            string Text = START_button.Text;
            if (Text == "开始识别" || Text == "StartIdentify")
            {
                if (Language == 0)
                    ListBoxAdd("开始识别");
                else
                    ListBoxAdd("Start Identify");

                OperantionTime_timer.Enabled = true;
                MultiTagIdentify_timer.Enabled = true;
            }
            if (Text == "开始读取" || Text == "StartRead")
            {
                if (Language == 0)
                    ListBoxAdd("开始读取");
                else
                    ListBoxAdd("Start Read");

                OperantionTime_timer.Enabled = true;
                MultiTagRead_timer.Enabled = true;
                READADDR_comboBox.Enabled = false;
            }
            START_button.Enabled = false;
            STOP_button.Enabled = true;
        }

        private void STOP_button_Click(object sender, EventArgs e)
        {
            string Text = START_button.Text;
            if (Text == "开始识别" || Text == "StartIdentify")
            {
                MultiTagIdentify_timer.Enabled = false;
                OperantionTime_timer.Enabled = false;
                if (Language == 0)
                    ListBoxAdd("停止识别");
                else
                    ListBoxAdd("Stop Identify");
            }
            if (Text == "开始读取" || Text == "StartRead")
            {
                MultiTagRead_timer.Enabled = false;
                OperantionTime_timer.Enabled = false;
                if (Language == 0)
                    ListBoxAdd("停止读取");
                else
                    ListBoxAdd("Stop Read");
            }
            START_button.Enabled = true;
            READADDR_comboBox.Enabled = true;
            STOP_button.Enabled = false;
        }

        private void OPERATION_button_Click(object sender, EventArgs e)
        {
            string Text = OPERATION_button.Text;
            if (Text == "读取" || Text == "Read")
            {
                SingleTagRead();
            }
            if (Text == "写入" || Text == "Write")
            {
                SingleTagWrite();
            }
            if (Text == "锁定" || Text == "Lock")
            {
                SingleSetTagLock();
            }
        }

        private void QUERYLOCK_button_Click(object sender, EventArgs e)
        {
            SingleQueryTagLock();
        }

        private void UID_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UID_checkBox.Checked)
                TAGID_textBox.Enabled = true;
            else
                TAGID_textBox.Enabled = false;
        }

        private void CLEAR_button_Click(object sender, EventArgs e)
        {
            TAGDATA_listView.Items.Clear();
            ELAPSEDTIME_label.Text = "";
            TOTALCOUNT_label.Text = "";
            OperTime6B = 0;
        }

        private void CLEARRESULT_button_Click(object sender, EventArgs e)
        {
            RESULT_listBox.Items.Clear();
        }

        private void TAGDATA_listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TAGDATA_listView.SelectedIndices.Count > 0)
                TAGID_textBox.Text = TAGDATA_listView.SelectedItems[0].SubItems[1].Text;
        }

        /// <summary>
        /// 界面更新
        /// </summary>
        /// <param name="tagID"></param>
        void DisplayNewTag(string tagID)
        {
            string SN = string.Format("{0:D}", DisplayCnt6B + 1);
            ListViewItem lvi = new ListViewItem(SN);
            lvi.SubItems.Add(tagID);
            lvi.SubItems.Add("1");
            TAGDATA_listView.Items.Add(lvi);
            TAGDATA_listView.EnsureVisible(TAGDATA_listView.Items.Count - 1);
            TOTALCOUNT_label.Text = SN;
        }

        void ResultAddString(string Str)
        {
            RESULT_listBox.Items.Add(Str);
            RESULT_listBox.TopIndex = RESULT_listBox.Items.Count - 1;
        }

        void OperationTime()
        {
            OperTime6B++;
            ELAPSEDTIME_label.Text = string.Format("{0:D}", OperTime6B);
        }

        /// <summary>
        /// 多标签识别
        /// </summary>
        void MultiTagIdentify()
        {
            int TagCount = 0;
            int GetCount = 0;
            int i, j = 0;
            int status;
            string TagID = "";
            BufferData[] Data = new BufferData[256];
            for (int index = 0; index < Data.Length; index++)
            {
                Data[index].Data = new byte[512];
            }

            status = rfid_sp.ISO_MultiTagIdentify(CS, ref TagCount, 0xFF);
            if (0x00 == status)
            {
                if (0 != TagCount)		//标签识别成功//tag identify success
                {
                    status = rfid_sp.BufferM_GetTagData(CS, ref GetCount, Data, 0xFF);
                    if (0x00 == status)
                    {
                        for (i = 0; i < GetCount; i++)
                        {
                            TagID = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}",
                                Data[i].Data[0], Data[i].Data[1], Data[i].Data[2], Data[i].Data[3],
                                Data[i].Data[4], Data[i].Data[5], Data[i].Data[6], Data[i].Data[7]);
                            if (TAGDATA_listView.Items.Count <= 0)
                            {
                                DisplayNewTag(TagID);
                                DisplayCnt6B++;
                            }
                            else
                            {
                                int flg = -1;
                                for (j = 0; j < TAGDATA_listView.Items.Count; j++)
                                {
                                    if (TagID == TAGDATA_listView.Items[j].SubItems[1].Text)
                                    {
                                        TAGDATA_listView.Items[j].SubItems[2].Text =
                                            Convert.ToString(Convert.ToInt32(TAGDATA_listView.Items[j].SubItems[2].Text) + 1);
                                        flg = i;
                                    }
                                }
                                if (flg < 0)
                                {
                                    DisplayNewTag(TagID);
                                    DisplayCnt6B++;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 多标签读取
        /// </summary>
        void MultiTagRead()
        {
            int TagCount = 0;
            int GetCount = 0;
            int i, j = 0;
            int status;
            string TagID = "";
            int RAddr = READADDR_comboBox.SelectedIndex;
            BufferData[] Data = new BufferData[256];
            for (int index = 0; index < Data.Length; index++)
            {
                Data[index].Data = new byte[512];
            }

            status = rfid_sp.ISO_MultiTagRead(CS, RAddr, ref TagCount, 0xFF);
            if (0x00 == status)
            {
                if (0 != TagCount)		//标签识别成功//tag identify success
                {
                    status = rfid_sp.BufferM_GetTagData(CS, ref GetCount, Data, 0xFF);
                    if (0x00 == status)
                    {
                        for (i = 0; i < GetCount; i++)
                        {
                            TagID = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}",
                                Data[i].Data[0], Data[i].Data[1], Data[i].Data[2], Data[i].Data[3],
                                Data[i].Data[4], Data[i].Data[5], Data[i].Data[6], Data[i].Data[7]);
                            if (TAGDATA_listView.Items.Count <= 0)
                            {
                                DisplayNewTag(TagID);
                                DisplayCnt6B++;
                            }
                            else
                            {
                                int flg = -1;
                                for (j = 0; j < TAGDATA_listView.Items.Count; j++)
                                {
                                    if (TagID == TAGDATA_listView.Items[j].SubItems[1].Text)
                                    {
                                        TAGDATA_listView.Items[j].SubItems[2].Text =
                                            Convert.ToString(Convert.ToInt32(TAGDATA_listView.Items[j].SubItems[2].Text) + 1);
                                        flg = i;
                                    }
                                }
                                if (flg < 0)
                                {
                                    DisplayNewTag(TagID);
                                    DisplayCnt6B++;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 单标签读取
        /// </summary>
        void SingleTagRead()
        {
            int status = 0;
            byte RAddr = 0;
            byte[] UID = new byte[8];
            byte[] RecvData = new byte[12];
            string ID = "";
            string ReadData = "";
            RAddr = (byte)TAGADDR_comboBox.SelectedIndex;

            if (UID_checkBox.Checked)
            {
                ID = TAGID_textBox.Text;
                HexStrToByte(UID, ID);
                status = rfid_sp.ISO_TagReadWithUID(CS, UID, RAddr, RecvData, 0xFF);
            }
            else
            {
                status = rfid_sp.ISO_TagRead(CS, RAddr, RecvData, 0xFF);
            }
            if (0x00 == status)
            {
                Array.Copy(RecvData, 1, RecvData, 0, 8);
                ReadData = ByteToHexStr(RecvData, 8);
                ReadData = "Read:  " + ReadData;
                ResultAddString(ReadData);
                if (0 == Language)
                    ListBoxAdd("IsoI8000单标签标签读取成功!");
                else
                    ListBoxAdd("IsoI8000 single tag read success!");

            }
            else		//读取失败//read failed	
            {
                ReadData = "Read fail, please again...";
                ResultAddString(ReadData);
                if (0 == Language)
                    ListBoxAdd("IsoI8000单标签标签读取失败!");
                else
                    ListBoxAdd("IsoI8000 single tag read failed!");

            }
        }

        /// <summary>
        /// 单标签写入
        /// </summary>
        void SingleTagWrite()
        {
            int status = 0;
            int WAddr = 0;
            byte[] UID = new byte[8];
            byte[] RecvData = new byte[9];
            byte[] WData = new byte[301];
            string ID = "";
            string WriteStr = "";
            int WriteLen = 0;
            WAddr = TAGADDR_comboBox.SelectedIndex;
            WriteStr = VALUE_textBox.Text;
            WriteLen = WriteStr.Length;
            HexStrToByte(WData, WriteStr);
            if (0 != WriteLen)
            {
                if (UID_checkBox.Checked)
                {
                    ID = TAGID_textBox.Text;
                    HexStrToByte(UID, ID);
                    status = rfid_sp.ISO_TagWriteWithUID(CS, UID, WAddr, WData, WriteLen / 2, 0xFF);
                }
                else
                {
                    status = rfid_sp.ISO_TagWrite(CS, WAddr, WData, WriteLen / 2, 0xFF);
                }

                if (0x00 == status)
                {
                    if (0 == Language)
                    {
                        ResultAddString("写入数据成功");
                        ListBoxAdd("IsoI8000单标签标签写入成功!");
                    }
                    else
                    {
                        ResultAddString("write data success");
                        ListBoxAdd("IsoI8000 single tag write success!");
                    }
                }
                else
                {
                    if (0 == Language)
                    {
                        ResultAddString("写入数据失败");
                        ListBoxAdd("IsoI8000单标签标签写入失败!");
                    }
                    else
                    {
                        ResultAddString("write data failed");
                        ListBoxAdd("IsoI8000 single tag wrie failed!");
                    }
                }
            }
            else
            {
                if (0 == Language)
                {
                    ResultAddString("写入数据为空!");
                    ListBoxAdd("IsoI8000单标签标签写入数据为空!");
                }
                else
                {
                    ResultAddString("write data is null!");
                    ListBoxAdd("IsoI8000 single tag write data is null!");
                }
            }
        }

        /// <summary>
        /// 单标签锁定设置
        /// </summary>
        void SingleSetTagLock()
        {
            int status = 0;
            int LAddr = 0;
            byte[] UID = new byte[8];
            string ID = "";
            LAddr = TAGADDR_comboBox.SelectedIndex;
            if (UID_checkBox.Checked)
            {
                ID = TAGID_textBox.Text;
                HexStrToByte(UID, ID);
                status = rfid_sp.ISO_SetTagLockWithUID(CS, UID, LAddr, 0xFF);
            }
            else
            {
                status = rfid_sp.ISO_SetTagLock(CS, LAddr, 0xFF);
            }

            if (0x00 == status)
            {
                if (0 == Language)
                {
                    ResultAddString("tag data lock success");
                    ListBoxAdd("IsoI8000 single tag data lock success!");
                }
                else
                {
                    ResultAddString("tag data lock success");
                    ListBoxAdd("IsoI8000 single tag data lock success!");
                }
            }
            else
            {
                if (0 == Language)
                {
                    ResultAddString("标签数据锁定失败");
                    ListBoxAdd("IsoI8000单标签标签数据锁定失败!");
                }
                else
                {
                    ResultAddString("tag data lock failed");
                    ListBoxAdd("IsoI8000 single tag data lock failed!");
                }
            }
        }

        /// <summary>
        /// 单标签锁定查询
        /// </summary>
        void SingleQueryTagLock()
        {
            int status = 0;
            int QAddr = 0;
            int LockStatus = 0;
            byte[] UID = new byte[8];
            string ID = "";
            QAddr = TAGADDR_comboBox.SelectedIndex;

            if (UID_checkBox.Checked)
            {
                ID = TAGID_textBox.Text;
                HexStrToByte(UID, ID);
                status = rfid_sp.ISO_QueryTagLockWithUID(CS, UID, QAddr, ref LockStatus, 0xFF);
            }
            else
            {
                status = rfid_sp.ISO_QueryTagLock(CS, QAddr, ref LockStatus, 0xFF);
            }

            if (0x00 == status)
            {
                if (0 == LockStatus)
                {
                    if (0 == Language)
                        ResultAddString("当前地址未锁定!");
                    else
                        ResultAddString("current address unlock!");
                }
                else if (1 == LockStatus)
                {
                    if (0 == Language)
                        ResultAddString("当前地址已被锁定!");
                    else
                        ResultAddString("current address locked!");

                }
                if (0 == Language)
                    ListBoxAdd("IsoI8000单标签标签数据锁查询成功!");
                else
                    ListBoxAdd("IsoI8000 single tag data lock query success!");

            }
            else
            {
                if (0 == Language)
                {
                    ResultAddString("标签数据锁查询失败");
                    ListBoxAdd("IsoI8000单标签标签数据锁查询失败!");
                }
                else
                {
                    ResultAddString("tag data lock query failed");
                    ListBoxAdd("IsoI8000 single tag data lock query failed!");
                }
            }
        }

        private void MultiTagIdentify_timer_Tick(object sender, EventArgs e)
        {
            MultiTagIdentify();
        }

        private void MultiTagRead_timer_Tick(object sender, EventArgs e)
        {
            MultiTagRead();
        }

        private void OperantionTime_timer_Tick(object sender, EventArgs e)
        {
            OperationTime();
        }

        /*EPC GEN2*/
        //多标签操作
        /// <summary>
        /// 多标签盘询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MULTITAG_INVENTORY_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            SELECTMEMBANK_label.Visible = false;
            RESERVE_checkBox.Visible = false;
            EPC_checkBox.Visible = false;
            TID_checkBox.Visible = false;
            USER_checkBox.Visible = false;

            STARTADDR_label.Visible = false;
            RESERVE_STARTADDR_comboBox.Visible = false;
            EPC_STARTADDR_comboBox.Visible = false;
            TID_STARTADDR_comboBox.Visible = false;
            USER_STARTADDR_comboBox.Visible = false;

            WORDCNTNUM_label.Visible = false;
            RESERVE_CNTNUM_comboBox.Visible = false;
            EPC_CNTNUM_comboBox.Visible = false;
            TID_CNTNUM_comboBox.Visible = false;
            USER_CNTNUM_comboBox.Visible = false;

            WRITEMEMBANK_label.Visible = false;
            WRITEMEMBANK_comboBox.Visible = false;

            WRITEWORDADDR_label.Visible = false;
            WRITEWORDADDR_comboBox.Visible = false;

            WRITEWORDNUM_label.Visible = false;
            WRITEWORDNUM_comboBox.Visible = false;

            WRITEVALUE_label.Visible = false;
            WRITEVALUE_textBox.Visible = false;

            ONCEINVENTORY_button.Visible = true;

            SPEED_label.Visible = true;
            SPEED_comboBox.Visible = true;

            SHOWDATA_listView.Columns.Clear();
            SHOWDATA_listView.Items.Clear();

            if (Language == 0)
            {
                SHOWDATA_listView.Columns.Add("序号", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("EPC盘询数据", 320, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("次数", 78, HorizontalAlignment.Center);
            }
            else
            {
                SHOWDATA_listView.Columns.Add("No.", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("EPC Data", 320, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Times", 78, HorizontalAlignment.Center);
            }

            DisplayCnt = 0;
            OperationType = "Inventory";
            SPEED_comboBox.SelectedIndex = 2;
        }

        /// <summary>
        /// 多标签读取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MULTITAG_READ_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            SELECTMEMBANK_label.Visible = true;
            RESERVE_checkBox.Visible = true;
            EPC_checkBox.Visible = true;
            TID_checkBox.Visible = true;
            USER_checkBox.Visible = true;

            STARTADDR_label.Visible = true;
            RESERVE_STARTADDR_comboBox.Visible = true;
            EPC_STARTADDR_comboBox.Visible = true;
            TID_STARTADDR_comboBox.Visible = true;
            USER_STARTADDR_comboBox.Visible = true;
            RESERVE_STARTADDR_comboBox.Enabled = false;
            EPC_STARTADDR_comboBox.Enabled = false;
            TID_STARTADDR_comboBox.Enabled = false;
            USER_STARTADDR_comboBox.Enabled = false;

            WORDCNTNUM_label.Visible = true;
            RESERVE_CNTNUM_comboBox.Visible = true;
            EPC_CNTNUM_comboBox.Visible = true;
            TID_CNTNUM_comboBox.Visible = true;
            USER_CNTNUM_comboBox.Visible = true;
            RESERVE_CNTNUM_comboBox.Enabled = false;
            EPC_CNTNUM_comboBox.Enabled = false;
            TID_CNTNUM_comboBox.Enabled = false;
            USER_CNTNUM_comboBox.Enabled = false;

            WRITEMEMBANK_label.Visible = false;
            WRITEMEMBANK_comboBox.Visible = false;

            WRITEWORDADDR_label.Visible = false;
            WRITEWORDADDR_comboBox.Visible = false;

            WRITEWORDNUM_label.Visible = false;
            WRITEWORDNUM_comboBox.Visible = false;

            WRITEVALUE_label.Visible = false;
            WRITEVALUE_textBox.Visible = false;

            ONCEINVENTORY_button.Visible = false;

            SPEED_label.Visible = false;
            SPEED_comboBox.Visible = false;

            if (RESERVE_checkBox.Checked)
            {
                RESERVE_STARTADDR_comboBox.Enabled = true;
                RESERVE_CNTNUM_comboBox.Enabled = true;
            }
            if (EPC_checkBox.Checked)
            {
                EPC_STARTADDR_comboBox.Enabled = true;
                EPC_CNTNUM_comboBox.Enabled = true;
            }
            if (TID_checkBox.Checked)
            {
                TID_STARTADDR_comboBox.Enabled = true;
                TID_CNTNUM_comboBox.Enabled = true;
            }
            if (USER_checkBox.Checked)
            {
                USER_STARTADDR_comboBox.Enabled = true;
                USER_CNTNUM_comboBox.Enabled = true;
            }

            SHOWDATA_listView.Columns.Clear();
            SHOWDATA_listView.Items.Clear();

            if (Language == 0)
            {
                SHOWDATA_listView.Columns.Add("序号", 60, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Reserve区", 80, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Epc区", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Tid区", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("User区", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("次数", 60, HorizontalAlignment.Center);
            }
            else
            {
                SHOWDATA_listView.Columns.Add("No.", 60, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Reserve", 80, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Epc", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Tid", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("User", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Times", 60, HorizontalAlignment.Center);
            }

            DisplayCnt = 0;
            OperationType = "Read";
        }

        /// <summary>
        /// 多标签写入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MULTITAGWRITE_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            SELECTMEMBANK_label.Visible = false;
            RESERVE_checkBox.Visible = false;
            EPC_checkBox.Visible = false;
            TID_checkBox.Visible = false;
            USER_checkBox.Visible = false;

            STARTADDR_label.Visible = false;
            RESERVE_STARTADDR_comboBox.Visible = false;
            EPC_STARTADDR_comboBox.Visible = false;
            TID_STARTADDR_comboBox.Visible = false;
            USER_STARTADDR_comboBox.Visible = false;

            WORDCNTNUM_label.Visible = false;
            RESERVE_CNTNUM_comboBox.Visible = false;
            EPC_CNTNUM_comboBox.Visible = false;
            TID_CNTNUM_comboBox.Visible = false;
            USER_CNTNUM_comboBox.Visible = false;

            WRITEMEMBANK_label.Visible = true;
            WRITEMEMBANK_comboBox.Visible = true;

            WRITEWORDADDR_label.Visible = true;
            WRITEWORDADDR_comboBox.Visible = true;

            WRITEWORDNUM_label.Visible = true;
            WRITEWORDNUM_comboBox.Visible = true;

            WRITEVALUE_label.Visible = true;
            WRITEVALUE_textBox.Visible = true;

            ONCEINVENTORY_button.Visible = false;

            SPEED_label.Visible = false;
            SPEED_comboBox.Visible = false;

            SHOWDATA_listView.Columns.Clear();
            SHOWDATA_listView.Items.Clear();

            if (Language == 0)
            {
                SHOWDATA_listView.Columns.Add("序号", 80, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("EPC盘询数据", 350, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("次数", 80, HorizontalAlignment.Center);
            }
            else
            {
                SHOWDATA_listView.Columns.Add("No.", 80, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("EPC Data", 350, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Times", 80, HorizontalAlignment.Center);
            }

            DisplayCnt = 0;
            OperationType = "Write";
        }

        /// <summary>
        /// RESERVE数据区域勾选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RESERVE_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (RESERVE_checkBox.Checked)
            {
                RESERVE_STARTADDR_comboBox.Enabled = true;
                RESERVE_CNTNUM_comboBox.Enabled = true;
            }
            else
            {
                RESERVE_STARTADDR_comboBox.Enabled = false;
                RESERVE_CNTNUM_comboBox.Enabled = false;
            }
        }

        /// <summary>
        /// EPC数据区域勾选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EPC_checkBox.Checked)
            {
                EPC_STARTADDR_comboBox.Enabled = true;
                EPC_CNTNUM_comboBox.Enabled = true;
            }
            else
            {
                EPC_STARTADDR_comboBox.Enabled = false;
                EPC_CNTNUM_comboBox.Enabled = false;
            }
        }

        /// <summary>
        /// TID数据区域勾选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TID_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TID_checkBox.Checked)
            {
                TID_STARTADDR_comboBox.Enabled = true;
                TID_CNTNUM_comboBox.Enabled = true;
            }
            else
            {
                TID_STARTADDR_comboBox.Enabled = false;
                TID_CNTNUM_comboBox.Enabled = false;
            }
        }

        /// <summary>
        /// USER数据区域勾选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void USER_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (USER_checkBox.Checked)
            {
                USER_STARTADDR_comboBox.Enabled = true;
                USER_CNTNUM_comboBox.Enabled = true;
            }
            else
            {
                USER_STARTADDR_comboBox.Enabled = false;
                USER_CNTNUM_comboBox.Enabled = false;
            }
        }

        /*EPC GEN2标签查询*/
        /// <summary>
        /// 单次盘询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ONCEINVENTORY_button_Click(object sender, EventArgs e)
        {
            ClearIDBuffer();
            EPCMultiTagInventory();
        }

        /// <summary>
        /// EPC多标签盘询定时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_MultiTagInventory_timer_Tick(object sender, EventArgs e)
        {
            ClearIDBuffer();
            EPCMultiTagInventory();
        }

        /// <summary>
        /// EPC多标签读取定时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_MultiTagRead_timer_Tick(object sender, EventArgs e)
        {
            //ClearIDBuffer();
            EPCMultiTagRead();
        }

        /// <summary>
        /// EPC多标签写入定时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_MultiTagWrite_timer_Tick(object sender, EventArgs e)
        {
            ClearIDBuffer();
            EPCMultiTagWrite();
        }

        /// <summary>
        /// EPC操作时间定时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_OperationTime_timer_Tick(object sender, EventArgs e)
        {
            EPCOperationTime();
        }

        private void EPC_START_button_Click(object sender, EventArgs e)
        {
            string TempStr = "", TempStrEnglish = "";
            int speed = SPEED_comboBox.SelectedIndex;
            int timer = 0;

            switch (speed)
            {
                case 0:
                    timer = 10;
                    break;
                case 1:
                    timer = 20;
                    break;
                case 2:
                    timer = 50;
                    break;
                case 3:
                    timer = 100;
                    break;
                case 4:
                    timer = 200;
                    break;
                case 5:
                    timer = 300;
                    break;
                case 6:
                    timer = 400;
                    break;
                case 7:
                    timer = 500;
                    break;
            }
            CLEARSHOWDATA_button_Click(sender, e);
            if (MULTITAG_INVENTORY_radioButton.Checked)
            {
                TempStr = "开始查询!";
                TempStrEnglish = "Start query!";
                EPC_OperationTime_timer.Enabled = true;
                EPC_MultiTagInventory_timer.Interval = timer;
                EPC_MultiTagInventory_timer.Enabled = true;
            }
            if (MULTITAG_READ_radioButton.Checked)
            {
                TempStr = "开始多标签读取!";
                TempStrEnglish = "Start multi-tag read!";
                EPC_OperationTime_timer.Enabled = true;
                EPC_MultiTagRead_timer.Enabled = true;
            }
            if (MULTITAG_WRITE_radioButton.Checked)
            {
                TempStr = "开始多标签写入!";
                TempStrEnglish = "Start multi-tag write";
                EPC_OperationTime_timer.Enabled = true;
                EPC_MultiTagWrite_timer.Enabled = true;
            }

            EPC_START_button.Enabled = false;
            EPC_STOP_button.Enabled = true;

            if (Language == 0)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        private void EPC_STOP_button_Click(object sender, EventArgs e)
        {
            string TempStr = "", TempStrEnglish = "";

            if (MULTITAG_INVENTORY_radioButton.Checked)
            {
                TempStr = "停止盘询!";
                TempStrEnglish = "Stop query!";
                EPC_MultiTagInventory_timer.Enabled = false;
                EPC_OperationTime_timer.Enabled = false;
            }
            if (MULTITAG_READ_radioButton.Checked)
            {
                TempStr = "停止多标签读取!";
                TempStrEnglish = "Stop multi-tag read!";
                EPC_MultiTagRead_timer.Enabled = false;
                EPC_OperationTime_timer.Enabled = false;
            }
            if (MULTITAG_WRITE_radioButton.Checked)
            {
                TempStr = "停止多标签写入!";
                TempStrEnglish = "Stop multi-tag write!";
                EPC_MultiTagWrite_timer.Enabled = false;
                EPC_OperationTime_timer.Enabled = false;
            }

            EPC_START_button.Enabled = true;
            EPC_STOP_button.Enabled = false;

            if (Language == 0)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        private void CLEARSHOWDATA_button_Click(object sender, EventArgs e)
        {
            SHOWDATA_listView.Items.Clear();
            DisplayCnt = 0;
            OperTime = 0;
            TIME_label.Text = "";
            NUMBER_label.Text = "";
        }

        //单标签操作
        /// <summary>
        /// 安全读
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SECREAD_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            MEMBANK_SINGLE_label.Visible = true;
            MEMBANK_SINGLE_comboBox.Visible = true;
            LOCKMEMBANK_comboBox.Visible = false;

            STARTADDR_SINGLE_label.Visible = true;
            STARTADDR_SINGLE_comboBox.Visible = true;

            LENGTH_SINGLE_label.Visible = true;
            LENGTH_SINGLE_comboBox.Visible = true;

            LOCKLEVEL_SINGLE_label.Visible = false;
            LOCKLEVEL_SINGLE_comboBox.Visible = false;

            ACCESSPASSWORD_SINGLE_label.Visible = true;
            ACCESSPASSWORD_SINGLE_textBox.Visible = true;

            MATCH_SINGLE_label.Visible = false;
            MATCH_SINGLE_comboBox.Visible = false;

            VALUE_SINGLE_label.Visible = true;
            VALUE_SINGLE_textBox.Visible = true;
        }

        /// <summary>
        /// 安全写
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SECWRITE_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            MEMBANK_SINGLE_label.Visible = true;
            MEMBANK_SINGLE_comboBox.Visible = true;
            LOCKMEMBANK_comboBox.Visible = false;

            STARTADDR_SINGLE_label.Visible = true;
            STARTADDR_SINGLE_comboBox.Visible = true;

            LENGTH_SINGLE_label.Visible = true;
            LENGTH_SINGLE_comboBox.Visible = true;

            LOCKLEVEL_SINGLE_label.Visible = false;
            LOCKLEVEL_SINGLE_comboBox.Visible = false;

            ACCESSPASSWORD_SINGLE_label.Visible = true;
            ACCESSPASSWORD_SINGLE_textBox.Visible = true;

            MATCH_SINGLE_label.Visible = false;
            MATCH_SINGLE_comboBox.Visible = false;

            VALUE_SINGLE_label.Visible = true;
            VALUE_SINGLE_textBox.Visible = true;
        }

        /// <summary>
        /// 安全锁
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SECLOCK_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            MEMBANK_SINGLE_label.Visible = true;
            MEMBANK_SINGLE_comboBox.Visible = false;
            LOCKMEMBANK_comboBox.Visible = true;

            STARTADDR_SINGLE_label.Visible = false;
            STARTADDR_SINGLE_comboBox.Visible = false;

            LENGTH_SINGLE_label.Visible = false;
            LENGTH_SINGLE_comboBox.Visible = false;

            LOCKLEVEL_SINGLE_label.Visible = true;
            LOCKLEVEL_SINGLE_comboBox.Visible = true;

            ACCESSPASSWORD_SINGLE_label.Visible = true;
            ACCESSPASSWORD_SINGLE_textBox.Visible = true;

            MATCH_SINGLE_label.Visible = false;
            MATCH_SINGLE_comboBox.Visible = false;

            VALUE_SINGLE_label.Visible = false;
            VALUE_SINGLE_textBox.Visible = false;
        }

        /// <summary>
        /// 销毁标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KILLTAG_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            MEMBANK_SINGLE_label.Visible = false;
            MEMBANK_SINGLE_comboBox.Visible = false;
            LOCKMEMBANK_comboBox.Visible = false;

            STARTADDR_SINGLE_label.Visible = false;
            STARTADDR_SINGLE_comboBox.Visible = false;

            LENGTH_SINGLE_label.Visible = false;
            LENGTH_SINGLE_comboBox.Visible = false;

            LOCKLEVEL_SINGLE_label.Visible = false;
            LOCKLEVEL_SINGLE_comboBox.Visible = false;

            ACCESSPASSWORD_SINGLE_label.Visible = true;
            ACCESSPASSWORD_SINGLE_textBox.Visible = true;

            MATCH_SINGLE_label.Visible = false;
            MATCH_SINGLE_comboBox.Visible = false;

            VALUE_SINGLE_label.Visible = false;
            VALUE_SINGLE_textBox.Visible = false;
        }

        /// <summary>
        /// 筛选配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CONFIGTAG_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            MEMBANK_SINGLE_label.Visible = true;
            MEMBANK_SINGLE_comboBox.Visible = true;
            LOCKMEMBANK_comboBox.Visible = false;

            STARTADDR_SINGLE_label.Visible = true;
            STARTADDR_SINGLE_comboBox.Visible = true;

            LENGTH_SINGLE_label.Visible = true;
            LENGTH_SINGLE_comboBox.Visible = true;

            LOCKLEVEL_SINGLE_label.Visible = false;
            LOCKLEVEL_SINGLE_comboBox.Visible = false;

            ACCESSPASSWORD_SINGLE_label.Visible = true;
            ACCESSPASSWORD_SINGLE_textBox.Visible = true;

            MATCH_SINGLE_label.Visible = true;
            MATCH_SINGLE_comboBox.Visible = true;

            VALUE_SINGLE_label.Visible = true;
            VALUE_SINGLE_textBox.Visible = true;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EXECUTE_button_Click(object sender, EventArgs e)
        {
            if (SECREAD_radioButton.Checked)
                SECRead();

            if (SECWRITE_radioButton.Checked)
                SECWrite();

            if (SECLOCK_radioButton.Checked)
                SECLock();

            if (KILLTAG_radioButton.Checked)
                SECKill();

            if (CONFIGTAG_radioButton.Checked)
                SECSelectConfig();
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        void ClearIDBuffer()
        {
            rfid_sp.BufferM_ClearBuffer(CS, 0xFF);
        }

        void EPCDisplayNewTag(string strEpc)
        {
            string SN = "";
            if (OperationType == "Inventory" || OperationType == "Write")
            {

                //StreamWriter _Log = new StreamWriter("D:\\RFID.txt", true);
                //_Log.WriteLine("RFID No : " + strEpc + " Read time=" + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString());
                //_Log.Close();
                SN = string.Format("{0:D}", DisplayCnt + 1);
                ListViewItem lvi = new ListViewItem(SN);
                lvi.SubItems.Add(strEpc);
                lvi.SubItems.Add("1");
                SHOWDATA_listView.Items.Add(lvi);
                SHOWDATA_listView.EnsureVisible(SHOWDATA_listView.Items.Count - 1);
            }
            if (OperationType == "Read")
            {
                //StreamWriter _Log = new StreamWriter("D:\\RFID.txt", true);
                //_Log.WriteLine("RFID No : " + strEpc + " Read time=" + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString());
                //_Log.Close();
                SN = string.Format("{0:D}", DisplayCnt + 1);
                ListViewItem lvi = new ListViewItem(SN);
                lvi.SubItems.Add(RStr);
                lvi.SubItems.Add(EStr);
                lvi.SubItems.Add(TStr);
                lvi.SubItems.Add(UStr);
                lvi.SubItems.Add("1");
                lvi.SubItems.Add(strEpc);
                SHOWDATA_listView.Items.Add(lvi);
                SHOWDATA_listView.EnsureVisible(SHOWDATA_listView.Items.Count - 1);
            }
            NUMBER_label.Text = SN;
        }

        /// <summary>
        /// EPC GEN2启动标签盘询
        /// </summary>
        void EPCMultiTagInventory()
        {
            int TagCount = 0;
            int CntGot = 0;
            int GetCount = 0;
            int i, j = 0;
            int status;
            string EPC = "";
            BufferData[] Data = new BufferData[1024];
            for (int index = 0; index < Data.Length; index++)
            {
                Data[index].Data = new byte[512];
            }
            status = rfid_sp.GEN2_MultiTagInventory(CS, ref TagCount, 0xFF);
            if (0x00 == status)
            {
                while (CntGot < TagCount)
                {
                    status = rfid_sp.BufferM_GetTagData(CS, ref GetCount, Data, 0xFF);
                    if (0x00 == status)
                    {
                        if (GetCount <= 0)
                        {
                            break;
                        }

                        for (i = 0; i < GetCount; i++)
                        {
                            EPC = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}{10:X2}{11:X2}",
                                Data[i].Data[0], Data[i].Data[1], Data[i].Data[2], Data[i].Data[3],
                                Data[i].Data[4], Data[i].Data[5], Data[i].Data[6], Data[i].Data[7],
                                Data[i].Data[8], Data[i].Data[9], Data[i].Data[10], Data[i].Data[11]);
                            if (SHOWDATA_listView.Items.Count <= 0)
                            {
                                EPCDisplayNewTag(EPC);
                                DisplayCnt++;
                            }
                            else
                            {
                                int flg = -1;
                                for (j = 0; j < SHOWDATA_listView.Items.Count; j++)
                                {
                                    if (EPC == SHOWDATA_listView.Items[j].SubItems[1].Text)
                                    {
                                        SHOWDATA_listView.Items[j].SubItems[2].Text =
                                            Convert.ToString(Convert.ToInt32(SHOWDATA_listView.Items[j].SubItems[2].Text) + 1);
                                        flg = i;
                                    }
                                }
                                if (flg < 0)
                                {
                                    EPCDisplayNewTag(EPC);
                                    DisplayCnt++;
                                }
                            }
                        } // for each item
                    } // if Get buffer success

                    CntGot += GetCount;
                    //StreamWriter _Log = new StreamWriter("D:\\RFID.txt", true);
                    //_Log.WriteLine("RFID No : " + EPC + " Read time=" + DateTime.Now.ToLongDateString() + "  " + System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    //_Log.Close();
                } // while buffer not empty
            } // if inventory success
        }

        /// <summary>
        /// EPC GEN2多标签读取
        /// </summary>
        void EPCMultiTagRead()
        {
            int TagCount = 0;
            int GetCount = 0;
            int i, j;
            int status;
            int RCnt = 0;							//Reserve区读取数据字节数(Reserve memory read data numbers of byte)
            int ECnt = 0;							//Epc区读取数据字节数(EPC memory read data numbers of byte)
            int TCnt = 0;							//Tid区读取数据字节数(TID memory read data numbers of byte)
            int UCnt = 0;							//User区读取数据字节数(User memory read data numbers of byte)
            byte[] Reserve = new byte[100];		    //Reserve区读取数据(Reserve memory read data)
            byte[] Epc = new byte[100];			    //Epc区读取数据(EPC memory read data)
            byte[] Tid = new byte[100];			    //Tid区读取数据(TID memory read data)
            byte[] User = new byte[100];			//User区读取数据(User memory read data)
            BufferData[] Data = new BufferData[256];
            for (int index = 0; index < Data.Length; index++)
            {
                Data[index].Data = new byte[512];
            }

            string EPC = "";
            //初始化(initialize)
            WordptrAndLength WpALen;
            WpALen.ReserveWordPtr = 0;
            WpALen.ReserveWordCnt = 0;
            WpALen.EpcWordPtr = 0;
            WpALen.EpcWordCnt = 0;
            WpALen.TidWordPtr = 0;
            WpALen.TidWordCnt = 0;
            WpALen.UserWordPtr = 0;
            WpALen.UserWordCnt = 0;
            WpALen.MembankMask = 0;

            if (RESERVE_checkBox.Checked)
            {
                WpALen.MembankMask += 1;
                WpALen.ReserveWordPtr = RESERVE_STARTADDR_comboBox.SelectedIndex;
                WpALen.ReserveWordCnt = RESERVE_CNTNUM_comboBox.SelectedIndex + 1;
                RCnt = WpALen.ReserveWordCnt * 2;
            }
            if (EPC_checkBox.Checked)
            {
                WpALen.MembankMask += 2;
                WpALen.EpcWordPtr = EPC_STARTADDR_comboBox.SelectedIndex;
                WpALen.EpcWordCnt = EPC_CNTNUM_comboBox.SelectedIndex + 1;
                ECnt = WpALen.EpcWordCnt * 2;
            }
            if (TID_checkBox.Checked)
            {
                WpALen.MembankMask += 4;
                WpALen.TidWordPtr = TID_STARTADDR_comboBox.SelectedIndex;
                WpALen.TidWordCnt = TID_CNTNUM_comboBox.SelectedIndex + 1;
                TCnt = WpALen.TidWordCnt * 2;
            }
            if (USER_checkBox.Checked)
            {
                WpALen.MembankMask += 8;
                WpALen.UserWordPtr = USER_STARTADDR_comboBox.SelectedIndex;
                WpALen.UserWordCnt = USER_CNTNUM_comboBox.SelectedIndex + 1;
                UCnt = WpALen.UserWordCnt * 2;
            }

            status = rfid_sp.GEN2_MultiTagRead(CS, WpALen, ref TagCount, 0xFF);
            if (0x00 == status)
            {
                if (0 != TagCount)		//标签读取成功//tag read success
                {
                    status = rfid_sp.BufferM_GetTagData(CS, ref GetCount, Data, 0xFF);
                    if (0x00 == status)
                    {
                        for (i = 0; i < GetCount; i++)
                        {
                            EPC = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}{10:X2}{11:X2}",
                                Data[i].Data[0], Data[i].Data[1], Data[i].Data[2], Data[i].Data[3],
                                Data[i].Data[4], Data[i].Data[5], Data[i].Data[6], Data[i].Data[7],
                                Data[i].Data[8], Data[i].Data[9], Data[i].Data[10], Data[i].Data[11]);
                            if (SHOWDATA_listView.Items.Count <= 0)
                            {
                                if (RCnt != 0)
                                {
                                    Array.Copy(Data[i].Data, 12, Reserve, 0, RCnt);
                                    RStr = ByteToHexStr(Reserve, RCnt);
                                    Array.Clear(Reserve, 0, 100);
                                }
                                if (ECnt != 0)
                                {
                                    Array.Copy(Data[i].Data, 12 + RCnt, Epc, 0, ECnt);
                                    EStr = ByteToHexStr(Epc, ECnt);
                                    Array.Clear(Epc, 0, 100);
                                }
                                if (TCnt != 0)
                                {
                                    Array.Copy(Data[i].Data, 12 + RCnt + ECnt, Tid, 0, TCnt);
                                    TStr = ByteToHexStr(Tid, TCnt);
                                    Array.Clear(Tid, 0, 100);
                                }
                                if (UCnt != 0)
                                {
                                    Array.Copy(Data[i].Data, 12 + RCnt + ECnt + TCnt, User, 0, UCnt);
                                    UStr = ByteToHexStr(User, UCnt);
                                    Array.Clear(User, 0, 100);
                                }
                                EPCDisplayNewTag(EPC);
                                DisplayCnt++;
                            }
                            else
                            {
                                int flg = -1;
                                for (j = 0; j < SHOWDATA_listView.Items.Count; j++)
                                {
                                    if (EPC == SHOWDATA_listView.Items[j].SubItems[6].Text)
                                    {
                                        SHOWDATA_listView.Items[j].SubItems[5].Text =
                                            Convert.ToString(Convert.ToInt32(SHOWDATA_listView.Items[j].SubItems[5].Text) + 1);
                                        flg = i;
                                    }
                                }
                                if (flg < 0)
                                {
                                    if (RCnt != 0)
                                    {
                                        Array.Copy(Data[i].Data, 12, Reserve, 0, RCnt);
                                        RStr = ByteToHexStr(Reserve, RCnt);
                                        Array.Clear(Reserve, 0, 100);
                                    }
                                    if (ECnt != 0)
                                    {
                                        Array.Copy(Data[i].Data, 12 + RCnt, Epc, 0, ECnt);
                                        EStr = ByteToHexStr(Epc, ECnt);
                                        Array.Clear(Epc, 0, 100);
                                    }
                                    if (TCnt != 0)
                                    {
                                        Array.Copy(Data[i].Data, 12 + RCnt + ECnt, Tid, 0, TCnt);
                                        TStr = ByteToHexStr(Tid, TCnt);
                                        Array.Clear(Tid, 0, 100);
                                    }
                                    if (UCnt != 0)
                                    {
                                        Array.Copy(Data[i].Data, 12 + RCnt + ECnt + TCnt, User, 0, UCnt);
                                        UStr = ByteToHexStr(User, UCnt);
                                        Array.Clear(User, 0, 100);
                                    }
                                    EPCDisplayNewTag(EPC);
                                    DisplayCnt++;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// EPC GEN2多标签写
        /// </summary>
        void EPCMultiTagWrite()
        {
            MutiWriteParam MutiWP = new MutiWriteParam();
            MutiWP.WriteValue = new byte[20];
            MutiWP.MemBank = WRITEMEMBANK_comboBox.SelectedIndex;
            MutiWP.StartAddr = WRITEWORDADDR_comboBox.SelectedIndex;
            MutiWP.WriteLen = WRITEWORDNUM_comboBox.SelectedIndex + 1;
            HexStrToByte(MutiWP.WriteValue, WRITEVALUE_textBox.Text);
            int i, j;
            int status;
            int TagCount = 0;
            int GetCount = 0;
            string EPC = "";
            BufferData[] Data = new BufferData[256];
            for (int index = 0; index < Data.Length; index++)
            {
                Data[index].Data = new byte[512];
            }

            status = rfid_sp.GEN2_MultiTagWrite(CS, MutiWP, ref TagCount, 0xFF);
            if (0x00 == status)
            {
                if (0 != TagCount)		//标签写入成功//tag write success
                {
                    status = rfid_sp.BufferM_GetTagData(CS, ref GetCount, Data, 0xFF);
                    if (0x00 == status)
                    {
                        for (i = 0; i < GetCount; i++)
                        {
                            EPC = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}{10:X2}{11:X2}",
                                Data[i].Data[0], Data[i].Data[1], Data[i].Data[2], Data[i].Data[3],
                                Data[i].Data[4], Data[i].Data[5], Data[i].Data[6], Data[i].Data[7],
                                Data[i].Data[8], Data[i].Data[9], Data[i].Data[10], Data[i].Data[11]);
                            if (SHOWDATA_listView.Items.Count <= 0)
                            {
                                EPCDisplayNewTag(EPC);
                                DisplayCnt++;
                            }
                            else
                            {
                                int flg = -1;
                                for (j = 0; j < SHOWDATA_listView.Items.Count; j++)
                                {
                                    if (EPC == SHOWDATA_listView.Items[j].SubItems[1].Text)
                                    {
                                        SHOWDATA_listView.Items[j].SubItems[2].Text =
                                            Convert.ToString(Convert.ToInt32(SHOWDATA_listView.Items[j].SubItems[2].Text) + 1);
                                        flg = i;
                                    }
                                }
                                if (flg < 0)
                                {
                                    EPCDisplayNewTag(EPC);
                                    DisplayCnt++;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 安全读
        /// </summary>
        void SECRead()
        {
            string TempStr = "";
            string TempStrEnglish = "";
            int status = 0;
            int SMembank = 0;
            string PWstr = "";
            int RSAddr = 0;
            int RCnt = 0;
            byte[] PassWord = new byte[4];
            byte[] RData = new byte[100];

            SMembank = MEMBANK_SINGLE_comboBox.SelectedIndex;
            PWstr = ACCESSPASSWORD_SINGLE_textBox.Text;
            RSAddr = STARTADDR_SINGLE_comboBox.SelectedIndex;
            RCnt = LENGTH_SINGLE_comboBox.SelectedIndex;

            HexStrToByte(PassWord, PWstr);

            status = rfid_sp.GEN2_SecRead(CS, SMembank, PassWord, RSAddr, RCnt, RData, 0xFF);
            if (0 == status)
            {
                TempStr = "标签读取成功";
                TempStrEnglish = "Tag read success";
                string TStr = "";
                TStr = ByteToHexStr(RData, RCnt * 2);
                VALUE_SINGLE_textBox.Text = TStr;
            }
            else
            {
                TempStr = "标签读取失败";
                TempStrEnglish = "Tag read failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 安全写
        /// </summary>
        void SECWrite()
        {
            string TempStr = "";
            string TempStrEnglish = "";
            int status = 0;
            int SMembank = 0;
            string PWstr = "";
            string WDataStr = "";
            int WSAddr = 0;
            int WCnt = 0;
            byte[] PassWord = new byte[4];
            byte[] WData = new byte[100];

            SMembank = MEMBANK_SINGLE_comboBox.SelectedIndex;
            PWstr = ACCESSPASSWORD_SINGLE_textBox.Text;
            WSAddr = STARTADDR_SINGLE_comboBox.SelectedIndex;
            WCnt = LENGTH_SINGLE_comboBox.SelectedIndex;
            WDataStr = VALUE_SINGLE_textBox.Text;

            if (WDataStr.Length == WCnt * 4)
            {
                HexStrToByte(PassWord, PWstr);
                HexStrToByte(WData, WDataStr);
                status = rfid_sp.GEN2_SecWrite(CS, SMembank, PassWord, WSAddr, WCnt, WData, 0xFF);
                if (0 == status)
                {
                    TempStr = "标签写入成功";
                    TempStrEnglish = "tag write success";
                }
                else
                {
                    TempStr = "标签写入失败";
                    TempStrEnglish = "tag write failed";
                }
            }
            else
            {
                if (0 == Language)
                    MessageBox.Show("写入标签的数据与写入的长度不匹配!请检查后重试");
                else
                    MessageBox.Show("the data input is incorrect length,please check and try again");
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 安全锁
        /// </summary>
        void SECLock()
        {
            string TempStr = "";
            string TempStrEnglish = "";
            int status = 0;
            int SMembank = 0;
            int LSLevel = 0;
            string PWstr = "";
            byte[] PassWord = new byte[4];

            SMembank = LOCKMEMBANK_comboBox.SelectedIndex;
            PWstr = ACCESSPASSWORD_SINGLE_textBox.Text;
            LSLevel = LOCKLEVEL_SINGLE_comboBox.SelectedIndex;

            HexStrToByte(PassWord, PWstr);
            status = rfid_sp.GEN2_SecLock(CS, SMembank, PassWord, LSLevel, 0xFF);
            if (0 == status)
            {
                TempStr = "标签锁定成功";
                TempStrEnglish = "tag lock success";
            }
            else
            {
                TempStr = "标签锁定失败";
                TempStrEnglish = "tag lock failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 标签销毁
        /// </summary>
        void SECKill()
        {
            string TempStr = "";
            string TempStrEnglish = "";
            int status = 0;
            string PWstr = "";
            byte[] PassWord = new byte[4];

            PWstr = ACCESSPASSWORD_SINGLE_textBox.Text;

            HexStrToByte(PassWord, PWstr);
            status = rfid_sp.GEN2_KillTag(CS, PassWord, 0xFF);
            if (0 == status)
            {
                TempStr = "标签销毁成功";
                TempStrEnglish = "tag kill success";
            }
            else
            {
                TempStr = "标签销毁失败";
                TempStrEnglish = "tag kill failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 筛选配置
        /// </summary>
        void SECSelectConfig()
        {
            string TempStr = "";
            string TempStrEnglish = "";
            int status = 0;
            int SMembank = 0;
            int SCSAddr = 0;
            int SCCnt = 0;
            int SCMatch = 0;
            string WDataStr = "";
            byte[] WData = new byte[100];

            SMembank = MEMBANK_SINGLE_comboBox.SelectedIndex;
            SCSAddr = STARTADDR_SINGLE_comboBox.SelectedIndex;
            SCCnt = LENGTH_SINGLE_comboBox.SelectedIndex;
            SCMatch = MATCH_SINGLE_comboBox.SelectedIndex;
            WDataStr = VALUE_SINGLE_textBox.Text;

            if (WDataStr.Length == SCCnt * 4)
            {
                HexStrToByte(WData, WDataStr);
                status = rfid_sp.GEN2_SecSelectConfig(CS, SMembank, SCMatch, SCSAddr, SCCnt, WData, 0xFF);
                if (0 == status)
                {
                    TempStr = "标签筛选配置成功";
                    TempStrEnglish = "tag selection set success";
                }
                else
                {
                    TempStr = "标签筛选配置失败";
                    TempStrEnglish = "tag selection set failed";
                }
            }
            else
            {
                if (0 == Language)
                    MessageBox.Show("标签的筛选数据长度与选择的筛选长度不匹配!请检查后重试");
                else
                    MessageBox.Show("tag selection data length is incorrect, please check and try again");
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// 操作时间
        /// </summary>
        void EPCOperationTime()
        {
            string time = "";
            OperTime++;
            time = string.Format("{0:D}", OperTime);
            TIME_label.Text = time;
        }

        void IsHexOrNot(KeyPressEventArgs e)
        {
            //如果输入的不是16进制，也不是回车键、Backspace键，则取消该输入
            if (e.KeyChar != (char)13 && e.KeyChar != (char)8)
                e.Handled = "0123456789ABCDEF".IndexOf(char.ToUpper(e.KeyChar)) < 0;
        }

        private void TAGID_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            IsHexOrNot(e);
        }

        private void VALUE_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            IsHexOrNot(e);
        }

        private void pbBack_Click(object sender, EventArgs e)
        {
            this.Close();
            _Main.PanelVisuable(true);
        }
    }
}
