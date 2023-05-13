using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
	
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }
	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CPlayerDestroy, MakePacket<C_PlayerDestroy>);
		_handler.Add((ushort)MsgId.CPlayerDestroy, PacketHandler.C_PlayerDestroyHandler);		
		_onRecv.Add((ushort)MsgId.CEnemyDestroy, MakePacket<C_EnemyDestroy>);
		_handler.Add((ushort)MsgId.CEnemyDestroy, PacketHandler.C_EnemyDestroyHandler);		
		_onRecv.Add((ushort)MsgId.CEnemyMove, MakePacket<C_EnemyMove>);
		_handler.Add((ushort)MsgId.CEnemyMove, PacketHandler.C_EnemyMoveHandler);		
		_onRecv.Add((ushort)MsgId.CPlayerMove, MakePacket<C_PlayerMove>);
		_handler.Add((ushort)MsgId.CPlayerMove, PacketHandler.C_PlayerMoveHandler);		
		_onRecv.Add((ushort)MsgId.CPlayerAction, MakePacket<C_PlayerAction>);
		_handler.Add((ushort)MsgId.CPlayerAction, PacketHandler.C_PlayerActionHandler);		
		_onRecv.Add((ushort)MsgId.CEnemyHit, MakePacket<C_EnemyHit>);
		_handler.Add((ushort)MsgId.CEnemyHit, PacketHandler.C_EnemyHitHandler);		
		_onRecv.Add((ushort)MsgId.CPlayerHit, MakePacket<C_PlayerHit>);
		_handler.Add((ushort)MsgId.CPlayerHit, PacketHandler.C_PlayerHitHandler);		
		_onRecv.Add((ushort)MsgId.CPlayerChat, MakePacket<C_PlayerChat>);
		_handler.Add((ushort)MsgId.CPlayerChat, PacketHandler.C_PlayerChatHandler);		
		_onRecv.Add((ushort)MsgId.CPlayerSelect, MakePacket<C_PlayerSelect>);
		_handler.Add((ushort)MsgId.CPlayerSelect, PacketHandler.C_PlayerSelectHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session,pkt,id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
			
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}