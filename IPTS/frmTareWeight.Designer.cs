namespace IPTS
{
    partial class frmTareWeight
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pbBack = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.ckAutoSave = new System.Windows.Forms.CheckBox();
            this.picSecondSensor = new System.Windows.Forms.PictureBox();
            this.label20 = new System.Windows.Forms.Label();
            this.logList = new DragonFactory.UserContols.DragonListLog();
            this.lblMessage = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.ckStatus = new System.Windows.Forms.CheckBox();
            this.txtRFIDValue = new System.Windows.Forms.TextBox();
            this.picFirstSensor = new System.Windows.Forms.PictureBox();
            this.txtTruckNumber = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTransporter = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTransportCOde = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.txtModel = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtMake = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtTruckID = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.pbCancel = new System.Windows.Forms.PictureBox();
            this.pbSave = new System.Windows.Forms.PictureBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtRemarks = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtNewTareWeight = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPreviousTareWeight = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.dgvTruckTareWeight = new System.Windows.Forms.DataGridView();
            this.label11 = new System.Windows.Forms.Label();
            this.tmrCheckWt = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbBack)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSecondSensor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFirstSensor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSave)).BeginInit();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTruckTareWeight)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(571, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 33);
            this.label1.TabIndex = 1;
            this.label1.Text = "Tare Weight";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.pbBack);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1276, 59);
            this.panel1.TabIndex = 1;
            // 
            // pbBack
            // 
            this.pbBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbBack.Image = global::IPTS.Properties.Resources.unnamed;
            this.pbBack.Location = new System.Drawing.Point(0, 0);
            this.pbBack.Name = "pbBack";
            this.pbBack.Size = new System.Drawing.Size(67, 59);
            this.pbBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbBack.TabIndex = 0;
            this.pbBack.TabStop = false;
            this.pbBack.Click += new System.EventHandler(this.pbBack_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 59);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1276, 2);
            this.panel2.TabIndex = 2;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel3.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel3.Controls.Add(this.ckAutoSave);
            this.panel3.Controls.Add(this.picSecondSensor);
            this.panel3.Controls.Add(this.label20);
            this.panel3.Controls.Add(this.logList);
            this.panel3.Controls.Add(this.lblMessage);
            this.panel3.Controls.Add(this.label21);
            this.panel3.Controls.Add(this.ckStatus);
            this.panel3.Controls.Add(this.txtRFIDValue);
            this.panel3.Controls.Add(this.picFirstSensor);
            this.panel3.Controls.Add(this.txtTruckNumber);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.txtTransporter);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.txtTransportCOde);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.txtStatus);
            this.panel3.Controls.Add(this.txtModel);
            this.panel3.Controls.Add(this.label12);
            this.panel3.Controls.Add(this.txtMake);
            this.panel3.Controls.Add(this.label13);
            this.panel3.Controls.Add(this.txtTruckID);
            this.panel3.Controls.Add(this.label14);
            this.panel3.Controls.Add(this.pbCancel);
            this.panel3.Controls.Add(this.pbSave);
            this.panel3.Controls.Add(this.label10);
            this.panel3.Controls.Add(this.txtRemarks);
            this.panel3.Controls.Add(this.label9);
            this.panel3.Controls.Add(this.txtNewTareWeight);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.txtPreviousTareWeight);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Location = new System.Drawing.Point(12, 67);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(508, 598);
            this.panel3.TabIndex = 3;
            this.panel3.Paint += new System.Windows.Forms.PaintEventHandler(this.panel3_Paint);
            // 
            // ckAutoSave
            // 
            this.ckAutoSave.AutoSize = true;
            this.ckAutoSave.Checked = true;
            this.ckAutoSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckAutoSave.Location = new System.Drawing.Point(479, 11);
            this.ckAutoSave.Name = "ckAutoSave";
            this.ckAutoSave.Size = new System.Drawing.Size(15, 14);
            this.ckAutoSave.TabIndex = 29;
            this.ckAutoSave.UseVisualStyleBackColor = true;
            // 
            // picSecondSensor
            // 
            this.picSecondSensor.BackColor = System.Drawing.Color.Green;
            this.picSecondSensor.Location = new System.Drawing.Point(382, 469);
            this.picSecondSensor.Name = "picSecondSensor";
            this.picSecondSensor.Size = new System.Drawing.Size(43, 15);
            this.picSecondSensor.TabIndex = 27;
            this.picSecondSensor.TabStop = false;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.Black;
            this.label20.Location = new System.Drawing.Point(376, 451);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(54, 15);
            this.label20.TabIndex = 25;
            this.label20.Text = "Sensor-2";
            // 
            // logList
            // 
            this.logList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.logList.FormattingEnabled = true;
            this.logList.Location = new System.Drawing.Point(0, 568);
            this.logList.MessageFormat = "{3} : {8}";
            this.logList.Name = "logList";
            this.logList.Paused = false;
            this.logList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.logList.Size = new System.Drawing.Size(508, 30);
            this.logList.TabIndex = 24;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(12, 471);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(61, 13);
            this.lblMessage.TabIndex = 4;
            this.lblMessage.Text = "lblMessage";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.ForeColor = System.Drawing.Color.Black;
            this.label21.Location = new System.Drawing.Point(252, 451);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(54, 15);
            this.label21.TabIndex = 26;
            this.label21.Text = "Sensor-1";
            // 
            // ckStatus
            // 
            this.ckStatus.AutoSize = true;
            this.ckStatus.Checked = true;
            this.ckStatus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckStatus.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ckStatus.Location = new System.Drawing.Point(183, 218);
            this.ckStatus.Name = "ckStatus";
            this.ckStatus.Size = new System.Drawing.Size(15, 14);
            this.ckStatus.TabIndex = 3;
            this.ckStatus.UseVisualStyleBackColor = true;
            this.ckStatus.Visible = false;
            // 
            // txtRFIDValue
            // 
            this.txtRFIDValue.BackColor = System.Drawing.Color.White;
            this.txtRFIDValue.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRFIDValue.Location = new System.Drawing.Point(113, 6);
            this.txtRFIDValue.Name = "txtRFIDValue";
            this.txtRFIDValue.ReadOnly = true;
            this.txtRFIDValue.Size = new System.Drawing.Size(121, 22);
            this.txtRFIDValue.TabIndex = 23;
            this.txtRFIDValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtRFIDValue.Visible = false;
            // 
            // picFirstSensor
            // 
            this.picFirstSensor.BackColor = System.Drawing.Color.Green;
            this.picFirstSensor.Location = new System.Drawing.Point(255, 469);
            this.picFirstSensor.Name = "picFirstSensor";
            this.picFirstSensor.Size = new System.Drawing.Size(43, 15);
            this.picFirstSensor.TabIndex = 28;
            this.picFirstSensor.TabStop = false;
            // 
            // txtTruckNumber
            // 
            this.txtTruckNumber.BackColor = System.Drawing.Color.White;
            this.txtTruckNumber.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTruckNumber.Location = new System.Drawing.Point(219, 81);
            this.txtTruckNumber.Name = "txtTruckNumber";
            this.txtTruckNumber.ReadOnly = true;
            this.txtTruckNumber.Size = new System.Drawing.Size(284, 28);
            this.txtTruckNumber.TabIndex = 9;
            this.txtTruckNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(3, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 21);
            this.label3.TabIndex = 9;
            this.label3.Text = "Truck Number";
            // 
            // txtTransporter
            // 
            this.txtTransporter.BackColor = System.Drawing.Color.White;
            this.txtTransporter.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTransporter.Location = new System.Drawing.Point(221, 298);
            this.txtTransporter.Name = "txtTransporter";
            this.txtTransporter.ReadOnly = true;
            this.txtTransporter.Size = new System.Drawing.Size(282, 28);
            this.txtTransporter.TabIndex = 5;
            this.txtTransporter.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(1, 301);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 21);
            this.label4.TabIndex = 10;
            this.label4.Text = "Transporter";
            // 
            // txtTransportCOde
            // 
            this.txtTransportCOde.BackColor = System.Drawing.Color.White;
            this.txtTransportCOde.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTransportCOde.Location = new System.Drawing.Point(221, 253);
            this.txtTransportCOde.Name = "txtTransportCOde";
            this.txtTransportCOde.ReadOnly = true;
            this.txtTransportCOde.Size = new System.Drawing.Size(282, 28);
            this.txtTransportCOde.TabIndex = 4;
            this.txtTransportCOde.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(1, 256);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(132, 21);
            this.label6.TabIndex = 11;
            this.label6.Text = "Transporter Code";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(375, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 21);
            this.label2.TabIndex = 12;
            this.label2.Text = "Auto Save";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(1, 211);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 21);
            this.label5.TabIndex = 12;
            this.label5.Text = "Status";
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.Color.White;
            this.txtStatus.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.Location = new System.Drawing.Point(219, 208);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(284, 28);
            this.txtStatus.TabIndex = 2;
            this.txtStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtModel
            // 
            this.txtModel.BackColor = System.Drawing.Color.White;
            this.txtModel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtModel.Location = new System.Drawing.Point(221, 167);
            this.txtModel.Name = "txtModel";
            this.txtModel.ReadOnly = true;
            this.txtModel.Size = new System.Drawing.Size(282, 28);
            this.txtModel.TabIndex = 2;
            this.txtModel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(1, 168);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 21);
            this.label12.TabIndex = 13;
            this.label12.Text = "Model";
            // 
            // txtMake
            // 
            this.txtMake.BackColor = System.Drawing.Color.White;
            this.txtMake.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMake.Location = new System.Drawing.Point(221, 124);
            this.txtMake.Name = "txtMake";
            this.txtMake.ReadOnly = true;
            this.txtMake.Size = new System.Drawing.Size(282, 28);
            this.txtMake.TabIndex = 1;
            this.txtMake.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(1, 125);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 21);
            this.label13.TabIndex = 15;
            this.label13.Text = "Make";
            // 
            // txtTruckID
            // 
            this.txtTruckID.BackColor = System.Drawing.Color.White;
            this.txtTruckID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTruckID.Location = new System.Drawing.Point(221, 42);
            this.txtTruckID.Name = "txtTruckID";
            this.txtTruckID.ReadOnly = true;
            this.txtTruckID.Size = new System.Drawing.Size(282, 28);
            this.txtTruckID.TabIndex = 0;
            this.txtTruckID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(1, 43);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(67, 21);
            this.label14.TabIndex = 16;
            this.label14.Text = "Truck ID";
            // 
            // pbCancel
            // 
            this.pbCancel.Image = global::IPTS.Properties.Resources.btnCance;
            this.pbCancel.Location = new System.Drawing.Point(255, 507);
            this.pbCancel.Name = "pbCancel";
            this.pbCancel.Size = new System.Drawing.Size(126, 50);
            this.pbCancel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCancel.TabIndex = 3;
            this.pbCancel.TabStop = false;
            this.pbCancel.Click += new System.EventHandler(this.pbCancel_Click);
            // 
            // pbSave
            // 
            this.pbSave.Image = global::IPTS.Properties.Resources.btnSave;
            this.pbSave.Location = new System.Drawing.Point(123, 507);
            this.pbSave.Name = "pbSave";
            this.pbSave.Size = new System.Drawing.Size(126, 50);
            this.pbSave.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbSave.TabIndex = 4;
            this.pbSave.TabStop = false;
            this.pbSave.Click += new System.EventHandler(this.pbSave_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Calibri", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Navy;
            this.label10.Location = new System.Drawing.Point(4, 4);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(103, 21);
            this.label10.TabIndex = 2;
            this.label10.Text = "Tare Weight :";
            // 
            // txtRemarks
            // 
            this.txtRemarks.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRemarks.Location = new System.Drawing.Point(219, 412);
            this.txtRemarks.Name = "txtRemarks";
            this.txtRemarks.Size = new System.Drawing.Size(284, 28);
            this.txtRemarks.TabIndex = 8;
            this.txtRemarks.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(1, 415);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 21);
            this.label9.TabIndex = 0;
            this.label9.Text = "Remarks";
            // 
            // txtNewTareWeight
            // 
            this.txtNewTareWeight.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNewTareWeight.Location = new System.Drawing.Point(219, 374);
            this.txtNewTareWeight.Name = "txtNewTareWeight";
            this.txtNewTareWeight.Size = new System.Drawing.Size(284, 28);
            this.txtNewTareWeight.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(1, 377);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(149, 21);
            this.label8.TabIndex = 0;
            this.label8.Text = "New Tare Weight(T)";
            // 
            // txtPreviousTareWeight
            // 
            this.txtPreviousTareWeight.BackColor = System.Drawing.Color.White;
            this.txtPreviousTareWeight.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPreviousTareWeight.Location = new System.Drawing.Point(221, 338);
            this.txtPreviousTareWeight.Name = "txtPreviousTareWeight";
            this.txtPreviousTareWeight.Size = new System.Drawing.Size(282, 28);
            this.txtPreviousTareWeight.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(1, 341);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(177, 21);
            this.label7.TabIndex = 0;
            this.label7.Text = "Previous Tare Weight(T)";
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel4.Controls.Add(this.dgvTruckTareWeight);
            this.panel4.Controls.Add(this.label11);
            this.panel4.Location = new System.Drawing.Point(526, 67);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(745, 598);
            this.panel4.TabIndex = 3;
            // 
            // dgvTruckTareWeight
            // 
            this.dgvTruckTareWeight.AllowUserToAddRows = false;
            this.dgvTruckTareWeight.AllowUserToDeleteRows = false;
            this.dgvTruckTareWeight.AllowUserToOrderColumns = true;
            this.dgvTruckTareWeight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTruckTareWeight.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvTruckTareWeight.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvTruckTareWeight.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTruckTareWeight.Location = new System.Drawing.Point(8, 40);
            this.dgvTruckTareWeight.Name = "dgvTruckTareWeight";
            this.dgvTruckTareWeight.RowHeadersVisible = false;
            this.dgvTruckTareWeight.Size = new System.Drawing.Size(730, 551);
            this.dgvTruckTareWeight.TabIndex = 3;
            this.dgvTruckTareWeight.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTruckTareWeight_CellContentDoubleClick);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Calibri", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Navy;
            this.label11.Location = new System.Drawing.Point(4, 4);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(199, 21);
            this.label11.TabIndex = 2;
            this.label11.Text = "Truck Tare Weight History :";
            // 
            // tmrCheckWt
            // 
            this.tmrCheckWt.Enabled = true;
            this.tmrCheckWt.Interval = 250;
            this.tmrCheckWt.Tick += new System.EventHandler(this.tmrCheckWt_Tick);
            // 
            // frmTareWeight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1276, 670);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmTareWeight";
            this.Text = "frmTareWeight";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmTareWeight_FormClosing);
            this.Load += new System.EventHandler(this.frmTareWeight_Load);
            this.Shown += new System.EventHandler(this.frmTareWeight_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbBack)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSecondSensor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFirstSensor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSave)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTruckTareWeight)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbBack;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TextBox txtRemarks;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtNewTareWeight;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtPreviousTareWeight;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DataGridView dgvTruckTareWeight;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.PictureBox pbCancel;
        private System.Windows.Forms.PictureBox pbSave;
        private System.Windows.Forms.CheckBox ckStatus;
        private System.Windows.Forms.TextBox txtRFIDValue;
        private System.Windows.Forms.TextBox txtTruckNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTransporter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTransportCOde;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtModel;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtMake;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtTruckID;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblMessage;
        private DragonFactory.UserContols.DragonListLog logList;
        private System.Windows.Forms.PictureBox picSecondSensor;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.PictureBox picFirstSensor;
        private System.Windows.Forms.Timer tmrCheckWt;
        private System.Windows.Forms.CheckBox ckAutoSave;
        private System.Windows.Forms.Label label2;
    }
}