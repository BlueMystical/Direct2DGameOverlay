using FontAwesome.Sharp;
using NonInvasiveKeyboardHookLibrary;
using Overlay.NET;
using Overlay.NET.Directx;
using Process.NET;
using Process.NET.Memory;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


//using FontAwesome.Sharp;

namespace DirectXOverlay
{
	public partial class MainForm : Form
	{
		//Iconos de FontAwesome.Sharp:   https://github.com/awesome-inc/FontAwesome.Sharp#windows-forms
		// Lista de Iconos:				https://fontawesome.com/search?m=free

		//Themes_Tool_Reload.Image = IconChar.Sync.ToBitmap(ColorTranslator.FromHtml("#FFFFFF"), 20);

		#region Private Members

		private KeyboardHookManager HotKeysManager;			//<- Global Hotkeys using NIKBHL https://github.com/kfirprods/NonInvasiveKeyboardHook
		private OverlayPlugin _DXOverlayPlugin;
		private DXOverlayPlugin d3DOverlay;
		private ProcessSharp _processSharp;

		private bool OverlayRunning = false;
		private bool GameIsRunning = false;

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;

		#endregion

		#region Public Properties

		public List<GameInstance> GameInstances { get; set; }
        public GameInstance SelectedInstance { get; set; }


        #endregion

        #region Constructors & form events

        public MainForm()
		{
			InitializeComponent();
		}
		private void Form1_Load(object sender, EventArgs e)
		{
			cmdAddModule.Image = IconChar.Plus.ToBitmap(Color.Gray, 18);
			cmdRemoveModule.Image = IconChar.Trash.ToBitmap(Color.Gray, 18);
		}
		private void Form1_Shown(object sender, EventArgs e)
		{
			HotKeysManager = new KeyboardHookManager();
			HotKeysManager.Start();

			LoadConfiguration(Path.Combine(AppExePath, "modules", "GameInstances.json"));

			//Task.Factory.StartNew(() => { GetGPUinfo(); });
			

			#region Create JSON

			/*
			GameInstances = new List<GameInstance>();
			GameInstances.Add(new GameInstance("Elite Dangerous", "EliteDangerous64"));
			GameInstances[0].Modules.Add(new OverlayModule("Targeting Reticle", "Just a custom Reticle to get a competitive advantage.") { HotKeys = 110 });
			var L1 = new LayerEx("Layer_1", ".")
			{
				Kind = LayerType.TargetReticle_1,
				//Location = new Point(d3DOverlay.TargetWindow.Width / 2, d3DOverlay.TargetWindow.Height / 2),
				Location = new PointEx("ScreenWidth / 2", "ScreenHeight / 2"),
				Colors = new ColorsEx()
				{
					ForeColor = "Orange"
				},
				Lines = new LinesEx()
				{
					Length = 40,
					Padding = 4
				}
			};
			GameInstances[0].Modules[0].Layers.Add("Layer_1", L1);

			GameInstances[0].Modules.Add(new OverlayModule("FPS Meter", "an un-accurate FPS counter.") { HotKeys = 131089 });
			var L2 = new LayerEx("FPS_Background", "a semi-transparent background for FPS counter.")
			{
				Kind = LayerType.Rectangle,
				Location = new PointEx("8","38"),
				Size = new SizeEx("100", "30"),
				Colors = new ColorsEx()
				{
					ForeColor = "#FC6E06",
					BackColor = "41,41,41,160"
				}
			};
			GameInstances[0].Modules[1].Layers.Add("Layer_2", L2);
			var L3 = new LayerEx("FPS Counter", "The actual FPS counter.")
			{
				Kind = LayerType.FPS,
				Location = new PointEx("10", "40"),
				Colors = new ColorsEx()
				{
					ForeColor = "255,255,255",
				},
				Text = new TextEx() { Font = "Arial,14,Bold" }
			};
			GameInstances[0].Modules[1].Layers.Add("Layer_3", L3);

			GameInstances[0].Modules.Add(new OverlayModule("Logo", "Orange EDHM Logo") { HotKeys = 20 });
			var L4 = new LayerEx("Logo", "Orange EDHM Logo")
			{
				Kind = LayerType.Texture,
				Location = new PointEx("ScreenWidth - 250", "10"),
				Size = new SizeEx("250", "220"),
				Texture = new TextureEx() 
				{ 
					TextureName = "Logo", 
					TextureFile = "triple-elite_250.png",
					Opacity = 0.7f
				}
			};
			GameInstances[0].Modules[2].Layers.Add("Layer_4", L4);

			string JsonString = JsonSerializer.Serialize(GameInstances);
			Helper.SaveTextFile(@"C:\Temp\GameInstances.json", JsonString, Helper.TextEncoding.UTF8);

			//TODO: Draw a Geometry
			//GameInstances[0].Modules.Add(new OverlayModule("Geometry", "Geometry Test") { HotKeys = 0 });
			//SharpDX.Direct2D1.Geometry G = new SharpDX.Direct2D1.Geometry();

			//var L5 = new LayerEx("Geometry", "Geometry Test")
			//{
			//	Kind = LayerType.Geometry,
			//	Geometry = new GeometryEx()
			//	{
			//		Geometry = new SharpDX.Direct2D1.Geometry(),
			//		StrokeStyle = new SharpDX.Direct2D1.StrokeStyle()
			//	}
			//};
			//GameInstances[0].Modules[2].Layers.Add("Layer_5", L5);

			*/
			#endregion

		}
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			OverlayRunning = false;
			Application.DoEvents();

