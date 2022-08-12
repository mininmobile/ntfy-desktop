using System;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public class DebugLogForm : Form {
		public DebugLog debugLog;

		TextArea Log = new TextArea{
			ReadOnly = true,
			Wrap = true,
		};

		public DebugLogForm(DebugLog dl) {
			Title = "Debug Log";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(200, 150);
			Size = new Size(500, 400);
			Resizable = true;
			Minimizable = false;

			debugLog = dl;
			Log.Text = debugLog.Text;

			debugLog.Updated += OnUpdate;

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

		private void OnUpdate(object sender, EventArgs args) {
			Application.Instance.Invoke(() => {
				Log.Text = debugLog.Text;
			});
		}
	}
}
