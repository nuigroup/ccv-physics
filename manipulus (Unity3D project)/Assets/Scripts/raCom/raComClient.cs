using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using OSC.NET;

namespace RACOM
{

	public class raComClient
	{
		public string losMsg;
		public ArrayList dataPoints;
		
		private bool connected = false;
		private int port = 3333;
		private OSCReceiver receiver;
		private Thread thread;
		
		private int currentFrame = 0;

		private string statusString;
		private int packetCount = 0;
		private int packetErrorCount = 0;
		private int bundleCount = 0;
		private int messageCount = 0;
		
		
		/**
		 * The default constructor creates a client that listens to the default port 3333
		 */
		public raComClient() {}

		/**
		 * This constructor creates a client that listens to the provided port
		 *
		 * @param port the listening port number
		 */
		public raComClient(int port) {
			this.port = port;
		}

		/**
		 * Returns the port number listening to.
		 *
		 * @return the listening port number
		 */
		public int getPort() {
			return port;
		}

		/**
		 * The TuioClient starts listening to TUIO messages on the configured UDP port
		 * All reveived TUIO messages are decoded and the resulting TUIO events are broadcasted to all registered TuioListeners
		 */
		public void connect() {
			
			try {
				receiver = new OSCReceiver(port);
				connected = true;
				statusString = "Thread Started";
				thread = new Thread(new ThreadStart(listen));
				thread.Start();
			} catch (Exception e) {
				Console.WriteLine("failed to connect to port "+port);
				Console.WriteLine(e.Message);
				statusString = "failed to connect to port " + port + " " + e.Message;
				Console.WriteLine(statusString);
			}
		}
		
		/**
		 * The TuioClient stops listening to TUIO messages on the configured UDP port
		 */
		public void disconnect() {
				if (receiver!=null) receiver.Close();
				receiver = null;
			
				connected = false;
		}
		
		public bool isConnected() { return connected; }
		public int currentFrameNumber() { return currentFrame; }
		
		private void listen() {
			statusString = "Listening:" + connected;
			while(connected) {
				try {
					OSCPacket packet = receiver.Receive();
					packetCount++;
					if (packet!=null) {
						if (packet.IsBundle()) {
							bundleCount++;
							ArrayList messages = packet.Values;
							for (int i=0; i<messages.Count; i++) {
								messageCount++;
								processMessage((OSCMessage)messages[i]);
							}
						} else processMessage((OSCMessage)packet);
					} else {
						packetErrorCount++;
						Console.WriteLine("null packet");
					}
				} catch (Exception e) { 
					Console.WriteLine(e.Message); 
					statusString = " " + e.Message;
				}
			}
		}
		
		private void processMessage(OSCMessage message) {
			string address = message.Address;
			ArrayList args = message.Values;
			string command = (string)args[0];
			
			if (address == "/raCom/test") {
				if (command == "set") {
					
					dataPoints = args;				

				}
			}
		}

	}
	
}
