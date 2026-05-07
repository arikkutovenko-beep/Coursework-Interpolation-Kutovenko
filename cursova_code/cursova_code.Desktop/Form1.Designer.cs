namespace cursova_code.Desktop
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            btnDraw = new Button();
            txtX = new TextBox();
            EnterX = new Label();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            SuspendLayout();
            // 
            // chart1
            // 
            chartArea2.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            chart1.Legends.Add(legend2);
            chart1.Location = new Point(12, 36);
            chart1.Name = "chart1";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            chart1.Series.Add(series2);
            chart1.Size = new Size(776, 300);
            chart1.TabIndex = 0;
            chart1.Text = "chart1";
            // 
            // btnDraw
            // 
            btnDraw.Location = new Point(632, 375);
            btnDraw.Name = "btnDraw";
            btnDraw.Size = new Size(103, 23);
            btnDraw.TabIndex = 1;
            btnDraw.Text = "Побудувати";
            btnDraw.UseVisualStyleBackColor = true;
            btnDraw.Click += btnDraw_Click;
            // 
            // txtX
            // 
            txtX.Location = new Point(79, 375);
            txtX.Name = "txtX";
            txtX.Size = new Size(225, 23);
            txtX.TabIndex = 2;
            // 
            // EnterX
            // 
            EnterX.AutoSize = true;
            EnterX.Location = new Point(79, 357);
            EnterX.Name = "EnterX";
            EnterX.Size = new Size(131, 15);
            EnterX.TabIndex = 3;
            EnterX.Text = "Точка інтерполяції (X):";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(EnterX);
            Controls.Add(txtX);
            Controls.Add(btnDraw);
            Controls.Add(chart1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private Button btnDraw;
        private TextBox txtX;
        private Label EnterX;
    }
}
