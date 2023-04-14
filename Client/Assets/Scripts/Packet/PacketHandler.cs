using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	private static int id;
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		id = Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
		
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
		Managers.Object.RemoveMyPlayer();
	}
	
	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
		foreach (PlayerInfo player in spawnPacket.Players)
		{
			Managers.Object.Add(player, myPlayer: false);
		}
	}
	
	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
		foreach (int id in despawnPacket.PlayerIds)
		{
			Managers.Object.Remove(id);
		}
	}
	
	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
		ServerSession serverSession = session as ServerSession;
		GameObject gameObject = Managers.Object.FindById(movePacket.PlayerId);
		if (id == movePacket.PlayerId)
			return;
		if (gameObject == null)
			return;
		Player player = gameObject.GetComponent<Player>();
		if (player == null)
			return;
		
		Vector3 moveVec = new Vector3(movePacket.PosInfo.PosX, 0, movePacket.PosInfo.PosZ);
		Vector3 lookAt = new Vector3(movePacket.PosInfo.HAxis, 0, movePacket.PosInfo.VAxis).normalized;
		
		//이동 관련 정보 전송 
		player.Lookat = lookAt;
		player.nextVec = moveVec;
		player.rDown = movePacket.PosInfo.RAxis;
		player.aDown = movePacket.PosInfo.AAxis;
		
		//이동 동기화
		gameObject.transform.position = moveVec;
	}
}
