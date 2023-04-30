using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Game;
using ServerCore;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		static EnemySpwaner spwaner = new EnemySpwaner();
		static int spawnId = 1;
		static System.Timers.Timer aTimer;
		static void FlushRoom()
		{
			JobTimer.Instance.Push(FlushRoom, 250);
		}

		private static void SetTimer()
        {
			aTimer = new System.Timers.Timer(5000);
			aTimer.Elapsed += OnTimerEvent;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;
        }

		private static void OnTimerEvent(Object source, ElapsedEventArgs e)
        {
			spwaner.spawn(spawnId++);
        }

		static void Main(string[] args)
		{
			RoomManager.Instance.Add();

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			//FlushRoom();
			JobTimer.Instance.Push(FlushRoom);
			SetTimer();
			while (true)
			{
				JobTimer.Instance.Flush();
			}

			aTimer.Stop();
			aTimer.Dispose();
		}
	}
}

