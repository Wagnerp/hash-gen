using Gtk;
using Gdk;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: AssemblyVersion("1.0")]
[assembly: AssemblyCopyright("Jamie Cerretelli 2012, England")]
[assembly: AssemblyTitle("Hash Generator")]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]

namespace HashGenerator
{
	public partial class hashgen
	{
		private static Assembly self;
		private static Pixbuf icon;
		private static ListStore hashStore;
		private static TreeModelFilter filter;
		private static Entry entry;
		private static Entry search;
		private static ComboBox cboType;
		
		public hashgen ()
		{
			self = Assembly.GetExecutingAssembly ();
			
			Stream ico = self.GetManifestResourceStream ("HashGenerator.icon.png");
			icon = new Pixbuf (ico);
			
			// Window
			Gtk.Window window = new Gtk.Window ((self.GetCustomAttributes (typeof(AssemblyTitleAttribute), false) [0] as AssemblyTitleAttribute).Title);
			window.SetDefaultSize (600, 500);
			window.DeleteEvent += OnDelete;
			window.Icon = icon;
			
			#region [Widgets]
			Box topBox = new VBox ();
			topBox.Homogeneous = false;
			
			// Toolbar
			Toolbar tb = new Toolbar ();
			
			// Toolbar New Button
			ToolButton tb_New = new ToolButton (Stock.New);
			tb_New.Clicked += OnToolbarNew_Clicked;
			tb.Add (tb_New);
			
			// Toolbar Open Button
			ToolButton tb_Open = new ToolButton (Stock.Open);
			tb_Open.Clicked += OnToolbarOpen_Clicked;
			tb.Add (tb_Open);
			
			// Toolbar Save Button
			ToolButton tb_Save = new ToolButton (Stock.Save);
			tb_Save.Clicked += OnToolbarSave_Clicked;
			tb.Add (tb_Save);
			
			tb.Add (new SeparatorToolItem ());
			
			// Toolbar About Button
			ToolButton tb_About = new ToolButton (Stock.About);
			tb_About.Clicked += new EventHandler (OnToolbarAbout_Clicked);
			tb.Add (tb_About);
			
			tb.ShowAll ();
			topBox.PackStart (tb, false, true, 0);
			
			Box mbox = new VBox ();
			topBox.PackStart (mbox, false, true, 0);
			
			Box mhbx = new HBox ();
			mbox.PackStart (mhbx, false, true, 0);
			
			entry = new Entry ();
			mhbx.PackStart (entry, true, true, 0);
			
			Button btnFile = new Button ();
			btnFile.Image = Gtk.Image.NewFromIconName (Stock.File, IconSize.Button);
			btnFile.Clicked += OnBtnFile_Clicked;
			mhbx.PackStart (btnFile, false, true, 0);
			
			string[] htype = { "MD5", "SHA128", "SHA256", "SHA512", "CRC32" };
			cboType = new ComboBox (htype);
			cboType.Active = 0;
			mhbx.PackStart (cboType, false, false, 0);
			
			Button btnHash = new Button ("Generate");
			btnHash.Clicked += OnBtnHash_Clicked;
			mhbx.PackStart (btnHash, false, false, 0);
			
			// Tree
			ScrolledWindow sw = new ScrolledWindow ();
			topBox.PackStart (sw, true, true, 0);
			TreeView tree = new TreeView ();
			sw.Add (tree);
			
			TreeViewColumn inColu = new TreeViewColumn ();
			inColu.Title = "Input";
			CellRendererText inCell = new CellRendererText ();
			inColu.PackStart (inCell, true);
			
			TreeViewColumn typeColu = new TreeViewColumn ();
			typeColu.Title = "Type";
			CellRendererText typeCell = new CellRendererText ();
			typeColu.PackStart (typeCell, true);
			
			TreeViewColumn hashColu = new TreeViewColumn ();
			hashColu.Title = "Hash";
			CellRendererText hashCell = new CellRendererText ();
			hashCell.Editable = true;
			hashColu.PackStart (hashCell, true);
			
			tree.AppendColumn (inColu);
			tree.AppendColumn (typeColu);
			tree.AppendColumn (hashColu);
			
			inColu.AddAttribute (inCell, "text", 0);
			typeColu.AddAttribute (typeCell, "text", 1);
			hashColu.AddAttribute (hashCell, "text", 2);
			hashStore = new ListStore (typeof(string), typeof(string), typeof(string));
			
			// Search Filter
			filter = new TreeModelFilter (hashStore, null);
			filter.VisibleFunc = SearchTree;
			
			tree.Model = filter;
			
			// StatusBar
			Statusbar sb = new Statusbar ();
			topBox.PackStart (sb, false, true, 0);
			
			sb.Add (new Label ("Search:"));			
			search = new Entry ();
			search.Changed += OnSearch;
			sb.Add (search);
			
			#endregion
			window.Add (topBox);
			window.ShowAll ();
		}
		
		static void OnDelete(object obj, DeleteEventArgs args)
		{
			Application.Quit ();
		}
		
