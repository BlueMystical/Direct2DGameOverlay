
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DirectXOverlay
{
	public partial class LayerForm : Form
	{
        public LayerEx Layer { get; set; }

		/// <summary>True si se ha cambiado algo</summary>
		public bool IsDirty { get; set; } = false;
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private System.Globalization.CultureInfo customCulture;

		// Describe las propiedades requerida para cada tipo de forma:
		private List<KeyValuePair<string, LayerType>> LayerTypes;
		public Dictionary<int, string> LayerTypeDefinitions;

		/// <summary>List of pre-loaded Colors.</summary>
        public Dictionary<string, Color> AvailableColors { get; set; }
		public Dictionary<string, Color> AddedColors { get; set; }
		private List<string> _AvailableColors = new List<string>(); //<- Lista mostrada en la propiedad

		/// <summary>List of pre-loaded Fonts</summary>
		public Dictionary<string, Font> AvailableFonts { get; set; }
		public Dictionary<string, Font> AddedFonts { get; set; }
		private List<string> _AvailableFonts = new List<string>(); //<- Lista mostrada en la propiedad

		/// <summary>List of pre-loaded Textures</summary>
		public Dictionary<string, Bitmap> AvailableTextures { get; set; }		
		private List<string> _AvailableTextures = new List<string>(); //<- Lista mostrada en la propiedad
		public List<TextureEx> AvailableTexturesDescriptor { get; set; } = new List<TextureEx>();

		public Dictionary<string, Bitmap> AddedTextures { get; set; }
		public List<TextureEx> AddedTexturesEx { get; set; } = new List<TextureEx>();

		public LayerForm(LayerEx layer)
		{
			InitializeComponent();
			customCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
			customCulture.NumberFormat.NumberDecimalSeparator = ",";
			customCulture.NumberFormat.NumberGroupSeparator = ".";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ",";
			customCulture.NumberFormat.CurrencyGroupSeparator = ".";
			customCulture.DateTimeFormat.DateSeparator = "/";
			customCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
			customCulture.DateTimeFormat.LongDatePattern = "dddd, MMMM d, yyyy";
			customCulture.DateTimeFormat.ShortTimePattern = "hh:mm tt";
			customCulture.DateTimeFormat.LongTimePattern = "hh:mm:ss tt";

			// The following line provides localization for Data formats. 
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
			// The following line provides localization for the application's user interface. 
			System.Threading.Thread.CurrentThread.CurrentUICulture = customCulture;

			// Set this culture as the default culture for all threads in this application. 
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = customCulture;
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = customCulture;

			Layer = layer;
		}

		private void LayerForm_Load(object sender, EventArgs e)
		{
			LayerTypes = new List<KeyValuePair<string, LayerType>>();
			LayerTypeDefinitions = new Dictionary<int, string>();

			foreach (LayerType _type in (LayerType[])Enum.GetValues(typeof(LayerType)))
			{
				LayerTypes.Add(new KeyValuePair<string, LayerType>(_type.ToString(), _type));
			}

			//AvailableColors.Add("[Add New]", Color.Transparent);

			LayerTypeDefinitions.Add((int)LayerType.Text, ".DrawText: [Text, FontName, FontSize, Forecolor, Location]");
			LayerTypeDefinitions.Add((int)LayerType.Line, ".DrawLine: [Location, Size, Stroke, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.Plus, ".DrawPlus: [Location, Length, Stroke, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.TargetReticle_1, ".DrawTargetReticle_1: [Location, Length, Padding, Stroke, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.TargetReticle_2, ".DrawTargetReticle_2: [Location, Length, Padding, Stroke, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.TargetReticle_3, ".DrawTargetReticle_3: [Location, Length, Padding, Stroke, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.Circle, ".DrawCircle: [Location, Length, Stroke, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.CircleFill, ".FillCircle: [Location, Length, BackColor]");
			LayerTypeDefinitions.Add((int)LayerType.Rectangle, ".DrawRectangle: [Location, Size, Stroke, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.RectangleFill, ".FillRectangle: [Location, Size, BackColor]]");
			LayerTypeDefinitions.Add((int)LayerType.Rectangle3D, ".DrawRectangle3D: [Location, Size, Length, Stroke, BackColor]]");
			LayerTypeDefinitions.Add((int)LayerType.Box2D, ".DrawBox2D: [Location, Size, Stroke, Forecolor, BackColor]]");
			LayerTypeDefinitions.Add((int)LayerType.Box3D, ".DrawBox3D: [Location, Size, Length, Stroke, Forecolor, BackColor]]");
			LayerTypeDefinitions.Add((int)LayerType.Edge, ".DrawEdge: [Location, Size, Length, Stroke, Forecolor]]");
			LayerTypeDefinitions.Add((int)LayerType.BarH, ".DrawBarH: [Location, Size, Length, Stroke, Forecolor, BackColor]]");
			LayerTypeDefinitions.Add((int)LayerType.BarV, ".DrawBarV: [Location, Size, Length, Stroke, Forecolor, BackColor]");
			LayerTypeDefinitions.Add((int)LayerType.Texture, ".DrawTexture: [TextureName, Location, Size, Opacity]");
			LayerTypeDefinitions.Add((int)LayerType.FPS, ".FPS Counter: [Location, FontName, FontSize, Forecolor]");
			LayerTypeDefinitions.Add((int)LayerType.Geometry, ".DrawGeometry: [Geometry, Forecolor, Stroke]");
			LayerTypeDefinitions.Add((int)LayerType.GeometryFilled, ".FillGeometry: [Geometry, BackColor]");
			LayerTypeDefinitions.Add((int)LayerType.MessageBox, ".MessageBox: [Location, Size, Stroke, BackColor, ForeColor, Font]");
			LayerTypeDefinitions.Add((int)LayerType.GpuUsage, ".GPU Use %: [Location, FontName, FontSize, Forecolor]");
		}
		private void LayerForm_Shown(object sender, EventArgs e)
		{
			if (Layer != null) { LoadLayer(); }
		}

		public void LoadLayer()
		{
			try
			{
				if (AvailableColors != null && AvailableColors.Count > 0)
				{
					/*  Carga la lista de colores pre-cargados  */
					_AvailableColors = new List<string>();
					_AvailableColors.Add("[Add New]");
					foreach (var item in AvailableColors)
					{
						_AvailableColors.Add(item.Key);
					}
					StringListConverter.RegisterValuesForProperty(typeof(ColorsEx), nameof(ColorsEx.ForeColor), _AvailableColors);
					StringListConverter.RegisterValuesForProperty(typeof(ColorsEx), nameof(ColorsEx.BackColor), _AvailableColors);
				}
				if (AvailableFonts != null && AvailableFonts.Count > 0)
				{
					_AvailableFonts = new List<string>();
					_AvailableFonts.Add("[Add New]");
					foreach (var item in AvailableFonts)
					{
						_AvailableFonts.Add(item.Key);
					}
					StringListConverter.RegisterValuesForProperty(typeof(TextEx), nameof(TextEx.Font), _AvailableFonts);
				}
				if (AvailableTextures != null && AvailableTextures.Count > 0)
				{
					_AvailableTextures = new List<string>();
					_AvailableTextures.Add("[Add New]");
					foreach (var item in AvailableTextures)
					{
						_AvailableTextures.Add(item.Key);
					}
					StringListConverter.RegisterValuesForProperty(typeof(TextureEx), nameof(TextureEx.TextureName), _AvailableTextures);
				}

				propertyGrid1.SelectedObject = Layer;

				this.lblStatus.Text = LayerTypeDefinitions[(int)Layer.Kind];
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			//TODO: validar datos obligatorios


			this.DialogResult = DialogResult.OK;
		}



		private void cmdPreview_Click(object sender, EventArgs e)
		{
			try
			{
				throw new NotImplementedException();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			if (e.ChangedItem.Label == "Shape")
			{
				this.lblStatus.Text = LayerTypeDefinitions[(int)Layer.Kind];

				if (Layer.Location is null) { Layer.Location = new PointEx(); }
			}
			if (e.ChangedItem.Label == "ForeColor")
			{
				if (Layer.Colors.ForeColor == "[Add New]")
				{
					ColorDialog Dialog = new ColorDialog()
					{
						AnyColor = true,
						FullOpen = true,
						AllowFullOpen = true,
						SolidColorOnly = false
					};
					if (Dialog.ShowDialog() == DialogResult.OK)
					{
						Layer.Colors.ForeColor = string.Format("{0},{1},{2},{3}", Dialog.Color.A, Dialog.Color.R, Dialog.Color.G, Dialog.Color.B);

						if (!AvailableColors.ContainsKey(Layer.Colors.ForeColor))
						{
							this.IsDirty = true;
							if (AddedColors is null) { AddedColors = new Dictionary<string, Color>(); }

							AddedColors.Add(Layer.Colors.ForeColor, Dialog.Color);
							AvailableColors.Add(Layer.Colors.ForeColor, Dialog.Color);
							_AvailableColors.Add(Layer.Colors.ForeColor);
						}
					}
				}
			}
			if (e.ChangedItem.Label == "BackColor")
			{
				if (Layer.Colors.BackColor == "[Add New]")
				{
					ColorDialog Dialog = new ColorDialog()
					{
						AnyColor = true,
						FullOpen = true,
						AllowFullOpen = true,
						SolidColorOnly = false
					};
					if (Dialog.ShowDialog() == DialogResult.OK)
					{
						Layer.Colors.BackColor = string.Format("{0},{1},{2},{3}", Dialog.Color.A, Dialog.Color.R, Dialog.Color.G, Dialog.Color.B);

						if (!AvailableColors.ContainsKey(Layer.Colors.BackColor))
						{
							this.IsDirty = true;
							if (AddedColors is null) { AddedColors = new Dictionary<string, Color>(); }

							AddedColors.Add(Layer.Colors.BackColor, Dialog.Color);
							AvailableColors.Add(Layer.Colors.BackColor, Dialog.Color);
							_AvailableColors.Add(Layer.Colors.BackColor);
						}
					}
				}
			}
			if (e.ChangedItem.Label == "Font")
			{
				if (Layer.Text.Font == "[Add New]")
				{
					FontDialog Dialog = new FontDialog()
					{
						ShowColor = false,
						ShowHelp = false,
						ShowApply = false,
						FontMustExist = true,
					};
					if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						// Courier New;15;FontStyle
						Layer.Text.Font = string.Format("{0};{1};{2}",
							Dialog.Font.Name,
							(int)Dialog.Font.Size,
							(int)Dialog.Font.Style
						);

						if (!AvailableFonts.ContainsKey(Layer.Text.Font))
						{
							this.IsDirty = true;
							if (AddedFonts is null) { AddedFonts = new Dictionary<string, System.Drawing.Font>(); }

							AddedFonts.Add(Layer.Text.Font, Dialog.Font);
							AvailableFonts.Add(Layer.Text.Font, Dialog.Font);
							_AvailableFonts.Add(Layer.Text.Font);
						}
					}
				}
			}
			if (e.ChangedItem.Label == "TextureName")
			{
				if (Layer.Texture.TextureName == "[Add New]")
				{
					OpenFileDialog OFDialog = new OpenFileDialog()
					{
						Filter = "Image Files|*.jpg;*.bmp;*.gif;*.png|All Files|*.*",
						FilterIndex = 0,
						DefaultExt = "png",
						AddExtension = true,
						CheckPathExists = true,
						CheckFileExists = true,
						InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
					};

					if (OFDialog.ShowDialog() == DialogResult.OK)
					{
						
						Bitmap TImage = (Bitmap)Image.FromFile(OFDialog.FileName);
						string TFile = System.IO.Path.GetFileName(OFDialog.FileName); //<- Nombre del Archivo con Extension (Sin Ruta)
						string TName = string.Empty;

                        if (Helper.InputBox("Texture Name?", "What would be the Key Name for this Texture?", ref TName) == DialogResult.OK)
                        {
							if (!string.IsNullOrEmpty(TName))
							{
								if (AvailableTextures is null || !AvailableTextures.ContainsKey(string.Format("{0}", TName)))
								{
									if (!Directory.Exists(Path.Combine(AppExePath, "modules"))) { Directory.CreateDirectory(Path.Combine(AppExePath, "modules")); }
									if (!Directory.Exists(Path.Combine(AppExePath, "modules", "textures"))) { Directory.CreateDirectory(Path.Combine(AppExePath, "modules")); }
									
									System.IO.FileInfo file = new System.IO.FileInfo(OFDialog.FileName);
									if (file.CopyTo(Path.Combine(AppExePath, "modules", "textures", TFile), true).Exists)
									{
										if (AvailableTextures is null) AvailableTextures = new Dictionary<string, Bitmap>();
										if (AddedTextures is null) { AddedTextures = new Dictionary<string, System.Drawing.Bitmap>(); }

										Layer.Texture.TextureName = TName;
										Layer.Texture.TextureFile = TFile;

										AvailableTextures.Add(	string.Format("{0}", TName, TFile), TImage);
										AddedTextures.Add(		string.Format("{0}", TName, TFile), TImage);
										_AvailableTextures.Add(	string.Format("{0}", TName, TFile));

										AvailableTexturesDescriptor.Add(new TextureEx() { TextureName = TName, TextureFile = TFile });
										AddedTexturesEx.Add(new TextureEx() { TextureName = TName, TextureFile = TFile });

										this.IsDirty = true;
									}
								}
							}
                        }
					}
				}
				else 
				{
					//Selected an existing Texture, need to update the properties for the selected one
					if (this.AvailableTexturesDescriptor != null && AvailableTexturesDescriptor.Count > 0)
					{
						var _ret = AvailableTexturesDescriptor.Find(x => x.TextureName == Layer.Texture.TextureName);
						if (_ret != null) 
						{
							Layer.Texture.TextureFile = _ret.TextureFile;
							Layer.Texture.Opacity = _ret.Opacity;
						}
					}
				}
			}

			if (e.ChangedItem.Label == "Opacity")
			{
				float _val = 0f;
				float.TryParse(e.ChangedItem.Value.ToString(), out _val);
				Layer.Texture.Opacity = _val;
			}
		}
	}
}
