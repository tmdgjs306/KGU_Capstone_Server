using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectManager
{
	public MyPlayer MyPlayer;
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	public int Add(PlayerInfo info, bool myPlayer = false)
	{
		if (myPlayer)
		{
			GameObject go = Managers.Resource.Instantiate("Creature/HeroPlayer");
			go.GetComponent<MyPlayer>().enabled = true;
			go.name = info.Name;
			_objects.Add(info.PlayerId, go);
			MyPlayer = go.GetComponent<MyPlayer>();
			MyPlayer.Id = info.PlayerId;
			MyPlayer.name = info.Name;
		}
		else
		{
			GameObject go = Managers.Resource.Instantiate("Creature/MC01");
			go.name = info.Name;
			go.GetComponent<Player>().enabled = true;
			go.GetComponent<Player>().moveSpeed = 5;
			_objects.Add(info.PlayerId, go);
			Player pr = go.GetComponent<Player>();
			pr.Id = info.PlayerId;
		}
		return MyPlayer.Id;
	}
	public void Add(int id, GameObject go)
	{
		_objects.Add(id, go);
	}

	public void Remove(int id)
	{
		GameObject gameObject = _objects[id];
		Managers.Resource.Destroy(gameObject);
		_objects.Remove(id);
		
	}

	public void RemoveMyPlayer()
	{
		if (MyPlayer == null)
			return;

		Remove(MyPlayer.Id);
		MyPlayer = null;
	}

	public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
	}

	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			//CreatureController cc = obj.GetComponent<CreatureController>();
			//if (cc == null)
				//continue;

			//if (cc.CellPos == cellPos)
				//return obj;
		}

		return null;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		_objects.Clear();
	}
}
