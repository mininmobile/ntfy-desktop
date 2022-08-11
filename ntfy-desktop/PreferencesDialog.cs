using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Net;

namespace ntfy_desktop {
	public class PreferencesDialog : Dialog {
		private AppSettings Settings => AppSettings.Default;
		private Scrollable layout;
		private NTFYD ntfyd;

		public PreferencesDialog(NTFYD _ntfyd) {
			ntfyd = _ntfyd;

			Title = "ntfy Desktop Preferences";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(200, 150);
			Size = new Size(600, 400);
			Resizable = true;

			InitSettings();
		}

		public void InitSettings() {
			var feeds = new TableLayout {
				Spacing = new Size(5, 5), // space between each cell
				Padding = new Padding(0, 10, 0, 10), // space around the table's sides
				Rows = {
					new TableRow(
						new TableCell(new Label { Text = "Domain" }, true),
						new TableCell(new Label { Text = "Topic" }, true),
						new TableCell(null, false)
					),
				}
			};

			// add subscribed feeds
			Settings.Feeds.ForEach((feed) => {
				Command removeCommand = new Command();
				removeCommand.Executed += (sender, e) => {
					Settings.Feeds.Remove(feed);
					Settings.Save();
					UpdateSettings();
				};

				feeds.Rows.Add(new TableRow(
					new TextBox { PlaceholderText = "Domain", Text = feed[0] },
					new TextBox { PlaceholderText = "Topic", Text = feed[1] },
					new Button { Text = "Remove", Width = 100, Command = removeCommand }
				));
			});

			// subscribe button
			var subscribeDomain = "";
			var subscribeTopic = "";
			var subscribeCommand = new Command();
			subscribeCommand.Executed += (sender, e) => {
				if (subscribeDomain.Length > 4 && subscribeDomain.Contains(".")
					&& subscribeTopic.Length > 0) {
					ntfyd.Subscribe(subscribeDomain, subscribeTopic);

					Settings.Feeds.Add(new List<string>{subscribeDomain, subscribeTopic});
					Settings.Save();
					UpdateSettings();
				}
			};

			// subscribe fields
			var subscribeInputDomain = new TextBox { PlaceholderText = "Domain" };
			subscribeInputDomain.TextChanged += (sender, e) => subscribeDomain = subscribeInputDomain.Text;
			var subscribeInputTopic = new TextBox { PlaceholderText = "Topic" };
			subscribeInputTopic.TextChanged += (sender, e) => subscribeTopic = subscribeInputTopic.Text;

			// subscribe row
			feeds.Rows.Add(new TableRow(
				subscribeInputDomain, subscribeInputTopic,
				new Button { Text = "Subscribe", Width = 100, Command = subscribeCommand }
			));

			// create main layout
			Content = new Scrollable{
				Border = BorderType.None,
				Content = new StackLayout{
					Padding = 10,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					Items = {
						new Label { Text = "Subscribed Topics", Font = Fonts.Sans(9, FontStyle.Bold) },
						feeds,
					}
				}
			};
		}

		public void UpdateSettings() {
			InitSettings();
		}
	}
}
