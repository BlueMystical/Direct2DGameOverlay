
using Blue.TextDataTable;
using Newtonsoft.Json.Linq;
using Overlay.NET.Directx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;


/* DO NOT change the NameSpace and Class names  */
namespace DXOverlay.ExternalModule
{
	public static class Programa
	{
		/// <summary>Name of this Module</summary>
		private static string ModuleName = string.Empty;

		/// <summary>[Pre-Loaded Resources] Colors</summary>
		private static Dictionary<string, int> brushes { get; set; }

		/// <summary>[Pre-Loaded Resources] Fonts</summary>
		private static Dictionary<string, int> fonts;

		/// <summary>[Pre-Loaded Resources] Images</summary>
		private static Dictionary<string, int> textures;

		/// <summary>Instance of the Overlay Window</summary>
		private static DirectXOverlayWindow Overlay;

		/// <summary>This has the Drawing methods</summary>
		private static Direct2DRenderer GraphicRender;

		#region ED Journal Events

		/* FOR SHOWING DATA WHEN A GAME EVENT IS DETECTED */
		private static List<dynamic> DataToShow = null;

		private static bool JournalWatcherIsRunning = true; //<- ED's Journal is being watched
		private static string WatchedFile = string.Empty;   //<- Current Journal File
		private static bool AllLinesRead { get; set; } = false;

        private static string TextToShow = string.Empty;    //<- to show data in Text mode
		private static SharpDX.Direct2D1.Bitmap ImageToShow = null; //<- To show Images

		#endregion

		// To auto-dismiss messages
		private static DateTime StartTime = DateTime.MinValue;
		private static int TimeOff = 20000;

		/// <summary>Main method is Mandatory.</summary>
		//[STAThread]
		//public static void Main()
		//{

		//}

		/// <summary>[To be called from the Parent Overlay] Sets all the available resources.</summary>
		/// <param name="pModuleName">Name of this Module</param>
		/// <param name="pOverlayWindow">A reference to the Overlay Window</param>
		/// <param name="pGraphics">A reference to the Graphics Device who has the drawing methods.</param>
		/// <param name="_brushes">List of Available Colors</param>
		/// <param name="_fonts">List of Available Fonts</param>
		/// <param name="_textures">List of Available Images</param>
		/// <param name="ShowLog">[Default true] write a log enumerating the available pre-loaded resources.</param>
		public static bool Initialize(string pModuleName, DirectXOverlayWindow pOverlayWindow, Direct2DRenderer pGraphics, Dictionary<string, int> _brushes, Dictionary<string, int> _fonts, Dictionary<string, int> _textures, bool ShowLog = true)
		{
			/*  THIS METHOD SHOULD BE INVOKED ONLY ONCE  */
			bool _ret = false;
			try
			{
				ModuleName = System.IO.Path.GetFileNameWithoutExtension(pModuleName);
				Overlay = pOverlayWindow;
				GraphicRender = pGraphics;

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

				// Here you can call methods who do non drawing stuff.
				ReadPlayerJournal();

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
		public static string Render()
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

				#region EXAMPLE 2:  Show Information from a Game Event: 'FSDJump' in a Text Table
				/*
				// The 'TextToShow' is generated by the 'PlayerJournal_DetectEvents' method.
				if (!string.IsNullOrEmpty(TextToShow))
				{
					Point Location = new Point(30, 150);
					System.Drawing.Rectangle Box = new Rectangle(Location, new Size(500, 600));
					pGraphics.DrawTextWithBackground(fonts["Consolas;14;0"], brushes["White"], brushes["Black50%"], Box, TextToShow);

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
						System.Drawing.Rectangle RBox = new Rectangle(Location, new Size(14, 14));
						pGraphics.DrawTextWithBackground(fonts["Consolas;12;0"], brushes["White"], brushes["Blue70%"], RBox, RemainingMS.ToString("00"));
					}
				}
				*/
				#endregion

				#region EXAMPLE 3:  Show Information from a Game Event: 'FSDJump' in a Image

				// The 'ImageToShow' is generated by the 'PlayerJournal_DetectEvents' method.
				if (ImageToShow != null)
				{

					Point Location = new Point(30, 150);

					GraphicRender.DrawImage(
						ImageToShow,
						new Rectangle(Location, new Size((int)ImageToShow.Size.Width, (int)ImageToShow.Size.Height)),
						1.0f);

					// CountDown for Message dimissal
					if (StartTime == DateTime.MinValue) StartTime = DateTime.Now;
					decimal ElapsedMS = (decimal)(DateTime.Now.Subtract(StartTime)).TotalMilliseconds;
					if (ElapsedMS >= TimeOff)
					{
						ImageToShow = null;
						StartTime = DateTime.MinValue;
					}
					else
					{
						//Show remaining Time
						int RemainingMS = Convert.ToInt32((TimeOff - ElapsedMS) / 1000m);

						GraphicRender.DrawTextWithBackground(
							fonts["Consolas;12;0"],
							brushes["White"], brushes["Blue70%"],
							new Rectangle(Location, new Size(14, 14)),
							RemainingMS.ToString("00"));
					}
				}

				#endregion
			}
			catch (Exception ex)
			{
				_Response = ex.Message; //<- Errors are reported back to the Parent Overlay.
				LogExeption(ex);
			}
			return _Response;
		}

