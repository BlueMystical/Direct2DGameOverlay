using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;


namespace DirectXOverlay
{
	public partial class ModuleGUI : UserControl
	{
        public OverlayModule Module { get; set; }

		public delegate void ResourceAddedHandler(object sender, OnResourceAddedArgs e);
		public event ResourceAddedHandler OnResourceAdded = delegate { }; //<- Evento con Manejador, para evitar los Null
		
		public event ResourceAddedHandler OnLayerAdded = delegate { }; //<- Evento con Manejador, para evitar los Null


		/// <summary>List of pre-loaded Colors.</summary>
		public Dictionary<string, Color> AvailableColors { get; set; }

		/// <summary>List of pre-loaded Fonts</summary>
		public Dictionary<string, Font> AvailableFonts { get; set; }

		/// <summary>List of pre-loaded Textures</summary>
		public Dictionary<string, Bitmap> AvailableTextures { get; set; }
		public List<TextureEx> AvailableTexturesDescriptor { get; set; } = new List<TextureEx>();


		private Keys _PressedKeys = Keys.None; //<- When assigning KeyBinds to a Module

		public ModuleGUI()
        {
			InitializeComponent();
        }
        public ModuleGUI(OverlayModule pModule)
		{
			InitializeComponent();
			this.Module = pModule;
		}
		private void ModuleGUI_Load(object sender, EventArgs e)
		{
			//Iconos de FontAwesome.Sharp:  https://github.com/awesome-inc/FontAwesome.Sharp#windows-forms
			// Documentation:				https://awesome-inc.github.io/FontAwesome.Sharp/
			// Lista de Iconos:				https://fontawesome.com/search?m=free

			cmdAddLayer.Image =		IconChar.Plus.ToBitmap(Color.Gray, 18);
			cmdDeleteLayer.Image =	IconChar.Trash.ToBitmap(Color.Gray, 18);
			cmdEditLayer.Image =	IconChar.Pen.ToBitmap(Color.Gray, 18);

			LoadModule(this.Module);
		}

