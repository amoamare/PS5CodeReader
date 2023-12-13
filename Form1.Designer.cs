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
            SuspendLayout();
            // 
            // ButtonRunOperation
            // 
            ButtonRunOperation.Location = new Point(688, 31);
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
            ComboBoxDevices.Location = new Point(12, 140);
            ComboBoxDevices.Name = "ComboBoxDevices";
            ComboBoxDevices.Size = new Size(670, 28);
            ComboBoxDevices.TabIndex = 3;
            ComboBoxDevices.DropDown += ComboBoxDevices_DropDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 117);
            label1.Name = "label1";
            label1.Size = new Size(151, 20);
            label1.TabIndex = 4;
            label1.Text = "Serial Devices (UART)";
            // 
            // LogBox
            // 
            LogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LogBox.Location = new Point(12, 174);
            LogBox.Name = "LogBox";
            LogBox.ReadOnly = true;
            LogBox.Size = new Size(869, 386);
            LogBox.TabIndex = 5;
            LogBox.TabStop = false;
            LogBox.Text = "";
            // 
            // ComboBoxDeviceType
            // 
            ComboBoxDeviceType.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxDeviceType.FormattingEnabled = true;
            ComboBoxDeviceType.Location = new Point(12, 32);
            ComboBoxDeviceType.Name = "ComboBoxDeviceType";
            ComboBoxDeviceType.Size = new Size(670, 28);
            ComboBoxDeviceType.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(98, 20);
            label2.TabIndex = 8;
            label2.Text = "Select Device";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 63);
            label3.Name = "label3";
            label3.Size = new Size(155, 20);
            label3.TabIndex = 10;
            label3.Text = "Select Operation Type";
            // 
            // ComboBoxOperationType
            // 
            ComboBoxOperationType.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxOperationType.FormattingEnabled = true;
            ComboBoxOperationType.Location = new Point(12, 86);
            ComboBoxOperationType.Name = "ComboBoxOperationType";
            ComboBoxOperationType.Size = new Size(670, 28);
            ComboBoxOperationType.TabIndex = 9;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(893, 572);
            Controls.Add(label3);
            Controls.Add(ComboBoxOperationType);
            Controls.Add(label2);
            Controls.Add(ComboBoxDeviceType);
            Controls.Add(LogBox);
            Controls.Add(label1);
            Controls.Add(ComboBoxDevices);
            Controls.Add(ButtonRunOperation);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PS5 Code Reader";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
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
    }
}