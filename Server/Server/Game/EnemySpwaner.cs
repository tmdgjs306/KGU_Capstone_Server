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

        public void spawn(GameRoom g, int id)
        {
            Random rand = new Random();
            float x = 5 + rand.Next(-10, 10);
            float z = 5 + rand.Next(-10, 10);
            S_EnemySpawn enemySpawnPacket = new S_EnemySpawn();
            EnemyInfo enemyInfo = new EnemyInfo();
            EnemyPositionInfo pos = new EnemyPositionInfo();
            // 위치 설정 
            pos.PosX = x;
            pos.PosZ = z;

            // 적 정보 설정 
            enemyInfo.EnemyId = id;
            enemyInfo.Type = 1;
            enemyInfo.PosInfo = pos;

            enemySpawnPacket.Enemys.Add(enemyInfo);
            g.Broadcast(enemySpawnPacket);
        }
    }
}
