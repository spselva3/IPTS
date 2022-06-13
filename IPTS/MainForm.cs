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
        //ȫ�ֱ���(global variable)
        hComSocket CS;                  //ͨ�ž���ṹ��(connect handle struct)

        int hCom = -1;			        //���ھ��(comm handle)

        Socket sockClient = null;		    //���ھ��(TCP/IP handle)

        int ConnectFlag = 0;		    //���ӱ�־λ(connect falg)
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

        /*******************************���ߺ���***************************************/
        //��IP��ַ���ַ���ת��Ϊʮ������BYTE
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


        //��16���Ƶ�BYTEת��Ϊʮ���Ƶ��ַ���
        public static string IntToInt_Str(int value)
        {
            string str = string.Format("{0:D}", value);
            return str;
        }

        //��ʮ������BYTEת��ΪIP��ַ���ַ���
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
                //    ListBoxAdd("���ڲ���ʹ�ã���ʹ����������!");
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

            //ϵͳ����
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
                SYSCONFIG_tabPage.Text = "ϵͳ����";
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
            tabPage1.Text = "����";
            tabPage2.Text = "����";

            label1.Text = "IP:";
            label2.Text = "�˿�:";
            label3.Text = "���ں�:";
            label4.Text = "������:";

            Connectbutton.Text = "����";
            Disconnectbutton.Text = "�Ͽ�";
            groupBox1.Text = "���ӷ�ʽ";
            groupBox2.Text = "������Ϣ";
            CLRSYSMSG_button.Text = "���";

            //ϵͳ����
            groupBox3.Text = "���߹���״̬";
            SETANTENNA_button.Text = "����";
            GETANTENNA_button.Text = "��ѯ";

            groupBox4.Text = "����";
            SETRFPWR_button.Text = "����";
            GETRFPWR_button.Text = "��ѯ";
            SETINTERNET_button.Text = "����";
            GETINTERNET_button.Text = "��ѯ";

            groupBox5.Text = "����";
            IPPORT_label.Text = "�˿�:";
            GATEWAY_label.Text = "����:";
            MASK_label.Text = "����:";

            groupBox6.Text = "Ƶ��-Ƶ��";
            CHINA_radioButton.Text = "�й�";
            NORTHAMERICA_radioButton.Text = "����";
            EUROPE_radioButton.Text = "ŷ��";
            OTHERS_radioButton.Text = "����";
            SETFREQ_button.Text = "����";
            GETFREQ_button.Text = "��ѯ";

            groupBox7.Text = "�ɱ��IO��";
            label12.Text = "I/O�˿�:";
            label13.Text = "�����ƽ:";
            LOWLEVEL_radioButton.Text = "0 �͵�ƽ";
            HIGHLEVEL_radioButton.Text = "1 �ߵ�ƽ";
            SETOUTPORT_button.Text = "����";
            GETOUTPUT_button.Text = "��ѯ";

            groupBox8.Text = "�ز���������";
            MODE_label.Text = "����:";
            SINGLETAG_radioButton.Text = "�������";
            MULTITAG_radioButton.Text = "�࿨ģʽ";
            CURRENTMODE_label.Text = "��ǰ����:";
            SETSFTM_button.Text = "����";
            GETSFTM_button.Text = "��ѯ";

            groupBox9.Text = "ģʽ-�ӿ�";
            CMD_radioButton.Text = "����ģʽ";
            ANIMATION_radioButton.Text = "�Զ�ģʽ";
            TRIGGER_radioButton.Text = "����ģʽ";
            WEIGAN_checkBox.Text = "Τ��";
            WEIGANSTYLE_label.Text = "����";
            IP_checkBox.Text = "����";
            RELAY_checkBox.Text = "�̵���";
            SETINTERFACE_button.Text = "����";
            GETINTERFACE_button.Text = "��ѯ";

            CERTIFICATION_button.Text = "��ǩ��֤";
            RESET_button.Text = "��д����λ";

            //ISO18000-6B
            TAGIDETIFY_groupBox.Text = "��ǩ����";
            MULTITAGIDENTIFY_radioButton.Text = "���ǩʶ��";
            MULTITAGREAD_radioButton.Text = "���ǩ��ȡ";
            READADDR_label.Text = "��ȡ��ַ";
            CLEAR_button.Text = "���";
            ISO_TIME_label.Text = "ʱ��";
            ISO_NUM_label.Text = "����";
            STOP_button.Text = "ֹͣ";
            UNIQUETAG_groupBox.Text = "����ǩ";
            SINGLEREAD_radioButton.Text = "��ȡ";
            SINGLEWRITE_radioButton.Text = "д��";
            SINGLELOCK_radioButton.Text = "����";
            ACCESSADDR_label.Text = "���ʵ�ַ:";
            ACCESSID_label.Text = "����ID:";
            RESULT_groupBox.Text = "���";
            VALUE_label.Text = "����:";
            CLEARRESULT_button.Text = "���";

            //EPC GEN2
            MULTITAG_groupBox.Text = "���ǩ����";
            SHOWDATA_groupBox.Text = "������ʾ";

            SELECTMEMBANK_label.Text = "��������:";
            WRITEMEMBANK_label.Text = "д������:";
            STARTADDR_label.Text = "��ʼ��ַ:";
            WRITEVALUE_label.Text = "д������:";
            WORDCNTNUM_label.Text = "��ȡ����:";
            WRITEWORDADDR_label.Text = "��ʼ��ַ:";
            WRITEWORDNUM_label.Text = "д������:";

            TIMES_label.Text = "ʱ��";
            NUM_label.Text = "����";
            SPEED_label.Text = "����";
            UNIQUETAG_groupBox.Text = "����ǩ����";
            MEMBANK_SINGLE_label.Text = "��������:";
            STARTADDR_SINGLE_label.Text = "��ʼ��ַ:";

            LENGTH_SINGLE_label.Text = "���ݳ���:";
            LOCKLEVEL_SINGLE_label.Text = "�����ȼ�:";
            ACCESSPASSWORD_SINGLE_label.Text = "��������:";
            MATCH_SINGLE_label.Text = "ƥ���:";
            VALUE_SINGLE_label.Text = "����:";

            MULTITAG_INVENTORY_radioButton.Text = "���ǩ��ѯ";
            MULTITAG_READ_radioButton.Text = "���ǩ��ȡ";
            MULTITAG_WRITE_radioButton.Text = "���ǩд��";
            SECREAD_radioButton.Text = "��ȫ��";
            SECWRITE_radioButton.Text = "��ȫд";
            SECLOCK_radioButton.Text = "��ȫ��";
            KILLTAG_radioButton.Text = "���ٱ�ǩ";
            CONFIGTAG_radioButton.Text = "ɸѡ����";
            EPC_START_button.Text = "��ʼ";
            EPC_STOP_button.Text = "ֹͣ";
            ONCEINVENTORY_button.Text = "������ѯ";
            CLEARSHOWDATA_button.Text = "���";
            EXECUTE_button.Text = "ִ��";
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

            //ϵͳ����
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
                    LOWLEVEL_radioButton.Text = "��";
                    HIGHLEVEL_radioButton.Text = "��";
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
                    LOWLEVEL_radioButton.Text = "0 �͵�ƽ";
                    HIGHLEVEL_radioButton.Text = "1 �ߵ�ƽ";
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
                return -2;			//������Ϣ������(input info incomplete)
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
                    return 0;		//���ӳɹ�(connect success)
                else
                    return -1;		//����ʧ��(connect fail)
            }
        }

        private int SocketDisConnect()
        {
            if (sockClient != null)
            {
                if (rfid_sp.Socket_CloseSocket(sockClient) == RFID_StandardProtocol.SUCCESS)
                {
                    CS.sockClient = null;
                    return 0;		//�رճɹ�(close success)
                }
                else
                    return -1;		//�ر�ʧ��(close fail)
            }
            else
            {
                return -2;			//�����Ч(handle unavailable)
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
                return -2;		//������Ϣ������(input info incomplete)
            }
            else
            {
                if (rfid_sp.Serial_OpenSeries(ref hCom,
                                             SPNumcomboBox.Text,
                                             Convert.ToInt32(SPBaudcomboBox.Text))
                    == RFID_StandardProtocol.SUCCESS)
                    return 0;		//���ӳɹ�(connect success)
                else
                    return -1;		//����ʧ��(connect fail)
            }
        }

        private int ComDisConnect()
        {
            if (hCom != -1)
            {

                if (rfid_sp.Serial_CloseSeries(hCom) == RFID_StandardProtocol.SUCCESS)
                {
                    CS.hCom = -1;
                    return 0;	//�رճɹ�(close success)
                }
                else
                    return -1;	//�ر�ʧ��(close fail)
            }
            else
            {
                return -2;		//�����Ч(handle unavailable)
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
        /// ��ȡ�汾��
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
                    ListBoxAdd("��д���̼��汾��Ϊ: " + TempStr);
                else
                    ListBoxAdd("Reader firmware version: " + TempStr);
                SetVersionText(TempStr);
                //SetText(TempStr);
            }
            else
            {
                TempStr = "��ȡ�汾��ʧ��!";
                TempStrEnglish = "Get version fail!";
                if (0 == Language)
                    ListBoxAdd(TempStr);
                else
                    ListBoxAdd(TempStrEnglish);
            }
        }

        /// <summary>
        /// ��ȡ���к�
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
                    ListBoxAdd("��д�����к�Ϊ: " + TempStr);
                else
                    ListBoxAdd("Reader serial No.: " + TempStr);
                SetSerialNoText(TempStr);
                //SetText(TempStr);
            }
            else
            {
                TempStr = "��ȡ���кź�ʧ��!";
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
                //����ת��Ϊ��ĸ(change consult to letter)
                if (v1 >= 0 && v1 <= 9)
                {
                    hex1 = (char)(48 + v1);
                }
                else
                {
                    hex1 = (char)(55 + v1);
                }
                //������ת����ĸ(change remainder to letter)
                if (v2 >= 0 && v2 <= 9)
                {
                    hex2 = (char)(48 + v2);
                }
                else
                {
                    hex2 = (char)(55 + v2);
                }
                //����ĸ����һ��(make letter a string)
                hexstr = hexstr + hex1 + hex2;
            }
            return hexstr;
        }

        /// <summary>
        /// 16�����ַ���ת�ֽ�����
        /// </summary>
        /// <param name="byteT">Ŀ������</param>
        /// <param name="str">Դ�ַ���</param>
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
                select = MessageBox.Show("��������Ӧ�ó���?", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            else
                select = MessageBox.Show("Restart the application?", "Reminder", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (select == DialogResult.Yes)			//�ر�Ӧ�ó���
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
                    TempStr = "���ӳɹ�!";
                    TempStrEnglish = "Connect success!";
                    Connectbutton.Enabled = false;
                    Disconnectbutton.Enabled = true;
                    break;
                case -1:
                    TempStr = "����ʧ��!";
                    TempStrEnglish = "Connect fail!";
                    Connectbutton.Enabled = true;
                    Disconnectbutton.Enabled = false;
                    break;
                case -2:
                    TempStr = "���Ӳ���������!";
                    TempStrEnglish = "Connect parameter incomplete!";
                    Connectbutton.Enabled = true;
                    Disconnectbutton.Enabled = false;
                    break;
                case -3:
                    TempStr = "����IP��ַ��Ч!";
                    TempStrEnglish = "Input IP Invalid!";
                    Connectbutton.Enabled = true;
                    Disconnectbutton.Enabled = false;
                    break;
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
            if (TempStr != "���ӳɹ�!" || TempStrEnglish != "Connect success!")
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
                    TempStr = "�رճɹ�!";
                    TempStrEnglish = "Close success!";
                    break;
                case -1:
                    TempStr = "�ر�ʧ��!";
                    TempStrEnglish = "Close fail!";
                    break;
                case -2:
                    TempStr = "�رվ����Ч!";
                    TempStrEnglish = "Handle unavailable!";
                    break;
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
            if (TempStr != "�رճɹ�!" || TempStrEnglish != "Close success!")
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

                ConnectFlag = 1;		//���ӱ�־λ(connect flag)

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
                ConnectFlag = 0;		//���ӱ�־λ(connect falg)
            }
        }

        /*ϵͳ����*/
        /// <summary>
        /// �������߹���״̬
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
                TempStr = "���߹���״̬���óɹ�!";
                TempStrEnglish = "antenna work state set success";
            }
            else
            {
                TempStr = "���߹���״̬����ʧ��!";
                TempStrEnglish = "antenna work state set failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ѯ���߹���״̬
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
                TempStr = "���߹���״̬��ѯ�ɹ�!";
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
                    //Image img = new Bitmap(@"E:\��Ŀ�ĵ�\2014-9-3\C#��-2014-9-15\C#��\λͼ\Single.bmp");
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
                TempStr = "���߹���״̬��ѯʧ��!";
                TempStrEnglish = "antenna work state query failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ���ù���
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
                TempStr = "���߹������óɹ�!";
                TempStrEnglish = "antenna power set success";
            }
            else
            {
                TempStr = "���߹�������ʧ��!";
                TempStrEnglish = "antenna power set failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ѯ����
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
                TempStr = "���߹��ʲ�ѯ�ɹ�!";
                TempStrEnglish = "antenna power query success";

                ANT1_textBox.Text = string.Format("{0:D}", (int)aPwr[0]);
                ANT2_textBox.Text = string.Format("{0:D}", (int)aPwr[1]);
                ANT3_textBox.Text = string.Format("{0:D}", (int)aPwr[2]);
                ANT4_textBox.Text = string.Format("{0:D}", (int)aPwr[3]);
            }
            else
            {
                TempStr = "���߹��ʲ�ѯʧ��!";
                TempStrEnglish = "antenna power query failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��������
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
                TempStr = "���ö�д�����ڲ����ɹ�!";
                TempStrEnglish = "Reader ethernet set success!";
                RESET_button_Click(sender, e);
            }
            else
            {
                TempStr = "���ö�д�����ڲ���ʧ��!";
                TempStrEnglish = "Reader ethernet set fail!";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ѯ����
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
                TempStr = "��ѯ��д�����ڲ����ɹ�!";
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
                TempStr = "��ѯ��д�����ڲ���ʧ��!";
                TempStrEnglish = "Reader ethernet query fail!";
            }

            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ���ö�д����Ƶ����
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
                TempStr = "����Ƶ�������óɹ�!";
                TempStrEnglish = "antenna frequency set success";
            }
            else
            {
                TempStr = "����Ƶ������ʧ��!";
                TempStrEnglish = "antenna frequency set failed";
            }

            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ѯ��д����Ƶ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETFREQ_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            if (0x00 == rfid_sp.Config_GetFreqPoint(CS, ref Freqnum, Freqpoints, 0xFF))
            {
                TempStr = "����Ƶ�ʲ�ѯ�ɹ�!";
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
                TempStr = "����Ƶ�ʲ�ѯʧ��!";
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
        /// ���ÿɱ��IO��
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
                TempStr = "���ö�д�����ÿɱ��IO�ڳɹ�!";
                TempStrEnglish = "set reader outport IO port success";
            }
            else
            {
                TempStr = "���ö�д�����ÿɱ��IO��ʧ��!";
                TempStrEnglish = "set reader outport IO port failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ѯ�ɱ��IO��
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
                TempStr = "��ѯ��д�����ÿɱ��IO�ڳɹ�!";
                TempStrEnglish = "query reader set outport IO port success";
            }
            else
            {
                TempStr = "��ѯ��д�����ÿɱ��IO��ʧ��!";
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
        /// ���ö�д���ز���������
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
                    TempStr = "���ö�д���ز��������Գɹ�!";
                    TempStrEnglish = "set single fast tag mode success";
                    CURRENTMODE_textBox.Text = "��������ģʽ";
                    CURRENTMODE_textBox.Text = "single tag fast mode";

                }
                else
                {
                    TempStr = "���ö�д���ز���������ʧ��!";
                    TempStrEnglish = "set single fast tag mode failed";
                }
            }
            if (MULTITAG_radioButton.Checked)
            {
                Mode = 0x01;
                if (0x00 == rfid_sp.Config_SetSingleFastTagMode(CS, Mode, 0xFF))
                {
                    TempStr = "���ö�д���ز��������Գɹ�!";
                    TempStrEnglish = "set reader single fast tag mode success";
                    CURRENTMODE_textBox.Text = "�࿨ģʽ";
                    CURRENTMODE_textBox.Text = "multitag mode";
                }
                else
                {
                    TempStr = "���ö�д���ز���������ʧ��!";
                    TempStrEnglish = "set reader single fast tag mode failed";
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ѯ��д���ز���������
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
                TempStr = "��ѯ��д���ز��������Գɹ�!";
                TempStrEnglish = "query reader single fast tag mode success";
                if (0x00 == Mode)
                {
                    SINGLETAG_radioButton.Checked = true;
                    if (0 == Language)
                        CURRENTMODE_textBox.Text = "�������ģʽ";
                    else
                        CURRENTMODE_textBox.Text = "single tag fast mode";
                }
                else
                {
                    MULTITAG_radioButton.Checked = true;
                    if (0 == Language)
                        CURRENTMODE_textBox.Text = "�࿨ģʽ";
                    else
                        CURRENTMODE_textBox.Text = "multitag mode";
                }
            }
            else
            {
                TempStr = "��ѯ��д���ز���������ʧ��!";
                TempStrEnglish = "query reader single fast tag mode failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ����ģʽ-�ӿ�
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
        /// ��ѯģʽ-�ӿ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GETINTERFACE_button_Click(object sender, EventArgs e)
        {
            string Text = GETINTERFACE_button.Text;
            WorkModel(255, Text);
            OutInterface(255, Text);
        }

        /*****************************���û��ѯ����ģʽ***************************************/
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

            if (Text == "����" || Text == "Set")
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
                    TempStr = "����ģʽ���óɹ�!";
                    TempStrEnglish = "Work model set success!";
                    if (TRIGGER_radioButton.Checked)
                        TriggerTime(255, Text);
                }
                else
                {
                    TempStr = "����ģʽ����ʧ��!";
                    TempStrEnglish = "Work model set fail!";
                }
            }
            else if (Text == "��ѯ" || Text == "Query")
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
                    TempStr = "��ѯ����ģʽ�ɹ�!";
                    TempStrEnglish = "Work model query success!";
                }
                else
                {
                    TempStr = "��ѯ����ģʽʧ��!";
                    TempStrEnglish = "Work model query fail!";
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
            return 0;
        }

        /********************************���û��ѯ�ӿ�***************************************/
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
            if (Text == "����" || Text == "Set")
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
                    TempStr = "��������ӿڳɹ�!";
                    TempStrEnglish = "OutInterface set success!";
                }
                else
                {
                    TempStr = "��������ӿ�ʧ��!";
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
            else if (Text == "��ѯ" || Text == "Query")
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
                    TempStr = "��ѯ����ӿڳɹ�!";
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
                    TempStr = "��ѯ����ӿ�ʧ��!";
                    TempStrEnglish = "OutInterface query fail!";
                    if (0 == Language)
                        ListBoxAdd(TempStr);
                    else
                        ListBoxAdd(TempStrEnglish);
                }
            }
            return 0;
        }

        /********************************���ô���ʱ��***************************************/
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
                TempStr = "����ʱ�䲻��Ϊ�ջ���0!";
                TempStrEnglish = "Trigger time doesn't null or zero!";
            }
            else
            {
                PSetData[0] = (byte)(triggertime & 0xFF);
                if (Text == "����" || Text == "Set")
                {
                    if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                    {
                        TempStr = "���ô���ʱ��ɹ�!";
                        TempStrEnglish = "Trigger time set success!";
                    }
                    else
                    {
                        TempStr = "���ô���ʱ��ʧ��!";
                        TempStrEnglish = "Trigger time set fail!";
                    }
                }
                else if (Text == "��ѯ" || Text == "Query")
                {
                    if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                    {
                        triggertime = (int)PRefData[0];
                        TRIGGERTIME_textBox.Text = triggertime.ToString();
                        TempStr = "��ѯ����ʱ��ɹ�!";
                        TempStrEnglish = "Trigger time query success!";
                    }
                    else
                    {
                        TempStr = "��ѯ����ʱ��ʧ��!";
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

        /********************************����Τ��������***************************************/
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
            if (Text == "����" || Text == "Set")
            {
                Weigen = WEIGANSTYLE_comboBox.SelectedIndex;
                PSetData[0] = (byte)(Weigen & 0xFF);
                PAddr = 0x98;
                if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                {
                    TempStr = "����Τ�����ͳɹ�!";
                    TempStrEnglish = "Weigen style set success!";
                }
                else
                {
                    TempStr = "����Τ������ʧ��!";
                    TempStrEnglish = "Weigen style set fail!";
                }
            }
            else if (Text == "��ѯ" || Text == "Query")
            {
                PAddr = 0x98;
                if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                {
                    Weigen = (int)PRefData[0];
                    WEIGANSTYLE_comboBox.SelectedIndex = Weigen;
                    TempStr = "��ѯΤ�����ͳɹ�!";
                    TempStrEnglish = "Weigen style query success!";
                }
                else
                {
                    TempStr = "��ѯΤ������ʧ��!";
                    TempStrEnglish = "Weigen style query fail!";
                }
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /********************************���ü̵���ʱ��***************************************/
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
            if (Text == "����" || Text == "Set")
            {
                Relay = Convert.ToInt32(DELAYTIME_textBox.Text);
                if (DELAYTIME_textBox.Text == "" || Relay == 0)
                {
                    TempStr = "�̵�������ʱ�䲻��Ϊ�ջ���0!";
                    TempStrEnglish = "Relay delaytime doesn't null or zero!";
                }
                else
                {
                    PSetData[0] = (byte)(Relay & 0xFF);
                    PAddr = 0x99;
                    if (0x00 == rfid_sp.Parameter_SetReader(CS, PAddr, PLen, PSetData, ReaderAddr))
                    {
                        TempStr = "���ü̵�������ʱ��ɹ�!";
                        TempStrEnglish = "Relay delaytime set success!";
                    }
                    else
                    {
                        TempStr = "���ü̵�������ʱ��ʧ��!";
                        TempStrEnglish = "Relay delaytime set fail!";
                    }
                }
            }
            else if (Text == "��ѯ" || Text == "Query")
            {
                PAddr = 0x99;
                if (0x00 == rfid_sp.Parameter_GetReader(CS, PAddr, PLen, PRefData, ReaderAddr))
                {
                    Relay = (int)PRefData[0];
                    DELAYTIME_textBox.Text = Relay.ToString();
                    TempStr = "��ѯ�̵�������ʱ��ɹ�!";
                    TempStrEnglish = "Relay delaytime query success!";
                }
                else
                {
                    TempStr = "��ѯ�̵�������ʱ��ɹ�!";
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
        /// ��λ��д��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RESET_button_Click(object sender, EventArgs e)
        {
            string TempStr = "";
            string TempStrEnglish = "";
            if (0x00 == rfid_sp.Config_ResetReader(CS, 0xFF))
            {
                TempStr = "��д����λ�ɹ�!";
                TempStrEnglish = "reset reader success";
                DisConnectReader(sender, e);
                Connectbutton.Enabled = true;
                Disconnectbutton.Enabled = false;
            }
            else
            {
                TempStr = "��д����λʧ��!";
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
                TAGDATA_listView.Columns.Add("���", 127, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("ISO18000-6Bʶ��ID", 406, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("����", 78, HorizontalAlignment.Center);
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
                START_button.Text = "��ʼʶ��";
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
                START_button.Text = "��ʼ��ȡ";
                TAGDATA_listView.Columns.Add("���", 127, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("ISO18000-6B��ȡ����", 406, HorizontalAlignment.Center);
                TAGDATA_listView.Columns.Add("����", 78, HorizontalAlignment.Center);
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
                OPERATION_button.Text = "��ȡ";
            else
                OPERATION_button.Text = "Read";
            QUERYLOCK_button.Visible = false;
            UID_checkBox.Enabled = true;
            RESULT_listBox.Items.Clear();
        }

        private void SINGLEWRITE_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Language == 0)
                OPERATION_button.Text = "д��";
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
                OPERATION_button.Text = "����";
                QUERYLOCK_button.Text = "��ѯ";
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
            if (Text == "��ʼʶ��" || Text == "StartIdentify")
            {
                if (Language == 0)
                    ListBoxAdd("��ʼʶ��");
                else
                    ListBoxAdd("Start Identify");

                OperantionTime_timer.Enabled = true;
                MultiTagIdentify_timer.Enabled = true;
            }
            if (Text == "��ʼ��ȡ" || Text == "StartRead")
            {
                if (Language == 0)
                    ListBoxAdd("��ʼ��ȡ");
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
            if (Text == "��ʼʶ��" || Text == "StartIdentify")
            {
                MultiTagIdentify_timer.Enabled = false;
                OperantionTime_timer.Enabled = false;
                if (Language == 0)
                    ListBoxAdd("ֹͣʶ��");
                else
                    ListBoxAdd("Stop Identify");
            }
            if (Text == "��ʼ��ȡ" || Text == "StartRead")
            {
                MultiTagRead_timer.Enabled = false;
                OperantionTime_timer.Enabled = false;
                if (Language == 0)
                    ListBoxAdd("ֹͣ��ȡ");
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
            if (Text == "��ȡ" || Text == "Read")
            {
                SingleTagRead();
            }
            if (Text == "д��" || Text == "Write")
            {
                SingleTagWrite();
            }
            if (Text == "����" || Text == "Lock")
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
        /// �������
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
        /// ���ǩʶ��
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
                if (0 != TagCount)		//��ǩʶ��ɹ�//tag identify success
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
        /// ���ǩ��ȡ
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
                if (0 != TagCount)		//��ǩʶ��ɹ�//tag identify success
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
        /// ����ǩ��ȡ
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
                    ListBoxAdd("IsoI8000����ǩ��ǩ��ȡ�ɹ�!");
                else
                    ListBoxAdd("IsoI8000 single tag read success!");

            }
            else		//��ȡʧ��//read failed	
            {
                ReadData = "Read fail, please again...";
                ResultAddString(ReadData);
                if (0 == Language)
                    ListBoxAdd("IsoI8000����ǩ��ǩ��ȡʧ��!");
                else
                    ListBoxAdd("IsoI8000 single tag read failed!");

            }
        }

        /// <summary>
        /// ����ǩд��
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
                        ResultAddString("д�����ݳɹ�");
                        ListBoxAdd("IsoI8000����ǩ��ǩд��ɹ�!");
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
                        ResultAddString("д������ʧ��");
                        ListBoxAdd("IsoI8000����ǩ��ǩд��ʧ��!");
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
                    ResultAddString("д������Ϊ��!");
                    ListBoxAdd("IsoI8000����ǩ��ǩд������Ϊ��!");
                }
                else
                {
                    ResultAddString("write data is null!");
                    ListBoxAdd("IsoI8000 single tag write data is null!");
                }
            }
        }

        /// <summary>
        /// ����ǩ��������
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
                    ResultAddString("��ǩ��������ʧ��");
                    ListBoxAdd("IsoI8000����ǩ��ǩ��������ʧ��!");
                }
                else
                {
                    ResultAddString("tag data lock failed");
                    ListBoxAdd("IsoI8000 single tag data lock failed!");
                }
            }
        }

        /// <summary>
        /// ����ǩ������ѯ
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
                        ResultAddString("��ǰ��ַδ����!");
                    else
                        ResultAddString("current address unlock!");
                }
                else if (1 == LockStatus)
                {
                    if (0 == Language)
                        ResultAddString("��ǰ��ַ�ѱ�����!");
                    else
                        ResultAddString("current address locked!");

                }
                if (0 == Language)
                    ListBoxAdd("IsoI8000����ǩ��ǩ��������ѯ�ɹ�!");
                else
                    ListBoxAdd("IsoI8000 single tag data lock query success!");

            }
            else
            {
                if (0 == Language)
                {
                    ResultAddString("��ǩ��������ѯʧ��");
                    ListBoxAdd("IsoI8000����ǩ��ǩ��������ѯʧ��!");
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
        //���ǩ����
        /// <summary>
        /// ���ǩ��ѯ
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
                SHOWDATA_listView.Columns.Add("���", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("EPC��ѯ����", 320, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("����", 78, HorizontalAlignment.Center);
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
        /// ���ǩ��ȡ
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
                SHOWDATA_listView.Columns.Add("���", 60, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Reserve��", 80, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Epc��", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("Tid��", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("User��", 100, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("����", 60, HorizontalAlignment.Center);
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
        /// ���ǩд��
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
                SHOWDATA_listView.Columns.Add("���", 80, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("EPC��ѯ����", 350, HorizontalAlignment.Center);
                SHOWDATA_listView.Columns.Add("����", 80, HorizontalAlignment.Center);
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
        /// RESERVE��������ѡ
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
        /// EPC��������ѡ
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
        /// TID��������ѡ
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
        /// USER��������ѡ
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

        /*EPC GEN2��ǩ��ѯ*/
        /// <summary>
        /// ������ѯ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ONCEINVENTORY_button_Click(object sender, EventArgs e)
        {
            ClearIDBuffer();
            EPCMultiTagInventory();
        }

        /// <summary>
        /// EPC���ǩ��ѯ��ʱ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_MultiTagInventory_timer_Tick(object sender, EventArgs e)
        {
            ClearIDBuffer();
            EPCMultiTagInventory();
        }

        /// <summary>
        /// EPC���ǩ��ȡ��ʱ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_MultiTagRead_timer_Tick(object sender, EventArgs e)
        {
            //ClearIDBuffer();
            EPCMultiTagRead();
        }

        /// <summary>
        /// EPC���ǩд�붨ʱ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EPC_MultiTagWrite_timer_Tick(object sender, EventArgs e)
        {
            ClearIDBuffer();
            EPCMultiTagWrite();
        }

        /// <summary>
        /// EPC����ʱ�䶨ʱ
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
                TempStr = "��ʼ��ѯ!";
                TempStrEnglish = "Start query!";
                EPC_OperationTime_timer.Enabled = true;
                EPC_MultiTagInventory_timer.Interval = timer;
                EPC_MultiTagInventory_timer.Enabled = true;
            }
            if (MULTITAG_READ_radioButton.Checked)
            {
                TempStr = "��ʼ���ǩ��ȡ!";
                TempStrEnglish = "Start multi-tag read!";
                EPC_OperationTime_timer.Enabled = true;
                EPC_MultiTagRead_timer.Enabled = true;
            }
            if (MULTITAG_WRITE_radioButton.Checked)
            {
                TempStr = "��ʼ���ǩд��!";
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
                TempStr = "ֹͣ��ѯ!";
                TempStrEnglish = "Stop query!";
                EPC_MultiTagInventory_timer.Enabled = false;
                EPC_OperationTime_timer.Enabled = false;
            }
            if (MULTITAG_READ_radioButton.Checked)
            {
                TempStr = "ֹͣ���ǩ��ȡ!";
                TempStrEnglish = "Stop multi-tag read!";
                EPC_MultiTagRead_timer.Enabled = false;
                EPC_OperationTime_timer.Enabled = false;
            }
            if (MULTITAG_WRITE_radioButton.Checked)
            {
                TempStr = "ֹͣ���ǩд��!";
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

        //����ǩ����
        /// <summary>
        /// ��ȫ��
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
        /// ��ȫд
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
        /// ��ȫ��
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
        /// ���ٱ�ǩ
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
        /// ɸѡ����
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
        /// ִ��
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
        /// ��ջ���
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
        /// EPC GEN2������ǩ��ѯ
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
        /// EPC GEN2���ǩ��ȡ
        /// </summary>
        void EPCMultiTagRead()
        {
            int TagCount = 0;
            int GetCount = 0;
            int i, j;
            int status;
            int RCnt = 0;							//Reserve����ȡ�����ֽ���(Reserve memory read data numbers of byte)
            int ECnt = 0;							//Epc����ȡ�����ֽ���(EPC memory read data numbers of byte)
            int TCnt = 0;							//Tid����ȡ�����ֽ���(TID memory read data numbers of byte)
            int UCnt = 0;							//User����ȡ�����ֽ���(User memory read data numbers of byte)
            byte[] Reserve = new byte[100];		    //Reserve����ȡ����(Reserve memory read data)
            byte[] Epc = new byte[100];			    //Epc����ȡ����(EPC memory read data)
            byte[] Tid = new byte[100];			    //Tid����ȡ����(TID memory read data)
            byte[] User = new byte[100];			//User����ȡ����(User memory read data)
            BufferData[] Data = new BufferData[256];
            for (int index = 0; index < Data.Length; index++)
            {
                Data[index].Data = new byte[512];
            }

            string EPC = "";
            //��ʼ��(initialize)
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
                if (0 != TagCount)		//��ǩ��ȡ�ɹ�//tag read success
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
        /// EPC GEN2���ǩд
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
                if (0 != TagCount)		//��ǩд��ɹ�//tag write success
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
        /// ��ȫ��
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
                TempStr = "��ǩ��ȡ�ɹ�";
                TempStrEnglish = "Tag read success";
                string TStr = "";
                TStr = ByteToHexStr(RData, RCnt * 2);
                VALUE_SINGLE_textBox.Text = TStr;
            }
            else
            {
                TempStr = "��ǩ��ȡʧ��";
                TempStrEnglish = "Tag read failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ȫд
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
                    TempStr = "��ǩд��ɹ�";
                    TempStrEnglish = "tag write success";
                }
                else
                {
                    TempStr = "��ǩд��ʧ��";
                    TempStrEnglish = "tag write failed";
                }
            }
            else
            {
                if (0 == Language)
                    MessageBox.Show("д���ǩ��������д��ĳ��Ȳ�ƥ��!���������");
                else
                    MessageBox.Show("the data input is incorrect length,please check and try again");
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ȫ��
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
                TempStr = "��ǩ�����ɹ�";
                TempStrEnglish = "tag lock success";
            }
            else
            {
                TempStr = "��ǩ����ʧ��";
                TempStrEnglish = "tag lock failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ��ǩ����
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
                TempStr = "��ǩ���ٳɹ�";
                TempStrEnglish = "tag kill success";
            }
            else
            {
                TempStr = "��ǩ����ʧ��";
                TempStrEnglish = "tag kill failed";
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ɸѡ����
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
                    TempStr = "��ǩɸѡ���óɹ�";
                    TempStrEnglish = "tag selection set success";
                }
                else
                {
                    TempStr = "��ǩɸѡ����ʧ��";
                    TempStrEnglish = "tag selection set failed";
                }
            }
            else
            {
                if (0 == Language)
                    MessageBox.Show("��ǩ��ɸѡ���ݳ�����ѡ���ɸѡ���Ȳ�ƥ��!���������");
                else
                    MessageBox.Show("tag selection data length is incorrect, please check and try again");
            }
            if (0 == Language)
                ListBoxAdd(TempStr);
            else
                ListBoxAdd(TempStrEnglish);
        }

        /// <summary>
        /// ����ʱ��
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
            //�������Ĳ���16���ƣ�Ҳ���ǻس�����Backspace������ȡ��������
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
