using System;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public class DebugLogDialog : Dialog {
		TextArea Log = new TextArea{
			ReadOnly = true,
			Wrap = true,
		};

		public DebugLogDialog(string text = "Debug Log") {
			Title = "Debug Log";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(200, 150);
			Size = new Size(500, 400);
			Resizable = true;

			Log.Text = text;

			SizeChanged += (sender, e) => Log.Height = Height - 64;

			Content = new StackLayout{
				Padding = new Padding(10, 10, 10, 0),
				Items = { new StackLayoutItem{
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Control = Log,
				}},
			};
		}

		public void UpdateLog(string text) {
			Log.Text = text;
		}
	}
}
