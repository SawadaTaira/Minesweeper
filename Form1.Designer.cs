namespace Minesweeper
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
            panel1 = new Panel();
            btn_start = new Button();
            btn_reset = new Button();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackgroundImageLayout = ImageLayout.None;
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Location = new Point(12, 45);
            panel1.Name = "panel1";
            panel1.Size = new Size(504, 504);
            panel1.TabIndex = 0;
            // 
            // btn_start
            // 
            btn_start.Location = new Point(12, 12);
            btn_start.Name = "btn_start";
            btn_start.Size = new Size(132, 23);
            btn_start.TabIndex = 0;
            btn_start.Text = "スタート";
            btn_start.Click += btn_start_Click;
            // 
            // btn_reset
            // 
            btn_reset.Location = new Point(198, 12);
            btn_reset.Name = "btn_reset";
            btn_reset.Size = new Size(75, 23);
            btn_reset.TabIndex = 1;
            btn_reset.Text = "リセット";
            btn_reset.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(534, 561);
            Controls.Add(btn_reset);
            Controls.Add(btn_start);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "MineSweeper";
            Load += Form1_Load;
            Click += btn_start_Click;
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btn_start;
        private Button btn_reset;
    }
}
