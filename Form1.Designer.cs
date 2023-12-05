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
            ButtonReadCodes = new Button();
            textBox1 = new TextBox();
            ButtonReloadErrorCodes = new Button();
            ComboBoxDevices = new ComboBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // ButtonReadCodes
            // 
            ButtonReadCodes.Location = new Point(688, 531);
            ButtonReadCodes.Name = "ButtonReadCodes";
            ButtonReadCodes.Size = new Size(94, 29);
            ButtonReadCodes.TabIndex = 0;
            ButtonReadCodes.Text = "Read Codes";
            ButtonReadCodes.UseVisualStyleBackColor = true;
            ButtonReadCodes.Click += ButtonReadCodes_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(11, 112);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(671, 448);
            textBox1.TabIndex = 1;
            // 
            // ButtonReloadErrorCodes
            // 
            ButtonReloadErrorCodes.Location = new Point(688, 473);
            ButtonReloadErrorCodes.Name = "ButtonReloadErrorCodes";
            ButtonReloadErrorCodes.Size = new Size(94, 29);
            ButtonReloadErrorCodes.TabIndex = 2;
            ButtonReloadErrorCodes.Text = "Reload Codes";
            ButtonReloadErrorCodes.UseVisualStyleBackColor = true;
            ButtonReloadErrorCodes.Click += ButtonReloadErrorCodes_Click;
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(794, 572);
            Controls.Add(label1);
            Controls.Add(ComboBoxDevices);
            Controls.Add(ButtonReloadErrorCodes);
            Controls.Add(textBox1);
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
        private TextBox textBox1;
        private Button ButtonReloadErrorCodes;
        private ComboBox ComboBoxDevices;
        private Label label1;
    }
}