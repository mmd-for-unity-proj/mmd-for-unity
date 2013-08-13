namespace MMDIKBakerGUI
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.pmdFileName = new System.Windows.Forms.TextBox();
            this.vmdFileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pmdRef = new System.Windows.Forms.Button();
            this.vmdRef = new System.Windows.Forms.Button();
            this.startBake = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.saveVmdName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // pmdFileName
            // 
            this.pmdFileName.Location = new System.Drawing.Point(14, 24);
            this.pmdFileName.Name = "pmdFileName";
            this.pmdFileName.Size = new System.Drawing.Size(270, 19);
            this.pmdFileName.TabIndex = 0;
            // 
            // vmdFileName
            // 
            this.vmdFileName.Location = new System.Drawing.Point(14, 61);
            this.vmdFileName.Name = "vmdFileName";
            this.vmdFileName.Size = new System.Drawing.Size(270, 19);
            this.vmdFileName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "PMD";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "VMD";
            // 
            // pmdRef
            // 
            this.pmdRef.Location = new System.Drawing.Point(290, 22);
            this.pmdRef.Name = "pmdRef";
            this.pmdRef.Size = new System.Drawing.Size(42, 23);
            this.pmdRef.TabIndex = 4;
            this.pmdRef.Text = "参照";
            this.pmdRef.UseVisualStyleBackColor = true;
            this.pmdRef.Click += new System.EventHandler(this.pmdRef_Click);
            // 
            // vmdRef
            // 
            this.vmdRef.Location = new System.Drawing.Point(290, 59);
            this.vmdRef.Name = "vmdRef";
            this.vmdRef.Size = new System.Drawing.Size(42, 23);
            this.vmdRef.TabIndex = 5;
            this.vmdRef.Text = "参照";
            this.vmdRef.UseVisualStyleBackColor = true;
            this.vmdRef.Click += new System.EventHandler(this.vmdRef_Click);
            // 
            // startBake
            // 
            this.startBake.Location = new System.Drawing.Point(257, 142);
            this.startBake.Name = "startBake";
            this.startBake.Size = new System.Drawing.Size(75, 23);
            this.startBake.TabIndex = 6;
            this.startBake.Text = "Bake !!";
            this.startBake.UseVisualStyleBackColor = true;
            this.startBake.Click += new System.EventHandler(this.startBake_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(290, 96);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(42, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "参照";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "Save VMD";
            // 
            // saveVmdName
            // 
            this.saveVmdName.Location = new System.Drawing.Point(14, 98);
            this.saveVmdName.Name = "saveVmdName";
            this.saveVmdName.Size = new System.Drawing.Size(270, 19);
            this.saveVmdName.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 177);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.saveVmdName);
            this.Controls.Add(this.startBake);
            this.Controls.Add(this.vmdRef);
            this.Controls.Add(this.pmdRef);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.vmdFileName);
            this.Controls.Add(this.pmdFileName);
            this.Name = "Form1";
            this.Text = "MMDIKBakerGUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox pmdFileName;
        private System.Windows.Forms.TextBox vmdFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button pmdRef;
        private System.Windows.Forms.Button vmdRef;
        private System.Windows.Forms.Button startBake;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox saveVmdName;
    }
}

