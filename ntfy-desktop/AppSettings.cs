using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;

namespace ntfy_desktop {
	public class AppSettings {
		private static string _jsonSource;
		private static AppConfig _appConf = new AppConfig();
		private static AppConfig _mutAppConf = null;

		public static event EventHandler Updated;
		public static event EventHandler TempUpdated;

		public static AppConfig Default {
			get {
				if (_mutAppConf == null) {
					_jsonSource = $"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}appsettings.json";

					using (StreamReader reader = new StreamReader(_jsonSource))
						_mutAppConf = JsonSerializer.Deserialize<AppConfig>(reader.ReadToEnd());

					_appConf.Feeds = _mutAppConf.Feeds.ToList();
					_appConf.Theme = _mutAppConf.Theme.ConvertAll<RGBAColor>(c => (RGBAColor)c.Clone());
				}

				return _mutAppConf;
			}
		}

		public static void Save() {
			if (_mutAppConf == null) return;
			_appConf.Feeds = _mutAppConf.Feeds.ToList();
			_appConf.Theme = _mutAppConf.Theme.ConvertAll<RGBAColor>(c => (RGBAColor)c.Clone());

			string json = JsonSerializer.Serialize(_appConf);
			System.IO.File.WriteAllText(_jsonSource, json);
			UpdateTemp();
			Update();
		}

		public static void Revert() {
			if (_mutAppConf == null) return;
			_mutAppConf.Feeds = _appConf.Feeds.ToList();
			_mutAppConf.Theme = _appConf.Theme.ConvertAll<RGBAColor>(c => (RGBAColor)c.Clone());
			UpdateTemp();
		}

		public static void Update() {
			if (_mutAppConf == null) return;
			Updated.Invoke(null, null);
		}

		public static void UpdateTemp() {
			if (_mutAppConf == null) return;
			TempUpdated.Invoke(null, null);
		}
	}

	public class AppConfig {
		public AppConfig() { }

		// contents arranged like this: [ ["domain", "topic"], ... ]
		// using a list here bc JsonSerializer can't handle tuples :|
		public List<List<string>> Feeds { get; set; }
		// contents arranged like this [ feedBackground, cardBackground, cardColor, cardColorLight ]
		public List<RGBAColor> Theme { get; set; }
	}

	public class RGBAColor : ICloneable {
		public int R { get; set; }
		public int G { get; set; }
		public int B { get; set; }
		public int A { get; set; }

		public RGBAColor(int r = 0, int g = 0, int b = 0, int a = 255) {
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public object Clone() {
			return new RGBAColor(R, G, B, A);
		}

		public Color ToColor() {
			return new Color(R / 255f, G / 255f, B / 255f, A / 255f);
		}
	}
}
