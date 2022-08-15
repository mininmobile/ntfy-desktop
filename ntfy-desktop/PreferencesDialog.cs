using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Net;

namespace ntfy_desktop {
	public class PreferencesDialog : Dialog {
		private AppConfig Settings => AppSettings.Default;
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
			#region feeds
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
					Settings.Feeds.Add(new List<string>{subscribeDomain, subscribeTopic});
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
			#endregion

			#region theme
			var theme = new TableLayout {
				Spacing = new Size(5, 5), // space between each cell
				Padding = new Padding(0, 10, 0, 10), // space around the table's sides
				Rows = { new TableRow("Preview",
					new TableCell(" R", true), new TableCell(null, false),
					new TableCell(" G", true), new TableCell(null, false),
					new TableCell(" B", true), new TableCell(null, false),
					new TableCell(" A", true), new TableCell(null, false))
				},
			};

			for (var _i = 0; _i < Settings.Theme.Count; _i++) {
				var i = _i;
				var color = Settings.Theme[i];

				var preview = new Panel{
					BackgroundColor = color.ToColor(),
				};

				// red
				var rSlider = new Slider{ Value = color.R, MinValue = 0, MaxValue = 255, Width = 60 };
				var rBox = new NumericStepper{ Value = color.R, MinValue = 0, MaxValue = 255, Width = 60 };
				rSlider.ValueChanged += (sender, e) => {
					if (!rSlider.HasFocus) return;
					rBox.Value = Settings.Theme[i].R = rSlider.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor();
					AppSettings.UpdateTemp();
				};
				rBox.ValueChanged += (sender, e) => { if (!rBox.HasFocus) return;
					rSlider.Value = Settings.Theme[i].R = (int)rBox.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor(); AppSettings.UpdateTemp(); };

				// green
				var gSlider = new Slider{ Value = color.G, MinValue = 0, MaxValue = 255, Width = 60 };
				var gBox = new NumericStepper{ Value = color.G, MinValue = 0, MaxValue = 255, Width = 60 };
				gSlider.ValueChanged += (sender, e) => { if (!gSlider.HasFocus) return;
					gBox.Value = Settings.Theme[i].G = gSlider.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor(); AppSettings.UpdateTemp(); };
				gBox.ValueChanged += (sender, e) => { if (!gBox.HasFocus) return;
					gSlider.Value = Settings.Theme[i].B = (int)gBox.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor(); AppSettings.UpdateTemp(); };

				// blue
				var bSlider = new Slider{ Value = color.B, MinValue = 0, MaxValue = 255, Width = 60 };
				var bBox = new NumericStepper{ Value = color.B, MinValue = 0, MaxValue = 255, Width = 60 };
				bSlider.ValueChanged += (sender, e) => { if (!bSlider.HasFocus) return;
					bBox.Value = Settings.Theme[i].B = bSlider.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor(); AppSettings.UpdateTemp(); };
				bBox.ValueChanged += (sender, e) => { if (!bBox.HasFocus) return;
					bSlider.Value = Settings.Theme[i].B = (int)bBox.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor(); AppSettings.UpdateTemp(); };

				// alpha
				var aSlider = new Slider{ Value = color.A, MinValue = 0, MaxValue = 255, Width = 60 };
				var aBox = new NumericStepper{ Value = color.A, MinValue = 0, MaxValue = 255, Width = 60 };
				aSlider.ValueChanged += (sender, e) => { if (!aSlider.HasFocus) return;
					aBox.Value = Settings.Theme[i].A = aSlider.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor(); AppSettings.UpdateTemp(); };
				aBox.ValueChanged += (sender, e) => { if (!aBox.HasFocus) return;
					aSlider.Value = Settings.Theme[i].A = (int)aBox.Value;
					preview.BackgroundColor = Settings.Theme[i].ToColor(); AppSettings.UpdateTemp(); };

				// add row
				theme.Rows.Add(new TableRow(preview, rSlider, rBox, gSlider, gBox, bSlider, bBox, aSlider, aBox));
			}
			#endregion

			// create main layout
			var okCommand = new Command(); okCommand.Executed += (sender, e) => {
				AppSettings.Save();
				Close();
			};
			var applyCommand = new Command(); applyCommand.Executed += (sender, e) => {
				AppSettings.Save();
			};
			var cancelCommand = new Command(); cancelCommand.Executed += (sender, e) => {
				AppSettings.Revert();
				Close();
			};

			Content = new Scrollable{
				Border = BorderType.None,
				Content = new StackLayout{
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					Items = {
						new StackLayout {
							Padding = 10,
							HorizontalContentAlignment = HorizontalAlignment.Stretch,
							Items = {
								new Label { Text = "Subscribed Topics", Font = Fonts.Sans(9, FontStyle.Bold) },
								feeds,
								new Label { Text = "Feed Theme", Font = Fonts.Sans(9, FontStyle.Bold) },
								theme,
							}
						},
						new StackLayout {
							Padding = new Padding(10, 0),
							Spacing = 10,
							Orientation = Orientation.Horizontal,
							HorizontalContentAlignment = HorizontalAlignment.Right,
							Items = {
								null,
								new Button { Text = "Ok", Width = 80, Command = okCommand },
								new Button { Text = "Cancel", Width = 80, Command = cancelCommand },
								new Button { Text = "Apply", Width = 80, Command = applyCommand }
							}
						}
					}
				}
			};
		}

		public void UpdateSettings() {
			InitSettings();
		}
	}
}
