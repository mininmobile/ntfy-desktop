using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Linq;

namespace ntfy_desktop {
	public class NTFYD {
		public event EventHandler MessageReceived;

		private List<TcpClient> _clients = new List<TcpClient>();
		private AppConfig Settings => AppSettings.Default;
		private DebugLog debugLog;
		private List<List<string>> currentFeeds;

		public NTFYD(DebugLog dl) {
			debugLog = dl;

			currentFeeds = Settings.Feeds.ToList();
			currentFeeds.ForEach((feed) => Subscribe(feed[0], feed[1]));

			AppSettings.Updated += (sender, e) => {
				DisposeAll();
				currentFeeds = Settings.Feeds.ToList();
				currentFeeds.ForEach((feed) => Subscribe(feed[0], feed[1]));
			};
		}

		public void Subscribe(string domain, string topic) {
			var task = GetSubscribeThread(domain, topic);
			task.ContinueWith((t) => {
				if (t.Exception == null)
					debugLog.Log($"← closed {domain}/{topic}");
				else
					debugLog.Log($"! {t.Exception.GetType()}: {t.Exception.Message} @ {t.Exception.StackTrace}");
			}, TaskContinuationOptions.OnlyOnFaulted);
			task.ContinueWith((t) => {
				debugLog.Log($"← unsubscribed from {domain}/{topic}");
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
			task.Start();
		}

		private Task GetSubscribeThread(string domain, string topic) {
			return new Task(() => {
				using (TcpClient client = new TcpClient()) {
					// generate request header
					string requestString = $"GET /{topic}/json HTTP/1.1\n"
						+ $"Host: {domain}\n"
						+ "Connection: keep-alive\n\n";

					client.Connect(domain, 80);

					_clients.Add(client);

					try {
						using (NetworkStream stream = client.GetStream()) {
							// send request
							StreamWriter writer = new StreamWriter(stream);
							writer.Write(requestString);
							writer.Flush();

							// process response
							StreamReader rdr = new StreamReader(stream);
							debugLog.Log($"← subscribed to {domain}/{topic}");

							while (!rdr.EndOfStream) {
								var line = rdr.ReadLine();
								if (!line.StartsWith("{") || !line.EndsWith("}") || !line.Contains("\"event\":\"message\""))
									continue;

								var o = JsonNode.Parse(line);

								OnMessageReceived(new MessageReceivedEventArgs {
									domain = domain,
									topic = topic,
									json = o,
								});
							}
						}
					} catch (Exception e) {
						if (e.GetType() != typeof(System.IO.IOException))
							debugLog.Log($"! {e.GetType()}: {e.Message} @ {e.StackTrace}");
					}
				}
			});
		}

		public void DisposeAll() {
			_clients.ForEach((client) => client.Close());
			_clients.Clear();
		}

		protected virtual void OnMessageReceived(MessageReceivedEventArgs e) {
			MessageReceived?.Invoke(this, e);
		}
	}

	public class MessageReceivedEventArgs : EventArgs {
		public string domain { get; set; }
		public string topic { get; set; }
		public JsonNode json { get; set; }
	}
}
