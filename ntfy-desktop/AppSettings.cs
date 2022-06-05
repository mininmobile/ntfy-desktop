using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ntfy_desktop {
	public class AppSettings {
		private AppSettings() {
			// marked as private to prevent outside classes from creating new.
		}

		private static string _jsonSource;
		private static AppSettings _appSettings = null;
		private static string _basePath;

		public static AppSettings Default {
			get {
				if (_appSettings == null) {
					var builder = new ConfigurationBuilder()
						.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

					_jsonSource = $"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}appsettings.json";

					var config = builder.Build();
					_appSettings = new AppSettings();
					config.Bind(_appSettings);
				}

				return _appSettings;
			}
		}

		public void Save() {
			string json = JsonSerializer.Serialize(_appSettings);
			System.IO.File.WriteAllText(_jsonSource, json);
		}

		// contents arranged like this: [ ["domain", "topic"], ... ]
		// using a list here bc JsonSerializer can't handle tuples :|
		public List<List<string>> Feeds { get; set; }
	}
}
