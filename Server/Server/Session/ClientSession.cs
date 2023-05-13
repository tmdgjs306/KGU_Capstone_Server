using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;

namespace Server
{
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		public void Send(IMessage packet)
        {
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);

			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);

			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
			Send(new ArraySegment<byte>(sendBuffer));
		}


		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			//플레이어 세션 설정 
			MyPlayer = PlayerManager.Instance.Add();
            {
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
				MyPlayer.Session = this;
            }
			bool isFindGameRoom = false;

			foreach(GameRoom gameRoom in RoomManager.Instance._rooms.Values)
            {
				if(gameRoom._players.Count < 2)
                {
					isFindGameRoom = true;
					gameRoom.EnterGame(MyPlayer);
                }
            }
			
			if(!isFindGameRoom)
            {
				GameRoom gameRoom = RoomManager.Instance.Add();
				gameRoom.EnterGame(MyPlayer);
            }

			//RoomManager.Instance.Find(1).EnterGame(MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.PlayerId);
			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			
		}
	}
}
