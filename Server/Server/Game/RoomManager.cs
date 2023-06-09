﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();

        public Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();

        int _roomId = 1;

        public GameRoom Add()
        {
            GameRoom gameRoom = new GameRoom();
            lock (_lock)
            {
                gameRoom.RoomId = _roomId;
                _rooms.Add(_roomId, gameRoom);
                _roomId++;
                gameRoom.SetTimer();
            }
            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            lock (_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)
        {
            lock (_lock)
            {
                GameRoom room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;
                return null;
            }
        }
        public int FInd(GameRoom gameRoom)
        {
            lock (_lock)
            {
                int _key = _rooms.FirstOrDefault(x => x.Value == gameRoom).Key;
                return _key;
            }
        }
        public Dictionary<int,GameRoom> FindAll()
        {
            return _rooms;
        }
    }
}