		static void PushToView(string input, string type, string hash)
		{
			hashStore.AppendValues (input, type, hash);
		}
		
		static void OnSearch (object obj, EventArgs args)
		{
			filter.Refilter ();
		}
		
		// Search
		static bool SearchTree (TreeModel model, TreeIter iter)
		{
			if (search.Text.Trim ().Length < 1)
				return true;
			
			string t = (string)model.GetValue (iter, 0);
			return t.StartsWith (search.Text.Trim ());
		}
		
		// OnBtnFile Clicked
		static void OnBtnFile_Clicked (object obj, EventArgs args)
		{
			FileChooserDialog fc = new FileChooserDialog (
				"File to hash",
				null,
				FileChooserAction.Open,
				"Cancel",
				ResponseType.Cancel,
				"Open",
				ResponseType.Accept
			);
			
			if (fc.Run () == (int)ResponseType.Accept) {
				byte[] rb = File.ReadAllBytes (fc.Filename);
				
				string name = fc.Filename.Substring (fc.Filename.LastIndexOf ("/") + 1);
				PushToView (name, cboType.ActiveText.ToString (), Hash (Encoding.UTF8.GetString (rb), cboType.ActiveText.ToString ()));
			}
			fc.Destroy();
		}
		
		// OnBtnHash Clicked
		static void OnBtnHash_Clicked(object obj, EventArgs args)
		{
			PushToView (entry.Text.ToString (), cboType.ActiveText.ToString (), Hash(entry.Text.ToString (), cboType.ActiveText.ToString ()));
		}
		
		// Toolbar New
		static void OnToolbarNew_Clicked (object obj, EventArgs args)
		{
			hashStore.Clear ();
		}
		
		// Toolbar Open
		static void OnToolbarOpen_Clicked (object obj, EventArgs args)
		{
			FileChooserDialog of = new FileChooserDialog (
				"Open",
				null,
				FileChooserAction.Open,
				"Cancel",
				ResponseType.Cancel,
				"Open",
				ResponseType.Accept
			);
			
			if (of.Run () == (int)ResponseType.Accept) {
				using (TextReader rd = new StreamReader (of.Filename)) {
					string data = string.Empty;
					while ((data = rd.ReadLine ()) != null) {
						string[] ds = new string[2];
						ds [0] = data.Substring (0, data.LastIndexOf (":"));
						ds [1] = data.Substring (data.LastIndexOf (":") + 1);
						
						int len = ds [1].Length;
						
						switch (len) {
						case 8:
							PushToView (ds [0], "CRC32", ds [1]);
							break;
						case 32:
							PushToView (ds [0], "MD5", ds [1]);
							break;
						case 42:
							PushToView (ds [0], "SHA128", ds [1]);
							break;
						case 66:
							PushToView (ds [0], "SHA256", ds [1]);
							break;
						case 130:
							PushToView (ds [0], "SHA512", ds [1]);
							break;
						default:
							break;
						}
					}
				}
			}
			of.Destroy ();			
		}
		
		// Toolbar Save
		static void OnToolbarSave_Clicked (object obj, EventArgs args)
		{
			FileChooserDialog sf = new FileChooserDialog (
				"Save",
				null,
				FileChooserAction.Save,
				"Cancel",
				ResponseType.Cancel,
				"Save",
				ResponseType.Accept
			);
			
			if (sf.Run () == (int)ResponseType.Accept) {
				string filename = sf.Filename.ToString ();
				
				TextWriter tw = new StreamWriter (filename, true);
				
				TreeIter iter = new TreeIter ();
				hashStore.GetIterFirst (out iter);
				while (hashStore.IterIsValid (iter)) {
					string a = string.Format (
					"{0}:{1}",
					(string)hashStore.GetValue (iter, 0),
					(string)hashStore.GetValue (iter, 2)
					);
					tw.WriteLine (a);
					hashStore.IterNext (ref iter);
				}				
				tw.Close ();
			}			
			sf.Destroy ();
		}
		
		// Toolbar About
		static void OnToolbarAbout_Clicked (object obj, EventArgs args)
		{
			AboutDialog dialog = new AboutDialog ();
			
			string [] authors = new string [] { "Jamie Cerretelli <jamie@cerretelli.co.uk>" };
			
			string license = @"Copyright (C) 2012 Jamie Cerretelli <jamie@cerretelli.co.uk>
Usage of the works is permitted provided that this instrument is retained with the works, 
so that any entity that uses the works is notified of this instrument.
DISCLAIMER: THE WORKS ARE WITHOUT WARRANTY.";
			
			dialog.ProgramName = (self.GetCustomAttributes (typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute).Title;
			dialog.Version = self.GetName ().Version.ToString ();
			dialog.Copyright = (self.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false) [0]	as AssemblyCopyrightAttribute).Copyright;
			dialog.License = license;
			dialog.Authors = authors;
			dialog.Logo = icon;
			dialog.Run();
			
			dialog.Destroy ();
			
		}
	}
}

