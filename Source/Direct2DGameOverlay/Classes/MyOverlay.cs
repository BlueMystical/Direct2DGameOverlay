using Overlay.NET.Common;
using Overlay.NET.Directx;
using Process.NET.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Font = System.Drawing.Font;

namespace DirectXOverlay
{
	// Example settings
	public class DemoOverlaySettings
	{
		// 60 frames/sec roughly
		public int UpdateRate { get; set; } = 60;

		public string Author { get; set; }
		public string Description { get; set; }
		public string Identifier { get; set; }
		public string Name { get; set; }
		public string Version { get; set; }
	}

	// https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	public enum vKeyCodes
	{
		VK_LBUTTON = 0x01,  //Left mouse button
		VK_RBUTTON = 0x02,   //Right mouse button
		VK_MBUTTON = 0x04,   //Middle mouse button (three-button mouse)
		VK_SHIFT = 0x10,    //SHIFT key
		VK_CONTROL = 0x11,  //CTRL key
		VK_MENU = 0x12,   //ALT key

		VK_LSHIFT = 0xA0,       //	Left SHIFT key
		VK_RSHIFT = 0xA1,       //	Right SHIFT key
		VK_LCONTROL = 0xA2,     //	Left CONTROL key
		VK_RCONTROL = 0xA3,     //	Right CONTROL key
		VK_LMENU = 0xA4,        //	Left ALT key
		VK_RMENU = 0xA5,        //	Right ALT key

		KEY_0 = 0x30,   //0 key
		KEY_1 = 0x31,   //1 key
		KEY_2 = 0x32,   //2 key
		KEY_3 = 0x33,   //3 key
		KEY_4 = 0x34,   //4 key
		KEY_5 = 0x35,   //5 key
		KEY_6 = 0x36,   //6 key
		KEY_7 = 0x37,   //7 key
		KEY_8 = 0x38,   //8 key
		KEY_9 = 0x39,   //9 key

		VK_NUMPAD0 = 0x60,  //Numeric keypad 0 key
		VK_NUMPAD1 = 0x61,  //Numeric keypad 1 key
		VK_NUMPAD2 = 0x62,  //Numeric keypad 2 key
		VK_NUMPAD3 = 0x63,  //Numeric keypad 3 key
		VK_NUMPAD4 = 0x64,  //Numeric keypad 4 key
		VK_NUMPAD5 = 0x65,  //Numeric keypad 5 key
		VK_NUMPAD6 = 0x66,  //Numeric keypad 6 key
		VK_NUMPAD7 = 0x67,  //Numeric keypad 7 key
		VK_NUMPAD8 = 0x68,  //Numeric keypad 8 key
		VK_NUMPAD9 = 0x69,  //Numeric keypad 9 key

		VK_MULTIPLY = 0x6A,     //	Multiply key
		VK_ADD = 0x6B,         //	Add key
		VK_SEPARATOR = 0x6C,  // 
		VK_SUBTRACT = 0x6D,  //	Subtract key
		VK_DECIMAL = 0x6E,  //	Decimal key
		VK_DIVIDE = 0x6F,  //	Divide key

		VK_F1 = 0x70,  //	F1 key
		VK_F2 = 0x71,  //	F2 key
		VK_F3 = 0x72,  //	F3 key
		VK_F4 = 0x73,  //	F4 key
		VK_F5 = 0x74,  //	F5 key
		VK_F6 = 0x75,  //	F6 key
		VK_F7 = 0x76,  //	F7 key
		VK_F8 = 0x77,  //	F8 key
		VK_F9 = 0x78,  //	F9 key
		VK_F10 = 0x79,  //	F10 key
		VK_F11 = 0x7A,  //	F11 key
		VK_F12 = 0x7B,  //	F12 key
	}

	public enum LayerType
	{
		Text = 0,
		BarH,
		BarV,
		Box2D,
		Box3D,

		Circle,	//5
		CircleFill,

		Rectangle,
		Rectangle3D,
		RectangleFill,

		Edge, //10
		Line,
		Plus,

		TargetReticle_1,
		TargetReticle_2,
		TargetReticle_3, //15

		FPS,	
		Texture,
		Geometry,
		GeometryFilled,

		MessageBox, //20
		GpuUsage,
		ExternalModule
	}
	


