using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace Telnet
{
	class Client
	{
		TcpClient clientSocket;
		SslStream sslStream;
		object readLock = new object();
		List<string> pendingMessages = new List<string>();
		bool closing = false;

		// 74.82.5.42 2069/6699
		public void Connect()
		{
			if (clientSocket != null)
			{
				clientSocket.Close();
			}

			clientSocket = new TcpClient("74.82.5.42", 6699);
			sslStream = new SslStream(clientSocket.GetStream(), false, ValidateServerCertificate, null);
			sslStream.AuthenticateAsClient("74.82.5.42");
			sslStream.ReadTimeout = 200;

			Thread readThread = new Thread(ReadThread);
			readThread.Start();
		}

		public void Close()
		{
			lock (readLock)
			{
				if (clientSocket != null)
				{
					clientSocket.Close();
				}

				if (sslStream != null)
				{
					sslStream.Close();
				}

				closing = true;
			}
		}

		public void Write(string message)
		{
			byte[] encoded = Encoding.UTF8.GetBytes(message + "\r\n");

			sslStream.Write(encoded);
			sslStream.Flush();
		}

		public List<string> GetPendingMessages()
		{
			List<string> returnList;

			lock (readLock)
			{
				returnList = new List<string>(pendingMessages);

				pendingMessages.Clear();
			}

			return returnList;
		}

		private void ReadThread()
		{
			Decoder decoder = Encoding.UTF8.GetDecoder();
			byte[] buffer = new byte[2048];
			StringBuilder messageData = new StringBuilder();

			while (!closing)
			{
				messageData.Clear();

				int bytes = 0;

				do
				{
					try
					{
						bytes = sslStream.Read(buffer, 0, buffer.Length);
					}
					catch (IOException)
					{
						break;
					}
					catch
					{
						return;
					}

					if (bytes > 0)
					{
						if (buffer[0] > 23)
						{
							char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
							decoder.GetChars(buffer, 0, bytes, chars, 0);

							messageData.Append(chars);

							if (chars.Count() > 20 && chars[0] == 'S' && chars[1] == 'o' && chars[2] == 'm' && chars[3] == 'e' && chars[4] == 'w' && chars[5] == 'h' && chars[6] == 'e' && chars[7] == 'r'
								&& chars[8] == 'e' && chars[9] == ' ' && chars[10] == 'o' && chars[11] == 'n' && chars[12] == ' ' && chars[13] == 't')
								messageData.Append("\r\n");
						}
						else
						{
							/*
							messageData.Append(string.Format("Alert: "));

							for (int i = 0; buffer[i] != 0; ++i)
							{
								messageData.Append(buffer[i].ToString() + " ");
							}*/
						}
					}

					if (messageData.ToString().IndexOf("\r\n") != -1)
					{
						break;
					}
				} while (bytes != 0);

				if (messageData.Length > 0)
				{
					messageData.Remove(messageData.Length - 2, 2);

					lock (readLock)
					{
						pendingMessages.Add(messageData.ToString());
					}
				}

				Thread.Sleep(3);
			}
		}

		public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;

			return true;
		}
	}
}
