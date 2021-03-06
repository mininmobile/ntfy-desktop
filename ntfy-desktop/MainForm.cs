using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public partial class MainForm : Form {
		private AppSettings Settings => AppSettings.Default;

		Label _debugLabel;
		ButtonMenuItem _trayToggle;

		public MainForm() {
			// initialize settings
			if (Settings.Feeds == null)
				Settings.Feeds = new List<List<string>>();

			// initialize gui
			Title = "ntfy Desktop";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(200, 150);
			Size = new Size(400, 600);

			_debugLabel = new Label { Text = "debug log:\n" };

			// create commands
			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About" };
			aboutCommand.Executed += (sender, e) => new CustomAboutDialog().ShowModalAsync();

			var preferencesCommand = new Command { MenuText = "Preferences", Shortcut = Application.Instance.CommonModifier | Keys.Comma };
			preferencesCommand.Executed += (sender, e) => new PreferencesDialog().ShowModalAsync();

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
					_debugLabel,
				}
			};

			// create ntfy.sh listener
			var ntfyd = new NTFYD();
			ntfyd.MessageReceived += ntfyd_MessageReceived;
			Closed += (sender, e) => ntfyd.DisposeAll();

			// subscribe to all feeds
			Settings.Feeds.ForEach((feed) => {
				_debugLabel.Text += $"subscribed to {feed[0]}/{feed[1]}\n";
				ntfyd.Subscribe(feed[0], feed[1]);
			});

			// minimize to tray
			WindowStateChanged += mainForm_windowStateChanged;
		}

		public void MinimizeToTray() {
			if (WindowState != WindowState.Minimized) {
				Minimize();
			} else {
				_trayToggle.Text = "Show";
				ShowInTaskbar = false;
				Visible = false;
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
					notification.ID = e.message["id"].ToString();
					notification.Message = e.message["message"].ToString();
					notification.Title = e.message["title"]?.ToString();
					notification.Show();
				}

				_debugLabel.Text += e.domain + "/" + e.topic + ": " + e.message.ToJsonString() + "\n";
			});
		}
	}
}
