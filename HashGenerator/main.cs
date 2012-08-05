using Gtk;
using System;

namespace HashGenerator
{
	public class main
	{
		static void Main ()
		{
			Application.Init ();
			new hashgen();
			Application.Run ();
		}
	}
}