		/// <summary>Loads and show the idicated module.</summary>
		/// <param name="pModule">Module to load</param>
		public void LoadModule(OverlayModule pModule)
		{
			try
			{
				if (pModule != null)
				{
					this.txtModuleName.Text = pModule.Name;
					this.txtDescripcion.Text = pModule.Description;
					this.chkEnabled.Checked = pModule.Enabled;

					Keys _PressedKeys = (Keys)pModule.HotKeys;
					this.txtHotKey.Text = new KeysConverter().ConvertToString(_PressedKeys);


					if (pModule.Layers != null && pModule.Layers.Count > 0)
					{
						listLayers.Items.Clear();

						foreach (var layer in pModule.Layers)
						{
							listLayers.Items.Add(layer, layer.Value.Visible);
						}

						listLayers.ValueMember = "Name";
						listLayers.DisplayMember = "Description";
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void ToggleEnabled(bool pEnabled)
		{
			try
			{
				Module.Enabled = pEnabled;
				this.chkEnabled.Checked = pEnabled;

				if (Module.Layers != null && Module.Layers.Count > 0)
				{
					listLayers.Items.Clear();

					foreach (var layer in Module.Layers)
					{
						listLayers.Items.Add(layer, pEnabled);
					}

					listLayers.ValueMember = "Name";
					listLayers.DisplayMember = "Description";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>Gets the data of the selected Layer.</summary>
		public LayerEx GetSelectedLayer()
		{
			LayerEx _ret = null;
			if (listLayers.SelectedItem != null)
			{
				KeyValuePair<string, LayerEx> _layer = ((KeyValuePair<string, LayerEx>)listLayers.SelectedItem);
				_ret = _layer.Value;
			}
			else
			{
				_ret = ((KeyValuePair<string, LayerEx>)listLayers.Items[0]).Value;
			}
			return _ret;
		}

		private void chkEnabled_CheckedChanged(object sender, EventArgs e)
		{
			//Deshabilita todo el modulo
			Module.Enabled = this.chkEnabled.Checked;
		}

		private void listLayers_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			//Deshabilita la Capa seleccionada
			if ((sender as CheckedListBox).SelectedItem != null)
			{
				KeyValuePair<string, LayerEx> _layer = ((KeyValuePair<string, LayerEx>)(sender as CheckedListBox).SelectedItem);

				_layer.Value.Visible = (e.NewValue == CheckState.Checked ? true : false);
			}
		}

		private void cmdAddLayer_Click(object sender, EventArgs e)
		{
			LayerEx layer = new LayerEx(string.Format("Layer_{0}", Module.Layers.Count + 1), "This is a new Layer.") { Kind = LayerType.Line };
			LayerForm _Form = new LayerForm(layer)
			{
				AvailableColors = this.AvailableColors,
				AvailableTextures = this.AvailableTextures,
				AvailableFonts = this.AvailableFonts,
				AvailableTexturesDescriptor = this.AvailableTexturesDescriptor,
			};
			if (_Form.ShowDialog() == DialogResult.OK)
			{
				layer = _Form.Layer;
				this.Module.Layers.Add(layer.Name, layer);
				//d3DOverlay.Layers.Add(string.Format("Module{0}_Layer{1}", modCounter, layerCounter), _layer.Value);

				LoadModule(this.Module);

				OnLayerAdded?.Invoke(this, new OnResourceAddedArgs() 
				{
					Layer = layer, 
					AddedColors = _Form.AddedColors, 
					AddedFonts = _Form.AddedFonts, 
					AddedTextures = _Form.AddedTexturesEx 
				});
			}
		}
		private void cmdDeleteLayer_Click(object sender, EventArgs e)
		{
			// TODO:
		}
		private void cmdEditLayer_Click(object sender, EventArgs e)
		{
			var myLayer = GetSelectedLayer();

			LayerForm _Form = new LayerForm(myLayer) 
			{ 
				AvailableColors = this.AvailableColors,
				AvailableTextures = this.AvailableTextures,
				AvailableFonts = this.AvailableFonts,
				AvailableTexturesDescriptor = this.AvailableTexturesDescriptor,
			};
			if (_Form.ShowDialog() == DialogResult.OK)
			{
				myLayer = _Form.Layer;
				LoadModule(this.Module);

				if (_Form.IsDirty)
				{
					OnResourceAdded?.Invoke(this, new OnResourceAddedArgs() 
					{ 
						AddedColors = _Form.AddedColors, 
						AddedFonts = _Form.AddedFonts, 
						AddedTextures = _Form.AddedTexturesEx 
					});
				}
			}
		}

		private void txtModuleName_DoubleClick(object sender, EventArgs e)
		{
			txtModuleName.ReadOnly = false;
		}
		private void txtModuleName_KeyPress(object sender, KeyPressEventArgs e)
		{
			//Se Presionó la Tecla ENTER
			if (e.KeyChar == (char)Keys.Enter)
			{
				Module.Name = txtModuleName.Text;
				txtModuleName.ReadOnly = true;
			}
			if (e.KeyChar == (char)Keys.Escape)
			{
				txtModuleName.Text = Module.Name;
				txtModuleName.ReadOnly = true;
			}
		}

		private void txtHotKey_DoubleClick(object sender, EventArgs e)
		{
			txtHotKey.ReadOnly = false;
		}
		private void txtHotKey_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = true;
			if (e.KeyChar == (char)Keys.Enter)
			{
				Module.HotKeys = (int)_PressedKeys;
				txtHotKey.ReadOnly = true;
			}
			if (e.KeyChar == (char) Keys.Escape)
			{
				_PressedKeys = (Keys)Module.HotKeys;
				txtHotKey.Text = new KeysConverter().ConvertToString(_PressedKeys);
				txtHotKey.ReadOnly = true;
			}
		}
		private void txtHotKey_KeyDown(object sender, KeyEventArgs e)
		{
			if ((sender as TextBox).ReadOnly == false)
			{
				e.Handled = true;
				if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Escape)
				{
					_PressedKeys = e.KeyData;
					(sender as TextBox).Text = new KeysConverter().ConvertToString(_PressedKeys);

					Console.WriteLine(string.Format("KeyDown:{0}, Value:{1}, KeyValue:{2}",
						e.KeyData, (int)e.KeyData, Helper.GetKeyCode(e.KeyData)));
				}
			}						
		}

		private void txtDescripcion_DoubleClick(object sender, EventArgs e)
		{
			txtDescripcion.ReadOnly = false;
		}
		private void txtDescripcion_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				Module.Description = txtDescripcion.Text;
				txtDescripcion.ReadOnly = true;
			}
			if (e.KeyChar == (char)Keys.Escape)
			{
				txtDescripcion.Text = Module.Description;
				txtDescripcion.ReadOnly = true;
			}
		}
	}
}
