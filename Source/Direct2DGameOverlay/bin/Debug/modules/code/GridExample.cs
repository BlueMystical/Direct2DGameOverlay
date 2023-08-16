
using Overlay.NET.Directx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace DXOverlay.ExternalModule
{
	/* Do NOT change the NameSpace and Class names  */
	public class Programa
	{
		private static string ModuleName = string.Empty;

		private static Dictionary<string, int> brushes { get; set; }
		private static Dictionary<string, int> fonts;
		private static Dictionary<string, int> textures;



		[STAThread]  // Main method is Mandatory.
		public static void Main()
		{
			// Here you can call methods who do non drawing or 'Rendering' stuff.
			// This method is invoked only once

			//var Suma = Calcular(10, 20);
		}

		/// <summary>[To be called from the Parent Overlay] Sets all the available resources.</summary>
		/// <param name="_brushes">List of Available Colors</param>
		/// <param name="_fonts">List of Available Fonts</param>
		/// <param name="_textures">List of Available Images</param>
		public static bool Initialize(string pName, Dictionary<string, int> _brushes, Dictionary<string, int> _fonts, Dictionary<string, int> _textures, bool ShowLog = true)
		{
			bool _ret = false;
			try
			{
				ModuleName = pName;
				brushes = _brushes;
				fonts = _fonts;
				textures = _textures;
				// Resources used here must be declared on the 'layer' this code is asociated to

				if (ShowLog)  //[OPTIONAL] Save log showing available Resources:
				{
					string _LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", "code", ModuleName + ".log");
					using (StreamWriter sw = new StreamWriter(_LogPath))
					{
						foreach (var item in brushes)
						{
							sw.WriteLine(string.Format("Brush Resource - Key:'{0}',		Value:[{1}]", item.Key, item.Value.ToString()));
						}
						foreach (var item in fonts)
						{
							sw.WriteLine(string.Format("Font Resource - Key:'{0}',	Value:[{1}]", item.Key, item.Value.ToString()));
						}
						foreach (var item in textures)
						{
							sw.WriteLine(string.Format("Texture Resource - Key:'{0}',	Value:[{1}]", item.Key, item.Value.ToString()));
						}

						sw.Close();
					};
				}
				_ret = true;
			}
			catch (Exception ex)
			{
				LogExeption(ex);
			}
			return _ret;
		}

		/// <summary>This method is called from the Parent Overlay once per rendered frame, which means around 60 times per second.
		/// <para>** DO NOT ADD TOO HEAVY STUFF HERE OR YOU WILL PAY FOR IT IN PERFORMANCE (FPS)! **</para></summary>
		/// <param name="OverlayWindow">A reference to the Overlay Window</param>
		/// <param name="Graphics">A reference to the Graphics Device who has the drawing methods.</param>
		public static string Render(DirectXOverlayWindow pOverlayWindow, Direct2DRenderer pGraphics)
		{
			string _Response = string.Empty;
			try
			{
				/*  EXAMPLE 1:  Drawing a full-size grid using 'Geometry' for complex shapes. 
				 *				https://learn.microsoft.com/en-us/windows/win32/direct2d/path-geometries-overview
				 */
				var _gridBounds = new System.Drawing.Rectangle(20, 60, pOverlayWindow.Width - 30, pOverlayWindow.Height - 20);
				var _gridGeometry = pGraphics.CreateGeometry();

				// Horizontal Lines:
				for (float x = _gridBounds.Left; x <= _gridBounds.Right; x += 20)
				{
					_gridGeometry.BeginLine(new Point((int)x, _gridBounds.Top), new Point((int)x, _gridBounds.Bottom));
					_gridGeometry.EndFigure(false);
				}
				// Vertical Lines:
				for (float y = _gridBounds.Top; y <= _gridBounds.Bottom; y += 20)
				{
					_gridGeometry.BeginLine(new Point(_gridBounds.Left, (int)y), new Point(_gridBounds.Right, (int)y));
					_gridGeometry.EndFigure(false);
				}
				_gridGeometry.Close();

				//Here we do the actual drawing:
				pGraphics.DrawGeometry(_gridGeometry, brushes["Blue50%"], 1.0f);


				_Response = "ok";
			}
			catch (Exception ex)
			{
				_Response = ex.Message; //<- Errors are reported back to the Parent Overlay.
				LogExeption(ex);
			}
			return _Response;
		}

		/// <summary>This writes a log file with the errors that occurred in the execution.</summary>
		/// <param name="ex">Details of the error.</param>
		private static void LogExeption(Exception ex)
		{
			string _LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", "code", ModuleName + "_Errors.log");
			using (StreamWriter sw = new StreamWriter(_LogPath, true, Encoding.UTF8))
			{
				sw.WriteLine(string.Format("[{0}]: {1}", DateTime.Now.ToString(), ex.Message + ex.StackTrace));
				sw.Close();
			}
		}

		/// <summary>Realiza una Operacion Matematica</summary>
		/// <param name = "x" > Primera Variable</param>
		/// <param name = "y" > Segunda Variable</param>
		public static double Calcular(double x, double y)
		{
			return x + 2 * y;
		}

	}
}

