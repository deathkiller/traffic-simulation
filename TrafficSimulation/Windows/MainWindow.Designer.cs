namespace TrafficSimulation.Windows
{
    partial class MainWindow
    {
        /// <summary>
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód generovaný Návrhářem Windows Form

        /// <summary>
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.newButton = new System.Windows.Forms.ToolStripButton();
            this.openButton = new System.Windows.Forms.ToolStripButton();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.runButton = new System.Windows.Forms.ToolStripButton();
            this.stepButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.deviceCombobox = new System.Windows.Forms.ToolStripComboBox();
            this.trafficView = new TrafficSimulation.Controls.TrafficView();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator3,
            this.newButton,
            this.openButton,
            this.saveButton,
            this.toolStripSeparator1,
            this.runButton,
            this.stepButton,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.deviceCombobox});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(830, 25);
            this.toolStrip.TabIndex = 9;
            this.toolStrip.Text = "toolStrip";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // newButton
            // 
            this.newButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(35, 22);
            this.newButton.Text = "New";
            this.newButton.Click += new System.EventHandler(this.OnNewButtonClick);
            // 
            // openButton
            // 
            this.openButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(40, 22);
            this.openButton.Text = "Open";
            this.openButton.Click += new System.EventHandler(this.OnOpenButtonClick);
            // 
            // saveButton
            // 
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(35, 22);
            this.saveButton.Text = "Save";
            this.saveButton.Click += new System.EventHandler(this.OnSaveButtonClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // runButton
            // 
            this.runButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(32, 22);
            this.runButton.Text = "Run";
            this.runButton.Click += new System.EventHandler(this.OnRunButtonClick);
            // 
            // stepButton
            // 
            this.stepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepButton.Name = "stepButton";
            this.stepButton.Size = new System.Drawing.Size(59, 22);
            this.stepButton.Text = "One Step";
            this.stepButton.Click += new System.EventHandler(this.OnStepButtonClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(48, 22);
            this.toolStripLabel1.Text = "Device: ";
            // 
            // deviceCombobox
            // 
            this.deviceCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceCombobox.DropDownWidth = 250;
            this.deviceCombobox.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.deviceCombobox.Name = "deviceCombobox";
            this.deviceCombobox.Size = new System.Drawing.Size(350, 25);
            // 
            // trafficView
            // 
            this.trafficView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trafficView.Location = new System.Drawing.Point(1, 26);
            this.trafficView.Name = "trafficView";
            this.trafficView.Simulation = null;
            this.trafficView.Size = new System.Drawing.Size(827, 388);
            this.trafficView.TabIndex = 0;
            this.trafficView.Text = "trafficView1";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(830, 416);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.trafficView);
            this.MinimumSize = new System.Drawing.Size(350, 200);
            this.Name = "MainWindow";
            this.Text = "Traffic Simulation";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TrafficSimulation.Controls.TrafficView trafficView;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton newButton;
        private System.Windows.Forms.ToolStripButton openButton;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton runButton;
        private System.Windows.Forms.ToolStripButton stepButton;
        private System.Windows.Forms.ToolStripComboBox deviceCombobox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

