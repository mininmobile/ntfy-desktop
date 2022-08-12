using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using System.Text.Json.Nodes;

namespace ntfy_desktop {
	public partial class MainForm : Form {
		private AppSettings Settings => AppSettings.Default;
		private NTFYD ntfyd;

		DebugLog debugLog;
		DebugLogForm currentDebugLogForm;
		ButtonMenuItem _trayToggle;

		public MainForm() {
			// initialize debug log
			debugLog = new DebugLog();

			// initialize settings
			if (Settings.Feeds == null)
				Settings.Feeds = new List<List<string>>();

			// initialize gui
			Title = "ntfy Desktop";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(200, 150);
			Size = new Size(400, 600);

			Closed += (sender, e) => Application.Instance.Quit();

			// create commands
			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About" };
			aboutCommand.Executed += (sender, e) => new CustomAboutDialog().ShowModalAsync();

			var preferencesCommand = new Command { MenuText = "Preferences", Shortcut = Application.Instance.CommonModifier | Keys.Comma };
			preferencesCommand.Executed += (sender, e) => new PreferencesDialog(ntfyd).ShowModalAsync();

			var debugLogCommand = new Command { MenuText = "Debug Log", Shortcut = Application.Instance.AlternateModifier | Keys.D };
			debugLogCommand.Executed += (sender, e) => {
				if (currentDebugLogForm == null) {
					currentDebugLogForm = new DebugLogForm(debugLog);
					currentDebugLogForm.Closed += (_sender, _e) => currentDebugLogForm = null;
					currentDebugLogForm.Show();
				} else {
					currentDebugLogForm.BringToFront();
				}
			};

			var toggleTrayCommand = new Command();
			toggleTrayCommand.Executed += _toggleMinimizedToTray;

			_trayToggle = new ButtonMenuItem{
				Text = "Hide",
				Command = toggleTrayCommand,
			};

			// tray icon
			var tray = new TrayIndicator {
				Image = Utility.ApplicationLogo,
				Title = "ntfy Desktop",
				Menu = new ContextMenu {
					Items = {
						_trayToggle,
						new Command { MenuText = "Quit", DelegatedCommand = quitCommand },
					}
				}
			};
			tray.Activated += _toggleMinimizedToTray;
			tray.Show();

			// create menu
			Menu = new MenuBar {
				// application (OS X) or file menu (others)
				HelpItems = {
					debugLogCommand,
				},
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
					// todo
				}
			};

			// create ntfy.sh listener
			ntfyd = new NTFYD(debugLog);
			ntfyd.MessageReceived += ntfyd_MessageReceived;
			Closed += (sender, e) => ntfyd.DisposeAll();

			// minimize to tray
			WindowStateChanged += mainForm_windowStateChanged;
		}

		public void MinimizeToTray() {
			if (WindowState != WindowState.Minimized) {
				Minimize();
			} else {
				_trayToggle.Text = "Show";
				ShowInTaskbar = false;
				Visible = true;
			}
		}

		public void OpenFromTray(bool force = false) {
			if (WindowState == WindowState.Minimized || force) {
				_trayToggle.Text = "Hide";
				ShowInTaskbar = true;
				Visible = true;
				WindowState = WindowState.Normal;
				BringToFront();
			}
		}

		void _toggleMinimizedToTray(object sender, EventArgs e)  {
			if (WindowState == WindowState.Minimized)
				OpenFromTray();
			else
				MinimizeToTray();
		}

		void mainForm_windowStateChanged(object sender, EventArgs e) {
			if (WindowState == WindowState.Minimized)
				MinimizeToTray();
		}

		void ntfyd_MessageReceived(object sender, EventArgs _e) {
			MessageReceivedEventArgs e = (MessageReceivedEventArgs)_e;
			Application.Instance.Invoke(() => {
				using (var notification = new Notification()) {
					notification.ID = e.json["id"].ToString();
					notification.Message = e.json["message"].ToString();
					notification.Title = e.json["title"]?.ToString();
					notification.Show();
				}

				debugLog.Log($"→ {new DateTime((long)e.json["time"] * 1000).ToShortTimeString()} | {e.domain}/{e.topic}");
				foreach (var entry in e.json.AsObject()) {
					if (entry.Key == "time" || entry.Key == "event" || entry.Key == "topic")
						continue;

					debugLog.Log($"· {entry.Key}: {entry.Value.ToJsonString()}");
				}
			});
		}
	}
}
