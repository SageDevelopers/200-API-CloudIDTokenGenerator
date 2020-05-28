namespace SageCloudIDTokenGeneratorv1._1
{
    partial class CloudIDTokenGen
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
            this.buttonGenerateAccessToken = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonGenerateAccessToken
            // 
            this.buttonGenerateAccessToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGenerateAccessToken.Location = new System.Drawing.Point(10, 14);
            this.buttonGenerateAccessToken.Name = "buttonGenerateAccessToken";
            this.buttonGenerateAccessToken.Size = new System.Drawing.Size(302, 43);
            this.buttonGenerateAccessToken.TabIndex = 0;
            this.buttonGenerateAccessToken.Text = "Generate Access Token";
            this.buttonGenerateAccessToken.UseVisualStyleBackColor = true;
            this.buttonGenerateAccessToken.Click += new System.EventHandler(this.buttonGenerateAccessToken_Click);
            // 
            // CloudIDTokenGen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 69);
            this.Controls.Add(this.buttonGenerateAccessToken);
            this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Name = "CloudIDTokenGen";
            this.Text = "Cloud ID Token Generator v1.1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonGenerateAccessToken;
    }
}