	[RegisterPlugin("DirectXOverlay", "Blue Mystic", "CustomDXOverlay", "1.0.0", "Custom Implementation of DirectXoverlay")]
	public class DXOverlayPlugin : DirectXOverlayPlugin
	{
		#region Private Members
		
		private readonly TickEngine _tickEngine = new TickEngine();
		public readonly ISettings<DemoOverlaySettings> Settings = new SerializableSettings<DemoOverlaySettings>();
		
		
		private int _Defaultfont, _DefaultfontBold, _DefaultfontUnderline;
		private int _DefaultBrush;
		private int _DefaultBrushTransparent;
		private int _DefaultBrushFill;
		private int _fontSize = 10;
		private int _hugeFont;

		private bool _ExecutingCode = false;

		//para el FPS
		private int _framesRendered = 0;
		private int _fps = 0;
		private Stopwatch _watch;

		//PerformanceCounter tempCount = new PerformanceCounter("Thermal Zone Information", "Temperature", @"\_TZ.THRM");
		List<PerformanceCounter> gpuCounters;

		private Dictionary<string, int> _Brushes;
		private Dictionary<string, int> _Fonts;
		private Dictionary<string, int> _Textures;

		#endregion

		#region Public Properties

		public IWindow _TargetWindow;

		public Dictionary<string, LayerEx> Layers { get; set; }
		public Dictionary<string, Color> Colors { get; set; }
		public Dictionary<string, Font> Fonts { get; set; }
		public Dictionary<string, Bitmap> Textures { get; set; }

		#endregion

		#region Public Methods
		
		public override void Initialize(IWindow targetWindow)
		{
			// Set target window by calling the base method
			base.Initialize(targetWindow);
			this._TargetWindow = targetWindow;

			// For demo, show how to use settings
			var current = Settings.Current;
			var type = GetType();

			if (current.UpdateRate == 0)
				current.UpdateRate = 1000 / 60;

			current.Author = GetAuthor(type);
			current.Description = GetDescription(type);
			current.Identifier = GetIdentifier(type);
			current.Name = GetName(type);
			current.Version = GetVersion(type);

			OverlayWindow = new DirectXOverlayWindow(targetWindow.Handle, false);
			_watch = Stopwatch.StartNew();

			//Here we prepare the Brushes, Fonts and Textures we will use, this is for Performance reasons:
			//First, the default ones:
			_Defaultfont = OverlayWindow.Graphics.CreateFont("Consolas", 12);
			_DefaultfontBold = OverlayWindow.Graphics.CreateFont("Consolas", 14, true);

			_DefaultBrush = OverlayWindow.Graphics.CreateBrush(0x7FFF0000);
			_DefaultBrushFill = OverlayWindow.Graphics.CreateBrush(0x7FFFFF00);
			_DefaultBrushTransparent = OverlayWindow.Graphics.CreateBrush(Color.FromArgb(70, 255, 0, 0));


			// Set up update interval and register events for the tick engine.

			_tickEngine.PreTick += OnPreTick;
			_tickEngine.Tick += OnTick;
		}

