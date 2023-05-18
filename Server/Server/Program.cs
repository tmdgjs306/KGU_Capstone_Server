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
		static void FlushRoom()
		{
			JobTimer.Instance.Push(FlushRoom, 250);
		}
		static void Main(string[] args)
		{
			RoomManager.Instance.Add();

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);

			//서버 주소 설정(로컬 호스트 구동)
			//IPAddress ipAddr = ipHost.AddressList[0];

			//서버 주소 설정(AWS 구동)
			IPAddress ipAddr = IPAddress.Parse("172.31.41.142");

			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			//FlushRoom();
			JobTimer.Instance.Push(FlushRoom);
			while (true)
			{
				JobTimer.Instance.Flush();
			}
		}
	}
}

