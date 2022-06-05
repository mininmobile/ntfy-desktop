using System;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public class PreferencesDialog : Dialog {
		private AppSettings Settings => AppSettings.Default;

		public PreferencesDialog() {
			Title = "ntfy Desktop Preferences";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(200, 150);
			Size = new Size(600, 400);
			Resizable = true;

			// initialize feed settings
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
				feeds.Rows.Add(new TableRow(
					new TextBox { PlaceholderText = "Domain", Text = feed[0] },
					new TextBox { PlaceholderText = "Topic", Text = feed[1] },
					new Button { Text = "Remove", Width = 100 }
				));
			});
			// add subscriber
			feeds.Rows.Add(new TableRow(
				new TextBox { PlaceholderText = "Domain" },
				new TextBox { PlaceholderText = "Topic" },
				new Button { Text = "Subscribe", Width = 100 }
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
	}
}
