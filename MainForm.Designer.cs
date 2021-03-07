namespace Cliver.testImageDetection
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.PageBox = new System.Windows.Forms.PictureBox();
            this.TemplateBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TemplateBox)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.PageBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.TemplateBox);
            this.splitContainer1.Size = new System.Drawing.Size(2177, 1042);
            this.splitContainer1.SplitterDistance = 1326;
            this.splitContainer1.TabIndex = 0;
            // 
            // PageBox
            // 
            this.PageBox.Location = new System.Drawing.Point(0, 0);
            this.PageBox.Name = "PageBox";
            this.PageBox.Size = new System.Drawing.Size(100, 50);
            this.PageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.PageBox.TabIndex = 0;
            this.PageBox.TabStop = false;
            // 
            // TemplateBox
            // 
            this.TemplateBox.Location = new System.Drawing.Point(3, 0);
            this.TemplateBox.Name = "TemplateBox";
            this.TemplateBox.Size = new System.Drawing.Size(100, 50);
            this.TemplateBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.TemplateBox.TabIndex = 1;
            this.TemplateBox.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2177, 1042);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TemplateBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.PictureBox PageBox;
        public System.Windows.Forms.PictureBox TemplateBox;
    }
}