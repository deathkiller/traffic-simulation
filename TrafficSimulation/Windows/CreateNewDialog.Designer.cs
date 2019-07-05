namespace TrafficSimulation.Windows
{
    partial class CreateNewDialog
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
            if (disposing && (components != null)) {
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
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cellBasedRadio = new System.Windows.Forms.RadioButton();
            this.carFollowingRadio = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.distanceBox = new System.Windows.Forms.NumericUpDown();
            this.junctionsXBox = new System.Windows.Forms.NumericUpDown();
            this.junctionsYBox = new System.Windows.Forms.NumericUpDown();
            this.carsBox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.maxCarsBox = new System.Windows.Forms.NumericUpDown();
            this.generatorProbabilityBox = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.distanceBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.junctionsXBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.junctionsYBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.carsBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxCarsBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.generatorProbabilityBox)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(240, 251);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "Create";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(321, 251);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Distance between junctions:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Number of junctions:";
            // 
            // cellBasedRadio
            // 
            this.cellBasedRadio.AutoSize = true;
            this.cellBasedRadio.Checked = true;
            this.cellBasedRadio.Location = new System.Drawing.Point(32, 37);
            this.cellBasedRadio.Name = "cellBasedRadio";
            this.cellBasedRadio.Size = new System.Drawing.Size(143, 17);
            this.cellBasedRadio.TabIndex = 4;
            this.cellBasedRadio.TabStop = true;
            this.cellBasedRadio.Text = "Cellular automaton model";
            this.cellBasedRadio.UseVisualStyleBackColor = true;
            // 
            // carFollowingRadio
            // 
            this.carFollowingRadio.AutoSize = true;
            this.carFollowingRadio.Location = new System.Drawing.Point(32, 60);
            this.carFollowingRadio.Name = "carFollowingRadio";
            this.carFollowingRadio.Size = new System.Drawing.Size(116, 17);
            this.carFollowingRadio.TabIndex = 5;
            this.carFollowingRadio.Text = "Car following model";
            this.carFollowingRadio.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Initial number of vehicles:";
            // 
            // distanceBox
            // 
            this.distanceBox.Location = new System.Drawing.Point(201, 91);
            this.distanceBox.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.distanceBox.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.distanceBox.Name = "distanceBox";
            this.distanceBox.Size = new System.Drawing.Size(198, 20);
            this.distanceBox.TabIndex = 7;
            this.distanceBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.distanceBox.ThousandsSeparator = true;
            this.distanceBox.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // junctionsXBox
            // 
            this.junctionsXBox.Location = new System.Drawing.Point(201, 120);
            this.junctionsXBox.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.junctionsXBox.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.junctionsXBox.Name = "junctionsXBox";
            this.junctionsXBox.Size = new System.Drawing.Size(87, 20);
            this.junctionsXBox.TabIndex = 8;
            this.junctionsXBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.junctionsXBox.ThousandsSeparator = true;
            this.junctionsXBox.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // junctionsYBox
            // 
            this.junctionsYBox.Location = new System.Drawing.Point(321, 120);
            this.junctionsYBox.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.junctionsYBox.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.junctionsYBox.Name = "junctionsYBox";
            this.junctionsYBox.Size = new System.Drawing.Size(78, 20);
            this.junctionsYBox.TabIndex = 9;
            this.junctionsYBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.junctionsYBox.ThousandsSeparator = true;
            this.junctionsYBox.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // carsBox
            // 
            this.carsBox.Location = new System.Drawing.Point(201, 151);
            this.carsBox.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.carsBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.carsBox.Name = "carsBox";
            this.carsBox.Size = new System.Drawing.Size(198, 20);
            this.carsBox.TabIndex = 10;
            this.carsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.carsBox.ThousandsSeparator = true;
            this.carsBox.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(300, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "x";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 182);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Max. number of vehicles:";
            // 
            // maxCarsBox
            // 
            this.maxCarsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxCarsBox.Location = new System.Drawing.Point(201, 180);
            this.maxCarsBox.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.maxCarsBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.maxCarsBox.Name = "maxCarsBox";
            this.maxCarsBox.Size = new System.Drawing.Size(198, 20);
            this.maxCarsBox.TabIndex = 13;
            this.maxCarsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.maxCarsBox.ThousandsSeparator = true;
            this.maxCarsBox.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            // 
            // generatorProbabilityBox
            // 
            this.generatorProbabilityBox.DecimalPlaces = 3;
            this.generatorProbabilityBox.Location = new System.Drawing.Point(201, 209);
            this.generatorProbabilityBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.generatorProbabilityBox.Name = "generatorProbabilityBox";
            this.generatorProbabilityBox.Size = new System.Drawing.Size(198, 20);
            this.generatorProbabilityBox.TabIndex = 14;
            this.generatorProbabilityBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.generatorProbabilityBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 211);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(170, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "λ parameter for vehicle generation:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(126, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Choose simulation model:";
            // 
            // CreateNewDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 286);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.generatorProbabilityBox);
            this.Controls.Add(this.maxCarsBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.carsBox);
            this.Controls.Add(this.junctionsYBox);
            this.Controls.Add(this.junctionsXBox);
            this.Controls.Add(this.distanceBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.carFollowingRadio);
            this.Controls.Add(this.cellBasedRadio);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateNewDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create new simulation";
            ((System.ComponentModel.ISupportInitialize)(this.distanceBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.junctionsXBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.junctionsYBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.carsBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxCarsBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.generatorProbabilityBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton cellBasedRadio;
        private System.Windows.Forms.RadioButton carFollowingRadio;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown distanceBox;
        private System.Windows.Forms.NumericUpDown junctionsXBox;
        private System.Windows.Forms.NumericUpDown junctionsYBox;
        private System.Windows.Forms.NumericUpDown carsBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown maxCarsBox;
        private System.Windows.Forms.NumericUpDown generatorProbabilityBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}