using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Server;
using ServerCore;

namespace Server.Game
{
    public  class EnemySpwaner
    {
      public  EnemySpwaner()
        {

        }

        public void spawn(int id)
        {
            Random rand = new Random();
            float x = 5 + rand.Next(-10, 10);
            float z = 5 + rand.Next(-10, 10);
            S_EnemySpawn enemySpawnPacket = new S_EnemySpawn();
            EnemyInfo enemyInfo = new EnemyInfo();
            PositionInfo pos = new PositionInfo();
            // 위치 설정 
            pos.PosX = x;
            pos.PosZ = z;

            // 적 정보 설정 
            enemyInfo.EnemyId = id;
            enemyInfo.Type = 1;
            enemyInfo.PosInfo = pos;

            enemySpawnPacket.Enemys.Add(enemyInfo);

            //전체 방 리스트 불러옴 
            Dictionary<int, GameRoom> rooms = RoomManager.Instance.FindAll();
            List<int> list = new List<int>(rooms.Keys);

            //모든 방에 적 생성 패킷 전송
            for(int i=0; i<rooms.Count; i++)
            {
                GameRoom g = rooms[list[i]];
                g.Broadcast(enemySpawnPacket);
            }
        }
    }
}