		public void AddUserResources()
		{
			try
			{
				//Now the User Defined ones:
				if (this.Colors != null && this.Colors.Count > 0)
				{
					_Brushes = new Dictionary<string, int>();
					foreach (var brush in this.Colors)
					{
						_Brushes[brush.Key] = OverlayWindow.Graphics.CreateBrush(brush.Value);
					}
				}
				if (this.Fonts != null && this.Fonts.Count > 0)
				{
					_Fonts = new Dictionary<string, int>();
					foreach (var font in this.Fonts)
					{
						_Fonts[font.Key] = OverlayWindow.Graphics.CreateFont(font.Value.Name, font.Value.Size, font.Value.Bold, font.Value.Italic);
						_fontSize = (int)font.Value.Size;
					}
				}
				if (this.Textures != null && this.Textures.Count > 0)
				{
					_Textures = new Dictionary<string, int>();
					foreach (var texture in this.Textures)
					{
						_Textures[texture.Key] = OverlayWindow.Graphics.CreateTexture(texture.Value);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// ReSharper disable once RedundantOverriddenMember
		public override void Enable()
		{
			_tickEngine.Interval = Settings.Current.UpdateRate.Milliseconds();
			_tickEngine.IsTicking = true;
			base.Enable();
		}

		// ReSharper disable once RedundantOverriddenMember
		public override void Disable()
		{
			_tickEngine.IsTicking = false;
			base.Disable();
		}

		/// <summary>Refresh every Frame.</summary>
		public override void Update() => _tickEngine.Pulse();

		public void ClearScreen()
		{
			OverlayWindow.Graphics.BeginScene();
			OverlayWindow.Graphics.ClearScene();
			OverlayWindow.Graphics.EndScene();
		}

		public override void Dispose()
		{
			OverlayWindow?.Dispose();
			base.Dispose();
		}

		#endregion

		#region Private Methods

		private void OnTick(object sender, EventArgs e)
		{
			if (!OverlayWindow.IsVisible)
			{
				return;
			}

			OverlayWindow.Update();
			InternalRender();
		}

		private void OnPreTick(object sender, EventArgs e)
		{
			var targetWindowIsActivated = TargetWindow.IsActivated;
			if (!targetWindowIsActivated && OverlayWindow.IsVisible)
			{
				_watch.Stop();
				ClearScreen();
				OverlayWindow.Hide();
			}
			else if (targetWindowIsActivated && !OverlayWindow.IsVisible)
			{
				OverlayWindow.Show();
			}
		}

	

		/// <summary>***HERE WE DRAW STUFF***</summary>
		protected void InternalRender()
		{
			if (!_watch.IsRunning)
			{
				_watch.Start();
			}

			OverlayWindow.Graphics.BeginScene();
			OverlayWindow.Graphics.ClearScene();

			if (this.Layers != null && this.Layers.Count > 0)
			{
				if (_Fonts is null || _Brushes is null) return;

				foreach (KeyValuePair<string, LayerEx> layer in this.Layers)
				{
					if (layer.Value.Visible)
					{
						Point _location = (layer.Value.Location != null) ? layer.Value.Location.ToPoint(this._TargetWindow) : Point.Empty;
						Size _size = (layer.Value.Size != null) ? layer.Value.Size.ToSize(this._TargetWindow) : Size.Empty;


						if (layer.Value.Kind == LayerType.ExternalModule)
						{
							//This calls External Code:
							if (!string.IsNullOrEmpty(layer.Value.CallBackCodeFile))
							{
								if (layer.Value.ExternalModule is null && _ExecutingCode == false)
								{
									// Here we load and Compile the External Module:
									string _Mpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", "code", layer.Value.CallBackCodeFile);
									if (File.Exists(_Mpath))
									{
										_ExecutingCode = true;
										Task.Factory.StartNew(() => {
											layer.Value.CallBackCode = Helper.ReadTextFile(_Mpath, Helper.TextEncoding.UTF8);
											if (!string.IsNullOrEmpty(layer.Value.CallBackCode))
											{
												layer.Value.ExternalModule = new DXOverlay.ExternalModule.CodeCompiler(layer.Value.CallBackCode);
												if (layer.Value.ExternalModule != null)
												{
													layer.Value.ExternalModule.Initialize(layer.Value.CallBackCodeFile, _Brushes, _Fonts, _Textures);
												}
											}
											_ExecutingCode = false;
										});
									}
								}
								else
								{
									// Here we call the 'Render' method from the External Module
									layer.Value.ExternalModule?.RenderCode(this.OverlayWindow, this.OverlayWindow.Graphics);
								}
							}
						}
						else
						{
							/* This Draws the shapes declared on the layer  */
							RenderLayer(layer, _location, _size);

							/*
							// Create a Grid using 'Geometry' for complex shapes:
							//	https://learn.microsoft.com/en-us/windows/win32/direct2d/path-geometries-overview

							var _overlay = this.OverlayWindow;
							var gFx = OverlayWindow.Graphics;

							var _gridBounds = new System.Drawing.Rectangle(20, 60, _overlay.Width - 40, _overlay.Height - 40);
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
							gFx.DrawGeometry(_gridGeometry, _Brushes["Blue50%"], 1.0f);
							*/
						}



					}
				}
			}
			OverlayWindow.Graphics.EndScene();
		}

		public void RenderLayer(KeyValuePair<string, LayerEx> layer, Point _location, Size _size)
		{
			try
			{
				if (layer.Value.Kind == LayerType.Text)
				{
					OverlayWindow.Graphics.Antialias = SharpDX.Direct2D1.AntialiasMode.Aliased;
					OverlayWindow.Graphics.DrawText(
						layer.Value.Text.Text, _Fonts[layer.Value.Text.Font],
						_Brushes[layer.Value.Colors.ForeColor],
						_location.X, _location.Y,
						false
					);
				}

				if (layer.Value.Kind == LayerType.Line)
				{
					OverlayWindow.Graphics.Antialias = SharpDX.Direct2D1.AntialiasMode.Aliased;
					OverlayWindow.Graphics.DrawLine(
						_location.X, _location.Y,
						_location.X + _size.Width, _location.Y + _size.Height,
						layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor]);
				}
				if (layer.Value.Kind == LayerType.Plus)
				{
					OverlayWindow.Graphics.Antialias = SharpDX.Direct2D1.AntialiasMode.Aliased;
					OverlayWindow.Graphics.DrawPlus(
						_location.X, _location.Y,
						layer.Value.Lines.Length, layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor]);
				}

				if (layer.Value.Kind == LayerType.TargetReticle_1)
				{
					OverlayWindow.Graphics.Antialias = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
					OverlayWindow.Graphics.DrawTargetReticle_1(
						_location.X, _location.Y,
						layer.Value.Lines.Length,
						layer.Value.Lines.Padding,
						layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor]);
				}
				if (layer.Value.Kind == LayerType.TargetReticle_2)
				{
					OverlayWindow.Graphics.Antialias = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
					OverlayWindow.Graphics.DrawTargetReticle_2(
						_location.X, _location.Y,
						layer.Value.Lines.Length, layer.Value.Lines.Padding,
						layer.Value.Lines.Stroke, _Brushes[layer.Value.Colors.ForeColor]);
				}
				if (layer.Value.Kind == LayerType.TargetReticle_3)
				{
					OverlayWindow.Graphics.Antialias = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
					OverlayWindow.Graphics.DrawTargetReticle_3(
						_location.X, _location.Y, layer.Value.Lines.Length, layer.Value.Lines.Padding, layer.Value.Lines.Stroke, _Brushes[layer.Value.Colors.ForeColor]);
				}

				if (layer.Value.Kind == LayerType.Circle)
				{
					OverlayWindow.Graphics.DrawCircle(
						_location.X, _location.Y,
						layer.Value.Lines.Length, layer.Value.Lines.Stroke, _Brushes[layer.Value.Colors.ForeColor]);
				}
				if (layer.Value.Kind == LayerType.CircleFill)
				{
					OverlayWindow.Graphics.FillCircle(
						_location.X, _location.Y, layer.Value.Lines.Length,
						_Brushes[layer.Value.Colors.BackColor]);
				}

				if (layer.Value.Kind == LayerType.Rectangle)
				{
					OverlayWindow.Graphics.DrawRectangle(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						layer.Value.Lines.Stroke, _Brushes[layer.Value.Colors.ForeColor]);
				}
				if (layer.Value.Kind == LayerType.RectangleFill)
				{
					OverlayWindow.Graphics.FillRectangle(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						_Brushes[layer.Value.Colors.BackColor]);
				}
				if (layer.Value.Kind == LayerType.Rectangle3D)
				{
					OverlayWindow.Graphics.DrawRectangle3D(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						layer.Value.Lines.Length, layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.BackColor]);
				}

				if (layer.Value.Kind == LayerType.Box2D)
				{
					OverlayWindow.Graphics.DrawBox2D(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor],
						_Brushes[layer.Value.Colors.BackColor]);
				}
				if (layer.Value.Kind == LayerType.Box3D)
				{
					OverlayWindow.Graphics.DrawBox3D(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						layer.Value.Lines.Length, layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor], _Brushes[layer.Value.Colors.BackColor]);
				}

