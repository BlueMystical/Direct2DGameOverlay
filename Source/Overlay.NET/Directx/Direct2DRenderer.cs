using Overlay.NET.Common;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;

using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Color = System.Drawing.Color;
using Factory = SharpDX.DirectWrite.Factory;
using Image = Overlay.NET.Common.Image;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;

namespace Overlay.NET.Directx
{
	public class Direct2DRenderer {

        #region Public Properties
        
        /// <summary>
        ///     Gets the size of the buffer brush.
        /// </summary>
        /// <value>
        ///     The size of the buffer brush.
        /// </value>
        public int BufferBrushSize { get; private set; }

        /// <summary>
        ///     Gets the size of the buffer font.
        /// </summary>
        /// <value>
        ///     The size of the buffer font.
        /// </value>
        public int BufferFontSize { get; private set; }

        /// <summary>
        ///     Gets the size of the buffer layout.
        /// </summary>
        /// <value>
        ///     The size of the buffer layout.
        /// </value>
        public int BufferLayoutSize { get; private set; }

        public SharpDX.Direct2D1.AntialiasMode Antialias { get; set; } = AntialiasMode.Aliased;

		/// <summary>
		/// Indicates whether this Graphics surface is currently drawing on a Scene.
		/// </summary>
		public bool IsDrawing { get; private set; }

		/// <summary>
		/// Indicates whether this Graphics surface is initialized.
		/// </summary>
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// Gets or sets the width of this Graphics surface.
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Gets or sets the width of this Graphics surface.
		/// </summary>
		public int Height { get; set; }

		#endregion

		#region Private Members

		//transparent background color
		/// <summary>
		///     The GDI transparent
		/// </summary>
		private static Color _gdiTransparent = Color.Transparent;

        /// <summary>
        ///     The transparent
        /// </summary>
        private static readonly RawColor4 Transparent = new RawColor4(_gdiTransparent.R, _gdiTransparent.G, _gdiTransparent.B,
            _gdiTransparent.A);

        //direct x vars
        /// <summary>
        ///     The device
        /// </summary>
        public readonly WindowRenderTarget _device;

		private StrokeStyle _strokeStyle;

		/// <summary>
		///     The factory
		/// </summary>
		private readonly SharpDX.Direct2D1.Factory _factory;

        /// <summary>
        ///     The font factory
        /// </summary>
        private readonly Factory _fontFactory;

        /// <summary>
        ///     The brush container
        /// </summary>
        private List<SolidColorBrush> _brushContainer = new List<SolidColorBrush>(32);

        /// <summary>Store for pre-loaded Textures (Bitmaps)</summary>
        private List<Bitmap> _TextureContainer = new List<Bitmap>(32);

        //thread safe resizing
        /// <summary>
        ///     The do resize
        /// </summary>
        private bool _doResize;

        /// <summary>
        ///     The font container
        /// </summary>
        private List<TextFormat> _fontContainer = new List<TextFormat>(32);

        /// <summary>
        ///     The layout container
        /// </summary>
        private List<TextLayoutBuffer> _layoutContainer = new List<TextLayoutBuffer>(32);

        /// <summary>
        ///     The resize x
        /// </summary>
        private int _resizeX;

        /// <summary>
        ///     The resize y
        /// </summary>
        private int _resizeY;



		#endregion

		#region Contructors

		/// <summary>
		///     Initializes a new instance of the <see cref="Direct2DRenderer" /> class.
		/// </summary>
		/// <param name="hwnd">The HWND.</param>
		/// <param name="limitFps">if set to <c>true</c> [limit FPS].</param>
		public Direct2DRenderer(IntPtr hwnd, bool limitFps) {
            _factory = new SharpDX.Direct2D1.Factory();

            _fontFactory = new Factory();

            Native.Rect bounds;
            Native.GetWindowRect(hwnd, out bounds);

            var targetProperties = new HwndRenderTargetProperties {
                Hwnd = hwnd,
                PixelSize = new Size2(bounds.Right - bounds.Left, bounds.Bottom - bounds.Top),
                PresentOptions = limitFps ? PresentOptions.None : PresentOptions.Immediately
            };

            var prop = new RenderTargetProperties(RenderTargetType.Hardware,
                new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 0, 0, RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT);

            _device = new WindowRenderTarget(_factory, prop, targetProperties) {
                TextAntialiasMode = TextAntialiasMode.Aliased,
                AntialiasMode = AntialiasMode.Aliased
            };

			_strokeStyle = new StrokeStyle(_factory, new StrokeStyleProperties
			{
				DashCap = CapStyle.Flat,
				DashOffset = -1.0f,
				DashStyle = DashStyle.Dash,
				EndCap = CapStyle.Flat,
				LineJoin = LineJoin.MiterOrBevel,
				MiterLimit = 1.0f,
				StartCap = CapStyle.Flat
			});

			IsInitialized = true;
		}

        /// <summary>
        ///     Do not call if you use OverlayWindow class
        /// </summary>
        public void Dispose() {
            DeleteBrushContainer();
            DeleteFontContainer();
            DeleteLayoutContainer();

            _brushContainer = null;
            _fontContainer = null;
            _layoutContainer = null;

			IsInitialized = false;

			_fontFactory.Dispose();
			_strokeStyle.Dispose();
			_factory.Dispose();
            _device.Dispose();
        }

		/// <summary>
		/// Destroys an already initialized Graphics surface and frees its resources.
		/// </summary>
		public void Destroy()
		{
			if (!IsInitialized) throw new InvalidOperationException("D2DDevice needs to be initialized first");

			try
			{
				DeleteBrushContainer();
				DeleteFontContainer();
				DeleteLayoutContainer();

				_brushContainer = null;
				_fontContainer = null;
				_layoutContainer = null;

				_strokeStyle.Dispose();
				_fontFactory.Dispose();
				_factory.Dispose();
				_device.Dispose();
			}
			catch { }

			IsInitialized = false;
		}

		#endregion

		#region Public Methods

		/// <summary>
		///     tells renderer to resize when possible
		/// </summary>
		/// <param name="x">Width</param>
		/// <param name="y">Height</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AutoResize(int x, int y) {
            _doResize = true;
            _resizeX = x;
            _resizeY = y;

			Width = _resizeX;
			Height = _resizeY;
		}

		/// <summary>
		///     Do your drawing after this
		/// </summary>
		public void BeginScene()
		{
			if (_doResize)
			{
				_device.Resize(new Size2(_resizeX, _resizeY));

				Width = _resizeX;
				Height = _resizeY;

				_doResize = false;
			}

			_device.BeginDraw();
			IsDrawing = true;
		}

