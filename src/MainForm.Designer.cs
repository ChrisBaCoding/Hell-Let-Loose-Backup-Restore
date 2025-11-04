namespace HLL_BackupAndRestore
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tabControl1 = new TabControl();
            tabBackup = new TabPage();
            label1 = new Label();
            txtLog = new TextBox();
            btnBackup = new Button();
            tabRestore = new TabPage();
            listBackups = new ListView();
            tabControl1.SuspendLayout();
            tabBackup.SuspendLayout();
            tabRestore.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabBackup);
            tabControl1.Controls.Add(tabRestore);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(800, 450);
            tabControl1.TabIndex = 0;
            // 
            // tabBackup
            // 
            tabBackup.Controls.Add(label1);
            tabBackup.Controls.Add(txtLog);
            tabBackup.Controls.Add(btnBackup);
            tabBackup.Location = new Point(4, 24);
            tabBackup.Name = "tabBackup";
            tabBackup.Padding = new Padding(3);
            tabBackup.Size = new Size(792, 422);
            tabBackup.TabIndex = 0;
            tabBackup.Text = "Backup";
            tabBackup.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.ControlDarkDark;
            label1.Location = new Point(278, 21);
            label1.Name = "label1";
            label1.Size = new Size(243, 15);
            label1.TabIndex = 2;
            label1.Text = "Save your custom settings for Hell Let Loose.";
            // 
            // txtLog
            // 
            txtLog.Location = new Point(6, 107);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(780, 307);
            txtLog.TabIndex = 1;
            // 
            // btnBackup
            // 
            btnBackup.FlatStyle = FlatStyle.Flat;
            btnBackup.Location = new Point(313, 58);
            btnBackup.Name = "btnBackup";
            btnBackup.Size = new Size(166, 23);
            btnBackup.TabIndex = 0;
            btnBackup.Text = "Backup now!";
            btnBackup.UseVisualStyleBackColor = true;
            btnBackup.Click += btnBackup_Click;
            // 
            // tabRestore
            // 
            tabRestore.Controls.Add(listBackups);
            tabRestore.Location = new Point(4, 24);
            tabRestore.Name = "tabRestore";
            tabRestore.Padding = new Padding(3);
            tabRestore.Size = new Size(792, 422);
            tabRestore.TabIndex = 1;
            tabRestore.Text = "Restore";
            tabRestore.UseVisualStyleBackColor = true;
            // 
            // listBackups
            // 
            listBackups.Dock = DockStyle.Fill;
            listBackups.Location = new Point(3, 3);
            listBackups.Name = "listBackups";
            listBackups.Size = new Size(786, 416);
            listBackups.TabIndex = 0;
            listBackups.UseCompatibleStateImageBehavior = false;
            listBackups.MouseClick += listBackups_MouseClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabControl1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "HLL - Backup & Restore";
            Load += MainForm_Load;
            tabControl1.ResumeLayout(false);
            tabBackup.ResumeLayout(false);
            tabBackup.PerformLayout();
            tabRestore.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabBackup;
        private TabPage tabRestore;
        private Button btnBackup;
        private TextBox txtLog;
        private ListView listBackups;
        private Label label1;
    }
}