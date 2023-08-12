namespace DirectXOverlay
{
	partial class MainForm
	{
		/// <summary>
		/// Variable del diseñador necesaria.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Limpiar los recursos que se estén usando.
		/// </summary>
		/// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Código generado por el Diseñador de Windows Forms

		/// <summary>
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido de este método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuLoadMudules = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSaveChanges = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.cboInstances = new System.Windows.Forms.ToolStripComboBox();
			this.cmdAddModule = new System.Windows.Forms.ToolStripButton();
			this.cmdRemoveModule = new System.Windows.Forms.ToolStripButton();
			this.cmdX = new System.Windows.Forms.ToolStripButton();
			this.tblModules = new System.Windows.Forms.TableLayoutPanel();
			this.moduleGUI1 = new DirectXOverlay.ModuleGUI();
			this.statusStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.tblModules.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
			this.statusStrip1.Location = new System.Drawing.Point(0, 473);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(545, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// lblStatus
			// 
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(42, 17);
			this.lblStatus.Text = "Ready.";
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolStripMenuItem1,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(545, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLoadMudules,
            this.mnuSaveChanges,
            this.toolStripSeparator1,
            this.mnuExit});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// mnuLoadMudules
			// 
			this.mnuLoadMudules.Name = "mnuLoadMudules";
			this.mnuLoadMudules.Size = new System.Drawing.Size(155, 22);
			this.mnuLoadMudules.Text = "&Load Modules..";
			this.mnuLoadMudules.Click += new System.EventHandler(this.mnuLoadMudules_Click);
			// 
			// mnuSaveChanges
			// 
			this.mnuSaveChanges.Name = "mnuSaveChanges";
			this.mnuSaveChanges.Size = new System.Drawing.Size(155, 22);
			this.mnuSaveChanges.Text = "Save Changes..";
			this.mnuSaveChanges.Click += new System.EventHandler(this.mnuSaveChanges_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
			// 
			// mnuExit
			// 
			this.mnuExit.Name = "mnuExit";
			this.mnuExit.Size = new System.Drawing.Size(155, 22);
			this.mnuExit.Text = "&Exit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(44, 20);
			this.toolStripMenuItem1.Text = "&View";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cboInstances,
            this.cmdAddModule,
            this.cmdRemoveModule,
            this.cmdX});
			this.toolStrip1.Location = new System.Drawing.Point(0, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(545, 25);
			this.toolStrip1.TabIndex = 2;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// cboInstances
			// 
			this.cboInstances.Name = "cboInstances";
			this.cboInstances.Size = new System.Drawing.Size(250, 25);
			// 
			// cmdAddModule
			// 
			this.cmdAddModule.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdAddModule.Image = global::DirectXOverlay.Properties.Resources.agregar_18;
			this.cmdAddModule.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdAddModule.Name = "cmdAddModule";
			this.cmdAddModule.Size = new System.Drawing.Size(23, 22);
			this.cmdAddModule.Text = "Add new Game Instance";
			this.cmdAddModule.Click += new System.EventHandler(this.cmdAddModule_Click);
			// 
			// cmdRemoveModule
			// 
			this.cmdRemoveModule.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdRemoveModule.Image = ((System.Drawing.Image)(resources.GetObject("cmdRemoveModule.Image")));
			this.cmdRemoveModule.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdRemoveModule.Name = "cmdRemoveModule";
			this.cmdRemoveModule.Size = new System.Drawing.Size(23, 22);
			this.cmdRemoveModule.Text = "toolStripButton1";
			this.cmdRemoveModule.Click += new System.EventHandler(this.cmdRemoveModule_Click);
			// 
			// cmdX
			// 
			this.cmdX.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdX.Image = ((System.Drawing.Image)(resources.GetObject("cmdX.Image")));
			this.cmdX.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdX.Name = "cmdX";
			this.cmdX.Size = new System.Drawing.Size(23, 22);
			this.cmdX.Text = "toolStripButton1";
			// 
			// tblModules
			// 
			this.tblModules.ColumnCount = 1;
			this.tblModules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tblModules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblModules.Controls.Add(this.moduleGUI1, 0, 0);
			this.tblModules.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tblModules.Location = new System.Drawing.Point(0, 49);
			this.tblModules.Name = "tblModules";
			this.tblModules.RowCount = 3;
			this.tblModules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblModules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblModules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblModules.Size = new System.Drawing.Size(545, 424);
			this.tblModules.TabIndex = 3;
			// 
			// moduleGUI1
			// 
			this.moduleGUI1.AvailableColors = null;
			this.moduleGUI1.AvailableFonts = null;
			this.moduleGUI1.AvailableTextures = null;
			this.moduleGUI1.AvailableTexturesDescriptor = ((System.Collections.Generic.List<DirectXOverlay.TextureEx>)(resources.GetObject("moduleGUI1.AvailableTexturesDescriptor")));
			this.moduleGUI1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleGUI1.Location = new System.Drawing.Point(3, 3);
			this.moduleGUI1.Module = null;
			this.moduleGUI1.Name = "moduleGUI1";
			this.moduleGUI1.Size = new System.Drawing.Size(539, 196);
			this.moduleGUI1.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(545, 495);
			this.Controls.Add(this.tblModules);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.Text = "Blue\'s Overlay Framework";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.tblModules.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel lblStatus;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuLoadMudules;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem mnuExit;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripComboBox cboInstances;
		private System.Windows.Forms.ToolStripButton cmdAddModule;
		private System.Windows.Forms.TableLayoutPanel tblModules;
		private ModuleGUI moduleGUI1;
		private System.Windows.Forms.ToolStripMenuItem mnuSaveChanges;
		private System.Windows.Forms.ToolStripButton cmdX;
		private System.Windows.Forms.ToolStripButton cmdRemoveModule;
	}
}

