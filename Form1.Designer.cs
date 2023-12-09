﻿namespace PS5CodeReader
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
            ButtonReadCodes = new Button();
            ComboBoxDevices = new ComboBox();
            label1 = new Label();
            LogBox = new ReadOnlyRichTextBox();
            ButtonClearLogs = new Button();
            SuspendLayout();
            // 
            // ButtonReadCodes
            // 
            ButtonReadCodes.Location = new Point(688, 31);
            ButtonReadCodes.Name = "ButtonReadCodes";
            ButtonReadCodes.Size = new Size(94, 29);
            ButtonReadCodes.TabIndex = 0;
            ButtonReadCodes.Text = "Read Codes";
            ButtonReadCodes.UseVisualStyleBackColor = true;
            ButtonReadCodes.Click += ButtonReadCodes_Click;
            // 
            // ComboBoxDevices
            // 
            ComboBoxDevices.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxDevices.FormattingEnabled = true;
            ComboBoxDevices.Location = new Point(12, 32);
            ComboBoxDevices.Name = "ComboBoxDevices";
            ComboBoxDevices.Size = new Size(670, 28);
            ComboBoxDevices.TabIndex = 3;
            ComboBoxDevices.DropDown += ComboBoxDevices_DropDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(151, 20);
            label1.TabIndex = 4;
            label1.Text = "Serial Devices (UART)";
            // 
            // LogBox
            // 
            LogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LogBox.Location = new Point(12, 66);
            LogBox.Name = "LogBox";
            LogBox.ReadOnly = true;
            LogBox.Size = new Size(869, 494);
            LogBox.TabIndex = 5;
            LogBox.TabStop = false;
            LogBox.Text = "";
            // 
            // ButtonClearLogs
            // 
            ButtonClearLogs.Location = new Point(788, 31);
            ButtonClearLogs.Name = "ButtonClearLogs";
            ButtonClearLogs.Size = new Size(94, 29);
            ButtonClearLogs.TabIndex = 6;
            ButtonClearLogs.Text = "Clear Logs";
            ButtonClearLogs.UseVisualStyleBackColor = true;
            ButtonClearLogs.Click += ButtonClearLogs_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(893, 572);
            Controls.Add(ButtonClearLogs);
            Controls.Add(LogBox);
            Controls.Add(label1);
            Controls.Add(ComboBoxDevices);
            Controls.Add(ButtonReadCodes);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PS5 Code Reader";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button ButtonReadCodes;
        private ComboBox ComboBoxDevices;
        private Label label1;
        private ReadOnlyRichTextBox LogBox;
        private Button ButtonClearLogs;
    }
}