using LoreSoft.MathExpressions;
using Process.NET.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace DirectXOverlay
{
	public static class Helper
	{
		/// <summary>Gets the Modifier Keys from a group of Keys.</summary>
		/// <param name="_keys">Keys to analyse.</param>
		public static NonInvasiveKeyboardHookLibrary.ModifierKeys GetModifiers(Keys _keys)
		{
			NonInvasiveKeyboardHookLibrary.ModifierKeys _ret = new NonInvasiveKeyboardHookLibrary.ModifierKeys();
			if ((_keys & Keys.Alt) == Keys.Alt) _ret |= NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt;
			if ((_keys & Keys.Control) == Keys.Control) _ret |= NonInvasiveKeyboardHookLibrary.ModifierKeys.Control;
			if ((_keys & Keys.Shift) == Keys.Shift) _ret |= NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift;
			return _ret;
		}

		/// <summary>Gets a Key that is not a Modifier key.</summary>
		/// <param name="_keys">Keys to analyse.</param>
		public static int GetKeyCode(Keys _keys) => (int)(_keys & ~Keys.Control & ~Keys.Shift & ~Keys.Alt);

		/// <summary>Constantes para los Codigos de Pagina al leer o guardar archivos de texto.</summary>
		public enum TextEncoding
		{
			/// <summary>CodePage:1252; windows-1252 ANSI Latin 1; Western European (Windows)</summary>
			ANSI = 1252,
			/// <summary>CodePage:850; ibm850; ASCII Multilingual Latin 1; Western European (DOS)</summary>
			DOS_850 = 850,
			/// <summary>CodePage:1200; utf-16; Unicode UTF-16, little endian byte order (BMP of ISO 10646);</summary>
			Unicode = 1200,
			/// <summary>CodePage:65001; utf-8; Unicode (UTF-8)</summary>
			UTF8 = 65001
		}

		/// <summary>Guarda Datos en un Archivo de Texto usando la Codificacion especificada.</summary>
		/// <param name="FilePath">Ruta de acceso al Archivo. Si no existe, se Crea. Si existe, se Sobreescribe.</param>
		/// <param name="Data">Datos a Grabar en el Archivo.</param>
		/// <param name="CodePage">[Opcional] Pagina de Codigos con la que se guarda el archivo. Por defecto se usa Unicode(UTF-16).</param>
		public static bool SaveTextFile(string FilePath, string Data, TextEncoding CodePage = TextEncoding.Unicode)
		{
			bool _ret = false;
			try
			{
				if (FilePath != null && FilePath != string.Empty)
				{
					/* ANSI code pages, like windows-1252, can be different on different computers, 
					 * or can be changed for a single computer, leading to data corruption. 
					 * For the most consistent results, applications should use UNICODE, 
					 * such as UTF-8 or UTF-16, instead of a specific code page. 
					 https://docs.microsoft.com/es-es/windows/desktop/Intl/code-page-identifiers  */

					System.Text.Encoding ENCODING = System.Text.Encoding.GetEncoding((int)CodePage); //<- Unicode Garantiza Maxima compatibilidad
					using (System.IO.FileStream FILE = new System.IO.FileStream(FilePath, System.IO.FileMode.Create))
					{
						using (System.IO.StreamWriter WRITER = new System.IO.StreamWriter(FILE, ENCODING))
						{
							WRITER.Write(Data);
							WRITER.Close();
						}
					}
					if (System.IO.File.Exists(FilePath)) _ret = true;
				}
			}
			catch (Exception ex) { throw ex; }
			return _ret;
		}

		/// <summary>Lee un Archivo de Texto usando la Codificacion especificada.</summary>
		/// <param name="FilePath">Ruta de acceso al Archivo. Si no existe se produce un Error.</param>
		/// <param name="CodePage">[Opcional] Pagina de Codigos con la que se Leerá el archivo. Por defecto se usa Unicode(UTF-16).</param>
		public static string ReadTextFile(string FilePath, TextEncoding CodePage = TextEncoding.Unicode)
		{
			string _ret = string.Empty;
			try
			{
				if (FilePath != null && FilePath != string.Empty)
				{
					if (System.IO.File.Exists(FilePath))
					{
						System.Text.Encoding ENCODING = System.Text.Encoding.GetEncoding((int)CodePage);
						_ret = System.IO.File.ReadAllText(FilePath, ENCODING);
					}
					else { throw new Exception(string.Format("ERROR 404: Archivo '{0}' NO Encontrado!", FilePath)); }
				}
				else { throw new Exception("No se ha Especificado la Ruta de acceso al Archivo!"); }
			}
			catch (Exception ex) { throw ex; }
			return _ret;
		}

		/// <summary>Guarda un array de bytes en un archivo.</summary>
		/// <param name="fileBytes">Datos a Guradar.</param>
		/// <param name="fileName">Ruta completa del Archivo.</param>
		public static void SaveFile(byte[] fileBytes, string fileName)
		{
			File.WriteAllBytes(fileName, fileBytes);
		}

		/// <summary>Muestra un mensaje en un cuadro de diálogo, espera a que el usuario escriba un texto 
		/// o haga clic en un botón y devuelve una cadena con el contenido del cuadro de texto.</summary>
		/// <param name="title">Expresión de tipo String que se muestra en la barra de título del cuadro de diálogo.</param>
		/// <param name="promptText">Expresión de tipo String que se muestra como mensaje en el cuadro de diálogo.</param>
		/// <param name="value">Valor Suministrado por el usuario.</param>
		public static DialogResult InputBox(string title, string promptText, ref string value)
		{
			Form form = new Form();
			Label label = new Label();
			TextBox textBox = new TextBox();
			Button buttonOk = new Button();
			Button buttonCancel = new Button();

			form.Text = title;
			label.Text = promptText;
			textBox.Text = value;

			buttonOk.Text = "Aceptar";
			buttonCancel.Text = "Cancelar";
			buttonOk.DialogResult = DialogResult.OK;
			buttonCancel.DialogResult = DialogResult.Cancel;

			label.SetBounds(9, 20, 372, 13);
			textBox.SetBounds(12, 36, 372, 20);
			buttonOk.SetBounds(228, 72, 75, 23);
			buttonCancel.SetBounds(309, 72, 75, 23);

			label.AutoSize = true;
			textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
			buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			form.ClientSize = new System.Drawing.Size(396, 107);
			form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
			form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.StartPosition = FormStartPosition.CenterScreen;
			form.MinimizeBox = false;
			form.MaximizeBox = false;
			form.AcceptButton = buttonOk;
			form.CancelButton = buttonCancel;

			DialogResult dialogResult = form.ShowDialog();
			value = textBox.Text;
			return dialogResult;
		}

		/// <summary>Parsea las palabras 'ScreenWidth' y 'ScreenHeight' a sus valores numericos y evalua la expresion matematica en 'pValue'.</summary>
		/// <param name="TargetWindow"></param>
		/// <param name="pValue"></param>
		public static int ResolveValue(IWindow TargetWindow, string pValue)
		{
			int _ret = 0;
			try
			{
				if (!string.IsNullOrEmpty(pValue) && TargetWindow != null)
				{
					double ScreenHeight = SystemParameters.FullPrimaryScreenHeight;
					double ScreenWidth = SystemParameters.FullPrimaryScreenWidth;
					double resolution = ScreenHeight * ScreenWidth;

					// "X":"ScreenWidth / 2",
					pValue = pValue.Replace("ScreenWidth", ScreenWidth.ToString());
					pValue = pValue.Replace("ScreenHeight", ScreenHeight.ToString());

					pValue = pValue.Replace("WindowWidth", TargetWindow.Width.ToString());
					pValue = pValue.Replace("WindowHeight", TargetWindow.Height.ToString());

					MathEvaluator eval = new MathEvaluator();
					double result = eval.Evaluate(pValue);

					_ret = Convert.ToInt32(Math.Round(result, 0));
				}
			}
			catch { }			
			return _ret;
		}

		/// <summary>Construye una Fuente usando los valores proporcionados en una cadena.</summary>
		/// <param name="pFontData">[FontName];[Size];[FontStyle] 'FontStyle' es el estilo de la fuente convertido a Int32.</param>
		public static Font GetFontFromString(string pFontData, char pDelimiter = ';')
		{
			Font theFont = new Font(new FontFamily("Consolas"), 12);
			try
			{
				if (!string.IsNullOrEmpty(pFontData))
				{
					string[] Palabras = pFontData.Split(new char[] { pDelimiter });

					if (Palabras.Length > 0)
					{
						//       1      2    3     
						// Courier New;15;FontStyle

						switch (Palabras.Length)
						{
							case 1: theFont = new Font(new FontFamily(Palabras[0]), 10); break;
							case 2: theFont = new Font(new FontFamily(Palabras[0]), Convert.ToInt32(Palabras[1])); break;
							case 3: theFont = new Font(new FontFamily(Palabras[0]), Convert.ToInt32(Palabras[1]), (FontStyle)Convert.ToInt32(Palabras[2])); break;
							default: break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return theFont;
		}

		/// <summary>Construye un Color usando los valores proporcionados en una cadena.</summary>
		/// <param name="pColorData">Puede ser un color #HTML, RGB, ARGB, NamedColor.
		/// <para>Ejem: '#FFFFF', '255,255,0', '127,0,0,255', 'DoodgerBlue'</para></param>
		public static Color GetColorFromString(string pColorData, bool IsARGB = false, char pDelimiter = ',')
		{
			Color SelectedColor = Color.Transparent;
			try
			{
				if (!string.IsNullOrEmpty(pColorData))
				{
					if (IsARGB)
					{
						string[] pARGB = pColorData.Split(new char[] { pDelimiter });
						if (pARGB.Length == 3)
						{
							SelectedColor = Color.FromArgb(Convert.ToInt32(pARGB[0]), Convert.ToInt32(pARGB[1]), Convert.ToInt32(pARGB[2]));
						}
						if (pARGB.Length == 4)
						{
							SelectedColor = Color.FromArgb(Convert.ToInt32(pARGB[0]), Convert.ToInt32(pARGB[1]), Convert.ToInt32(pARGB[2]), Convert.ToInt32(pARGB[3]));
						}
					}
					else
					{
						SelectedColor = ColorTranslator.FromHtml(pColorData);
					}
				}								
			}
			catch { }
			return SelectedColor;
		}


		public static List<PerformanceCounter> GetGPUCounters()
		{
			var category = new PerformanceCounterCategory("GPU Engine");
			var counterNames = category.GetInstanceNames();

			var gpuCounters = counterNames
								.Where(counterName => counterName.EndsWith("engtype_3D"))
								.SelectMany(counterName => category.GetCounters(counterName))
								.Where(counter => counter.CounterName.Equals("Utilization Percentage"))
								.ToList();

			return gpuCounters;
		}

		public static float GetGPUUsage(List<PerformanceCounter> gpuCounters)
		{
			gpuCounters.ForEach(x => x.NextValue());

			//Thread.Sleep(1000);

			var result = gpuCounters.Sum(x => x.NextValue());


			//System.Management.ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
			//foreach (ManagementObject queryObj in searcher.Get())
			//{
			//	double temp = Convert.ToDouble(queryObj["CurrentTemperature"].ToString());
			//	temp = (temp - 2732) / 10.0;
			//	Console.WriteLine("GPU Temperature: {0}°C", temp);
			//}
			//Console.ReadKey();

			return result;
		}

		public static string GetUniqueID()
		{
			string _ret = string.Empty;
			try
			{
				long ticks = DateTime.Now.Ticks;
				byte[] bytes = BitConverter.GetBytes(ticks);
				_ret = Convert.ToBase64String(bytes)
										.Replace('+', '_')
										.Replace('/', '-')
										.TrimEnd('=');
			}
			catch (Exception ex)
			{
				
			}
			return _ret;
		}

	}
}
