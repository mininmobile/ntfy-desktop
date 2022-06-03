using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public class Utility {
		public static Icon ApplicationIcon => Icon.FromResource("ntfy_desktop.Images.ntfy.ico");
		public static Bitmap ApplicationLogo => Bitmap.FromResource("ntfy_desktop.Images.ntfy.ico");

		public static void OpenURL(string url) {
			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				Process.Start("xdg-open", url);
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				Process.Start("open", url);
			} else {
				throw new Exception();
			}
		}
	}
}
