using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;

namespace ntfy_desktop {
	class NewNoteDialog : Dialog {
		private AppSettings Settings => AppSettings.Default;
		private DebugLog debugLog;
		private ListItemCollection priorities = new ListItemCollection();

		string noteTitle = "";
		string noteMessage = "";
		string noteTags = "";
		int    notePriority = 3;
		int selectedIndex = 0;
		string destinationDomain = "";
		string destinationTopic = "";

		public NewNoteDialog(DebugLog dl) {
			debugLog = dl;

			priorities.Add("Min Priority");
			priorities.Add("Low Priority");
			priorities.Add("Default Priority");
			priorities.Add("High Priority");
			priorities.Add("Max Priority");

			destinationDomain = Settings.Feeds[0][0];
			destinationTopic = Settings.Feeds[0][1];

			Title = "New Note";
			Icon = Utility.ApplicationIcon;
			Resizable = false;

			InitContent();
		}

		private void InitContent(bool large = false) {
			Size = new Size(600, large ? 400 : 200);

			var sendLocations = new ListItemCollection();
			Settings.Feeds.ForEach((f) => sendLocations.Add(f[0] + "/" + f[1]));
			var sendDropdown =  new DropDown { SelectedIndex = selectedIndex, DataStore = sendLocations };
			sendDropdown.SelectedIndexChanged += (sender, e) => {
				selectedIndex = sendDropdown.SelectedIndex;
				var selected = Settings.Feeds[selectedIndex];
				destinationDomain = selected[0]; destinationTopic = selected[1];
			};

			var titleField = new TextBox { Text = noteTitle, PlaceholderText = "Title (optional)" };
			titleField.TextChanged += (sender, e) => noteTitle = titleField.Text;
			var messageField = new TextArea { Text = noteMessage, Wrap = true, AcceptsTab = false };
			messageField.TextChanged += (sender, e) => noteMessage = messageField.Text;

			var cancelCommand = new Command(); cancelCommand.Executed += (sender, e) => Close();
			var cancelButton = new Button { Text = "Cancel", Command = cancelCommand };

			var toggleCommand = new Command();
			toggleCommand.Executed += (sender, e) => InitContent(!large);
			var toggleButton = new Button { Text = large ? "Collapse" : "Expand", Command = toggleCommand };

			var sendCommand = new Command();
			sendCommand.Executed += (sender, e) => {
				if (noteMessage.Trim().Length == 0)
					noteMessage = "triggered";

				var fields = new List<string>();
				fields.Add($"\"topic\": \"{destinationTopic}\"");
				fields.Add($"\"message\": \"{noteMessage.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n").Replace("\n", "")}\"");
				if (noteTitle.Length > 0) fields.Add($"\"title\": \"{noteTitle.Replace("\"", "\\\"")}\"");
				if (notePriority != 3) fields.Add($"\"priority\": {notePriority.ToString()}");

				if (noteTags.Trim().Length > 0) {
					// todo: sanitize input and add to json
				}

				var json = "{" + string.Join(",", fields.ToArray()) + "}";
				var content = new StringContent(json);

				new Thread(async () => {
					var output = $"{json} → {destinationDomain}/{destinationTopic}";
					try {
						using (HttpClient client = new HttpClient())
						await client.PostAsync($"https://{destinationDomain}", content);
					} catch (Exception err) {
						output += "\n" + err.ToString();
					}

					Application.Instance.Invoke(() => debugLog.Log(output));
				}).Start();

				Close();
			};
			var sendButton = new Button { Text = "Send", Command = sendCommand };

			var layout = new DynamicLayout {
				Padding = 10,
				Spacing = new Size(10, 10),
				DefaultSpacing = new Size(10, 10),
			};
			layout.BeginVertical();
			layout.AddRow(sendDropdown, titleField);
			layout.EndBeginVertical();
			layout.BeginVertical(yscale: true);
			layout.AddRow(messageField);
			layout.EndBeginVertical();

			if (large) {
				var tagField = new TextBox { Text = noteTags, PlaceholderText = "tag1, tag2, ..." };
				tagField.TextChanged += (sender, e) => noteTags = tagField.Text;

				var priorityDropdown = new DropDown { SelectedIndex = notePriority - 1, DataStore = priorities };
				priorityDropdown.SelectedIndexChanged += (sender, e) => notePriority = priorityDropdown.SelectedIndex + 1;

				layout.BeginVertical();
				layout.AddRow(priorityDropdown, tagField);
				layout.EndBeginVertical();
			}

			layout.BeginVertical(yscale: false);
			layout.AddRow(null, toggleButton, sendButton, cancelButton);
			layout.EndBeginVertical();

			Content = layout;
		}
	}
}
