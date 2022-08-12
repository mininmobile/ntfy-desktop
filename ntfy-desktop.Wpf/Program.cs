using System;
using Eto.Forms;

namespace ntfy_desktop.Wpf
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application app = new Application(Eto.Platforms.Wpf);
			app.NotificationActivated += (sender, e) => ((MainForm)app.MainForm).OpenFromTray(true);
			app.Run(new MainForm());
		}
	}
}