			_DXOverlayPlugin?.Dispose();

			HotKeysManager?.Stop();
			HotKeysManager?.UnregisterAll();
		}

		#endregion

		public void LoadConfiguration(string pFilePath)
		{
			try
			{
				if (File.Exists(pFilePath))
				{
					string JsonString = Helper.ReadTextFile(pFilePath, Helper.TextEncoding.UTF8);
					GameInstances = (List<GameInstance>)JsonSerializer.Deserialize<List<GameInstance>>(JsonString);

					if (GameInstances != null)
					{
						cboInstances.Items.Clear();
						foreach (GameInstance instance in GameInstances)
						{
							cboInstances.Items.Add(instance);
						}
						cboInstances.SelectedItem = GameInstances[0];
					}
				}
				else
				{
					//Si el archivo no exite creamos uno de ejemplo con el FPS counter
					if (!Directory.Exists(Path.Combine(AppExePath, "modules"))) { Directory.CreateDirectory(Path.Combine(AppExePath, "modules")); }

					GameInstances = new List<GameInstance>();
					GameInstances.Add(new GameInstance("Elite Dangerous", "EliteDangerous64"));
					GameInstances[0].Modules.Add(new OverlayModule("FPS Meter", "An un-accurate FPS counter.") { HotKeys = 131089 });
					var L2 = new LayerEx("FPS_Background", "a semi-transparent background for FPS counter.")
					{
						Kind = LayerType.Rectangle,
						Location = new PointEx("8", "38"),
						Size = new SizeEx("100", "30"),
						Colors = new ColorsEx()
						{
							ForeColor = "#FC6E06",
							BackColor = "41,41,41,160"
						}
					};
					GameInstances[0].Modules[0].Layers.Add("Layer_1", L2);
					var L3 = new LayerEx("FPS Counter", "The actual FPS counter.")
					{
						Kind = LayerType.FPS,
						Location = new PointEx("10", "40"),
						Colors = new ColorsEx()
						{
							ForeColor = "255,255,255",
						},
						Text = new TextEx() { Font = "Arial,14,Bold" }
					};
					GameInstances[0].Modules[0].Layers.Add("Layer_2", L3);

					cboInstances.Items.Clear();
					foreach (GameInstance instance in GameInstances)
					{
						cboInstances.Items.Add(instance);
					}
					cboInstances.SelectedItem = GameInstances[0];
				}

				LoadModules(GameInstances[0]);
				InitializeOverlay();

				Task.Factory.StartNew(() => { HookGameWindow(); }); 
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void LoadModules(GameInstance pInstance)
		{
			try
			{
				if (pInstance != null)
				{
					SelectedInstance = pInstance;

					tblModules.Controls.Clear();
					tblModules.ColumnCount = 1;
					tblModules.AutoScroll = true;
					tblModules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
					tblModules.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddRows;

					tblModules.RowStyles.Clear();
					tblModules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));

					tblModules.ColumnStyles.Clear();
					tblModules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));

					if (SelectedInstance.Modules != null && SelectedInstance.Modules.Count > 0)
					{
						foreach (var _module in SelectedInstance.Modules)
						{
							var _modControl = new ModuleGUI(_module) { Dock = DockStyle.Fill };
							
							tblModules.Controls.Add(_modControl);

							/* If the Module has Keybindings here we detect and react to them when pressed  */
							if (_module.HotKeys != 0)
							{
								Keys _PressedKeys = (Keys)_module.HotKeys;
								var Modifiers = Helper.GetModifiers(_PressedKeys);
								if (Modifiers != 0)
								{
									//a keybinding with modifiers like CTRL, ALT or SHIFT
									HotKeysManager.RegisterHotkey(Modifiers, Helper.GetKeyCode(_PressedKeys), () =>
									{
										_module.Enabled = !_module.Enabled;

										if (_module.Layers != null && _module.Layers.Count > 0)
										{
											foreach (var _layer in _module.Layers)
											{
												_layer.Value.Visible = _module.Enabled;
											}
										}

										Invoke((MethodInvoker)(() => _modControl.ToggleEnabled(_module.Enabled)));
										Console.WriteLine(string.Format("{0} detected!", _PressedKeys));
									});
								}
								else
								{
									//a single key binding
									HotKeysManager.RegisterHotkey(Helper.GetKeyCode(_PressedKeys), () =>
									{
										_module.Enabled = !_module.Enabled;

										if (_module.Layers != null && _module.Layers.Count > 0)
										{
											foreach (var _layer in _module.Layers)
											{
												_layer.Value.Visible = _module.Enabled;
											}
										}

										Invoke((MethodInvoker)(() => _modControl.ToggleEnabled(_module.Enabled)));
										Console.WriteLine(string.Format("{0} detected!", _PressedKeys));
									});
								}
							}

							/* Si el usuario agrega nuevos recursos  */
							_modControl.OnResourceAdded += (object sender, OnResourceAddedArgs e) =>
							{
								OverlayRunning = false;
								_DXOverlayPlugin.Disable();
								InitializeOverlay();

								if (GameIsRunning)
								{
									StartOverlay();
								}
							};

							/* Si el usuario agrega nuevas capas  */
							_modControl.OnLayerAdded += (object sender, OnResourceAddedArgs e) =>
							{
								// Reiniciamos todo el Overlay para que cargue los nuevos recursos
								OverlayRunning = false;
								_DXOverlayPlugin.Disable();
								InitializeOverlay();

								if (GameIsRunning)
								{
									StartOverlay();
								}
							};
						}						
					}

					int vertScrollWidth = SystemInformation.VerticalScrollBarWidth;
					tblModules.Padding = new Padding(0, 0, vertScrollWidth, 0);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void InitializeOverlay()
		{
			try
			{
				_DXOverlayPlugin = new DXOverlayPlugin();

				d3DOverlay = (DXOverlayPlugin)_DXOverlayPlugin;
				d3DOverlay.Settings.Current.UpdateRate = 1000 / 60;

				/*  Resources like Colors, Fonts and Textures, need to be pre-loaded before running the Overlay  */
				
				d3DOverlay.Colors = new System.Collections.Generic.Dictionary<string, Color>();
				d3DOverlay.Fonts = new Dictionary<string, Font>();
				d3DOverlay.Textures = new Dictionary<string, Bitmap>();
				d3DOverlay.Layers = new Dictionary<string, LayerEx>();

				/* Here we pre-load default resources  */
				d3DOverlay.Colors["White"] = Color.White;
				d3DOverlay.Colors["Gray"] = Color.Gray;
				d3DOverlay.Colors["Red"] = Color.Red;
				d3DOverlay.Colors["Orange"] = Color.FromArgb(255, Color.Orange);
				d3DOverlay.Colors["Yellow"] = Color.FromArgb(255, Color.Yellow);
				d3DOverlay.Colors["Green"] = Color.FromArgb(255, Color.Green);
				d3DOverlay.Colors["Blue"] = Color.Blue;
				d3DOverlay.Colors["Blue50%"] = Color.FromArgb(127, Color.Blue);
				d3DOverlay.Colors["Blue70%"] = Color.FromArgb(180, Color.Blue);
				d3DOverlay.Colors["Purple"] = Color.FromArgb(255, Color.Purple);
				d3DOverlay.Colors["Pink"] = Color.FromArgb(255, Color.Pink);
				d3DOverlay.Colors["Brown"] = Color.FromArgb(255, Color.Brown);

				d3DOverlay.Fonts["Consolas;12;0"] = new Font(new FontFamily("Consolas"), 12);
				d3DOverlay.Fonts["Arial;14;0"] = new Font(new FontFamily("Arial"), 14);

				//Fonts for MessageBox:
				d3DOverlay.Fonts["MessageTitle"] = new Font(new FontFamily("Arial"), 14, FontStyle.Bold | FontStyle.Underline);
				d3DOverlay.Fonts["MessageText"] = new Font(new FontFamily("Arial"), 12);

				foreach (ModuleGUI _modControl in tblModules.Controls)
                {
					_modControl.AvailableColors = d3DOverlay.Colors;
					_modControl.AvailableFonts = d3DOverlay.Fonts;
					_modControl.AvailableTextures = d3DOverlay.Textures;					
				}
            }
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void StartOverlay()
		{
			try
			{
				_DXOverlayPlugin.Initialize(_processSharp.WindowFactory.MainWindow);
				List<TextureEx> _AvailableTexturesDescriptor = new List<TextureEx>();

				if (SelectedInstance.Modules != null && SelectedInstance.Modules.Count > 0)
				{
					int modCounter = 1;
					foreach (var _module in SelectedInstance.Modules)
					{
						if (_module.Layers != null && _module.Layers.Count > 0)
						{
							int layerCounter = 1;
							foreach (var _layer in _module.Layers)
							{
								/* Here we pre-load User Resources */
								if (_layer.Value.Colors != null)
								{
									AddColorBrush(_layer.Value.Colors.BackColor);
									AddColorBrush(_layer.Value.Colors.ForeColor);
								}
								if (_layer.Value.Text != null)
								{
									AddFontResource(_layer.Value.Text.Font);
								}
								if (_layer.Value.Texture != null)
								{
									if (AddTextureResource(_layer.Value.Texture))
									{
										_AvailableTexturesDescriptor.Add(_layer.Value.Texture);
									}
								}

								/* Here we Crete Layers to Draw Stuff on the Overlay */
								//d3DOverlay.Layers.Add(_layer.Key, _layer.Value);
								d3DOverlay.Layers.Add(string.Format("Module{0}_Layer{1}", modCounter, layerCounter), _layer.Value);
								layerCounter++;
							}
						}
						modCounter++;
					}
				}
				d3DOverlay.AddUserResources();
				_DXOverlayPlugin.Enable();

				foreach (ModuleGUI _modControl in tblModules.Controls)
				{
					_modControl.AvailableColors = d3DOverlay.Colors;
					_modControl.AvailableFonts = d3DOverlay.Fonts;
					_modControl.AvailableTextures = d3DOverlay.Textures;
					_modControl.AvailableTexturesDescriptor = _AvailableTexturesDescriptor;
				}

				OverlayRunning = true;
				//Task.Factory.StartNew(() => { DrawGrid(); });

				PlayOverlay();
				//Task.Factory.StartNew(() => { PlayOverlay(); });
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void StopOverlay()
		{
			try
			{
				OverlayRunning = false;
				d3DOverlay.ClearScreen();

				Application.DoEvents();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void PlayOverlay()
		{
			try
			{
				while (OverlayRunning)
				{
					_DXOverlayPlugin.Update();
					Application.DoEvents();
				}
			}
			catch (Exception ex)
			{
				StopOverlay();
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);				
			}
		}

		public void HookGameWindow()
		{
			try
			{
				System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(SelectedInstance.ExeName).FirstOrDefault();
				if (process != null)
				{
					_processSharp = new ProcessSharp(process, MemoryType.Remote);
					_processSharp.ProcessExited += Game_ProcessExited;
					GameIsRunning = true;

					StartOverlay();

					Invoke((MethodInvoker)(() =>
					{
						lblStatus.Text = string.Format("'{0}' is running.", SelectedInstance.ExeName);
						lblStatus.ForeColor = Color.Green;						
					}));
				}
				else
				{
					Invoke((MethodInvoker)(() =>
					{
						lblStatus.Text = string.Format("'{0}' not found running.", SelectedInstance.ExeName);
						lblStatus.ForeColor = Color.Red;
					}));
					
					while (!GameIsRunning)
					{
						process = System.Diagnostics.Process.GetProcessesByName(SelectedInstance.ExeName).FirstOrDefault();
						if (process != null) 
						{
							GameIsRunning = true;
							_processSharp = new ProcessSharp(process, MemoryType.Remote);
							_processSharp.ProcessExited += Game_ProcessExited;

							StartOverlay();

							Invoke((MethodInvoker)(() =>
							{
								lblStatus.Text = string.Format("'{0}' is running.", SelectedInstance.ExeName);
								lblStatus.ForeColor = Color.Green;								
							}));
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void Game_ProcessExited(object sender, EventArgs e)
		{
			Invoke((MethodInvoker)(() =>
			{
				StopOverlay();

				lblStatus.Text = string.Format("'{0}' not found running.", SelectedInstance.ExeName);
				lblStatus.ForeColor = Color.Red;

				Task.Factory.StartNew(() => { HookGameWindow(); });
			}));		
		}

		private bool AddColorBrush(string pColorValue)
		{
			bool _ret = false;
			try
			{
				if (!string.IsNullOrEmpty(pColorValue))
				{
					//Adds the resource only if it doesnt exists previously:
					if (!d3DOverlay.Colors.ContainsKey(pColorValue)) //<- SelectedColor.Name
					{
						d3DOverlay.Colors[pColorValue] = Helper.GetColorFromString(pColorValue, pColorValue.Contains(","));
						_ret = true;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		private bool AddFontResource(string pFontData)
		{
			bool _ret = false;
			try
			{
				if (!string.IsNullOrEmpty(pFontData))
				{
					//Adds the resource only if it doesnt exists previously:
					if (!d3DOverlay.Fonts.ContainsKey(pFontData)) 
					{
						d3DOverlay.Fonts[pFontData] = Helper.GetFontFromString(pFontData);
						_ret = true;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		private bool AddTextureResource(TextureEx pTextureData)
		{
			bool _ret = false;
			try
			{
				if (pTextureData != null)
				{
					//Adds the resource only if it doesnt exists previously:
					if (!d3DOverlay.Textures.ContainsKey(pTextureData.TextureName))
					{
						string _file = Path.Combine(AppExePath, "modules", "textures", pTextureData.TextureFile);
						if (File.Exists(_file))
						{
							//Logo EDHM
							d3DOverlay.Textures[pTextureData.TextureName] = (Bitmap)Image.FromFile(_file);
							_ret = true;
						}
					}					
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		public void GetGPUinfo()
		{
			try
			{
				// Gets temperature info from OS and prints it to the console
				//PerformanceCounter perfCount = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				//PerformanceCounter tempCount = new PerformanceCounter("Thermal Zone Information", "Temperature", @"\_TZ.THRM");
				var gpuCounters = Helper.GetGPUCounters();
				while (true)
				{
					try
					{
						//var gpuCounters = Helper.GetGPUCounters();
						var gpuUsage = Helper.GetGPUUsage(gpuCounters);
						Console.WriteLine(string.Format("GPU Use: {0:n2}%", gpuUsage));

						continue;
					}
					catch { }

					Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void DrawGrid(DirectXOverlayWindow OverlayWindow, Direct2DRenderer Graphics)
		{
			try
			{
				var _overlay = d3DOverlay.OverlayWindow;
				var gFx = d3DOverlay.OverlayWindow.Graphics;

				var _gridBounds = new System.Drawing.Rectangle(20, 60, _overlay.Width - 20, _overlay.Height - 20);
				var _gridGeometry = gFx.CreateGeometry();

				for (float x = _gridBounds.Left; x <= _gridBounds.Right; x += 20)
				{
					_gridGeometry.BeginLine(new Point((int)x, _gridBounds.Top), new Point((int)x, _gridBounds.Bottom));
					_gridGeometry.EndFigure(false);
				}

				for (float y = _gridBounds.Top; y <= _gridBounds.Bottom; y += 20)
				{
					_gridGeometry.BeginLine(new Point(_gridBounds.Left, (int)y), new Point(_gridBounds.Right, (int)y));
					_gridGeometry.EndFigure(false);
				}
				_gridGeometry.Close();
				gFx.DrawGeometry(_gridGeometry, 1, 1.0f);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		private void mnuLoadMudules_Click(object sender, EventArgs e)
		{
			LoadConfiguration(Path.Combine(AppExePath, "modules", "GameInstances.json"));
		}
		private void mnuSaveChanges_Click(object sender, EventArgs e)
		{
			string JsonString = JsonSerializer.Serialize(this.GameInstances, new JsonSerializerOptions { WriteIndented = true });
			Helper.SaveTextFile(
				Path.Combine(AppExePath, "modules", "GameInstances.json"), 
				JsonString, Helper.TextEncoding.UTF8);
		}
		private void mnuExit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void cmdAddModule_Click(object sender, EventArgs e)
		{
			var _module = new OverlayModule("New Module", "...");
			var _modControl = new ModuleGUI(_module) 
			{ 
				Dock = DockStyle.Fill,
				AvailableColors = d3DOverlay.Colors,
				AvailableFonts = d3DOverlay.Fonts,
				AvailableTextures = d3DOverlay.Textures,

			};
			tblModules.Controls.Add(_modControl);
			SelectedInstance.Modules.Add(_module);

			/* If the Module has Keybindings here we detect and react to them when pressed  */
			if (_module.HotKeys != 0)
			{
				Keys _PressedKeys = (Keys)_module.HotKeys;
				var Modifiers = Helper.GetModifiers(_PressedKeys);
				if (Modifiers != 0)
				{
					//a keybinding with modifiers like CTRL, ALT or SHIFT
					HotKeysManager.RegisterHotkey(Modifiers, Helper.GetKeyCode(_PressedKeys), () =>
					{
						_module.Enabled = !_module.Enabled;

						if (_module.Layers != null && _module.Layers.Count > 0)
						{
							foreach (var _layer in _module.Layers)
							{
								_layer.Value.Visible = _module.Enabled;
							}
						}

						Invoke((MethodInvoker)(() => _modControl.ToggleEnabled(_module.Enabled)));
						Console.WriteLine(string.Format("{0} detected!", _PressedKeys));
					});
				}
				else
				{
					//a single key binding
					HotKeysManager.RegisterHotkey(Helper.GetKeyCode(_PressedKeys), () =>
					{
						_module.Enabled = !_module.Enabled;

						if (_module.Layers != null && _module.Layers.Count > 0)
						{
							foreach (var _layer in _module.Layers)
							{
								_layer.Value.Visible = _module.Enabled;
							}
						}

						Invoke((MethodInvoker)(() => _modControl.ToggleEnabled(_module.Enabled)));
						Console.WriteLine(string.Format("{0} detected!", _PressedKeys));
					});
				}
			}

			/* Si el usuario agrega nuevos recursos  */
			_modControl.OnResourceAdded += (object Sender, OnResourceAddedArgs E) =>
			{
				OverlayRunning = false;
				_DXOverlayPlugin.Disable();
				InitializeOverlay();

				if (GameIsRunning)
				{
					StartOverlay();
				}
			};

			/* Si el usuario agrega nuevas capas  */
			_modControl.OnLayerAdded += (object Sender, OnResourceAddedArgs E) =>
			{
				// Reiniciamos todo el Overlay para que cargue los nuevos recursos
				OverlayRunning = false;
				_DXOverlayPlugin.Disable();
				InitializeOverlay();

				if (GameIsRunning)
				{
					StartOverlay();
				}
			};
		}

		private void cmdRemoveModule_Click(object sender, EventArgs e)
		{

		}
	}
}
