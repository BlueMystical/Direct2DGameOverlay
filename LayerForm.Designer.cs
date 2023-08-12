namespace DirectXOverlay
{
	partial class LayerForm
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.cmdPreview = new System.Windows.Forms.Button();
			this.cmdOK = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cmdPreview);
			this.panel1.Controls.Add(this.cmdOK);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 536);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(464, 37);
			this.panel1.TabIndex = 0;
			// 
			// cmdPreview
			// 
			this.cmdPreview.Location = new System.Drawing.Point(12, 7);
			this.cmdPreview.Name = "cmdPreview";
			this.cmdPreview.Size = new System.Drawing.Size(75, 23);
			this.cmdPreview.TabIndex = 1;
			this.cmdPreview.Text = "&Preview Changes";
			this.cmdPreview.UseVisualStyleBackColor = true;
			this.cmdPreview.Click += new System.EventHandler(this.cmdPreview_Click);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.Location = new System.Drawing.Point(377, 7);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 0;
			this.cmdOK.Text = "&OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// lblStatus
			// 
			this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblStatus.ForeColor = System.Drawing.Color.Blue;
			this.lblStatus.Location = new System.Drawing.Point(0, 15);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(464, 44);
			this.lblStatus.TabIndex = 1;
			this.lblStatus.Text = "label17";
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(464, 477);
			this.propertyGrid1.TabIndex = 14;
			this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.lblStatus);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 477);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(464, 59);
			this.panel2.TabIndex = 15;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(62, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Required:";
			// 
			// LayerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 573);
			this.Controls.Add(this.propertyGrid1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "LayerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Layer Options:";
			this.Load += new System.EventHandler(this.LayerForm_Load);
			this.Shown += new System.EventHandler(this.LayerForm_Shown);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdPreview;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label1;
	}
}