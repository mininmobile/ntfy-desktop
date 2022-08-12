using System;

namespace ntfy_desktop {
	public class DebugLog {
		public string Text;
		public event EventHandler Updated;

		public DebugLog() {
			Text = $"Debug Log {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}\n";
		}

		public void Log(string text = "") {
			Text += text + "\n";
			OnUpdate(new DebugLogUpdatedEventArgs{ text = Text, newText = text + "\n" });
		}

		protected virtual void OnUpdate(DebugLogUpdatedEventArgs e) {
			Updated?.Invoke(this, e);
		}
	}

	public class DebugLogUpdatedEventArgs : EventArgs {
		public string text { get; set; }
		public string newText { get; set; }
	}
}
