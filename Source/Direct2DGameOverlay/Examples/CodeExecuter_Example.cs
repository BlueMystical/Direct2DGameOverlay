
using Newtonsoft.Json.Linq;
using Overlay.NET.Directx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
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

		private static bool JournalWatcherIsRunning = true;
		private static string WatchedFile = string.Empty;

		///// <summary>Main method is Mandatory.</summary>
		//[STAThread]
		//public static void Main()
		//{
		//	// Here you can call methods who do non drawing or 'Rendering' stuff.
		//	// This method is invoked only once
		//	ReadPlayerJournal();
		//}

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

				index = 0; // when jumping from one star system to another
				index = JsonLine.IndexOf("\"event\":\"FSDJump\"", index);
				if (index != -1)
				{
					dynamic data = JObject.Parse(JsonLine);
					if (data != null)
					{
						StringBuilder MsgLines = new StringBuilder();
						MsgLines.AppendLine(string.Format("Welcome to '{0}'! You jumped {1:n1}LYs", data.StarSystem, data.JumpDist));
						MsgLines.AppendLine(string.Format("Allegiance:	{0}", data.SystemAllegiance));
						MsgLines.AppendLine(string.Format("Economy:	{0} - {1}", data.SystemEconomy_Localised, data.SystemSecondEconomy_Localised));
						MsgLines.AppendLine(string.Format("Government:	{0}", data.SystemGovernment_Localised));
						MsgLines.AppendLine(string.Format("Security:	{0}", data.SystemSecurity_Localised));
						MsgLines.AppendLine(string.Format("Population:	{0:n0}", data.Population));

						if (data.SystemFaction != null)
						{
							MsgLines.AppendLine(string.Format("Owner Faction: {0:n0}", data.SystemFaction.Name));
							MsgLines.AppendLine(string.Format("Faction State: {0:n0}", data.SystemFaction.FactionState));
						}

						if (data.Conflicts != null)
						{
							MsgLines.AppendLine(string.Format("Conflicts: {0:n0}", data.Conflicts.Count));
							foreach (var conflict in data.Conflicts)
							{
								MsgLines.AppendLine(string.Format("	{0} {1}: '{2}' vs '{3}'", conflict.WarType, conflict.Status, conflict.Faction1.Name, conflict.Faction2.Name));
							}
						}

						if (data.ThargoidWar)
						{
							MsgLines.AppendLine(string.Format("ThargoidWar: {0} {1}", data.ThargoidWar.CurrentState, data.ThargoidWar.WarProgress));
						}

						LogMessage(MsgLines.ToString());

						int[] numeros = new int[] { 10, 20, 30, 40 };
						
					}
				}

				/*
				{ "timestamp":"2023-07-30T18:05:21Z", "event":"FSDJump", 
					"Taxi":true, "Multicrew":false, 
					"StarSystem":"Hranit", "SystemAddress":5068732310921, "StarPos":[-11.59375,-80.62500,-66.21875], 
					"SystemAllegiance":"Independent", 
					"SystemEconomy":"$economy_Industrial;", "SystemEconomy_Localised":"Industrial", "SystemSecondEconomy":"$economy_Extraction;", "SystemSecondEconomy_Localised":"Extraction", 
					"SystemGovernment":"$government_Corporate;", "SystemGovernment_Localised":"Corporate", 
					"SystemSecurity":"$SYSTEM_SECURITY_medium;", "SystemSecurity_Localised":"Medium Security", 
					"Population":9961728, 
					"Body":"Hranit", "BodyID":0, "BodyType":"Star", 
					"Powers":[ "Li Yong-Rui" ], "PowerplayState":"Exploited", 
					"JumpDist":13.402, "FuelUsed":0.633262, "FuelLevel":7.366738, 
					"Factions":[ { "Name":"Hranit Democrats", "FactionState":"None", "Government":"Democracy", "Influence":0.120000, "Allegiance":"Federation", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, 
								 { "Name":"Bureau of Hranit League", "FactionState":"None", "Government":"Dictatorship", "Influence":0.038000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, 
								 { "Name":"Pikum Public Group", "FactionState":"InfrastructureFailure", "Government":"Corporate", "Influence":0.578000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":3.300000, 
										"PendingStates":[ { "State":"Expansion", "Trend":0 } ], 
										"ActiveStates":[ { "State":"InfrastructureFailure" } ] }, 
								{ "Name":"Hranit Galactic Solutions", "FactionState":"None", "Government":"Corporate", "Influence":0.054000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 }, 
								{ "Name":"United Hranit Liberty Party", "FactionState":"CivilWar", "Government":"Dictatorship", "Influence":0.076000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000, 
										"ActiveStates":[ { "State":"CivilWar" } ] }, 
								{ "Name":"The Guardians of AeSir", "FactionState":"CivilWar", "Government":"Corporate", "Influence":0.076000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000, 
										"ActiveStates":[ { "State":"CivilWar" } ] }, { "Name":"11th Squadron", "FactionState":"None", "Government":"Corporate", "Influence":0.058000, "Allegiance":"Independent", "Happiness":"$Faction_HappinessBand2;", "Happiness_Localised":"Happy", "MyReputation":0.000000 } ], 
					"SystemFaction":{ "Name":"Pikum Public Group", "FactionState":"InfrastructureFailure" }, 
					"Conflicts":[
						{
							"WarType":"war",
							"Status":"active",
							"Faction1":{ "Name":"Independents of HIP 9599", "Stake":"Swanwick Vision", "WonDays":2 },
							"Faction2":{ "Name":"Inhers Silver Natural Company", "Stake":"", "WonDays":0 }
						},
						{
							"WarType":"civilwar",
							"Status":"pending",
							"Faction1":{ "Name":"Inhers Alliance", "Stake":"", "WonDays":0 },
							"Faction2":{ "Name":"Inhers Ltd", "Stake":"Carpenter Point", "WonDays":0 }
						}
					]
				}	
				---------------------------------------------------------------
				{ "timestamp":"2023-07-30T02:43:38Z", "event":"FSDJump", 
					"Taxi":false, "Multicrew":false, 
					"StarSystem":"Tascheter Sector JH-V a2-3", "SystemAddress":58141508913936, "StarPos":[-15.28125,-38.78125,-84.90625], 
					"SystemAllegiance":"", 
					"SystemEconomy":"$economy_None;", "SystemEconomy_Localised":"None", "SystemSecondEconomy":"$economy_None;", "SystemSecondEconomy_Localised":"None", 
					"SystemGovernment":"$government_None;", "SystemGovernment_Localised":"None", 
					"SystemSecurity":"$GAlAXY_MAP_INFO_state_anarchy;", "SystemSecurity_Localised":"Anarchy", 
					"Population":0, 
					"Body":"Tascheter Sector JH-V a2-3", "BodyID":0, "BodyType":"Star", 
					"JumpDist":30.395, "FuelUsed":2.697782, "FuelLevel":13.302217 
				}
				*/


				//TODO: Add other event detections here.
			}
			catch (Exception ex)
			{
				LogExeption(ex);
			}
		}


	}
}

