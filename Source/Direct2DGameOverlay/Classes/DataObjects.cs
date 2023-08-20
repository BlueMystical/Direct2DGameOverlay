using DXOverlay.ExternalModule;
using Newtonsoft.Json;
using Process.NET.Windows;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace DirectXOverlay
{
	public class GameInstance
	{
		public GameInstance() { }
        public GameInstance(string pName, string pExeName)
        {
			Name = pName;
            ExeName = pExeName;
            Modules = new List<OverlayModule>();
		}

        public string Name { get; set; }
        public string ExeName { get; set; }

        public List<OverlayModule> Modules { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.ExeName);
		}
	}

    public class OverlayModule
    {
		public OverlayModule() { }
        public OverlayModule(string pName, string pDescripcion)
        {
            Name = pName;
            Description = pDescripcion;
            Layers = new Dictionary<string, LayerEx>();
		}

        public string Name { get; set; }
        public string Description { get; set;}
        public bool Enabled { get; set; } = true;

        public int HotKeys { get; set; }

        public Dictionary<string, LayerEx> Layers { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Description);
		}
	}

	public class LayerEx
    {
        public LayerEx() { }
		public LayerEx(string pName, string pDescripcion)
        {
			Name = pName;
			Description = pDescripcion;
		}

		[Description("Unique name for this layer."), Category("Action")]
		public string Name { get; set; }

		[Description("Description for this layer. (Do NOT use special Symbols!)"), Category("Action")]
		public string Description { get; set; } = string.Empty;

		[DisplayName("Shape"), Description("Shape to Draw on this layer."), Category("Behavior")]  //<- Action, Appearance, Behavior, Data, Default, Design, DragDrop, Format, Key, Layout
		public LayerType Kind { get; set; }

		[Description("Only Visible layers gets drawn."), Category("Behavior")]
		public bool Visible { get; set; } = true;

		[Description("Position X,Y related to the TargetWindow, where this layer will be drawn. (in Pixels)"), Category("Design")]
		[TypeConverter(typeof(ExpandableObjectConverter)), Editor(typeof(PointExINI), typeof(UITypeEditor))]
		public PointEx Location { get; set; }

		[Description("Width and Height of the shape. (in Pixels)"), Category("Design")]
		[TypeConverter(typeof(ExpandableObjectConverter)), Editor(typeof(SizeExINI), typeof(UITypeEditor))]
		public SizeEx Size { get; set; }

		[Description("Colors for the Shape"), Category("Appearance")]
		[TypeConverter(typeof(ExpandableObjectConverter)), Editor(typeof(ColorsExINI), typeof(UITypeEditor))]
		public ColorsEx Colors { get; set; }

		[DisplayName("LineOptions"), Description("For Lines and Borders"), Category("Appearance")]
		[TypeConverter(typeof(ExpandableObjectConverter)), Editor(typeof(LinesExINI), typeof(UITypeEditor))]
		public LinesEx Lines { get; set; }

		[DisplayName("TextOptions"), Description("Only for shapes that contain Texts."), Category("Appearance")]
		[TypeConverter(typeof(ExpandableObjectConverter)), Editor(typeof(TextExINI), typeof(UITypeEditor))]
		public TextEx Text { get; set; }

		[DisplayName("ImageOptions"), Description("Only for Shapes that contain Images"), Category("Appearance")]
		[TypeConverter(typeof(ExpandableObjectConverter)), Editor(typeof(TextureExINI), typeof(UITypeEditor))]
		public TextureEx Texture { get; set; }

		[DisplayName("Geometry"), Description("To Draw Paths and Geometries."), Category("Appearance")]
		[TypeConverter(typeof(ExpandableObjectConverter)), Editor(typeof(GeometryINI), typeof(UITypeEditor))]
		public GeometryEx Geometry { get; set; }

		[JsonIgnore, Browsable(false)]
		public IWindow TargetWindow = null;

		[JsonIgnore, Browsable(false)]
		public CodeCompiler ExternalModule { get; set; }

		/// <summary>Codigo externo para realizar actualizaciones en la Capa.</summary>
		public string CallBackCodeFile { get; set; }

		[JsonIgnore, Browsable(false)]
		public string CallBackCode { get; set; }

		/// <summary>For the CountDown</summary>
		[JsonIgnore, Browsable(false)]
		public DateTime StartTime { get; set; }

		/// <summary>Miliseconds before hiding out (1s = 1000ms)</summary>
		[Description("Time in miliseconds to trigger an action."), Category("Default"), DefaultValue(5)]
		public int TimeOff { get; set; } = 5000;

		public bool Executing { get; set; } = false;


        public event EventHandler OnAction = delegate { }; //<- Evento con Manejador, para evitar los Null

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Name, this.Description);
        }
    }

	public class PointEx
    {
        public PointEx() { }
        public PointEx(string pX, string pY)
        {
            X = pX;
            Y = pY;
        }

		[Description("Horizontal Location. Acepted values: 'ScreenWidth','ScreenHeight','WindowWidth','WindowHeight', [integer value in pixels], acepts basic Math operations (+, -, *, /)")]
		public string X { get; set; }

		[Description("Vertical Location. Acepted values: 'ScreenWidth','ScreenHeight','WindowWidth','WindowHeight', [integer value in pixels], acepts basic Math operations (+, -, *, /)")]
		public string Y { get; set; }

        public Point ToPoint(IWindow TargetWindow)
        {
            Point _ret = Point.Empty;
            if (!string.IsNullOrEmpty(X) && !string.IsNullOrEmpty(Y))
            {
                _ret.X = Helper.ResolveValue(TargetWindow, X);
				_ret.Y = Helper.ResolveValue(TargetWindow, Y);
			}
			return _ret;
        }

		public override string ToString()
		{
			string ret = string.Empty;
			if (!string.IsNullOrEmpty(X))
			{
				ret += string.Format("X:[{0}] ", X);
			}
			if (!string.IsNullOrEmpty(Y))
			{
				ret += string.Format("Y:[{0}]", Y);
			}
			return ret;
		}
	}
	
	public class SizeEx
    {
		public SizeEx() { }
		public SizeEx(string pWidth, string pHeight)
        {
            Width = pWidth;
            Height = pHeight;
        }

		[Description("Width of the shape, acepted values: 'ScreenWidth','ScreenHeight','WindowWidth','WindowHeight', [integer value in pixels], acepts basic Math operations (+, -, *, /)")]
		public string Width { get; set; }

		[Description("Height of the shape, acepted values: 'ScreenWidth','ScreenHeight','WindowWidth','WindowHeight', [integer value in pixels], acepts basic Math operations (+, -, *, /)")]		
		public string Height { get; set; }

        public Size ToSize(IWindow TargetWindow)
        {
            Size _ret = Size.Empty;
            if (!string.IsNullOrEmpty(Width) && !string.IsNullOrEmpty(Height))
            {
				_ret.Width = Helper.ResolveValue(TargetWindow, Width);
				_ret.Height = Helper.ResolveValue(TargetWindow, Height);
			}
            return _ret;
        }

		public override string ToString()
		{
			string ret = string.Empty;
			if (!string.IsNullOrEmpty(Width))
			{
				ret += string.Format("Width:[{0}] ", Width);
			}
			if (!string.IsNullOrEmpty(Height))
			{
				ret += string.Format("Height:[{0}]", Height);
			}
			return ret;
		}
	}

	
	public class ColorsEx
    {
        public ColorsEx() { }

		/// <summary>Key of color in the 'Colors' Dictionary.</summary>
		[TypeConverter(typeof(StringListConverter))]
		[Description("Color for Texts, Borders and Lines.")]
		public string ForeColor { get; set; }

		/// <summary>Key of color in the 'Colors' Dictionary.</summary>
		[TypeConverter(typeof(StringListConverter))]
		[Description("Background color.")]		
		public string BackColor { get; set; }

		public override string ToString()
		{
			string ret = string.Empty;
			if (!string.IsNullOrEmpty(BackColor))
			{
				ret += string.Format("BackColor:[{0}] ", BackColor);
			}
			if (!string.IsNullOrEmpty(ForeColor))
			{
				ret += string.Format("ForeColor:[{0}]", ForeColor);
			}
			return ret;
		}
	}

	public class LinesEx
    {
        public LinesEx() { }

		/// <summary>Border Size or Font Size.</summary>
		[Description("Border Size")]
		public float Stroke { get; set; } = 1.0f;

		/// <summary>Z Axis Deep for 3D objects or Radius for Circles</summary>
		[Description("Z Axis Deep for 3D objects or Radius for Circles.")]
		public int Length { get; set; } = 0;

		[Description("Spacing between")]
		public int Padding { get; set; } = 0;

        public override string ToString()
		{
			string ret = string.Empty;
			if (Stroke > 0)
			{
				ret += string.Format("Stroke:[{0}] ", Stroke);
			}
			if (Length > 0)
			{
				ret += string.Format("Length:[{0}] ", Length);
			}
			if (Padding > 0)
			{
				ret += string.Format("Padding:[{0}]", Padding);
			}
			return ret;
		}
	}
	public class GeometryEx
	{
		public GeometryEx() { }

		[Description("Represents a Geometry which can be drawn by a Graphics device.")]
		public Geometry Geometry { get; set; }

		public StrokeStyle StrokeStyle { get; set; }
	}
	[Serializable]
	public class TextEx
    {
        public TextEx() { }

		[Description("Text to be written")]
		public string Text { get; set; }

		[TypeConverter(typeof(StringListConverter))]
		public string Font { get; set; } = "Consolas,10,Bold";

		public override string ToString()
		{
			string ret = string.Empty;
			if (!string.IsNullOrEmpty(Font))
			{
				ret += string.Format("Font:[{0}] ", Font);
			}
			if (!string.IsNullOrEmpty(Text))
			{
				ret += string.Format("Text:[{0}]", Text);
			}
			return ret;
		}
	}

	[Serializable]
	public class TextureEx
    {
        public TextureEx() { }

		[TypeConverter(typeof(StringListConverter))]
		public string TextureName { get; set; }

		[ReadOnly(true)]
		public string TextureFile { get; set; }

		/// <summary>Transparency 0.0 - 1.0</summary>
		[TypeConverter(typeof(CustomFloatConverter))]
		[DefaultValue(typeof(float), "1")]
		public float Opacity { get; set; } = 1.0f;

		public override string ToString()
		{
			string ret = string.Empty; //"{0}:{1}"
			if (!string.IsNullOrEmpty(TextureName))
			{
				ret += string.Format("TName:[{0}] ", TextureName);
			}
			if (!string.IsNullOrEmpty(TextureFile))
			{
				ret += string.Format("File:[{0}] ", TextureFile);
			}
			if (Opacity > 0)
			{
				ret += string.Format("Opacity:[{0}]", Opacity);
			}
			return ret;
		}
	}

	/* ------------------------------------------------------------------------  */

	public class TextureEditor : UITypeEditor
	{
		private IWindowsFormsEditorService editorService;

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			//if (value != null) // already initialized
			//	return base.EditValue(context, provider, value);

			editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (editorService != null)
			{
				var button1 = new Button { Text = "Add New.." };
				button1.Click += (sender, args) => MessageBox.Show("Button 1 clicked");

				var values = context.PropertyDescriptor.Converter.GetStandardValues(context);

				ListBox listAvailableItems = new ListBox() 
				{
					SelectionMode = SelectionMode.One,
					Width = 192,
					Height = 350,
				};
				if (values != null)
				{
                    foreach (var item in values)
                    {
						listAvailableItems.Items.Add(item);
					}                    
				}
				listAvailableItems.SelectedValueChanged += (object sender, EventArgs e) =>
				{

				};

				var button2 = new Button { Text = "Button 2" };
				button2.Click += (sender, args) => MessageBox.Show("Button 2 clicked");

				var panel = new FlowLayoutPanel { AutoSize = true, Height = 360 };
				panel.Controls.Add(button1);
				panel.Controls.Add(listAvailableItems);

				editorService.DropDownControl(panel);
			}

			return value;
		}
	}

	/* ------------------------------------------------------------------------  */


	/* BOTONES PARA LOS INICIALIZADORES DE LAS PROPIEDADES  */
	public class TextureExINI : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
				return base.EditValue(context, provider, value);

			//Crea una nueva instancia del tipo de la propiedad
			return Activator.CreateInstance(typeof(TextureEx));
		}
	}
	public class ColorsExINI : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
				return base.EditValue(context, provider, value);

			//Crea una nueva instancia del tipo de la propiedad
			return Activator.CreateInstance(typeof(ColorsEx));
		}
	}
	public class LinesExINI : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
				return base.EditValue(context, provider, value);

			//Crea una nueva instancia del tipo de la propiedad
			return Activator.CreateInstance(typeof(LinesEx));
		}
	}
	public class TextExINI : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
				return base.EditValue(context, provider, value);

			//Crea una nueva instancia del tipo de la propiedad
			return Activator.CreateInstance(typeof(TextEx));
		}
	}
	public class GeometryINI : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
				return base.EditValue(context, provider, value);

			//Crea una nueva instancia del tipo de la propiedad
			return Activator.CreateInstance(typeof(GeometryEx));
		}
	}
	public class SizeExINI : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
				return base.EditValue(context, provider, value);

			//Crea una nueva instancia del tipo de la propiedad
			return Activator.CreateInstance(typeof(SizeEx));
		}
	}
	public class PointExINI : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (value != null) // already initialized
				return base.EditValue(context, provider, value);

			//Crea una nueva instancia del tipo de la propiedad
			return Activator.CreateInstance(typeof(PointEx));
		}
	}

	/* ------------------------------------------------------------------------  */

	// Previene errores al convertir numeros con puntos o comas en los decimales
	public class CustomFloatConverter : ExpandableObjectConverter
	{
		TypeConverter converter;
		public CustomFloatConverter()
		{
			converter = TypeDescriptor.GetConverter(typeof(float));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return converter.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				string strValue = value.ToString().Replace('.', ',');
				return converter.ConvertFrom(context, culture, strValue);
			}
			catch (Exception)
			{
				var d = context.PropertyDescriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
				if (d != null)
					return d.Value;
				else
					return 1.0f; // new Size(0, 0);
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			return converter.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return converter.CanConvertTo(context, destinationType);
		}
	}

	/* ------------------------------------------------------------------------  */

	/* MUESTRA UNA LISTA DESPLEGABLE DE ITEMS  */
	public class StringListConverter : TypeConverter
	{
		/// <summary>
		/// Dictionary that maps a combination of type and property name to a list of strings
		/// </summary>
		private static Dictionary<(Type type, string propertyName), IEnumerable<string>> _lists = new Dictionary<(Type type, string propertyName), IEnumerable<string>>();

		public static void RegisterValuesForProperty(Type type, string propertyName, IEnumerable<string> list)
		{
			_lists[(type, propertyName)] = list;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (context != null)
			{
				if (_lists.TryGetValue((context.PropertyDescriptor.ComponentType, context.PropertyDescriptor.Name), out var list))
				{
					return new StandardValuesCollection(list.ToList());
				}
				else
				{
					//throw new Exception("Unknown property " + context.PropertyDescriptor.ComponentType + " " + context.PropertyDescriptor.Name);
					return new StandardValuesCollection(new List<string>());
				}
			}
			else
			{
				return new StandardValuesCollection(new List<string>());
			}
		}
	}

	public class OnResourceAddedArgs : EventArgs
	{
		public Dictionary<string, Color> AddedColors { get; set; }
		public Dictionary<string, Font> AddedFonts { get; set; }
		public List<TextureEx> AddedTextures { get; set; }
        public LayerEx Layer { get; set; }

        public OnResourceAddedArgs() { }
        public OnResourceAddedArgs(Dictionary<string, Color> pAddedColors)
		{
			this.AddedColors = pAddedColors;
		}
        public OnResourceAddedArgs(Dictionary<string, Font> pAddedFonts)
        {
            this.AddedFonts = pAddedFonts;
        }
        public OnResourceAddedArgs(List<TextureEx> pAddedTextures)
        {
            this.AddedTextures = pAddedTextures;
        }
    }
}
