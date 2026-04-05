namespace FastCdReplace.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                _stopwatch?.Stop();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnSelectInputFolder = new System.Windows.Forms.Button();
            txtInputFolderPath = new System.Windows.Forms.TextBox();
            chkStatistics = new System.Windows.Forms.CheckBox();
            chkNoOutputFolder = new System.Windows.Forms.CheckBox();
            lblElapsedText = new System.Windows.Forms.Label();
            lblAlgorithmText = new System.Windows.Forms.Label();
            lblPercentText = new System.Windows.Forms.Label();
            progressBarMain = new FastCdReplace.UI.SmoothProgressBar();
            btnCancel = new System.Windows.Forms.Button();
            btnExecute = new System.Windows.Forms.Button();
            timerExecution = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // btnSelectInputFolder
            // 
            btnSelectInputFolder.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnSelectInputFolder.Location = new System.Drawing.Point(18, 28);
            btnSelectInputFolder.Name = "btnSelectInputFolder";
            btnSelectInputFolder.Size = new System.Drawing.Size(160, 44);
            btnSelectInputFolder.TabIndex = 0;
            btnSelectInputFolder.Text = "Папка “CD-in”";
            btnSelectInputFolder.UseVisualStyleBackColor = true;
            btnSelectInputFolder.Click += btnSelectInputFolder_Click;
            // 
            // txtInputFolderPath
            // 
            txtInputFolderPath.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtInputFolderPath.Location = new System.Drawing.Point(192, 34);
            txtInputFolderPath.Name = "txtInputFolderPath";
            txtInputFolderPath.ReadOnly = true;
            txtInputFolderPath.Size = new System.Drawing.Size(667, 30);
            txtInputFolderPath.TabIndex = 1;
            txtInputFolderPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            // 
            // chkStatistics
            // 
            chkStatistics.AutoSize = true;
            chkStatistics.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkStatistics.Location = new System.Drawing.Point(615, 115);
            chkStatistics.Name = "chkStatistics";
            chkStatistics.Size = new System.Drawing.Size(123, 26);
            chkStatistics.TabIndex = 2;
            chkStatistics.Text = "Статистика";
            chkStatistics.UseVisualStyleBackColor = true;
            // 
            // chkNoOutputFolder
            // 
            chkNoOutputFolder.AutoSize = true;
            chkNoOutputFolder.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkNoOutputFolder.Location = new System.Drawing.Point(615, 145);
            chkNoOutputFolder.Name = "chkNoOutputFolder";
            chkNoOutputFolder.Size = new System.Drawing.Size(183, 26);
            chkNoOutputFolder.TabIndex = 3;
            chkNoOutputFolder.Text = "Без папки “CD-out”";
            chkNoOutputFolder.UseVisualStyleBackColor = true;
            // 
            // lblElapsedText
            // 
            lblElapsedText.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblElapsedText.Location = new System.Drawing.Point(18, 287);
            lblElapsedText.Name = "lblElapsedText";
            lblElapsedText.Size = new System.Drawing.Size(240, 25);
            lblElapsedText.TabIndex = 4;
            lblElapsedText.Text = "00.00.00 мин/сек/мсек";
            lblElapsedText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAlgorithmText
            // 
            lblAlgorithmText.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblAlgorithmText.Location = new System.Drawing.Point(344, 287);
            lblAlgorithmText.Name = "lblAlgorithmText";
            lblAlgorithmText.Size = new System.Drawing.Size(220, 25);
            lblAlgorithmText.TabIndex = 5;
            lblAlgorithmText.Text = "00.00.00 алгоритм";
            lblAlgorithmText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPercentText
            // 
            lblPercentText.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblPercentText.Location = new System.Drawing.Point(789, 287);
            lblPercentText.Name = "lblPercentText";
            lblPercentText.Size = new System.Drawing.Size(70, 25);
            lblPercentText.TabIndex = 6;
            lblPercentText.Text = "%";
            lblPercentText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // progressBarMain
            // 
            progressBarMain.BackColor = System.Drawing.Color.WhiteSmoke;
            progressBarMain.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            progressBarMain.Location = new System.Drawing.Point(18, 317);
            progressBarMain.Name = "progressBarMain";
            progressBarMain.Size = new System.Drawing.Size(841, 45);
            progressBarMain.TabIndex = 7;
            progressBarMain.Value = 0;
            // 
            // btnCancel
            // 
            btnCancel.Font = new System.Drawing.Font("Times New Roman", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnCancel.Location = new System.Drawing.Point(18, 390);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(421, 57);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Отменить";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnExecute
            // 
            btnExecute.Font = new System.Drawing.Font("Times New Roman", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnExecute.Location = new System.Drawing.Point(454, 390);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new System.Drawing.Size(405, 57);
            btnExecute.TabIndex = 9;
            btnExecute.Text = "Выполнить";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // timerExecution
            // 
            timerExecution.Interval = 50;
            timerExecution.Tick += timerExecution_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(878, 470);
            Controls.Add(btnExecute);
            Controls.Add(btnCancel);
            Controls.Add(progressBarMain);
            Controls.Add(lblPercentText);
            Controls.Add(lblAlgorithmText);
            Controls.Add(lblElapsedText);
            Controls.Add(chkNoOutputFolder);
            Controls.Add(chkStatistics);
            Controls.Add(txtInputFolderPath);
            Controls.Add(btnSelectInputFolder);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "CD-Замена";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnSelectInputFolder;
        private System.Windows.Forms.TextBox txtInputFolderPath;
        private System.Windows.Forms.CheckBox chkStatistics;
        private System.Windows.Forms.CheckBox chkNoOutputFolder;
        private System.Windows.Forms.Label lblElapsedText;
        private System.Windows.Forms.Label lblAlgorithmText;
        private System.Windows.Forms.Label lblPercentText;
        private UI.SmoothProgressBar progressBarMain;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Timer timerExecution;
    }
}