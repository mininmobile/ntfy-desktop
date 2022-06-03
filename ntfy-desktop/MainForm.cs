using System;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public partial class MainForm : Form {
		public MainForm() {
			Title = "NTFY Desktop";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(400, 600);

			// create commands
			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About" };
			aboutCommand.Executed += (sender, e) => new CustomAboutDialog().ShowModalAsync();

			var preferencesCommand = new Command { MenuText = "Preferences", Shortcut = Application.Instance.CommonModifier | Keys.Comma };
			preferencesCommand.Executed += (sender, e) => new PreferencesDialog().ShowModalAsync();

			// create menu
			Menu = new MenuBar {
				// application (OS X) or file menu (others)
				ApplicationItems = {
					preferencesCommand,
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar
			ToolBar = new ToolBar { Items = { } };

			// create content
			Content = new StackLayout {
				Padding = 10,
			};
		}
	}
}
