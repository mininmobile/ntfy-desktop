using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ntfy_desktop {
	public class AppSettings {
		// marked as private to prevent outside classes from creating new
		private AppSettings() {

		}

		private static string _jsonSource;
		private static AppSettings _appSettings = null;
		private static AppSettings _mutableAppSettings = null;

		public static event EventHandler Updated;

		public static AppSettings Default {
			get {
				if (_appSettings == null) {
					var builder = new ConfigurationBuilder()
						.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

					_jsonSource = $"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}appsettings.json";

					var config = builder.Build();
					_appSettings = new AppSettings();
					_mutableAppSettings = new AppSettings();
					config.Bind(_appSettings);
					config.Bind(_mutableAppSettings);
				}

				return _mutableAppSettings;
			}
		}

		public void Save() {
			_appSettings.Feeds = _mutableAppSettings.Feeds.ToList();

			string json = JsonSerializer.Serialize(_appSettings);
			System.IO.File.WriteAllText(_jsonSource, json);
			Updated.Invoke(this, null);
		}

		public void Revert() {
			_mutableAppSettings.Feeds = _appSettings.Feeds.ToList();
		}

		// contents arranged like this: [ ["domain", "topic"], ... ]
		// using a list here bc JsonSerializer can't handle tuples :|
		public List<List<string>> Feeds { get; set; }
	}
}
