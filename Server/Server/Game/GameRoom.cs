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
        public int time;
        List<Player> _players = new List<Player>();
        EnemyManager enemyManager = new EnemyManager();
        List<Enemy> _enemies = new List<Enemy>();
        static System.Timers.Timer spawnTimer;
        static System.Timers.Timer gameEndTimer;
        static System.Timers.Timer clockTimer;
        static System.Timers.Timer enemyMoveTimer;
        // 게임 진행시 사용되는 타이머 설정
        public void SetTimer()
        {
            // 스폰 패킷 생성 주기 설정
            spawnTimer = new System.Timers.Timer(2000);
            spawnTimer.Elapsed += SpawnEvent;
            spawnTimer.AutoReset = true;
            spawnTimer.Enabled = true;

            //게임 클리어 시간 설정 
            gameEndTimer = new System.Timers.Timer(90000);
            gameEndTimer.Elapsed += EndGameEvent;
            gameEndTimer.AutoReset = true;
            gameEndTimer.Enabled = true;

            //사용자 시간 동기화
            clockTimer = new System.Timers.Timer(1000);
            clockTimer.Elapsed += ClockEvent;
            clockTimer.AutoReset = true;
            clockTimer.Enabled = true;

            //몬스터 이동
            enemyMoveTimer = new System.Timers.Timer(100);
            enemyMoveTimer.Elapsed += EnemyMoveEvent;
            enemyMoveTimer.AutoReset = true;
            enemyMoveTimer.Enabled = true;
        }

        // 0,1초 마다 적 이동 명령 전송 
        private void EnemyMoveEvent(Object source, ElapsedEventArgs e)
        {
            foreach (Enemy  T in _enemies)
            {
                S_EnemyMove enemyMovePacket = new S_EnemyMove();
                Player Target = PlayerManager.Instance.Find(T.enemyInfo.PlayerId);
                 
                EnemyPositionInfo ePos = new EnemyPositionInfo();
                ePos.PosX = Target.Info.PosInfo.PosX;
                ePos.PosZ = Target.Info.PosInfo.PosZ;

                enemyMovePacket.Posinfo = ePos;
                enemyMovePacket.EnemyId = T.enemyInfo.EnemyId;
                T.enemyInfo.PosInfo = ePos;
                Broadcast(enemyMovePacket);
            }
        }

        // 1초마다 시간 동기화
        private void ClockEvent(Object source, ElapsedEventArgs e)
        {
            S_TimeInfo timePacket = new S_TimeInfo();
            Console.WriteLine($"{time}");
            timePacket.Now = time;
            time--;
            Broadcast(timePacket);
        }

        // 스폰 주기마다 몬스터 생성 
        private void SpawnEvent(Object source, ElapsedEventArgs e)
        {
            // TODO 
            // 몬스터 일정 마리 수 이상일 시 생성 중단
            Console.WriteLine(_enemies.Count);
            if (_enemies.Count >= 5)
            {
                return;
            }
            EnemySpawn();
        }

        private void EndGameEvent(Object source, ElapsedEventArgs e)
        {
            //TODO
            //스테이지 이동 기능 구현 

        }

        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
                return;
            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;

                //만약 사용자가 처음 입장 하였다면 시간을 90초로 설정 
                if(_players.Count==1)
                {
                    time = 90;
                }

                // 자신에게 정보 전송 { 다른 플레이어 정보, 몬스터 스폰 정보 }
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
                    foreach(Enemy e in _enemies)
                    {
                        enemySpawnPacket.Enemys.Add(e.enemyInfo);
                    }
                    newPlayer.Session.Send(spawnPacket);
                    newPlayer.Session.Send(enemySpawnPacket);
                }

                // 타인에게 정보 전송 { 신규 입장한 플레이어 정보  }
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
                _players.Remove(player);
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
            EnemyPositionInfo pos = new EnemyPositionInfo();

            // 위치 설정 
            pos.PosX = x;
            pos.PosZ = z;

            // 적 정보 설정 
            enemyInfo.EnemyId = enemyManager._enemyId++;
            enemyInfo.Type = 1;
            enemyInfo.PosInfo = pos;

          
            double temp = 9999999;
            Player p1 = null;

            // 생성된 위치에서 가장 가까운 플레이어를 타겟으로 설정 
            foreach(Player p in _players)
            {
                double dist = Math.Pow(x - p.Info.PosInfo.PosX, 2) + Math.Pow(z - p.Info.PosInfo.PosZ, 2);
                if (temp> dist)
                {
                    p1 = p;
                    temp = dist;
                }
            }
            enemyInfo.PlayerId = p1.Info.PlayerId;
            // enemyID, Position, playerID 정보 삽입

            Enemy enemy = new Enemy();
            enemy.enemyInfo = enemyInfo;
            _enemies.Add(enemy);

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
