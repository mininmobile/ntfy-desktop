using System;
using Eto.Drawing;
using Eto.Forms;

namespace ntfy_desktop {
	class Card : Panel {
		private AppConfig Settings => AppSettings.Default;

		private Color bg;
		private Color fg;
		private Color fg2;

		public readonly string domain;
		public readonly string topic;
		public readonly string id;
		public readonly DateTime time;
		public Command DismissCommand;

		public readonly string message;
		public readonly string title;
		public readonly string click;
		public readonly int priority;

		public Card(MessageReceivedEventArgs e, Command dismissCommand) {
			DismissCommand = dismissCommand;
			domain = e.domain;
			topic = e.topic;
			id = e.json["id"].ToString();
			time = new DateTime(long.Parse(e.json["time"].ToString() + "000"));
			// decode message content
			message = e.json["message"]?.ToString();
			title = e.json["title"]?.ToString();
			click = e.json["click"]?.ToString();
			priority = int.Parse(e.json["priority"]?.ToString() ?? "3");

			AppSettings.TempUpdated += (sender, _e) => Application.Instance.Invoke(() => Render());

			Render();
		}

		public void Render() {
			// update colors
			bg = Settings.Theme[1].ToColor();
			fg = Settings.Theme[2].ToColor();
			fg2 = Settings.Theme[3].ToColor();

			var layout = new DynamicLayout { Padding = 10 };

			// time + priority
			layout.BeginVertical();
			layout.BeginHorizontal();
			layout.Add(new Label {
				Text = time.ToShortDateString() + ", " + time.ToShortTimeString() + " ", Font = Fonts.Sans(9), TextColor = fg2 });
			if (priority != 3) layout.Add(new Label {
				Text = "(" + priority + ") ", Font = Fonts.Sans(9), TextColor = fg2 });
			layout.Add(new LinkButton {
				Text = "(dismiss)", Font = Fonts.Sans(9), Command = DismissCommand, TextColor = fg2 });
			layout.EndBeginHorizontal();
			layout.EndVertical();

			layout.BeginVertical();
			// title
			if (title != null) layout.AddRow(new Label { Text = title, Font = Fonts.Sans(9, FontStyle.Bold), TextColor = fg, Wrap = WrapMode.Word });
			// message
			if (message != null) layout.AddRow(new Label { Text = message, TextColor = fg, Wrap = WrapMode.Word });
			layout.EndVertical();

			Content = new Panel{
				BackgroundColor = bg,
				Content = layout,
			};
		}
	}
}
