using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ntfy_desktop {
	public class NTFYD {
		private List<TcpClient> _clients = new List<TcpClient>();
		public event EventHandler MessageReceived;

		public NTFYD() {

		}

		public void Subscribe(string domain, string topic) {
			// TODO: add to a list of subscriptions

			var task = GetSubscribeThread(domain, topic);
			task.ContinueWith((t) => { Console.WriteLine(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
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
					using (NetworkStream stream = client.GetStream()) {
						// send request
						StreamWriter writer = new StreamWriter(stream);
						writer.Write(requestString);
						writer.Flush();

						// process response
						StreamReader rdr = new StreamReader(stream);

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
				}
			});
		}

		public void DisposeAll() {
			_clients.ForEach((client) => client.Close());
		}

		protected virtual void OnMessageReceived(MessageReceivedEventArgs e) {
			EventHandler handler = MessageReceived;
			handler?.Invoke(this, e);
		}
	}

	public class MessageReceivedEventArgs : EventArgs {
		public string domain { get; set; }
		public string topic { get; set; }
		public JsonNode json { get; set; }
	}
}
