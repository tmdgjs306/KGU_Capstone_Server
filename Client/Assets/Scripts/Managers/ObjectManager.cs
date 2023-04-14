using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectManager
{
	// 플레이어 리스트 
	Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();
	
	// 몬스터 리스트 
	Dictionary<int, GameObject> _enemys = new Dictionary<int, GameObject>();
	
	// 몬스터와 플레이어 Add 함수 구분
	public void Add(PlayerInfo info, bool myPlayer = false)
	{
		if (myPlayer)
		{
			GameObject gameObject = Managers.Resource.Instantiate("Creature/HeroPlayer");
			PlayerCamera.targetTransform = gameObject.transform;
			gameObject.GetComponent<MyPlayer>().enabled = true;
			gameObject.name = info.Name;
			_players.Add(info.PlayerId, gameObject);
		}
		else
		{
			GameObject gameObject = Managers.Resource.Instantiate("Creature/MC01");
			gameObject.name = info.Name;
			gameObject.GetComponent<Player>().enabled = true;
			gameObject.GetComponent<Player>().moveSpeed = 5;
			_players.Add(info.PlayerId, gameObject);
		}
		
	}

	public void AddMonster()
	{
		GameObject gameObject = Managers.Resource.Instantiate("Creature/MC01");
		gameObject.name = "bat";
	}
	
	public void Remove(int id)
	{
		GameObject gameObject = _players[id];
		Managers.Resource.Destroy(gameObject);
		_players.Remove(id);
	}
	
	public GameObject FindById(int id)
	{
		GameObject gameObject = null;
		_players.TryGetValue(id, out gameObject);
		return gameObject;
	}
	
}
