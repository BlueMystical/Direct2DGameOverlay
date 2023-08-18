
using Blue.TextDataTable;
using Newtonsoft.Json.Linq;
using Overlay.NET.Directx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

		private static List<dynamic> DataToShow = null;

		private static bool JournalWatcherIsRunning = true;
		private static string WatchedFile = string.Empty;

		private static TextDataTable BlueDataTable;
		private static TableConfiguration BlueDTConfig;
		private static string TextToShow = string.Empty;

		private static DateTime StartTime = DateTime.MinValue;
		private static int TimeOff = 20000;

		/// <summary>Main method is Mandatory.</summary>
		[STAThread]
		public static void Main()
		{
			// Here you can call methods who do non drawing or 'Rendering' stuff.
			// This method is invoked only once
			ReadPlayerJournal();
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
				#region EXAMPLE 1:  Drawing a full-size grid using 'Geometry' for complex shapes. 

				/*  
				 *				https://learn.microsoft.com/en-us/windows/win32/direct2d/path-geometries-overview
				 
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

				*/
				#endregion

				#region EXAMPLE 2:  Show Information from a Game Event: 'FSDJump'

				// 
				// The 'TextToShow' is generated by the 'PlayerJournal_DetectEvents' method.
				if (!string.IsNullOrEmpty(TextToShow))
				{
					System.Drawing.Rectangle Box = new Rectangle(new Point(50, 100), new Size(500, 600));
					pGraphics.DrawTextWithBackground(fonts["Consolas;12;0"], brushes["Orange"], brushes["Black50%"], Box, TextToShow);

					// CountDown for Message dimissal
					if (StartTime == DateTime.MinValue) StartTime = DateTime.Now;
					decimal ElapsedMS = (decimal)(DateTime.Now.Subtract(StartTime)).TotalMilliseconds;
					if (ElapsedMS >= TimeOff)
					{
						TextToShow = string.Empty;
						StartTime = DateTime.MinValue;
					}
                    else
                    {
                        //Show remaining Time
						int RemainingMS = Convert.ToInt32((TimeOff - ElapsedMS) / 1000m);
						System.Drawing.Rectangle RBox = new Rectangle(new Point(50, 100), new Size(14, 14));
						pGraphics.DrawTextWithBackground(fonts["Consolas;12;0"], brushes["White"], brushes["Blue70%"], RBox, RemainingMS.ToString());
					}
                }

				#endregion

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
			LogMessage(ex.Message + ex.StackTrace);
		}
		private static void LogMessage(string pMessage)
		{
			string _LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", "code", ModuleName + ".log");
			using (StreamWriter sw = new StreamWriter(_LogPath, true, Encoding.UTF8))
			{
				sw.WriteLine(string.Format("[{0}]: {1}", DateTime.Now.ToString(), pMessage));
				sw.Close();
			}
		}

		public static bool NullOrEmpty(object pValue)
		{
			bool result = false;
			if (pValue != null && pValue.ToString() != string.Empty)
			{
				result = true;
			}
			return result;
		}

		// LogMessage(string.Format(""));

		/// <summary>Locates the latest ED Journal file and watches changes on it detecting the events when happen.</summary>
		public static void ReadPlayerJournal()
		{
			try
			{
				if (true)
				{
					string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					string EDJournalDir = Path.Combine(UserProfile,
						@"Saved Games\Frontier Developments\Elite Dangerous");

					if (Directory.Exists(EDJournalDir))
					{
						FileInfo JournalFile;

						LogMessage(string.Format("%USERPROFILE% found at '{0}'.", EDJournalDir));
						var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
						{
							JournalFile = new DirectoryInfo(EDJournalDir).GetFiles("Journal.*.log")
										.OrderByDescending(f => f.LastWriteTime).First();
							if (JournalFile != null)
							{
								PlayerJournal_WatchFile(JournalFile.FullName);
							}

							//Queda vigilando el Directoro x si se crea un nuevo archivo de log:
							//----PlayerJournal_WatchDirectory
							FileSystemWatcher watcher = new FileSystemWatcher
							{
								Path = EDJournalDir,
								EnableRaisingEvents = true,
								Filter = "Journal.*.log"
							};
							watcher.Created += (sender, e) => // Evento ocurre cuando se detecta la creacion de un nuevo archivo de log:
							{
								JournalWatcherIsRunning = false; //<- Deja de Leer el archivo actual (si hay)

								//Busca el Archivo de Log mas reciente:
								JournalFile = new DirectoryInfo(EDJournalDir).GetFiles("Journal.*.log")
										.OrderByDescending(f => f.LastWriteTime).First();
								if (JournalFile != null)
								{
									//Abre el archivo en modo compartido y 'Escucha' si ha sido modificado:
									JournalWatcherIsRunning = true;
									PlayerJournal_WatchFile(JournalFile.FullName);
								}
							};
						});
					}
					else
					{
						LogMessage(string.Format("ERROR 404: Not Found. '{0}'", EDJournalDir));
					}
				}
			}
			catch (Exception ex)
			{
				LogExeption(ex);
			}
		}
		private static void PlayerJournal_WatchFile(string pFilePath)
		{
			/* LEE EL ARCHIVO DE LOG DEL JORNAL Y LO MANTIENE ABIERTO REACCIONANDO A SUS CAMBIOS  */
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				//System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				if (File.Exists(pFilePath) && WatchedFile != pFilePath)
				{
					WatchedFile = pFilePath;
					LogMessage(string.Format("Journal file: {0}", pFilePath));

					var wh = new System.Threading.AutoResetEvent(false);
					var LogWatcher = new FileSystemWatcher(".")
					{
						Path = System.IO.Path.GetDirectoryName(pFilePath),  //<- Obtiene el Path: (Sin archivo ni extension)
						Filter = System.IO.Path.GetFileName(pFilePath),     //<- Nombre del Archivo con Extension (Sin Ruta)
						NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite,
						EnableRaisingEvents = true
					};
					LogWatcher.Changed += (sender, eventArgs) =>
					{
						wh.Set(); //<- Avisa que hay Cambios en el Archivo
					};

					//El archivo se abre en modo Compartido:
					var fs = new FileStream(pFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					using (var sr = new StreamReader(fs))
					{
						string Line = string.Empty;
						while (JournalWatcherIsRunning) //<- Poner en False para dejar de Leer el Archivo
						{
							Line = sr.ReadLine(); //<- Lee Linea x Linea
							if (!string.IsNullOrEmpty(Line))
							{
								//Analiza la Linea Buscando los Eventos deseados:
								PlayerJournal_DetectEvents(Line);
							}
							else
							{
								wh.WaitOne(1000); //<- Cuando ya no hay más lineas, pausa el proceso de lectura y Espera a que FileSystemWatcher notifique algun cambio
							}
						}
						wh.Close();
						LogWatcher.Dispose();
					}
				}
			});
		}
		private static void PlayerJournal_DetectEvents(string JsonLine)
		{
			/* AQUI SE LEEN LAS LINEAS NUEVAS DEL LOG Y SE DETECTAN LOS EVENTOS DESEADOS   */
			// Esto sigue ejecutandose dentro del proceso iniciado x 'ReadPlayerJournal()'
			try
			{
				/* https://elite-journal.readthedocs.io/en/latest/
				 * Cada linea del Log es un Objeto JSON completo */

				//1. Buscar el Nombre del Jugador:
				int index = 0;
				index = JsonLine.IndexOf("\"event\":\"Commander\"", index);
				if (index != -1) //<- Event Detected!
				{
					dynamic data = JObject.Parse(JsonLine);
					if (data != null)
					{
						LogMessage(string.Format("Welcome Commander '{0}'", data.Name));
					}
				}

				//2. Detectar cuando se Cambia la Nave:
				index = 0;
				index = JsonLine.IndexOf("\"event\":\"Loadout\"", index);
				if (index != -1)
				{
					dynamic data = JObject.Parse(JsonLine);
					if (data != null)
					{
						LogMessage(string.Format("Ship Loadout: {0} ({1} {2})", (data.Ship is null ? string.Empty : data.Ship), data.ShipName, data.ShipIdent));
					}
				}

				//3. Detectar cuando se lanza el SRV:
				index = 0;
				index = JsonLine.IndexOf("\"event\":\"LaunchSRV\"", index);
				if (index != -1)
				{
					dynamic data = JObject.Parse(JsonLine);
					if (data != null)
					{
						LogMessage(string.Format("LaunchSRV Event: {0} ({1})", data.SRVType, data.SRVType_Localised));
					}
				}

				//4. Detectar cuando Vuelve a la nave desde el SRV:
				index = 0;
				index = JsonLine.IndexOf("\"event\":\"DockSRV\"", index);
				if (index != -1)
				{
					dynamic data = JObject.Parse(JsonLine);
					if (data != null)
					{
						LogMessage(string.Format("DockSRV Event: {0} ({1})", data.SRVType, data.SRVType_Localised));
					}
				}

				index = 0; // When plotting a multi-star route the file "NavRoute.json" is written
				index = JsonLine.IndexOf("\"event\":\"NavRoute\"", index);
				if (index != -1)
				{

				}
				index = 0; // When the current plotted nav route is cleared
				index = JsonLine.IndexOf("\"event\":\"NavRouteClear\"", index);
				if (index != -1)
				{

				}

				//
				index = 0; // when performing a full system scan ("Honk")
				index = JsonLine.IndexOf("\"event\":\"FSSDiscoveryScan\"", index);
				if (index != -1)
				{

				}
				//SupercruiseDestinationDrop


				index = 0; // when jumping from one star system to another
				index = JsonLine.IndexOf("\"event\":\"FSDJump\"", index);
				if (index != -1)
				{
					//Task.Factory.StartNew(() =>
					//{

						dynamic data = JObject.Parse(JsonLine);
						if (data != null)
						{
							DataToShow = new List<dynamic>();
							DataToShow.Add(new { SystemInfo = "Allegiance:", Value = data.SystemAllegiance });
							DataToShow.Add(new { SystemInfo = "Economy:", Value = string.Format("{0} - {1}", data.SystemEconomy_Localised, data.SystemSecondEconomy_Localised) });
							DataToShow.Add(new { SystemInfo = "Government:", Value = data.SystemGovernment_Localised });
							DataToShow.Add(new { SystemInfo = "Population:", Value = string.Format("{0:n0}", data.Population) });
							DataToShow.Add(new { SystemInfo = "Security:", Value = data.SystemSecurity_Localised });

							if (data.Powers != null)
							{
								string Powers = string.Empty;
								foreach (var item in data.Powers)
								{
									Powers += item.ToString() + ", ";
								}
								DataToShow.Add(new { SystemInfo = "Powers:", Value = Powers });

								if (data.PowerplayState != null)
								{
									DataToShow.Add(new { SystemInfo = "Powerplay State:", Value = data.PowerplayState });
								}
							}

							if (data.SystemFaction != null)
							{
								DataToShow.Add(new { SystemInfo = "Controlling Faction:", Value = data.SystemFaction.Name });
								DataToShow.Add(new { SystemInfo = "Faction State:", Value = data.SystemFaction.FactionState });
							}

							if (data.Conflicts != null)
							{
								DataToShow.Add(new { SystemInfo = "Conflicts:", Value = string.Format("{0:n0}", data.Conflicts.Count) });
								foreach (var conflict in data.Conflicts)
								{
									DataToShow.Add(new
									{
										SystemInfo = string.Format("{0} {1}:", conflict.WarType, conflict.Status),
										Value = string.Format("{0} {1}", conflict.Faction1.Name, conflict.Faction2.Name)
									});
								}
							}

							if (data.ThargoidWar != null)
							{
								DataToShow.Add(new
								{
									SystemInfo = "ThargoidWar:",
									Value = string.Format("{0} {1}", data.ThargoidWar.CurrentState, data.ThargoidWar.WarProgress)
								});
							}

							//This data will be shown by the Render method.
							BlueDataTable = new TextDataTable
							{
								TConfiguration = new TableConfiguration()
								{
									columns = new List<Column>(new Column[] {
										new Column("SystemInfo", "System Info")
										{
											type = "string",
											width = 250,
											length = 18,
											align = "center"
										},
										new Column("Value", "Value")
										{
											type = "string",
											width = 250,
											length = 25,
											align = "left"
										}
									}),
									header = new Header(string.Format("Welcome to '{0}", data.StarSystem)),
									footer = new Header(string.Format("You just jumped {0:n1} Lys", data.JumpDist)),
									sorting = null,
									grouping = null,
									summary = null,
									data = DataToShow
								},
								DataSource = DataToShow
							};

							TextToShow = BlueDataTable.Build_TextDataTable();
						}
					//});
				}


				//TODO: Add other event detections here.
			}
			catch (Exception ex)
			{
				LogExeption(ex);
			}
		}


	}
}

