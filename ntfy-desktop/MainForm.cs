using System;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public partial class MainForm : Form {
		public Label debugLabel;
		public MainForm() {
			Title = "ntfy Desktop";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(400, 600);

			debugLabel = new Label { Text = "debug log:\n" };

			// create commands
			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About" };
			aboutCommand.Executed += (sender, e) => new CustomAboutDialog().ShowModalAsync();

			var preferencesCommand = new Command { MenuText = "Preferences", Shortcut = Application.Instance.CommonModifier | Keys.Comma };
			preferencesCommand.Executed += (sender, e) => new PreferencesDialog().ShowModalAsync();

			// tray icon
			var tray = new TrayIndicator {
				Image = Utility.ApplicationLogo,
				Title = "ntfy Desktop",
				Menu = new ContextMenu {
					Items = {
						// show/hide
						aboutCommand,
						new Command { MenuText = "Preferences", DelegatedCommand = preferencesCommand },
						new Command { MenuText = "Quit", DelegatedCommand = quitCommand },
					}
				}
			};
			tray.Show();

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
				Items = {
					debugLabel,
				}
			};

			// create ntfy.sh listener
			var ntfyd = new NTFYD();
			ntfyd.Subscribe("ntfy.sh", "balls");
			ntfyd.MessageReceived += ntfyd_MessageReceived;
			Closed += (sender, e) => ntfyd.DisposeAll();
		}

		public void ntfyd_MessageReceived(object sender, System.EventArgs e) {
			Application.Instance.Invoke(() => {
				debugLabel.Text += ((MessageReceivedEventArgs)e).domain + "/" + ((MessageReceivedEventArgs)e).topic + ": " + ((MessageReceivedEventArgs)e).message + "\n";
			});
		}
	}
}