				if (layer.Value.Kind == LayerType.Edge)
				{
					OverlayWindow.Graphics.DrawEdge(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						layer.Value.Lines.Length, layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor]);
				}

				if (layer.Value.Kind == LayerType.BarH)
				{
					OverlayWindow.Graphics.DrawBarH(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						layer.Value.Lines.Length, layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor], _Brushes[layer.Value.Colors.BackColor]);
				}
				if (layer.Value.Kind == LayerType.BarV)
				{
					OverlayWindow.Graphics.DrawBarV(
						_location.X, _location.Y,
						_size.Width, _size.Height,
						layer.Value.Lines.Length, layer.Value.Lines.Stroke,
						_Brushes[layer.Value.Colors.ForeColor], _Brushes[layer.Value.Colors.BackColor]);
				}

				if (layer.Value.Kind == LayerType.Texture)
				{
					//Logo:[triple-elite_250.png]
					OverlayWindow.Graphics.DrawTexture(
						_Textures[layer.Value.Texture.TextureName],
						new Rectangle(_location, _size),
						layer.Value.Texture.Opacity);
				}

				if (layer.Value.Kind == LayerType.Geometry)
				{
					OverlayWindow.Graphics.DrawGeometry(
						layer.Value.Geometry.Geometry,
						_Brushes[layer.Value.Colors.ForeColor],
						layer.Value.Lines.Stroke,
						layer.Value.Geometry.StrokeStyle);
				}
				if (layer.Value.Kind == LayerType.GeometryFilled)
				{
					OverlayWindow.Graphics.FillGeometry(layer.Value.Geometry.Geometry, _Brushes[layer.Value.Colors.BackColor]);
				}

				if (layer.Value.Kind == LayerType.FPS)
				{
					_framesRendered++;
					if (layer.Value.StartTime == DateTime.MinValue) layer.Value.StartTime = DateTime.Now;
					if ((DateTime.Now - layer.Value.StartTime).TotalSeconds >= 1)
					{
						// one second has elapsed 
						_fps = _framesRendered;
						_framesRendered = 0;
						layer.Value.Text.Text = string.Format("FPS: {0}", _fps);
						layer.Value.StartTime = DateTime.Now;
					}

					OverlayWindow.Graphics.DrawText(layer.Value.Text.Text,
						_Fonts[layer.Value.Text.Font],
						_Brushes[layer.Value.Colors.ForeColor],
						_location.X, _location.Y, false);
				}
				if (layer.Value.Kind == LayerType.GpuUsage)
				{
					try
					{
						//this should only be called every second or 2
						if (layer.Value.StartTime == DateTime.MinValue) layer.Value.StartTime = DateTime.Now;
						var MSeconds = (DateTime.Now.Subtract(layer.Value.StartTime)).TotalMilliseconds;

						if (MSeconds > layer.Value.TimeOff)
						{
							if (gpuCounters is null) gpuCounters = Helper.GetGPUCounters();
							float usage = Helper.GetGPUUsage(gpuCounters) * 10.0f;

							layer.Value.Text.Text = string.Format("GPU Use: {0:n1}%", usage);
							layer.Value.StartTime = layer.Value.StartTime = DateTime.Now; //<- re-starts the timer
						}

						OverlayWindow.Graphics.DrawText(
								layer.Value.Text.Text,
								_Fonts[layer.Value.Text.Font],
								_Brushes[layer.Value.Colors.ForeColor],
								_location.X, _location.Y, false
						);
					}
					catch
					{
						gpuCounters = null;
						layer.Value.StartTime = layer.Value.StartTime = DateTime.Now; //<- re-starts the timer
					}
				}

				if (layer.Value.Kind == LayerType.MessageBox)
				{
					//OverlayWindow.Graphics.Antialias = SharpDX.Direct2D1.AntialiasMode.Aliased;
					OverlayWindow.Graphics._device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Aliased;

					Point TitleSize = Point.Empty, MsgSize = Point.Empty;
					int CurrHeight = 0, TitleY = 0, LineY = 0, MessageY = 0, padding = 4;
					string pTitle = string.Empty, pMessage = string.Empty;
					bool HayTitulo = false, HayMensaje = false;
					Rectangle MBox = new Rectangle();
					Rectangle TBox = new Rectangle();


					// Check if we have something to show:
					var Palabras = layer.Value.Text.Text.Split(new char[] { '|' }).ToList();
					switch (Palabras.Count)
					{
						case 0: break;
						case 1: HayMensaje = true; pMessage = Palabras[0]; break;
						case 2: HayTitulo = true; HayMensaje = true; pTitle = Palabras[0]; pMessage = Palabras[1]; break;
						default: break;
					}

					CurrHeight += padding;

					// Calculate how much space the title needs and where it will be:
					if (HayTitulo) //<- Hay Titulo
					{
						TitleSize = OverlayWindow.Graphics.MeasureString(_Fonts["MessageTitle"], pTitle);
						TitleY = _location.Y + padding;

						decimal MaxW = _size.Width - (padding * 2);
						int Lines = (int)Math.Ceiling(TitleSize.X / MaxW);
						TBox = new Rectangle(
							_location.X + padding, TitleY,
							(int)MaxW + 100, (TitleSize.Y * Lines) + padding
						);

						// Where to draw the line:
						LineY = TitleY + TitleSize.Y + padding;

						CurrHeight += _location.Y + TitleSize.Y + padding;
					}
					// Calculate how much space the Message needs and where it will be:
					if (HayMensaje) //<- Hay mensaje
					{
						MsgSize = OverlayWindow.Graphics.MeasureString(_Fonts["MessageText"], pMessage);

						MessageY = _location.Y + padding + (HayTitulo ? TitleSize.Y + (padding * 2) : 0);

						decimal MaxW = _size.Width - (padding * 2);
						int Lines = (int)Math.Ceiling(MsgSize.X / MaxW);
						MBox = new Rectangle(
							_location.X + padding, MessageY,
							(int)MaxW + 100, (MsgSize.Y * Lines) + padding
						);
						CurrHeight = padding + TitleSize.Y + padding + (MsgSize.Y * Lines) + (padding * 3);
					}

					// Draw the Background Box:
					//OverlayWindow.Graphics.FillRectangle(
					//	_location.X, _location.Y,
					//	_size.Width, CurrHeight,
					//	_Brushes[layer.Value.Colors.BackColor]);

					//OverlayWindow.Graphics.FillRoundedRectangle(
					//	_Brushes[layer.Value.Colors.BackColor],
					//	_location.X, _location.Y,
					//	_size.Width, CurrHeight, 8.0f
					//);

					if (HayTitulo)
					{
						// Draw The Title
						//OverlayWindow.Graphics.DrawText(
						//	pTitle, 
						//	_Fonts["MessageTitle"],
						//	_Brushes[layer.Value.Colors.ForeColor],
						//	_location.X + padding, TitleY,
						//	false
						//);
						OverlayWindow.Graphics.DrawTextWithBackground(
							_Fonts["MessageTitle"],
							_Brushes[layer.Value.Colors.ForeColor],
							_Brushes[layer.Value.Colors.BackColor],
							TBox,
							pTitle
						);

						// Draw a Separator:
						OverlayWindow.Graphics.DrawLine(
							_location.X + padding, LineY,
							_location.X + _size.Width - padding, LineY,
							0.5f,
							_Brushes[layer.Value.Colors.ForeColor]);
					}

					if (HayMensaje)
					{
						// Draw The Message
						//OverlayWindow.Graphics.DrawText(
						//	pMessage,
						//	_Fonts["MessageText"],
						//	_Brushes[layer.Value.Colors.ForeColor],
						//	Box.Left, Box.Top,
						//	true
						//);
						//OverlayWindow.Graphics.DrawTextBox(
						//	pMessage,
						//	_Fonts["MessageText"],
						//	Box,
						//	_Brushes[layer.Value.Colors.ForeColor]
						//);

						OverlayWindow.Graphics.DrawTextWithBackground(
							_Fonts["MessageText"],
							_Brushes[layer.Value.Colors.ForeColor],
							_Brushes[layer.Value.Colors.BackColor],
							MBox,
							pMessage
						);
					}

					//OverlayWindow.Graphics.DrawEdge()

					// 5. CountDown for Message dimissal
					if (layer.Value.StartTime == DateTime.MinValue) layer.Value.StartTime = DateTime.Now;
					var MSeconds = (DateTime.Now.Subtract(layer.Value.StartTime)).TotalMilliseconds;
					if (MSeconds >= layer.Value.TimeOff)
					{
						layer.Value.Visible = false;
						layer.Value.StartTime = DateTime.MinValue;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion
	}
}
