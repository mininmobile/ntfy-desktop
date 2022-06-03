using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace ntfy_desktop {
	public class NTFYD {
		private List<Thread> threads = new List<Thread>();
		public event EventHandler MessageReceived;

		public NTFYD() {

		}

		public void Subscribe(string domain, string topic) {
			// TODO: add to a list of subscriptions

			var thread = GetSubscribeThread(domain, topic);
			thread.Start();
			threads.Add(thread);
		}

		private Thread GetSubscribeThread(string domain, string topic) {
			return new Thread(() => {
				using (TcpClient client = new TcpClient()) {
					// generate request header
					string requestString = "GET /" + topic + "/json HTTP/1.1\n";
					requestString += "Host: " + domain + "\n";
					requestString += "Connection: keep-alive\n";
					requestString += "\n";

					client.Connect(domain, 80);

					using (NetworkStream stream = client.GetStream()) {
						// send request
						StreamWriter writer = new StreamWriter(stream);
						writer.Write(requestString);
						writer.Flush();

						// process response
						StreamReader rdr = new StreamReader(stream);

						while (!rdr.EndOfStream) {
							var line = rdr.ReadLine();
							if (!line.StartsWith("{") || !line.EndsWith("}"))
								continue;

							OnMessageReceived(new MessageReceivedEventArgs {
								domain = domain,
								topic = topic,
								message = line,
							});
						}
					}
				}
			});
		}

		public void DisposeAll() {
			threads.ForEach((thread) => thread.Abort());
		}

		protected virtual void OnMessageReceived(MessageReceivedEventArgs e) {
			EventHandler handler = MessageReceived;
			handler?.Invoke(this, e);
		}
	}

	public class MessageReceivedEventArgs : EventArgs {
		public string domain { get; set; }
		public string topic { get; set; }
		public string message { get; set; }
	}
}