		/// <summary>
		///     Present frame. Do not draw after this.
		/// </summary>
		public void EndScene()
		{
			try
			{
				_device.EndDraw();
				if (!_doResize)
				{
					return;
				}
				_device.Resize(new Size2(_resizeX, _resizeY));

				_doResize = false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace);
			}
		}

		/// <summary>
		///     Clears the frame
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearScene() => _device.Clear(Transparent);

		/// <summary>
		/// Gets the Factory used by this Graphics surface.
		/// </summary>
		/// <returns>The Factory of this Graphics surface.</returns>
		public SharpDX.Direct2D1.Factory GetFactory()
		{
			if (!IsInitialized) throw new InvalidOperationException("The DirectX device is not initialized");

			return _factory;
		}

		/// <summary>
		/// Gets the FontFactory used by this Graphics surface.
		/// </summary>
		/// <returns>The FontFactory of this Graphics surface.</returns>
		public Factory GetFontFactory()
		{
			//if (!IsInitialized) throw new InvalidOperationException("The DirectX device is not initialized");

			return _fontFactory;
		}

		/// <summary>
		/// Gets the RenderTarget used by this Graphics surface.
		/// </summary>
		/// <returns>The RenderTarget of this Graphics surface.</returns>
		public RenderTarget GetRenderTarget()
		{
			//if (!IsInitialized) throw new InvalidOperationException("The DirectX device is not initialized");

			return _device;
		}

		public List<SolidColorBrush> GetBrushes()
		{
			return _brushContainer;
		}
		public List<TextFormat> GetFonts()
		{
			return _fontContainer;
		}

		#endregion

		#region Resources Methods

		/// <summary>
		///     Call this after EndScene if you created brushes within a loop
		/// </summary>
		public void DeleteBrushContainer() {
            if (_brushContainer != null)
            {
				BufferBrushSize = _brushContainer.Count;
				foreach (var solidColorBrush in _brushContainer)
				{
					solidColorBrush.Dispose();
				}
				_brushContainer = new List<SolidColorBrush>(BufferBrushSize);
			}            
        }

        /// <summary>
        ///     Call this after EndScene if you created fonts within a loop
        /// </summary>
        public void DeleteFontContainer() {
            if (_fontContainer != null)
            {
				BufferFontSize = _fontContainer.Count;
				foreach (var textFormat in _fontContainer)
				{
					textFormat.Dispose();
				}
				_fontContainer = new List<TextFormat>(BufferFontSize);
			}           
        }

        /// <summary>
        ///     Call this after EndScene if you changed your text's font or have problems with huge memory usage
        /// </summary>
        public void DeleteLayoutContainer() {
            if (_layoutContainer != null)
            {
				BufferLayoutSize = _layoutContainer.Count;
				foreach (var layoutBuffer in _layoutContainer)
				{
					layoutBuffer.Dispose();
				}
				_layoutContainer = new List<TextLayoutBuffer>(BufferLayoutSize);
			}            
        }

        /// <summary>
        ///     Creates a new SolidColorBrush
        /// </summary>
        /// <param name="color">0x7FFFFFF Premultiplied alpha color</param>
        /// <returns>
        ///     int Brush identifier
        /// </returns>
        public int CreateBrush(int color) {
            _brushContainer.Add(new SolidColorBrush(_device,
                new RawColor4((color >> 16) & 255L, (color >> 8) & 255L, (byte) color & 255L, (color >> 24) & 255L)));
            return _brushContainer.Count - 1;
        }

        /// <summary>
        ///     Creates a new SolidColorBrush. Make sure you applied an alpha value
        /// </summary>
        /// <param name="color">System.Drawing.Color struct</param>
        /// <returns>
        ///     int Brush identifier
        /// </returns>
        public int CreateBrush(System.Drawing.Color color) {
            if (color.A == 0) {
                color = Color.FromArgb(255, color);
            }

            _brushContainer.Add(new SolidColorBrush(_device, new RawColor4(color.R, color.G, color.B, color.A / 255.0f)));
            return _brushContainer.Count - 1;
        }

        /// <summary>
        ///     Creates a new Font
        /// </summary>
        /// <param name="fontFamilyName">i.e. Arial</param>
        /// <param name="size">size in units</param>
        /// <param name="bold">print bold text</param>
        /// <param name="italic">print italic text</param>
        /// <returns></returns>
        public int CreateFont(string fontFamilyName, float size, bool bold = false, bool italic = false) {
           _fontContainer.Add(
               new TextFormat(_fontFactory, fontFamilyName,
               bold ? FontWeight.Bold : FontWeight.Normal,
               italic ? FontStyle.Italic : FontStyle.Normal, 
               size));
            return _fontContainer.Count - 1;
        }
		public int CreateFont(string fontFamilyName, FontWeight weight, FontStyle Style, float size)
		{
			_fontContainer.Add(
				new TextFormat(_fontFactory, fontFamilyName, weight, Style,  size));
			return _fontContainer.Count - 1;
		}

		/// <summary>Initializes a new DirectXTexture from a Bitmap.</summary>
		/// <param name="bmp">The Bitmap.</param>
		public int CreateTexture(System.Drawing.Bitmap bmp)
		{
			var RawBitmap = (System.Drawing.Bitmap)bmp.Clone();
			var _width = bmp.Width;
			var _height = bmp.Height;

			System.Drawing.Rectangle sourceArea = 
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

			var bitmapProperties = new BitmapProperties(
				new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied), 0.0f, 0.0f);  // 0.0f, default dpi

			var size = new Size2(bmp.Width, bmp.Height);

			int stride = bmp.Width * sizeof(int);

			using (var tempStream = new DataStream(bmp.Height * stride, true, true))
			{
				BitmapData bitmapData = bmp.LockBits(sourceArea, ImageLockMode.ReadOnly,
													 System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

				for (int y = 0; y < bmp.Height; y++)
				{
					int offset = bitmapData.Stride * y;
					for (int x = 0; x < bmp.Width; x++)
					{
						byte b = Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte g = Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte r = Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte a = Marshal.ReadByte(bitmapData.Scan0, offset++);
						int rgba = r | (g << 8) | (b << 16) | (a << 24);
						tempStream.Write(rgba);
					}
				}
				bmp.UnlockBits(bitmapData);
				tempStream.Position = 0;
				_TextureContainer.Add( new Bitmap(_device, size, tempStream, stride, bitmapProperties));
				return _TextureContainer.Count - 1;
			}
		}


		/// <summary>
		/// Creates a new Geometry used to draw complex figures.
		/// </summary>
		/// <returns>The Geometry this method creates.</returns>
		public Common.Geometry CreateGeometry()
		{
			return new Common.Geometry(this);
		}


		#endregion

		#region Drawing Methods

		/// <summary>
		///     Draws the line.
		/// </summary>
		/// <param name="startX">The start x.</param>
		/// <param name="startY">The start y.</param>
		/// <param name="endX">The end x.</param>
		/// <param name="endY">The end y.</param>
		/// <param name="stroke">The stroke.</param>
		/// <param name="brush">The brush.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(int startX, int startY, int endX, int endY, float stroke, int brush) => _device.DrawLine(new RawVector2(startX, startY), new RawVector2(endX, endY), _brushContainer[brush], stroke);

        /// <summary>
        ///     Draws the rectangle.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="brush">The brush.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangle(int x, int y, int width, int height, float stroke, int brush) => _device.DrawRectangle(new RawRectangleF(x, y, x + width, y + height), _brushContainer[brush], stroke);

		/// <summary>
		/// Draws a rectangle by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="left">The x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="top">The y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="right">The x-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="bottom">The y-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DrawRectangle(int brush, float left, float top, float right, float bottom, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawRectangle(new RawRectangleF(left, top, right, bottom), _brushContainer[brush], stroke);
		}


		/// <summary>
		/// Draws a rectangle with rounded edges by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="left">The x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="top">The y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="right">The x-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="bottom">The y-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="radius">A value that determines radius of corners.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DrawRoundedRectangle(int brush, float left, float top, float right, float bottom, float radius, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var rect = new SharpDX.Direct2D1.RoundedRectangle()
			{
				RadiusX = radius,
				RadiusY = radius,
				Rect = new RawRectangleF(left, top, right, bottom)
			};

			_device.DrawRoundedRectangle(rect, _brushContainer[brush], stroke);
		}

		/// <summary>
		/// Draws a rectangle with rounded edges by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="rectangle">A RoundedRectangle structure including the dimension of the rounded rectangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DrawRoundedRectangle(int brush, RoundedRectangle rectangle, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawRoundedRectangle(rectangle, _brushContainer[brush], stroke);
		}


		/// <summary>
		///     Draws the circle.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="radius">The radius.</param>
		/// <param name="stroke">The stroke.</param>
		/// <param name="brush">The brush.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(int x, int y, int radius, float stroke, int brush) => _device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius, radius), _brushContainer[brush], stroke);

		/// <summary>
		/// Draws a circle with an outline around it using the given brush and dimension.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the circle.</param>
		/// <param name="x">The x-coordinate of the center of the circle.</param>
		/// <param name="y">The y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the circle.</param>
		public void OutlineCircle(int brush, int fill, float x, float y, float radius, float stroke)
		{
			if (!IsDrawing) ThrowHelper.UseBeginScene();

			var ellipse = new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radius, radius);

			_device.DrawEllipse(ellipse, _brushContainer[brush], stroke);

			float halfStroke = stroke * 0.5f;

			ellipse.RadiusX += halfStroke;
			ellipse.RadiusY += halfStroke;

			_device.DrawEllipse(ellipse, _brushContainer[brush], halfStroke);

			ellipse.RadiusX -= stroke;
			ellipse.RadiusY -= stroke;

			_device.DrawEllipse(ellipse, _brushContainer[brush], halfStroke);
		}

		/// <summary>
		/// Draws a circle with an outline around it using the given brush and dimension.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the circle.</param>
		/// <param name="location">A Point structureure which includes the x- and y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the circle.</param>
		public void OutlineCircle(int outline, int fill, Point location, float radius, float stroke)
			=> OutlineCircle(outline, fill, location.X, location.Y, radius, stroke);



		/// <summary>
		/// Draws an ellipse by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the ellipse.</param>
		/// <param name="x">The x-coordinate of the center of the ellipse.</param>
		/// <param name="y">The y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of this ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of this ellipse on the y-axis.</param>
		/// <param name="stroke">A value that determines the width/thickness of the circle.</param>
		public void DrawEllipse(int brush, float x, float y, float radiusX, float radiusY, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radiusX, radiusY), _brushContainer[brush], stroke);
		}

		/// <summary>
		/// Draws an ellipse by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the ellipse.</param>
		/// <param name="location">A Point structureure which includes the x- and y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of this ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of this ellipse on the y-axis.</param>
		/// <param name="stroke">A value that determines the width/thickness of the circle.</param>
		public void DrawEllipse(int brush, Point location, float radiusX, float radiusY, float stroke)
			=> DrawEllipse(brush, location.X, location.Y, radiusX, radiusY, stroke);

		/// <summary>
		/// Draws an ellipse with an outline around it using the given brush and dimension.
		/// </summary>
		/// <param name="forecolor">A brush that determines the color of the outline.</param>
		/// <param name="backColor">A brush that determines the color of the ellipse.</param>
		/// <param name="x">The x-coordinate of the center of the ellipse.</param>
		/// <param name="y">The y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of the ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of the ellipse on the y-axis.</param>
		/// <param name="stroke">A value that determines the width/thickness of the ellipse.</param>
		public void OutlineEllipse(int forecolor, int backColor, float x, float y, float radiusX, float radiusY, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var ellipse = new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radiusX, radiusY);

			_device.DrawEllipse(ellipse, _brushContainer[backColor], stroke);

			float halfStroke = stroke * 0.5f;

			ellipse.RadiusX += halfStroke;
			ellipse.RadiusY += halfStroke;

			_device.DrawEllipse(ellipse, _brushContainer[forecolor], halfStroke);

			ellipse.RadiusX -= stroke;
			ellipse.RadiusY -= stroke;

			_device.DrawEllipse(ellipse, _brushContainer[forecolor], halfStroke);
		}


		/// <summary>
		/// Draws an ellipse with an outline around it using the given brush and dimension.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the ellipse.</param>
		/// <param name="location">A Point structureure which includes the x- and y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of the ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of the ellipse on the y-axis.</param>
		/// <param name="stroke">A value that determines the width/thickness of the ellipse.</param>
		public void OutlineEllipse(int outline, int fill, Point location, float radiusX, float radiusY, float stroke)
			=> OutlineEllipse(outline, fill, location.X, location.Y, radiusX, radiusY, stroke);

		/// <summary>
		/// Draws a filled circle with an outline around it.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the circle.</param>
		/// <param name="x">The x-coordinate of the center of the circle.</param>
		/// <param name="y">The y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the circle.</param>
		public void OutlineFillCircle(int outline, int fill, float x, float y, float radius, float stroke)
		{
			var ellipseGeometry = new EllipseGeometry(_factory, new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radius, radius));

			var geometry = new PathGeometry(_factory);

			var sink = geometry.Open();

			ellipseGeometry.Outline(sink);

			sink.Close();

			_device.FillGeometry(geometry, _brushContainer[fill]);
			_device.DrawGeometry(geometry, _brushContainer[outline], stroke);

			sink.Dispose();
			geometry.Dispose();
			ellipseGeometry.Dispose();
		}

		/// <summary>
		/// Draws a filled circle with an outline around it.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the circle.</param>
		/// <param name="location">A Point structureure which includes the x- and y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the circle.</param>
		public void OutlineFillCircle(int outline, int fill, Point location, float radius, float stroke)
			=> OutlineFillCircle(outline, fill, location.X, location.Y, radius, stroke);

		/// <summary>
		/// Draws a filled ellipse with an outline around it.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the ellipse.</param>
		/// <param name="x">The x-coordinate of the center of the ellipse.</param>
		/// <param name="y">The y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of the ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of the ellipse on the y-axis.</param>
		/// <param name="stroke">A value that determines the width/thickness of the ellipse.</param>
		public void OutlineFillEllipse(int outline, int fill, float x, float y, float radiusX, float radiusY, float stroke)
		{
			var ellipseGeometry = new EllipseGeometry(_factory, new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radiusX, radiusY));

			var geometry = new PathGeometry(_factory);

			var sink = geometry.Open();

			ellipseGeometry.Outline(sink);

			sink.Close();

			_device.FillGeometry(geometry, _brushContainer[fill]);
			_device.DrawGeometry(geometry, _brushContainer[outline], stroke);

			sink.Dispose();
			geometry.Dispose();
			ellipseGeometry.Dispose();
		}

		/// <summary>
		/// Draws a filled ellipse with an outline around it.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the ellipse.</param>
		/// <param name="location">A Point structureure which includes the x- and y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of the ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of the ellipse on the y-axis.</param>
		/// <param name="stroke">A value that determines the width/thickness of the ellipse.</param>
		public void OutlineFillEllipse(int outline, int fill, Point location, float radiusX, float radiusY, float stroke)
			=> OutlineFillEllipse(outline, fill, location.X, location.Y, radiusX, radiusY, stroke);

		/// <summary>
		/// Draws a filled rectangle with an outline around it by using the given brush and dimension.
		/// </summary>
		/// <param name="outline">A brush that determines the color of the outline.</param>
		/// <param name="fill">A brush that determines the color of the rectangle.</param>
		/// <param name="left">The x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="top">The y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="right">The x-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="bottom">The y-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void OutlineFillRectangle(int outline, int fill, float left, float top, float right, float bottom, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var rectangleGeometry = new RectangleGeometry(_factory, new RawRectangleF(left, top, right, bottom));

			var geometry = new PathGeometry(_factory);

			var sink = geometry.Open();

			rectangleGeometry.Widen(stroke, sink);
			rectangleGeometry.Outline(sink);

			sink.Close();

			_device.FillGeometry(geometry, _brushContainer[fill]);
			_device.DrawGeometry(geometry, _brushContainer[outline], stroke);

			sink.Dispose();
			geometry.Dispose();
			rectangleGeometry.Dispose();
		}


		/// <summary>
		///     Draws the box2 d.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="stroke">The stroke.</param>
		/// <param name="brush">The brush.</param>
		/// <param name="interiorBrush">The interior brush.</param>
		public void DrawBox2D(int x, int y, int width, int height, float stroke, int brush, int interiorBrush) {
            _device.DrawRectangle(new RawRectangleF(x, y, x + width, y + height), _brushContainer[brush], stroke);
            _device.FillRectangle(new RawRectangleF(x + stroke, y + stroke, x + width - stroke, y + height - stroke),
                _brushContainer[interiorBrush]);
        }

        /// <summary>
        ///     Draws the box3 d.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="length">The length.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="interiorBrush">The interior brush.</param>
        public void DrawBox3D(int x, int y, int width, int height, int length, float stroke, int brush,
            int interiorBrush) {
            var first = new RawRectangleF(x, y, x + width, y + height);
            var second = new RawRectangleF(x + length, y - length, first.Right + length, first.Bottom - length);

            var lineStart = new RawVector2(x, y);
            var lineEnd = new RawVector2(second.Left, second.Top);

            _device.DrawRectangle(first, _brushContainer[brush], stroke);
            _device.DrawRectangle(second, _brushContainer[brush], stroke);

            _device.FillRectangle(first, _brushContainer[interiorBrush]);
            _device.FillRectangle(second, _brushContainer[interiorBrush]);

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);

            lineStart.X += width;
            lineEnd.X = lineStart.X + length;

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);

            lineStart.Y += height;
            lineEnd.Y += height;

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);

            lineStart.X -= width;
            lineEnd.X -= width;

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);
        }

        /// <summary>
        ///     Draws the rectangle3 d.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="length">The length.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="brush">The brush.</param>
        public void DrawRectangle3D(int x, int y, int width, int height, int length, float stroke, int brush) {
            var first = new RawRectangleF(x, y, x + width, y + height);
            var second = new RawRectangleF(x + length, y - length, first.Right + length, first.Bottom - length);

            var lineStart = new RawVector2(x, y);
            var lineEnd = new RawVector2(second.Left, second.Top);

            _device.DrawRectangle(first, _brushContainer[brush], stroke);
            _device.DrawRectangle(second, _brushContainer[brush], stroke);

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);

            lineStart.X += width;
            lineEnd.X = lineStart.X + length;

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);

            lineStart.Y += height;
            lineEnd.Y += height;

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);

            lineStart.X -= width;
            lineEnd.X -= width;

            _device.DrawLine(lineStart, lineEnd, _brushContainer[brush], stroke);
        }

        /// <summary>
        ///     Draws the plus.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="length">The length.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="brush">The brush.</param>
        public void DrawPlus(int x, int y, int length, float stroke, int brush) {
            var first = new RawVector2(x - length, y);
            var second = new RawVector2(x + length, y);

            var third = new RawVector2(x, y - length);
            var fourth = new RawVector2(x, y + length);

            _device.DrawLine(first, second, _brushContainer[brush], stroke);
            _device.DrawLine(third, fourth, _brushContainer[brush], stroke);
        }
		public void DrawTargetReticle_1(int x, int y, int length, int pad,  float stroke, int brush)
		{
			_device.AntialiasMode = this.Antialias;

			#region Horizontal Lines

			//LEFT SIDE: Linea delgada
			var HLine_A = new RawVector2(x - pad - length, y);
            var HLine_B = new RawVector2(x - pad, y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke);
			//LEFT SIDE: Linea Gruesa
			HLine_B = new RawVector2(x - pad - (length / 2), y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke + 2);


			//RIGHT SIDE: Linea delgada
			HLine_A = new RawVector2(x + pad, y);
			HLine_B = new RawVector2(x + pad + length, y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke);
			//RIGHT SIDE:  Linea Gruesa
			HLine_A = new RawVector2(x + pad + (length /2), y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke + 2);

			#endregion

			#region Vertical Lines

            //TOP SIDE: linea delgada
			var VLine_A = new RawVector2(x, y - pad - length);
			var VLine_B = new RawVector2(x, y - pad);
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke);
			//TOP SIDE: linea Gruesa
			VLine_B = new RawVector2(x, y - pad - (length /2));
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke + 2);

            //BOTTOM SIDE: linea delgada
			VLine_A = new RawVector2(x, y + pad); 
			VLine_B = new RawVector2(x, y + pad + length);
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke);
			//BOTTOM SIDE: linea gruesa
			VLine_A = new RawVector2(x, y + pad + (length / 2));
			_device.DrawLine(VLine_B, VLine_A, _brushContainer[brush], stroke + 2);

			#endregion
		}
		public void DrawTargetReticle_2(int x, int y, int length, int pad, float stroke, int brush)
		{
			_device.AntialiasMode = this.Antialias;

			#region Horizontal Lines

			//LEFT SIDE: Linea delgada
			var HLine_A = new RawVector2(x - pad - length, y);
			var HLine_B = new RawVector2(x - pad, y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke);
			//LEFT SIDE: Linea Gruesa
			HLine_B = new RawVector2(x - pad - (length / 2), y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke + 2);


			//RIGHT SIDE: Linea delgada
			HLine_A = new RawVector2(x + pad, y);
			HLine_B = new RawVector2(x + pad + length, y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke);
			//RIGHT SIDE:  Linea Gruesa
			HLine_A = new RawVector2(x + pad + (length / 2), y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke + 2);

			#endregion

			#region Vertical Lines

			//TOP SIDE: linea delgada
			var VLine_A = new RawVector2(x, y - pad - length);
			var VLine_B = new RawVector2(x, y - pad);
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke);
			//TOP SIDE: linea Gruesa
			VLine_B = new RawVector2(x, y - pad - (length / 2));
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke + 2);

			//BOTTOM SIDE: linea delgada
			VLine_A = new RawVector2(x, y + pad);
			VLine_B = new RawVector2(x, y + pad + length);
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke);
			//BOTTOM SIDE: linea gruesa
			VLine_A = new RawVector2(x, y + pad + (length / 2));
			_device.DrawLine(VLine_B, VLine_A, _brushContainer[brush], stroke + 2);

			#endregion

			#region Outer Circle

			_device.DrawEllipse(new Ellipse(new RawVector2(x, y), length - 2, length -2), _brushContainer[brush], stroke);
			//_device.FillRectangle(new RawRectangleF(x-1, y-1, x+1, y+1), _brushContainer[brush]);

			#endregion
		}
		public void DrawTargetReticle_3(int x, int y, int length, int pad, float stroke, int brush)
		{
            _device.AntialiasMode = this.Antialias;
            _device.DotsPerInch = new Size2F(120,120); //<- 96dpi (default), 120dpi, 144dpi

			#region Horizontal Lines

			//LEFT SIDE: Linea delgada
			var HLine_A = new RawVector2(x - pad - length, y);
			var HLine_B = new RawVector2(x - pad, y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke);

			//RIGHT SIDE: Linea delgada
			HLine_A = new RawVector2(x + pad, y);
			HLine_B = new RawVector2(x + pad + length, y);
			_device.DrawLine(HLine_A, HLine_B, _brushContainer[brush], stroke);

			#endregion

			#region Vertical Lines

			//TOP SIDE: linea delgada
			var VLine_A = new RawVector2(x, y - pad - length);
			var VLine_B = new RawVector2(x, y - pad);
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke);

			//BOTTOM SIDE: linea delgada
			VLine_A = new RawVector2(x, y + pad);
			VLine_B = new RawVector2(x, y + pad + length);
			_device.DrawLine(VLine_A, VLine_B, _brushContainer[brush], stroke);

			#endregion

			#region Outer Circle

			_device.DrawEllipse(new Ellipse(new RawVector2(x, y), length, length), _brushContainer[brush], stroke);
			_device.FillRectangle(new RawRectangleF(x-1, y-1, x+1, y+1), _brushContainer[brush]);

			#endregion
		}

		/// <summary>
		///     Draws the edge.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="length">The length.</param>
		/// <param name="stroke">The stroke.</param>
		/// <param name="brush">The brush.</param>
		public void DrawEdge(int x, int y, int width, int height, int length, float stroke, int brush) //geht
        {
            var first = new RawVector2(x, y);
            var second = new RawVector2(x, y + length);
            var third = new RawVector2(x + length, y);

            _device.DrawLine(first, second, _brushContainer[brush], stroke);
            _device.DrawLine(first, third, _brushContainer[brush], stroke);

            first.Y += height;
            second.Y = first.Y - length;
            third.Y = first.Y;
            third.X = first.X + length;

            _device.DrawLine(first, second, _brushContainer[brush], stroke);
            _device.DrawLine(first, third, _brushContainer[brush], stroke);

            first.X = x + width;
            first.Y = y;
            second.X = first.X - length;
            second.Y = first.Y;
            third.X = first.X;
            third.Y = first.Y + length;

            _device.DrawLine(first, second, _brushContainer[brush], stroke);
            _device.DrawLine(first, third, _brushContainer[brush], stroke);

            first.Y += height;
            second.X += length;
            second.Y = first.Y - length;
            third.Y = first.Y;
            third.X = first.X - length;

            _device.DrawLine(first, second, _brushContainer[brush], stroke);
            _device.DrawLine(first, third, _brushContainer[brush], stroke);
        }

        /// <summary>
        ///     Draws the bar h.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="value">The value.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="interiorBrush">The interior brush.</param>
        public void DrawBarH(int x, int y, int width, int height, float value, float stroke, int brush,
            int interiorBrush) {
            var first = new RawRectangleF(x, y, x + width, y + height);

            _device.DrawRectangle(first, _brushContainer[brush], stroke);

            if (Math.Abs(value) < 0) {
                return;
            }

            first.Top += height - height / 100.0f * value;

            _device.FillRectangle(first, _brushContainer[interiorBrush]);
        }

        /// <summary>
        ///     Draws the bar v.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="value">The value.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="interiorBrush">The interior brush.</param>
        public void DrawBarV(int x, int y, int width, int height, float value, float stroke, int brush,
            int interiorBrush) {
            var first = new RawRectangleF(x, y, x + width, y + height);

            _device.DrawRectangle(first, _brushContainer[brush], stroke);

            if (Math.Abs(value) < 0) {
                return;
            }

            first.Right -= width - width / 100.0f * value;

            _device.FillRectangle(first, _brushContainer[interiorBrush]);
        }

        /// <summary>
        ///     Fills the rectangle.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="brush">The brush.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRectangle(int x, int y, int width, int height, int brush) => _device.FillRectangle(new RawRectangleF(x, y, x + width, y + height), _brushContainer[brush]);

		/// <summary>
		/// Fills a rounded rectangle using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="left">The x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="top">The y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="right">The x-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="bottom">The y-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="radius">A value that determines radius of corners.</param>
		public void FillRoundedRectangle(int brush, float left, float top, float right, float bottom, float radius)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var rect = new SharpDX.Direct2D1.RoundedRectangle()
			{
				RadiusX = radius,
				RadiusY = radius,
				Rect = new RawRectangleF(left, top, right, bottom)
			};

			_device.FillRoundedRectangle(rect, _brushContainer[brush]);
		}

		/// <summary>
		///     Fills the circle.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="radius">The radius.</param>
		/// <param name="brush">The brush.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(int x, int y, int radius, int brush) => _device.FillEllipse(new Ellipse(new RawVector2(x, y), radius, radius), _brushContainer[brush]);

		/// <summary>
		/// Fills an ellipse by using the given brush and dimesnion.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the ellipse.</param>
		/// <param name="x">The x-coordinate of the center of the ellipse.</param>
		/// <param name="y">The y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of the ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of the ellipse on the y-axis.</param>
		public void FillEllipse(int brush, float x, float y, float radiusX, float radiusY)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radiusX, radiusY), _brushContainer[brush]);
		}

		/// <summary>
		/// Fills an ellipse by using the given brush and dimesnion.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the ellipse.</param>
		/// <param name="location">A Point structureure which includes the x- and y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of the ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of the ellipse on the y-axis.</param>
		public void FillEllipse(int brush, Point location, float radiusX, float radiusY)
			=> FillEllipse(brush, location.X, location.Y, radiusX, radiusY);


		/// <summary>
		/// Draws a circle with a dashed line by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the circle.</param>
		/// <param name="x">The x-coordinate of the center of the circle.</param>
		/// <param name="y">The y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the circle.</param>
		public void DashedCircle(int brush, float x, float y, float radius, float stroke)
		{
			//if (!IsDrawing) ThrowHelper.UseBeginScene();

			_device.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radius, radius), _brushContainer[brush], stroke, _strokeStyle);
		}

		/// <summary>
		/// Draws an ellipse with a dashed line by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the ellipse.</param>
		/// <param name="x">The x-coordinate of the center of the ellipse.</param>
		/// <param name="y">The y-coordinate of the center of the ellipse.</param>
		/// <param name="radiusX">The radius of the ellipse on the x-axis.</param>
		/// <param name="radiusY">The radius of the ellipse on the y-axis.</param>
		/// <param name="stroke">A value that determines the width/thickness of the ellipse.</param>
		public void DashedEllipse(int brush, float x, float y, float radiusX, float radiusY, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(x, y), radiusX, radiusY), _brushContainer[brush], stroke, _strokeStyle);
		}

		/// <summary>
		/// Draws a triangle with dashed lines using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the triangle.</param>
		/// <param name="aX">The x-coordinate lower-left corner of the triangle.</param>
		/// <param name="aY">The y-coordinate lower-left corner of the triangle.</param>
		/// <param name="bX">The x-coordinate lower-right corner of the triangle.</param>
		/// <param name="bY">The y-coordinate lower-right corner of the triangle.</param>
		/// <param name="cX">The x-coordinate upper-center corner of the triangle.</param>
		/// <param name="cY">The y-coordinate upper-center corner of the triangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedTriangle(int brush, float aX, float aY, float bX, float bY, float cX, float cY, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var geometry = new PathGeometry(_factory);

			var sink = geometry.Open();

			sink.BeginFigure(new RawVector2(aX, aY), FigureBegin.Hollow);
			sink.AddLine(new RawVector2(bX, bY));
			sink.AddLine(new RawVector2(cX, cY));
			sink.EndFigure(FigureEnd.Closed);

			sink.Close();

			_device.DrawGeometry(geometry, _brushContainer[brush], stroke, _strokeStyle);

			sink.Dispose();
			geometry.Dispose();
		}

		/// <summary>
		/// Draws a triangle with dashed lines using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the triangle.</param>
		/// <param name="a">A Point structure including the coordinates of the lower-left corner of the triangle.</param>
		/// <param name="b">A Point structure including the coordinates of the lower-right corner of the triangle.</param>
		/// <param name="c">A Point structure including the coordinates of the upper-center corner of the triangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedTriangle(int brush, Point a, Point b, Point c, float stroke)
			=> DashedTriangle(brush, a.X, a.Y, b.X, b.Y, c.X, c.Y, stroke);

		/// <summary>
		/// Draws a Geometry with dashed lines using the given brush and thickness.
		/// </summary>
		/// <param name="geometry">The Geometry to be drawn.</param>
		/// <param name="brush">A brush that determines the color of the text.</param>
		/// <param name="stroke">A value that determines the width/thickness of the lines.</param>
		public void DashedGeometry(Common.Geometry geometry, int brush, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawGeometry(geometry, _brushContainer[brush], stroke, _strokeStyle);
		}

		/// <summary>
		/// Draws a dashed line at the given start and end point.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the line.</param>
		/// <param name="startX">The start position of the line on the x-axis</param>
		/// <param name="startY">The start position of the line on the y-axis</param>
		/// <param name="endX">The end position of the line on the x-axis</param>
		/// <param name="endY">The end position of the line on the y-axis</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedLine(int brush, float startX, float startY, float endX, float endY, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawLine(new RawVector2(startX, startY), new RawVector2(endX, endY), _brushContainer[brush], stroke, _strokeStyle);
		}

		/// <summary>
		/// Draws a rectangle with dashed lines by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="left">The x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="top">The y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="right">The x-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="bottom">The y-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedRectangle(int brush, float left, float top, float right, float bottom, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawRectangle(new RawRectangleF(left, top, right, bottom), _brushContainer[brush], stroke, _strokeStyle);
		}

		/// <summary>
		/// Draws a rectangle with dashed lines by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="rectangle">A Rectangle structure that determines the boundaries of the rectangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedRectangle(int brush, Rectangle rectangle, float stroke)
			=> DashedRectangle(brush, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, stroke);

		/// <summary>
		/// Draws a rectangle with rounded edges and dashed lines by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="left">The x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="top">The y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="right">The x-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="bottom">The y-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="radius">A value that determines radius of corners.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedRoundedRectangle(int brush, float left, float top, float right, float bottom, float radius, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var rect = new SharpDX.Direct2D1.RoundedRectangle()
			{
				RadiusX = radius,
				RadiusY = radius,
				Rect = new RawRectangleF(left, top, right, bottom)
			};

			_device.DrawRoundedRectangle(rect, _brushContainer[brush], stroke, _strokeStyle);
		}

		/// <summary>
		/// Draws a rectangle with rounded edges and dashed lines by using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the rectangle.</param>
		/// <param name="rectangle">A RoundedRectangle structure including the dimension of the rounded rectangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedRoundedRectangle(int brush, RoundedRectangle rectangle, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawRoundedRectangle(rectangle, _brushContainer[brush], stroke, _strokeStyle);
		}



		/// <summary>
		/// Draws a dashed line at the given start and end point.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the line.</param>
		/// <param name="start">A Point structure including the start position of the line.</param>
		/// <param name="end">A Point structure including the end position of the line.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DashedLine(int brush, Point start, Point end, float stroke)
			=> DashedLine(brush, start.X, start.Y, end.X, end.Y, stroke);

		/// <summary>
		/// Draws a pointed line using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the arrow line.</param>
		/// <param name="startX">The x-coordinate of the start of the arrow line. (the direction it points to)</param>
		/// <param name="startY">The y-coordinate of the start of the arrow line. (the direction it points to)</param>
		/// <param name="endX">The x-coordinate of the end of the arrow line.</param>
		/// <param name="endY">The y-coordinate of the end of the arrow line.</param>
		/// <param name="size">A value determining the size of the arrow line.</param>
		public void DrawArrowLine(int brush, float startX, float startY, float endX, float endY, float size)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			float deltaX = endX >= startX ? endX - startX : startX - endX;
			float deltaY = endY >= startY ? endY - startY : startY - endY;

			float length = (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

			float xm = length - size;
			float xn = xm;

			float ym = size;
			float yn = -ym;

			float sin = deltaY / length;
			float cos = deltaX / length;

			float x = (xm * cos) - (ym * sin) + endX;
			ym = (xm * sin) + (ym * cos) + endY;
			xm = x;

			x = (xn * cos) - (yn * sin) + endX;
			yn = (xn * sin) + (yn * cos) + endY;
			xn = x;

			FillTriangle(brush, startX, startY, xm, ym, xn, yn);
		}

		/// <summary>
		/// Draws a pointed line using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the arrow line.</param>
		/// <param name="start">A Point structure including the start position of the arrow line. (the direction it points to)</param>
		/// <param name="end">A Point structure including the end position of the arrow line. (the direction it points to)</param>
		/// <param name="size">A value determining the size of the arrow line.</param>
		public void DrawArrowLine(int brush, Point start, Point end, float size)
			=> DrawArrowLine(brush, start.X, start.Y, end.X, end.Y, size);

		/// <summary>
		///     Bordereds the line.
		/// </summary>
		/// <param name="startX">The start x.</param>
		/// <param name="startY">The start y.</param>
		/// <param name="endX">The end x.</param>
		/// <param name="endY">The end y.</param>
		/// <param name="stroke">The stroke.</param>
		/// <param name="brush">The brush.</param>
		/// <param name="borderBrush">The border brush.</param>
		public void BorderedLine(int startX, int startY, int endX, int endY, float stroke, int brush, int borderBrush) {
            _device.DrawLine(new RawVector2(startX, startY), new RawVector2(endX, endY), _brushContainer[brush], stroke);

            _device.DrawLine(new RawVector2(startX, startY - stroke), new RawVector2(endX, endY - stroke),
                _brushContainer[borderBrush], stroke);
            _device.DrawLine(new RawVector2(startX, startY + stroke), new RawVector2(endX, endY + stroke),
                _brushContainer[borderBrush], stroke);

            _device.DrawLine(new RawVector2(startX - stroke / 2, startY - stroke * 1.5f),
                new RawVector2(startX - stroke / 2, startY + stroke * 1.5f), _brushContainer[borderBrush], stroke);
            _device.DrawLine(new RawVector2(endX - stroke / 2, endY - stroke * 1.5f),
                new RawVector2(endX - stroke / 2, endY + stroke * 1.5f), _brushContainer[borderBrush], stroke);
        }

        /// <summary>
        ///     Bordereds the rectangle.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="borderStroke">The border stroke.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="borderBrush">The border brush.</param>
        public void BorderedRectangle(int x, int y, int width, int height, float stroke, float borderStroke, int brush,
            int borderBrush) {
            _device.DrawRectangle(
                new RawRectangleF(x - (stroke - borderStroke), y - (stroke - borderStroke),
                    x + width + stroke - borderStroke, y + height + stroke - borderStroke), _brushContainer[borderBrush],
                borderStroke);

            _device.DrawRectangle(new RawRectangleF(x, y, x + width, y + height), _brushContainer[brush], stroke);

            _device.DrawRectangle(
                new RawRectangleF(x + (stroke - borderStroke), y + (stroke - borderStroke),
                    x + width - stroke + borderStroke, y + height - stroke + borderStroke), _brushContainer[borderBrush],
                borderStroke);
        }

        /// <summary>
        ///     Bordereds the circle.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="stroke">The stroke.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="borderBrush">The border brush.</param>
        public void BorderedCircle(int x, int y, int radius, float stroke, int brush, int borderBrush) {
            _device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius + stroke, radius + stroke),
                _brushContainer[borderBrush], stroke);

            _device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius, radius), _brushContainer[brush], stroke);

            _device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius - stroke, radius - stroke),
                _brushContainer[borderBrush], stroke);
        }

		/// <summary>
		/// Draws a triangle using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the triangle.</param>
		/// <param name="aX">The x-coordinate lower-left corner of the triangle.</param>
		/// <param name="aY">The y-coordinate lower-left corner of the triangle.</param>
		/// <param name="bX">The x-coordinate lower-right corner of the triangle.</param>
		/// <param name="bY">The y-coordinate lower-right corner of the triangle.</param>
		/// <param name="cX">The x-coordinate upper-center corner of the triangle.</param>
		/// <param name="cY">The y-coordinate upper-center corner of the triangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DrawTriangle(int brush, float aX, float aY, float bX, float bY, float cX, float cY, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var geometry = new PathGeometry(_factory);

			var sink = geometry.Open();

			sink.BeginFigure(new RawVector2(aX, aY), FigureBegin.Hollow);
			sink.AddLine(new RawVector2(bX, bY));
			sink.AddLine(new RawVector2(cX, cY));
			sink.EndFigure(FigureEnd.Closed);

			sink.Close();

			_device.DrawGeometry(geometry, _brushContainer[brush], stroke);

			sink.Dispose();
			geometry.Dispose();
		}

		/// <summary>
		/// Draws a triangle using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the triangle.</param>
		/// <param name="a">A Point structure including the coordinates of the lower-left corner of the triangle.</param>
		/// <param name="b">A Point structure including the coordinates of the lower-right corner of the triangle.</param>
		/// <param name="c">A Point structure including the coordinates of the upper-center corner of the triangle.</param>
		/// <param name="stroke">A value that determines the width/thickness of the line.</param>
		public void DrawTriangle(int brush, Point a, Point b, Point c, float stroke)
			=> DrawTriangle(brush, a.X, a.Y, b.X, b.Y, c.X, c.Y, stroke);



		

        /// <summary>
        ///     Do not buffer text if you draw i.e. FPS. Use buffer for player names, rank....
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="bufferText">if set to <c>true</c> [buffer text].</param>
        public void DrawText(string text, int font, int brush, int x, int y, bool bufferText = true) {
			// _device.AntialiasMode = AntialiasMode.Aliased;
			//_device.DotsPerInch = new Size2F(144, 144); //<- 96dpi (default), 120dpi, 144dpi

			try
			{
				if (bufferText)
				{
					var bufferPos = -1;
					_fontContainer[font].ParagraphAlignment = ParagraphAlignment.Near;
					_fontContainer[font].TextAlignment = TextAlignment.Justified;
					_fontContainer[font].WordWrapping = WordWrapping.Wrap;

					_device.TextAntialiasMode = TextAntialiasMode.Aliased;

					for (var i = 0; i < _layoutContainer.Count; i++)
					{
						if (_layoutContainer[i].Text.Length != text.Length || _layoutContainer[i].Text != text)
						{
							continue;
						}
						bufferPos = i;
						break;
					}

					if (bufferPos == -1)
					{
						_layoutContainer.Add(new TextLayoutBuffer(text,
							new TextLayout(_fontFactory, text, _fontContainer[font], float.MaxValue, float.MaxValue)));
						bufferPos = _layoutContainer.Count - 1;
					}

					_device.DrawTextLayout(new RawVector2(x, y), _layoutContainer[bufferPos].TextLayout,
						_brushContainer[brush], DrawTextOptions.Clip);
				}
				else
				{
					var layout = new TextLayout(_fontFactory, text, _fontContainer[font], float.MaxValue, float.MaxValue);
					_device.DrawTextLayout(new RawVector2(x, y), layout, _brushContainer[brush], DrawTextOptions.Clip);
					layout.Dispose();
				}
			}
			catch { }           
        }
		public void DrawTextBox(string text, int font, System.Drawing.Rectangle Box, int brush)
        {
			_device.TextAntialiasMode = TextAntialiasMode.Aliased;

			_fontContainer[font].ParagraphAlignment = ParagraphAlignment.Near;
			_fontContainer[font].TextAlignment = TextAlignment.Justified;
			_fontContainer[font].WordWrapping = WordWrapping.Wrap;

			var TF = new TextFormat(_fontFactory, 
                _fontContainer[font].FontFamilyName, 
                _fontContainer[font].FontWeight, 
                _fontContainer[font].FontStyle, 
                _fontContainer[font].FontSize 
            );
			RawRectangleF BoxF = new RawRectangleF(Box.X, Box.Y, Box.X +  Box.Width, Box.Y + Box.Height);
			
			_device.DrawText(text, TF, BoxF, _brushContainer[brush], DrawTextOptions.Clip);
		}

		/// <summary>
		/// Draws a string with a background box in behind using the given font, size and position.
		/// </summary>
		/// <param name="font">The Font to be used to draw the string.</param>
		/// <param name="fontSize">The size of the Font. (does not need to be the same as in Font.FontSize)</param>
		/// <param name="brush">A brush that determines the color of the text.</param>
		/// <param name="background">A brush that determines the color of the background box.</param>
		/// <param name="x">The x-coordinate of the starting position.</param>
		/// <param name="y">The y-coordinate of the starting position.</param>
		/// <param name="text">The string to be drawn.</param>
		public void DrawTextWithBackground(int font, int brush, int background, System.Drawing.Rectangle Box, string text)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			if (text == null) throw new ArgumentNullException(nameof(text));
			if (text.Length == 0) return;

			float clippedWidth =	Box.X < 0 ? Box.Width + Box.X : Box.Width - Box.X;
			float clippedHeight =	Box.Y < 0 ? Box.Height + Box.Y : Box.Height - Box.Y;

			float fontSize = _fontContainer[font].FontSize;
			var TF = new TextFormat(_fontFactory,
				_fontContainer[font].FontFamilyName,
				_fontContainer[font].FontWeight,
				_fontContainer[font].FontStyle,
				_fontContainer[font].FontSize
			);

			if (clippedWidth <= fontSize) clippedWidth = Box.Width;
			if (clippedHeight <= fontSize) clippedHeight = Box.Height;

			var layout = new TextLayout(_fontFactory, text, TF, clippedWidth, clippedHeight);

			if (fontSize != _fontContainer[font].FontSize)
			{
				layout.SetFontSize(fontSize, new TextRange(0, text.Length));
			}

			float modifier = layout.FontSize * 0.25f;
			var rectangle = new RawRectangleF(Box.X - modifier, Box.Y - modifier, Box.X + layout.Metrics.Width + modifier, Box.Y + layout.Metrics.Height + modifier);

			_device.FillRectangle(rectangle, _brushContainer[background]);
			_device.TextAntialiasMode = TextAntialiasMode.Cleartype;
			_device.DrawTextLayout(new RawVector2(Box.X, Box.Y), layout, _brushContainer[brush], DrawTextOptions.Clip);

			layout.Dispose();
		}
		public void DrawTextWithBackground_OLD(int font, int brush, int background, float x, float y, string text)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			if (text == null) throw new ArgumentNullException(nameof(text));
			if (text.Length == 0) return;

			float clippedWidth = x < 0 ? Width + x : Width - x;
			float clippedHeight = y < 0 ? Height + y : Height - y;

			float fontSize = _fontContainer[font].FontSize;
			var TF = new TextFormat(_fontFactory,
				_fontContainer[font].FontFamilyName,
				_fontContainer[font].FontWeight,
				_fontContainer[font].FontStyle,
				_fontContainer[font].FontSize
			);

			if (clippedWidth <= fontSize) clippedWidth = Width;
			if (clippedHeight <= fontSize) clippedHeight = Height;

			var layout = new TextLayout(_fontFactory, text, TF, clippedWidth, clippedHeight);

			if (fontSize != _fontContainer[font].FontSize)
			{
				layout.SetFontSize(fontSize, new TextRange(0, text.Length));
			}

			float modifier = layout.FontSize * 0.25f;
			var rectangle = new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier);

			_device.FillRectangle(rectangle, _brushContainer[background]);
			_device.TextAntialiasMode = TextAntialiasMode.Cleartype;
			_device.DrawTextLayout(new RawVector2(x, y), layout, _brushContainer[brush], DrawTextOptions.Clip);

			layout.Dispose();
		}


		/// <summary>
		/// Measures the specified string when drawn with the specified Font.
		/// </summary>
		/// <param name="font">Font that defines the text format of the string.</param>
		/// <param name="fontSize">The size of the Font. (does not need to be the same as in Font.FontSize)</param>
		/// <param name="text">String to measure.</param>
		/// <returns>This method returns a Point containing the width (x) and height (y) of the given text.</returns>
		public System.Drawing.Point MeasureString(int font, string text)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			if (text == null) throw new ArgumentNullException(nameof(text));
			if (text.Length == 0) return default;

			float fontSize = _fontContainer[font].FontSize;
			var TF = new TextFormat(_fontFactory,
				_fontContainer[font].FontFamilyName,
				_fontContainer[font].FontWeight,
				_fontContainer[font].FontStyle,
				_fontContainer[font].FontSize
			);

			var layout = new TextLayout(_fontFactory, text, TF, Width, Height);

			if (fontSize != _fontContainer[font].FontSize)
			{
				layout.SetFontSize(fontSize, new TextRange(0, text.Length));
			}

			var result = new System.Drawing.Point((int)layout.Metrics.Width, (int)layout.Metrics.Height);

			layout.Dispose();

			return result;
		}


		/// <summary>Draws the Outline of the specified Geometry</summary>
		/// <param name="pGeometry">https://michel-pi.github.io/GameOverlay.Net/api/GameOverlay.Drawing.Geometry.html</param>
		/// <param name="brush"></param>
		/// <param name="stroke"></param>
		public void DrawGeometry(SharpDX.Direct2D1.Geometry pGeometry, int brush, float stroke, StrokeStyle _StrokeStyle = null)
		{
			if(_StrokeStyle is null) _StrokeStyle = new StrokeStyle(
				this._factory, new StrokeStyleProperties() { StartCap = CapStyle.Round, EndCap = CapStyle.Round });

            _device.DrawGeometry(pGeometry, _brushContainer[brush], stroke, _StrokeStyle);
		}

		/// <summary>
		/// Draws a Geometry using the given brush and thickness.
		/// </summary>
		/// <param name="geometry">The Geometry to be drawn.</param>
		/// <param name="brush">A brush that determines the color of the text.</param>
		/// <param name="stroke">A value that determines the width/thickness of the lines.</param>
		public void DrawGeometry(Common.Geometry geometry, int brush, float stroke)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawGeometry(geometry, _brushContainer[brush], stroke);
		}

		/// <summary>
		/// Fills a triangle using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the triangle.</param>
		/// <param name="aX">The x-coordinate lower-left corner of the triangle.</param>
		/// <param name="aY">The y-coordinate lower-left corner of the triangle.</param>
		/// <param name="bX">The x-coordinate lower-right corner of the triangle.</param>
		/// <param name="bY">The y-coordinate lower-right corner of the triangle.</param>
		/// <param name="cX">The x-coordinate upper-center corner of the triangle.</param>
		/// <param name="cY">The y-coordinate upper-center corner of the triangle.</param>
		public void FillTriangle(int brush, float aX, float aY, float bX, float bY, float cX, float cY)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			var geometry = new PathGeometry(_factory);

			var sink = geometry.Open();

			sink.BeginFigure(new RawVector2(aX, aY), FigureBegin.Filled);
			sink.AddLine(new RawVector2(bX, bY));
			sink.AddLine(new RawVector2(cX, cY));
			sink.EndFigure(FigureEnd.Closed);

			sink.Close();

			_device.FillGeometry(geometry, _brushContainer[brush]);

			sink.Dispose();
			geometry.Dispose();
		}

		/// <summary>
		/// Fills a triangle using the given brush and dimension.
		/// </summary>
		/// <param name="brush">A brush that determines the color of the triangle.</param>
		/// <param name="a">A Point structure including the coordinates of the lower-left corner of the triangle.</param>
		/// <param name="b">A Point structure including the coordinates of the lower-right corner of the triangle.</param>
		/// <param name="c">A Point structure including the coordinates of the upper-center corner of the triangle.</param>
		public void FillTriangle(int brush, Point a, Point b, Point c)
			=> FillTriangle(brush, a.X, a.Y, b.X, b.Y, c.X, c.Y);

		/// <summary>Paints the Interior of the specified Geometry.</summary>
		/// <param name="pGeometry"></param>
		/// <param name="brush"></param>
		public void FillGeometry(SharpDX.Direct2D1.Geometry pGeometry, int brush)
        {
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();
			_device.FillGeometry(pGeometry, _brushContainer[brush]);
		}


		/// <summary>Draw a pre-loaded Texture on the Destination Box.</summary>
		/// <param name="TextureIndex">Index of the preloaded texture</param>
		/// <param name="box">Location and Size for the image.</param>
		/// <param name="pOpacity">Transparency: 0.0-1.0</param>
		public void DrawTexture(int TextureIndex, System.Drawing.Rectangle box, float pOpacity)
		{
			RawRectangleF RF = new RawRectangleF(box.X, box.Y, box.X + box.Width, box.Y + box.Height);

			_device.DrawBitmap(_TextureContainer[TextureIndex], RF, pOpacity, BitmapInterpolationMode.Linear, null);
		}

		/// <summary>
		/// Draws an image to the given position and optional applies an alpha value.
		/// </summary>
		/// <param name="image">The Image to be drawn.</param>
		/// <param name="x">The x-coordinate upper-left corner of the image.</param>
		/// <param name="y">The y-coordinate upper-left corner of the image.</param>
		/// <param name="opacity">A value indicating the opacity of the image. (alpha)</param>
		public void DrawImage(Overlay.NET.Common.Image image, float x, float y, float opacity = 1.0f)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			float destRight = x + image.Bitmap.PixelSize.Width;
			float destBottom = y + image.Bitmap.PixelSize.Height;

			_device.DrawBitmap(
				image,
				new RawRectangleF(x, y, destRight, destBottom),
				opacity,
				BitmapInterpolationMode.Linear, 
				null);
		}

		/// <summary>Draws an image to the given position and optional applies an alpha value.</summary>
		/// <param name="image">The Image to be drawn.</param>
		/// <param name="box">Location and Size of the Image</param>
		/// <param name="opacity">A value indicating the opacity of the image (alpha). Value Range: [0.0 to 1.0]</param>
		/// <param name="linearScale"></param>
		public void DrawImage(SharpDX.Direct2D1.Bitmap image, System.Drawing.Rectangle box, float opacity = 1.0f, bool linearScale = true)
		{
			if (!IsDrawing) throw ThrowHelper.UseBeginScene();

			_device.DrawBitmap(
				image,
				new RawRectangleF(box.X, box.Y, box.Right, box.Bottom),
				opacity,
				linearScale ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor,
				new RawRectangleF(0, 0, image.PixelSize.Width, image.PixelSize.Height));
		}

		public SharpDX.Direct2D1.Bitmap BitmapFromBytes(byte[] bytes, RawRectangle Box, float dpi = 0.0f)
		{
			var BProps = new BitmapProperties() 
			{ 
				PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Straight),
				DpiX = dpi,
				DpiY = dpi
			};

			var pixelRect = new RawRectangle(Box.Left, Box.Top, Box.Right, Box.Bottom);
			var pixelSize = new Size2(pixelRect.Right - pixelRect.Left, pixelRect.Bottom - pixelRect.Top);
			var newBitmap = new	Bitmap(_device, pixelSize, BProps);

			Overlay.NET.Common.Image image = new Image(_device, bytes);

			newBitmap.CopyFromBitmap(image.Bitmap, new RawPoint(0, 0), pixelRect);
			return newBitmap;
		}

		/// <summary>Gets a Direct2D Bitmap from an Standard Bitmap.</summary>
		/// <param name="bmp">Image to convert.</param>
		public SharpDX.Direct2D1.Bitmap BitmapFromBitmap(System.Drawing.Bitmap bmp)
		{
			var RawBitmap = (System.Drawing.Bitmap)bmp.Clone();
			var _width = bmp.Width;
			var _height = bmp.Height;

			System.Drawing.Rectangle sourceArea =
				new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

			var bitmapProperties = new BitmapProperties(
				new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied), 0.0f, 0.0f);  // 0.0f, default dpi

			var size = new Size2(bmp.Width, bmp.Height);

			int stride = bmp.Width * sizeof(int);

			using (var tempStream = new DataStream(bmp.Height * stride, true, true))
			{
				BitmapData bitmapData = bmp.LockBits(sourceArea, ImageLockMode.ReadOnly,
													 System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

				for (int y = 0; y < bmp.Height; y++)
				{
					int offset = bitmapData.Stride * y;
					for (int x = 0; x < bmp.Width; x++)
					{
						byte b = Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte g = Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte r = Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte a = Marshal.ReadByte(bitmapData.Scan0, offset++);
						int rgba = r | (g << 8) | (b << 16) | (a << 24);
						tempStream.Write(rgba);
					}
				}
				bmp.UnlockBits(bitmapData);
				tempStream.Position = 0;

				return new Bitmap(_device, size, tempStream, stride, bitmapProperties);
			}
		}

		#endregion
	}
}