		#region Utilitary Methods


		/// <summary>This writes a log file with the errors that occurred in the execution.</summary>
		/// <param name="ex">Details of the error.</param>
		private static void LogExeption(Exception ex)
		{
			LogMessage(ex.Message + ex.StackTrace);
		}
		/// <summary>Writes a log file with the provided message.</summary>
		/// <param name="pMessage">Message to be logged.</param>
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

		// To Convert Images to and from Byte Array.
		public static Image ToImage(this byte[] byteArrayIn)
		{
			Image returnImage = null;
			using (MemoryStream ms = new MemoryStream(byteArrayIn))
			{
				returnImage = Image.FromStream(ms);
			}
			return returnImage;
		}
		public static byte[] ToByteArray(this System.Drawing.Image imageIn)
		{
			byte[] retorno = null;
			MemoryStream ms = null;
			try
			{
				ms = new MemoryStream();
				imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
				retorno = ms.ToArray();
			}
			catch (Exception ex) { throw ex; }
			finally { if (ms != null) { ms.Close(); } }
			return retorno;
		}

		private static string Send_WEB_Request(string pURL, string pMethod = "POST", string jsonData = "", params key_value[] pHeaders)
		{
			string _ret = string.Empty;

			try
			{
				if (pURL != null && pURL != string.Empty)
				{
					byte[] byteArray = Encoding.UTF8.GetBytes(jsonData);

					//Esto es para permitir la Solicitud con el protocolo HTTPS:
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
								| SecurityProtocolType.Tls11
								| SecurityProtocolType.Tls12
								| SecurityProtocolType.Ssl3;

					WebRequest request = WebRequest.Create(pURL);
					request.Method = pMethod;
					request.ContentType = "application/json";
					request.ContentLength = byteArray.Length;

					if (pHeaders != null && pHeaders.Length > 0)
					{
						foreach (key_value _Header in pHeaders)
						{
							request.Headers.Add(_Header.key, _Header.value);
						}
					}

					//Aqui se adjuntan los datos al Cuerpo de la Solicitud:
					if (jsonData != null && jsonData != string.Empty)
					{
						using (var reqStream = request.GetRequestStream())
						{
							reqStream.Write(byteArray, 0, byteArray.Length);
						}
					}

					using (var response = request.GetResponse())
					{
						if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
						{
							using (var respStream = response.GetResponseStream())
							{
								using (var reader = new StreamReader(respStream))
								{
									_ret = reader.ReadToEnd();
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogExeption(ex);
			}
			return _ret;
		}

		#endregion

		// LogMessage(string.Format(""));

		/// <summary>Locates the latest ED Journal file and watches changes on it detecting the events when happen.</summary>
		public static void ReadPlayerJournal()
		{
			try
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
						else
						{
							LogMessage("ERROR 404: Not Found. No journal");
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
			catch (Exception ex)
			{
				LogExeption(ex);
			}
		}
		private static void PlayerJournal_WatchFile(string pFilePath)
		{
			/* LEE EL ARCHIVO DE LOG DEL JORNAL Y LO MANTIENE ABIERTO REACCIONANDO A SUS CAMBIOS  */
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
							// los procesos aqui no cargan bien el archivonal_DetectEvents(Line); });
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
			else
			{
				LogMessage(string.Format("ERROR 404 Journal file NOT FOUND '{0}'", pFilePath));
			}
		}
		private static void PlayerJournal_DetectEvents(string JsonLine)
		{
			/* AQUI SE LEEN LAS LINEAS NUEVAS DEL LOG Y SE DETECTAN LOS EVENTOS DESEADOS   */
			try
			{
				/* https://elite-journal.readthedocs.io/en/latest/
				 * Cada linea del Log es un Objeto JSON completo */

				if (!string.IsNullOrEmpty(JsonLine))
				{
					int index = 0; // Get the Player's Name
					index = JsonLine.IndexOf("\"event\":\"Commander\"", index);
					if (index != -1) //<- Event Detected!
					{
						dynamic data = JObject.Parse(JsonLine);
						if (data != null)
						{
							LogMessage(string.Format("Welcome Commander '{0}'", data.Name));
						}
					}

					index = 0; // when loading from main menu or when embarking or switching ships
					index = JsonLine.IndexOf("\"event\":\"Loadout\"", index);
					if (index != -1)
					{
						dynamic data = JObject.Parse(JsonLine);
						if (data != null)
						{
							LogMessage(string.Format("Ship Loadout: {0} ({1} {2})", (data.Ship is null ? string.Empty : data.Ship), data.ShipName, data.ShipIdent));
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


					index = 0; // when jumping from one star system to another "event":"FSDJump"
					index = JsonLine.IndexOf("\"event\":\"FSDJump\"", index);
					if (index != -1)
					{
						List<dynamic> SystemInformation = new List<dynamic>();

						dynamic data = JObject.Parse(JsonLine);
						if (data != null)
						{
							LogMessage(string.Format("FSDJump Event: {0} ({1}ly)", data.StarSystem, data.JumpDist));

							#region Data Gathering

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

							string EDSM_SYSTEM_API = @"https://www.edsm.net/api-v1/system/";        //?systemName = GCRV 1568 &showId=1 &showInformation=1 &showPrimaryStar=1
							string EDSM_BODIES_API = @"https://www.edsm.net/api-system-v1/bodies";  //?systemName = GCRV 1568							
							string EDSM_STATIONS_API = @"https://www.edsm.net/api-system-v1/stations";    //?systemName = GCRV 1568

							string system_name_url = string.Format("&systemName={0}", Convert.ToString(data.StarSystem).Replace(" ", "%20"));
							string FullURLrequest = string.Empty;

							// Get The Stations in the System:			
							FullURLrequest = string.Format("{0}{1}{2}", EDSM_STATIONS_API, "?", system_name_url);
							LogMessage(FullURLrequest);
							string Response = Send_WEB_Request(FullURLrequest, "GET");
							if (!string.IsNullOrEmpty(Response))
							{
								//LogMessage(Response);

								dynamic stations_data = JObject.Parse(Response);
								//dynamic SystemsInfo = Newtonsoft.Json.Linq.JArray.Parse(Response);

								if (stations_data != null && stations_data.stations != null)
								{
									foreach (var station in stations_data.stations)
									{
										if (station.type != "Fleet Carrier")
										{
											string Observaciones = string.Empty;
											if (station.haveShipyard == true) Observaciones += "Shipyard, ";
											if (station.haveOutfitting == true) Observaciones += "Outfitting, ";
											if (station.haveMarket == true) Observaciones += "Market, ";

											if (station.otherServices != null)
											{
												foreach (string item in station.otherServices)
												{
													if (item.ToString().Contains("Trader")) Observaciones += item;
												}
											}
											SystemInformation.Add(new
											{
												Type = station.type,
												Name = station.name,
												Distance = station.distanceToArrival,
												Subtype = station.economy,
												Observations = Observaciones,
												Related = station.allegiance
											});
										}
									}
								}
							}

							// Get the Bodies in the System:
							FullURLrequest = string.Format("{0}{1}{2}", EDSM_BODIES_API, "?", system_name_url);
							LogMessage(FullURLrequest);
							Response = Send_WEB_Request(FullURLrequest, "GET");
							if (!string.IsNullOrEmpty(Response))
							{
								dynamic bodies_data = JObject.Parse(Response);
								if (bodies_data != null && bodies_data.bodies != null)
								{
									foreach (var body in bodies_data.bodies)
									{
										string Observaciones = string.Empty;
										if (body.type == "Star")
										{
											if ((bool)body.isScoopable) Observaciones += "Scoopable, ";
											if ((bool)body.isMainStar) Observaciones += "Main Star ";
										}
										if (body.type == "Planet")
										{
											//2 Rings: Rocky, Metal Rich
											int RingCount = 0;
											if (body.rings != null)
											{
												foreach (var ring in body.rings)
												{
													Observaciones += ring.type + ", ";
													RingCount++;
												}
												Observaciones = string.Format("{0} Rings: {1}", RingCount, Observaciones);
											}
										}

										SystemInformation.Add(new
										{
											Type = body.type,
											Name = body.name,
											Distance = body.distanceToArrival,
											Subtype = body.subType,
											Observations = Observaciones,
											Related = string.Empty
										});
									}
								}
							}

							#endregion

							#region Data Formating

							//This arranges the data into a Table
							var BlueDataTable = new TextDataTable
							{
								TConfiguration = new TableConfiguration()
								{
									columns = new List<Column>(new Column[] {
										new Column("SystemInfo", "System Info")
										{
											type = "string",
											align = "center",
											width = 250,
											length = 18
										},
										new Column("Value", "Value")
										{
											type = "string",
											align = "left",
											width = 350,
											length = 35
										}
									}),
									header = new Header(string.Format(" Welcome to '{0}' ", data.StarSystem)),
									footer = new Header(string.Format(" You just jumped {0:n1}Ly ", data.JumpDist)),
									sorting = null,
									grouping = null,
									summary = null,
									data = DataToShow
								},
								DataSource = DataToShow
							};
							BlueDataTable.TConfiguration.properties.table.column_headers.Visible = false;
							BlueDataTable.TConfiguration.properties.table.data_rows.colors.backcolor_argb = "150, 0, 0, 10";

							#endregion

							// Build the data into a Text String:
							//TextToShow = BlueDataTable.Build_TextDataTable();

							// Build the data into an Image:
							ImageToShow = GraphicRender.BitmapFromBitmap(BlueDataTable.Build_ImageDataTable(new Size(630, 500)));

							if (SystemInformation != null)
							{
								//LogMessage(Newtonsoft.Json.JsonConvert.SerializeObject(SystemInformation, Newtonsoft.Json.Formatting.Indented));

								List<dynamic> SystemInfoResume = new List<dynamic>();

								#region Resume about the Stars in the System
								
								var Stars = ((IEnumerable)SystemInformation)
									.Cast<dynamic>()
									.Where(p => p.Type == "Star")
									.ToList();

								foreach (var star in Stars)
                                {
									SystemInfoResume.Add(new
									{
										InSystem = string.Format("Stars ({0})", Stars.Count),
										Info = star.Subtype + star.Observations
									});
								}

								#endregion

								#region Resume about the Planets in the System
								
								var Planets = ((IEnumerable)SystemInformation)
									.Cast<dynamic>()
									.Where(p => p.Type == "Planet")
									.ToList();

								var GPlanets = ((IEnumerable)Planets)
									.Cast<dynamic>()
									.GroupBy(x => x.Subtype)
									.ToList();

								var GRings = ((IEnumerable)Planets)
									.Cast<dynamic>()
									.Where(p => !string.IsNullOrEmpty(p.Observations))
									.ToList();

								foreach (var planet in GPlanets)
								{
									var Details = planet.ToList();
									SystemInfoResume.Add(new
									{
										InSystem = string.Format("Planets ({0})", Planets.Count),
										Info = string.Format("{0} ({1})", planet.Key, Details.Count)
									});
								}
                                foreach (var ring in GRings)
                                {
									//var Details = ring.ToList();
									SystemInfoResume.Add(new
									{
										InSystem = string.Format("Rings ({0})", GRings.Count),
										Info = string.Format("{0} ({1})", ring.Name, ring.Observations)
									});
								}

								#endregion

								#region Resume about the Stations in the System

								var Stations = ((IEnumerable)SystemInformation)
									.Cast<dynamic>()
									.Where(p => p.Type != "Star" && p.Type != "Planet")
									.ToList();

								var GStations = ((IEnumerable)Stations)
									.Cast<dynamic>()
									.GroupBy(x => x.Type)
									.ToList();

								foreach (var station in GStations)
								{
									var Details = station.ToList();
									SystemInfoResume.Add(new
									{
										InSystem = string.Format("Stations ({0})", Stations.Count),
										Info = string.Format("{0} ({1})", station.Key, Details.Count)
									});
								}

								#endregion

								LogMessage(Newtonsoft.Json.JsonConvert.SerializeObject(SystemInfoResume, Newtonsoft.Json.Formatting.Indented));
							}

							LogMessage("FSDJump ready to show");
						}
					}


					//TODO: Add other event detections here.
				}
			}
			catch (Exception ex)
			{
				LogExeption(ex);
			}
		}


	}

	public class key_value
	{
		public key_value() { }
		public key_value(string Key, string Value)
		{
			this.key = Key;
			this.value = Value;
		}

		public string key { get; set; }
		public string value { get; set; }
	}
}

