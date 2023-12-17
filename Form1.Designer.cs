namespace PS5CodeReader
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ButtonRunOperation = new Button();
            ComboBoxDevices = new ComboBox();
            label1 = new Label();
            LogBox = new ReadOnlyRichTextBox();
            ComboBoxDeviceType = new ComboBox();
            label2 = new Label();
            label3 = new Label();
            ComboBoxOperationType = new ComboBox();
            panel4 = new Panel();
            PanelRawCommand = new Panel();
            label4 = new Label();
            TextBoxRawCommand = new TextBox();
            panel3 = new Panel();
            panel4.SuspendLayout();
            PanelRawCommand.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // ButtonRunOperation
            // 
            ButtonRunOperation.Location = new Point(679, 22);
            ButtonRunOperation.Name = "ButtonRunOperation";
            ButtonRunOperation.Size = new Size(94, 29);
            ButtonRunOperation.TabIndex = 0;
            ButtonRunOperation.Text = "Run Operation";
            ButtonRunOperation.UseVisualStyleBackColor = true;
            ButtonRunOperation.Click += ButtonRunOperations_Click;
            // 
            // ComboBoxDevices
            // 
            ComboBoxDevices.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxDevices.FormattingEnabled = true;
            ComboBoxDevices.Location = new Point(3, 131);
            ComboBoxDevices.Name = "ComboBoxDevices";
            ComboBoxDevices.Size = new Size(670, 28);
            ComboBoxDevices.TabIndex = 3;
            ComboBoxDevices.DropDown += ComboBoxDevices_DropDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 108);
            label1.Name = "label1";
            label1.Size = new Size(151, 20);
            label1.TabIndex = 4;
            label1.Text = "Serial Devices (UART)";
            // 
            // LogBox
            // 
            LogBox.Dock = DockStyle.Fill;
            LogBox.Location = new Point(0, 0);
            LogBox.Name = "LogBox";
            LogBox.ReadOnly = true;
            LogBox.Size = new Size(782, 438);
            LogBox.TabIndex = 5;
            LogBox.TabStop = false;
            LogBox.Text = "";
            // 
            // ComboBoxDeviceType
            // 
            ComboBoxDeviceType.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxDeviceType.FormattingEnabled = true;
            ComboBoxDeviceType.Location = new Point(3, 23);
            ComboBoxDeviceType.Name = "ComboBoxDeviceType";
            ComboBoxDeviceType.Size = new Size(670, 28);
            ComboBoxDeviceType.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(98, 20);
            label2.TabIndex = 8;
            label2.Text = "Select Device";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 54);
            label3.Name = "label3";
            label3.Size = new Size(155, 20);
            label3.TabIndex = 10;
            label3.Text = "Select Operation Type";
            // 
            // ComboBoxOperationType
            // 
            ComboBoxOperationType.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxOperationType.FormattingEnabled = true;
            ComboBoxOperationType.Location = new Point(3, 77);
            ComboBoxOperationType.Name = "ComboBoxOperationType";
            ComboBoxOperationType.Size = new Size(670, 28);
            ComboBoxOperationType.TabIndex = 9;
            // 
            // panel4
            // 
            panel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel4.Controls.Add(LogBox);
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(0, 216);
            panel4.Name = "panel4";
            panel4.Size = new Size(782, 438);
            panel4.TabIndex = 14;
            // 
            // PanelRawCommand
            // 
            PanelRawCommand.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PanelRawCommand.Controls.Add(label4);
            PanelRawCommand.Controls.Add(TextBoxRawCommand);
            PanelRawCommand.Dock = DockStyle.Top;
            PanelRawCommand.Location = new Point(0, 160);
            PanelRawCommand.Name = "PanelRawCommand";
            PanelRawCommand.Size = new Size(782, 56);
            PanelRawCommand.TabIndex = 13;
            PanelRawCommand.Visible = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(3, 3);
            label4.Name = "label4";
            label4.Size = new Size(110, 20);
            label4.TabIndex = 11;
            label4.Text = "Raw Command";
            // 
            // TextBoxRawCommand
            // 
            TextBoxRawCommand.Enabled = false;
            TextBoxRawCommand.Location = new Point(3, 26);
            TextBoxRawCommand.Name = "TextBoxRawCommand";
            TextBoxRawCommand.Size = new Size(670, 27);
            TextBoxRawCommand.TabIndex = 12;
            TextBoxRawCommand.KeyPress += TextBoxRawCommand_KeyPress;
            // 
            // panel3
            // 
            panel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel3.Controls.Add(label2);
            panel3.Controls.Add(ComboBoxDeviceType);
            panel3.Controls.Add(ButtonRunOperation);
            panel3.Controls.Add(ComboBoxOperationType);
            panel3.Controls.Add(ComboBoxDevices);
            panel3.Controls.Add(label1);
            panel3.Controls.Add(label3);
            panel3.Dock = DockStyle.Top;
            panel3.Location = new Point(0, 0);
            panel3.Margin = new Padding(3, 3, 3, 5);
            panel3.Name = "panel3";
            panel3.Size = new Size(782, 160);
            panel3.TabIndex = 11;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 654);
            Controls.Add(panel4);
            Controls.Add(PanelRawCommand);
            Controls.Add(panel3);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PS5 Code Reader";
            Load += Form1_Load;
            panel4.ResumeLayout(false);
            PanelRawCommand.ResumeLayout(false);
            PanelRawCommand.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button ButtonRunOperation;
        private ComboBox ComboBoxDevices;
        private Label label1;
        private ReadOnlyRichTextBox LogBox;
        private ComboBox ComboBoxDeviceType;
        private Label label2;
        private Label label3;
        private ComboBox ComboBoxOperationType;
        private Panel PanelRawCommand;
        private Panel panel3;
        private Panel panel4;
        private TextBox TextBoxRawCommand;
        private Label label4;
    }
}