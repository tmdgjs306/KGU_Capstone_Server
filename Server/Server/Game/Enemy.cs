using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Enemy
    {
        public EnemyInfo enemyInfo { get; set; } = new EnemyInfo() { PosInfo = new EnemyPositionInfo() };
        public GameRoom Room { get; set; }
        public ClientSession Session { get; set; }
        
        public enum EnemyType
        {
            Bat,
            TurtleShaell,
            Skeleton,
            Spider,
            Golam,
            Orc
        };
        public EnemyType type;
    }
}
