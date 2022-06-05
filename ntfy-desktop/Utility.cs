using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Eto.Drawing;

namespace ntfy_desktop {
	public class Utility {
		public static Icon ApplicationIcon => Icon.FromResource("ntfy_desktop.Images.ntfy.ico");
		public static Bitmap ApplicationLogo => Bitmap.FromResource("ntfy_desktop.Images.ntfy.ico");
		public static JsonNode EmojiTags => JsonFromResource("ntfy_desktop.Data.tags.json");

		public static void OpenURL(string url) {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				Process.Start("xdg-open", url);
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				Process.Start("open", url);
			}
		}

		public static JsonNode JsonFromResource(string resource) {
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream(resource))
				using (StreamReader reader = new StreamReader(stream))
					return JsonNode.Parse(reader.ReadToEnd());
		}
	}
}
