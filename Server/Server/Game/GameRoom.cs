using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System.Text;
using System.Timers;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        List<Player> _players = new List<Player>();
        EnemyManager enemyManager = new EnemyManager();
        static System.Timers.Timer spawnTimer;

        public void SetTimer()
        {
            spawnTimer = new System.Timers.Timer(2000);
            spawnTimer.Elapsed += OnTimerEvent;
            spawnTimer.AutoReset = true;
            spawnTimer.Enabled = true;
        }
        private void OnTimerEvent(Object source, ElapsedEventArgs e)
        {
            EnemySpawn();
        }
        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
                return;
            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;
                // 자신에게 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);
                    S_OtherPlayerSpawn spawnPacket = new S_OtherPlayerSpawn();
                    foreach (Player p in _players) 
                    {
                        if (newPlayer != p)
                            spawnPacket.Players.Add(p.Info);
                    }
                    S_EnemySpawn enemySpawnPacket = new S_EnemySpawn();
                    foreach(Enemy e in enemyManager._enemys.Values)
                    {
                        enemySpawnPacket.Enemys.Add(e.enemyInfo);
                    }
                    newPlayer.Session.Send(spawnPacket);
                    newPlayer.Session.Send(enemySpawnPacket);
                }

                // 타인에게 정보 전송
                {
                    S_OtherPlayerSpawn spawnPacket = new S_OtherPlayerSpawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player p in _players)
                    {
                        if (newPlayer != p)
                        {
                            p.Session.Send(spawnPacket);
                        }
                    }
                }
            }
        }

        public void LeaveGame(int PlayerId)
        {
            lock (_lock)
            {
                Player player = _players.Find(p => p.Info.PlayerId == PlayerId);
                if (player == null)
                    return;

                _players.Remove(player);
                player.Room = null;

                //자신에게 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                //타인에게 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.Info.PlayerId);
                    foreach (Player p in _players)
                    {
                        if (player != p)
                            p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        // 적 생성 
        private void EnemySpawn()
        {
            // 적 좌표 랜덤 설정
            Random rand = new Random();
            float x = 5+rand.Next(-30, 30);
            float z = 5+rand.Next(-30, 30);
            S_EnemySpawn enemySpawnPacket = new S_EnemySpawn();
            EnemyInfo enemyInfo = new EnemyInfo();
            PositionInfo pos = new PositionInfo();

            // 위치 설정 
            pos.PosX = x;
            pos.PosZ = z;

            // 적 정보 설정 
            enemyInfo.EnemyId = enemyManager._enemyId++;
            enemyInfo.Type = 1;
            enemyInfo.PosInfo = pos;

            enemySpawnPacket.Enemys.Add(enemyInfo);
            Broadcast(enemySpawnPacket);
        }

        // 클라이언트에서 받은 패킷 전송(패킷 송신자 제외 브로드캐스팅)
        public void Broadcast(IMessage packet, int playerId)
        {
            lock (_lock)
            {
                foreach (Player p in _players)
                {
                    //전송자 제외 
                    if (p.Info.PlayerId == playerId)
                        continue;
                    p.Session.Send(packet);
                }
            }
        }

        // 서버에서 생성한 패킷 전송 
        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players)
                {
                    p.Session.Send(packet);
                }
            }
        }


    }
}
