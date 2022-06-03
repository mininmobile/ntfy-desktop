using System;
using Eto.Forms;
using Eto.Drawing;

namespace ntfy_desktop {
	public class CustomAboutDialog : Dialog {
		public CustomAboutDialog() {
			Title = "About NTFY Desktop";
			Icon = Utility.ApplicationIcon;
			MinimumSize = new Size(450, 200);

			Content = new StackLayout {
				Padding = new Padding(20, 10, 10, 10),
				Spacing = 20,
				Orientation = Orientation.Horizontal,
				VerticalContentAlignment = VerticalAlignment.Center,
				Items = {
					new ImageView { Image = Utility.ApplicationLogo },

					new StackLayout {
						Spacing = 5,
						Items = {
							new Label {
								Text = "NTFY Desktop",
								Font = Fonts.Sans(24, FontStyle.Bold),
							},
							new Label {
								Text = "a cross-platform desktop application for ntfy.sh, not affiliated with Philipp Heckel",
								Width = 300,
								Wrap = WrapMode.Word,
							},
							new LinkButton {
								Text = "GitHub Repository",
								Command = new Command((sender, e) => Utility.OpenURL("https://github.com/mininmobile/ntfy-desktop")),
							},
							new LinkButton {
								Text = "ntfy.sh",
								Command = new Command((sender, e) => Utility.OpenURL("https://ntfy.sh")),
							}
						}
					}
				}
			};
		}
	}
}
