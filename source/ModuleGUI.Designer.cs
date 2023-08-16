namespace DirectXOverlay
{
	partial class ModuleGUI
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

		#region Código generado por el Diseñador de componentes

		/// <summary> 
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido de este método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.chkEnabled = new System.Windows.Forms.CheckBox();
			this.lblKeyBinds = new System.Windows.Forms.Label();
			this.listLayers = new System.Windows.Forms.CheckedListBox();
			this.label3 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.txtHotKey = new System.Windows.Forms.TextBox();
			this.txtModuleName = new System.Windows.Forms.TextBox();
			this.txtDescripcion = new System.Windows.Forms.TextBox();
			this.cmdEditLayer = new System.Windows.Forms.Button();
			this.cmdDeleteLayer = new System.Windows.Forms.Button();
			this.cmdAddLayer = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// chkEnabled
			// 
			this.chkEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkEnabled.AutoSize = true;
			this.chkEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.chkEnabled.Location = new System.Drawing.Point(437, 5);
			this.chkEnabled.Name = "chkEnabled";
			this.chkEnabled.Size = new System.Drawing.Size(68, 17);
			this.chkEnabled.TabIndex = 2;
			this.chkEnabled.Text = "Enabled:";
			this.chkEnabled.UseVisualStyleBackColor = true;
			this.chkEnabled.CheckedChanged += new System.EventHandler(this.chkEnabled_CheckedChanged);
			// 
			// lblKeyBinds
			// 
			this.lblKeyBinds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblKeyBinds.AutoSize = true;
			this.lblKeyBinds.Location = new System.Drawing.Point(264, 7);
			this.lblKeyBinds.Name = "lblKeyBinds";
			this.lblKeyBinds.Size = new System.Drawing.Size(45, 13);
			this.lblKeyBinds.TabIndex = 3;
			this.lblKeyBinds.Text = "HotKey:";
			this.lblKeyBinds.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// listLayers
			// 
			this.listLayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listLayers.FormattingEnabled = true;
			this.listLayers.Location = new System.Drawing.Point(9, 67);
			this.listLayers.Name = "listLayers";
			this.listLayers.Size = new System.Drawing.Size(470, 64);
			this.listLayers.TabIndex = 4;
			this.listLayers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listLayers_ItemCheck);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(6, 51);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Layers:";
			// 
			// txtHotKey
			// 
			this.txtHotKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtHotKey.Location = new System.Drawing.Point(312, 3);
			this.txtHotKey.Name = "txtHotKey";
			this.txtHotKey.ReadOnly = true;
			this.txtHotKey.Size = new System.Drawing.Size(126, 20);
			this.txtHotKey.TabIndex = 9;
			this.txtHotKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.toolTip1.SetToolTip(this.txtHotKey, "- Doble Click to Edit.\r\n- ENTER to Save.\r\n- ESCAPE to Cancel.");
			this.txtHotKey.DoubleClick += new System.EventHandler(this.txtHotKey_DoubleClick);
			this.txtHotKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtHotKey_KeyDown);
			this.txtHotKey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtHotKey_KeyPress);
			// 
			// txtModuleName
			// 
			this.txtModuleName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtModuleName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtModuleName.ForeColor = System.Drawing.Color.Blue;
			this.txtModuleName.Location = new System.Drawing.Point(9, 3);
			this.txtModuleName.Name = "txtModuleName";
			this.txtModuleName.ReadOnly = true;
			this.txtModuleName.Size = new System.Drawing.Size(249, 20);
			this.txtModuleName.TabIndex = 10;
			this.txtModuleName.Text = "Module Name";
			this.toolTip1.SetToolTip(this.txtModuleName, "- Doble Click to Edit.\r\n- ENTER to Save.\r\n- ESCAPE to Cancel.");
			this.txtModuleName.DoubleClick += new System.EventHandler(this.txtModuleName_DoubleClick);
			this.txtModuleName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtModuleName_KeyPress);
			// 
			// txtDescripcion
			// 
			this.txtDescripcion.Location = new System.Drawing.Point(9, 23);
			this.txtDescripcion.Name = "txtDescripcion";
			this.txtDescripcion.ReadOnly = true;
			this.txtDescripcion.Size = new System.Drawing.Size(429, 20);
			this.txtDescripcion.TabIndex = 11;
			this.toolTip1.SetToolTip(this.txtDescripcion, "- Doble Click to Edit.\r\n- ENTER to Save.\r\n- ESCAPE to Cancel.");
			this.txtDescripcion.DoubleClick += new System.EventHandler(this.txtDescripcion_DoubleClick);
			this.txtDescripcion.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDescripcion_KeyPress);
			// 
			// cmdEditLayer
			// 
			this.cmdEditLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdEditLayer.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.cmdEditLayer.FlatAppearance.BorderSize = 0;
			this.cmdEditLayer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.cmdEditLayer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
			this.cmdEditLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdEditLayer.Image = global::DirectXOverlay.Properties.Resources.editar_18;
			this.cmdEditLayer.Location = new System.Drawing.Point(485, 111);
			this.cmdEditLayer.Name = "cmdEditLayer";
			this.cmdEditLayer.Size = new System.Drawing.Size(22, 22);
			this.cmdEditLayer.TabIndex = 8;
			this.toolTip1.SetToolTip(this.cmdEditLayer, "Edit Selected Layer");
			this.cmdEditLayer.UseVisualStyleBackColor = true;
			this.cmdEditLayer.Click += new System.EventHandler(this.cmdEditLayer_Click);
			// 
			// cmdDeleteLayer
			// 
			this.cmdDeleteLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdDeleteLayer.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.cmdDeleteLayer.FlatAppearance.BorderSize = 0;
			this.cmdDeleteLayer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.cmdDeleteLayer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
			this.cmdDeleteLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdDeleteLayer.Image = global::DirectXOverlay.Properties.Resources.cruz_18;
			this.cmdDeleteLayer.Location = new System.Drawing.Point(485, 89);
			this.cmdDeleteLayer.Name = "cmdDeleteLayer";
			this.cmdDeleteLayer.Size = new System.Drawing.Size(22, 22);
			this.cmdDeleteLayer.TabIndex = 7;
			this.toolTip1.SetToolTip(this.cmdDeleteLayer, "Delete Selected Layer");
			this.cmdDeleteLayer.UseVisualStyleBackColor = true;
			this.cmdDeleteLayer.Click += new System.EventHandler(this.cmdDeleteLayer_Click);
			// 
			// cmdAddLayer
			// 
			this.cmdAddLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdAddLayer.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.cmdAddLayer.FlatAppearance.BorderSize = 0;
			this.cmdAddLayer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.cmdAddLayer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
			this.cmdAddLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdAddLayer.Image = global::DirectXOverlay.Properties.Resources.agregar_18;
			this.cmdAddLayer.Location = new System.Drawing.Point(485, 67);
			this.cmdAddLayer.Name = "cmdAddLayer";
			this.cmdAddLayer.Size = new System.Drawing.Size(22, 22);
			this.cmdAddLayer.TabIndex = 6;
			this.toolTip1.SetToolTip(this.cmdAddLayer, "Add new Layer..");
			this.cmdAddLayer.UseVisualStyleBackColor = true;
			this.cmdAddLayer.Click += new System.EventHandler(this.cmdAddLayer_Click);
			// 
			// ModuleGUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.txtDescripcion);
			this.Controls.Add(this.txtModuleName);
			this.Controls.Add(this.txtHotKey);
			this.Controls.Add(this.cmdEditLayer);
			this.Controls.Add(this.cmdDeleteLayer);
			this.Controls.Add(this.cmdAddLayer);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.listLayers);
			this.Controls.Add(this.lblKeyBinds);
			this.Controls.Add(this.chkEnabled);
			this.Name = "ModuleGUI";
			this.Size = new System.Drawing.Size(512, 134);
			this.Load += new System.EventHandler(this.ModuleGUI_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.CheckBox chkEnabled;
		private System.Windows.Forms.Label lblKeyBinds;
		private System.Windows.Forms.CheckedListBox listLayers;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button cmdAddLayer;
		private System.Windows.Forms.Button cmdDeleteLayer;
		private System.Windows.Forms.Button cmdEditLayer;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TextBox txtHotKey;
		private System.Windows.Forms.TextBox txtModuleName;
		private System.Windows.Forms.TextBox txtDescripcion;
	}
}
