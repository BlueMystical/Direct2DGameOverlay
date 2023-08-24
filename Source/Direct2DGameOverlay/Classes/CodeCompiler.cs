using DirectXOverlay;
using Overlay.NET.Directx;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace DXOverlay.ExternalModule
{
	public class CodeCompiler
	{
		private Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider provider;
		private CompilerParameters parameters = new CompilerParameters();
		private CompilerResults results;
		private Assembly assembly;
		private Type program;
		private MethodInfo main;


		public DirectXOverlayWindow OverlayWindow { get; set; }
		public Direct2DRenderer Graphics { get; set; }
		public Dictionary<string, LayerEx> Layers { get; set; }

		public CodeCompiler(string pCode)
		{
			try
			{
				// RoslynCodeDomProvider for C# Compiler:  https://github.com/aspnet/RoslynCodeDomProvider
				provider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider(new Dictionary<string, string>
				{
					{ "CompilerVersion", "v4.8" }
				});
				parameters = new CompilerParameters();

				// Agregar Referencias a Ensamblados del Framework que necesite el codigo a ejecutar:
				parameters.ReferencedAssemblies.Add("System.dll");                  //<- Obligatorio
				parameters.ReferencedAssemblies.Add("System.Core.dll");             //<- Linq y otros
				parameters.ReferencedAssemblies.Add("System.Drawing.dll");          //<- Pues eso -> Drawing.
				parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");    //<- Mostrar Cuadros de Texto y Ventanas
				parameters.ReferencedAssemblies.Add("System.Net.Http.dll");         //<- Porn watching
				parameters.ReferencedAssemblies.Add("System.Memory.dll");
				parameters.ReferencedAssemblies.Add("System.Xml.dll");
				parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
				parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

				parameters.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay.NET.dll"));
				parameters.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SharpDX.dll"));
				parameters.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SharpDX.Direct2D1.dll"));
				parameters.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SharpDX.DXGI.dll"));				
				parameters.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SharpDX.Mathematics.dll")); 
				parameters.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Newtonsoft.Json.dll"));
				parameters.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Blue.TextDataTable.dll"));

				//Opciones de Generacion:
				parameters.GenerateInMemory = true;     //<- True = Compilacion en Memoria, false = Compilacion en Archivo          
				parameters.GenerateExecutable = true;   //<- True - Generacion de un EXE, false - Generacion de un DLL														
				parameters.TreatWarningsAsErrors = false; // Set whether to treat all warnings as errors.

				if (!string.IsNullOrEmpty(pCode))
				{
					results = provider.CompileAssemblyFromSource(parameters, pCode); //<- Compilar el codigo            
					if (results != null)
					{
						if (results.Errors != null && results.Errors.Count > 0)
						{
                            foreach (var error in results.Errors)
                            {
								throw new Exception( error.ToString());
							}
							return;
                        }
						assembly = results.CompiledAssembly;  //<- Carga en Memoria el Ensamblado Compilado
						program = assembly.GetType("DXOverlay.ExternalModule.Programa"); //<- NameSpace y Nombre de la Clase Definidos en el codigo    
						if (program != null)
						{
							//program.GetProperty
							main = program.GetMethod("Main"); //<- (Tiene que haber un Main)   
							if (main != null)
							{
								main.Invoke(null, null); //<- Invocar al Main (sin parametros)

								/* Solo llamar al Main deberia ser suficiente para la mayoria de Programas, 
								 * pero si hace falta se pueden llamar manualmente a otros metodos (Publicos Estaticos) declarados dentro del codigo */
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "UN-EXPECTED ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			
		}

		private static string CompilerFullPath(string relativePath)
		{
			string frameworkFolder = Path.GetDirectoryName(typeof(object).Assembly.Location);
			return Path.Combine(frameworkFolder, relativePath);
		}

		/// <summary>[To be called from the Overlay] Sets all the available resources and leaves a log</summary>
		/// <param name="pModuleName">Name of this module</param>
		/// <param name="OverlayWindow">A reference to the Overlay Window</param>
		/// <param name="Graphics">A reference to the Graphics Device who has the drawing methods.</param>
		/// <param name="_brushes">List of Available Colors</param>
		/// <param name="_fonts">List of Available Fonts</param>
		/// <param name="_textures">List of Available Images</param>
		internal void Initialize(string pModuleName, DirectXOverlayWindow pOverlayWindow, Direct2DRenderer pGraphics, Dictionary<string, int> brushes, Dictionary<string, int> fonts, Dictionary<string, int> textures)
		{
			if (program != null)
			{
				MethodInfo func = program.GetMethod("Initialize");
				if (func != null)
				{
					object result = func.Invoke(null, new object[] //<- Multiples parametros 
					{ 
						Path.GetFileNameWithoutExtension(pModuleName),
						pOverlayWindow, pGraphics,
						brushes, fonts, 
						textures, true 
					});
					
					if (result != null)
					{
						if (results.Errors != null && results.Errors.Count > 0)
						{
							foreach (var error in results.Errors)
							{
								throw new Exception(error.ToString());
							}
							return;
						}
					}
				}
			}
		}

		/// <summary>This calls the 'Render' method in the External Module who will draw stuff on the current layer.
		/// This method is called from the Parent Overlay once per rendered frame, which means around 60 times per second.</summary>
		public bool RenderCode()
		{
			bool _ret = false;
			if (results != null)
			{
				if (results.Errors.HasErrors)
				{
					StringBuilder sb = new StringBuilder();
					foreach (CompilerError error in results.Errors)
					{
						sb.AppendLine(string.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
					}
					MessageBox.Show(sb.ToString(), "ERROR:", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					if (program != null)
					{
						//Ejecutar una Funcion pasandole parametros y hacer algo con el resultado:
						MethodInfo func = program.GetMethod("Render"); //<- Nombre de un Metodo (o Funcion) a invocar
						if (func != null)
						{
							object result = func.Invoke(null, null); //<- NO parametros 
							if (result != null)
							{
								string _Response = Convert.ToString(result);
								if (string.IsNullOrEmpty(_Response) || _Response != "ok")
								{
									// An Error ocurred
									_ret = false;
								}
								else
								{
									//Success! method was executed correctly.
									_ret = true;
								}
							}
						}

						// Otros ejemplos:
						/*
						MethodInfo func = program.GetMethod("Calcular"); //<- Nombre de un Metodo (o Funcion) a invocar
						if (func != null)
						{
							object result = func.Invoke(null, new object[] { 2, 3 }); //<- Multiples parametros tipo entero
							if (result != null)
							{
								MessageBox.Show(string.Format("El numero: '{0}' no tiene tanta suerte.", result.ToString()), "Función Ejecutada!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							}
						}
						*/
						/* ** Este metodo Interactua con el Usuario ** 
						DialogResult _ret = DialogResult.None;
						MethodInfo Method = program.GetMethod("CrearVentana"); //<- Nombre de un Metodo (o Funcion) a invocar
						if (Method != null)
						{
							_ret = (DialogResult)Method.Invoke(null, null);
						}

						//Ejecutar otro Metodo declarado en el codigo:							
						MethodInfo DQquery = program.GetMethod("ExecuteQuery_ODBC"); //<- Nombre del Metodo a invocar
						if (DQquery != null)
						{
							//Es una Consulta a Base de Datos
							string Query = "SELECT FIRST 20 pers_identificador, cedula, nombre_persona, sexo, estado_civil, edad ";
							Query += "FROM vs_per_personas_01 where(edad BETWEEN 15 AND 18) AND sexo = 'FEMENINO' ";
							Query += "AND cedula IS NOT NULL ORDER BY 3;";

							object result = DQquery.Invoke(null, new object[] { Query }); //<- Unico parametro tipo string
							if (result != null)
							{
								DataTable DT = (DataTable)result;
								switch (_ret)
								{
									case DialogResult.OK:
										if (MessageBox.Show(string.Format("{0} Chicas lindas!! ;-p\r\nQuieres Conocerlas?  ;-)", DT.Rows.Count.ToString()), "Tu Destino:",
											MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
										{
											StringBuilder Texto = new StringBuilder();
											foreach (DataRow Chica in DT.Rows)
											{
												Texto.AppendLine(string.Format("{0}, {1} años.", Chica["nombre_persona"].ToString(), Chica["edad"].ToString()));
											}
											MessageBox.Show(Texto.ToString(), "Chicas", MessageBoxButtons.OK, MessageBoxIcon.Information);
										}
										break;

									case DialogResult.Cancel:
										MessageBox.Show(string.Format("{0} Chicas lindas!! ;-p", DT.Rows.Count.ToString()), "Te las Perdiste!", MessageBoxButtons.OK, MessageBoxIcon.Information);
										break;

									default:
										MessageBox.Show("Deberias haber elejido!", ".", MessageBoxButtons.OK, MessageBoxIcon.Information);
										break;
								}

							}
						}
						*/

					}
				}
			}
			return _ret;
		}

		
	}
}
