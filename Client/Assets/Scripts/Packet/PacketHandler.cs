using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
		
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
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
	
	// 이름 변경
	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
		GameObject gameObject = Managers.Object.FindById(movePacket.PlayerId);
		if (gameObject == null)
			return;
		Player player = gameObject.GetComponent<Player>();
		if (player == null)
			return;
		
		Vector3 curPos = new Vector3(movePacket.PosInfo.PosX, 0, movePacket.PosInfo.PosZ);
		Vector3 moveVec = new Vector3(movePacket.PosInfo.HAxis, 0, movePacket.PosInfo.VAxis).normalized;
		
		// 이동 관련 정보 전송 
		player.moveVec = moveVec;
		player.curPos = curPos;

		// 이동 동기화
		gameObject.transform.position = curPos;
	}

	public static void S_ActionHandler(PacketSession session, IMessage packet)
	{
		S_Action actionPacket = packet as S_Action;
		GameObject gameObject = Managers.Object.FindById(actionPacket.PlayerId);
		if (gameObject == null)
			return;
		Player player = gameObject.GetComponent<Player>();
		if (player == null)
			return;
		
		// 액션 정보 전송 
		player.rDown = actionPacket.ActInfo.RDown;
		player.aDown = actionPacket.ActInfo.ADown;
	}
}
