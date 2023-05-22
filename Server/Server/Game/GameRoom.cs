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
        public HashSet<int> selects = new HashSet<int>();  
        object _lock = new object();
        public int RoomId { get; set; }
        public int time;
        public int count = 0;
        public int selectCount = 0;
        public int enemyId = 1;
        public int curStage = 0;
        public int readyCount = 0;
        public bool isReady = true;
        public List<Player> _players = new List<Player>();
        //List<Enemy> _enemies = new List<Enemy>();
        //EnemyManager enemyManager = new EnemyManager();
        //public List<Enemy> enemies = new List<Enemy>();
        public Dictionary<int,Enemy> enemies= new Dictionary<int,Enemy>();
        Random rand = new Random();

        //플레이어 초기 좌표 
        int x = 0;
        int z = 0;

        static System.Timers.Timer spawnTimer;
        static System.Timers.Timer gameEndTimer;
        static System.Timers.Timer clockTimer;
        static System.Timers.Timer enemyMoveTimer;
        static System.Timers.Timer tartgetResetTimer;

        //호스트 플레이어 id 초기화
        int hostId = 0;
        public bool isGameStart = false;
        // 게임 진행시 사용되는 타이머 설정
        int SpawnTime = 1000;
        
        public void SetTimer()
        {
            // 몬스터 스폰 주기 설정 => stage 단계에 맞춰 레벨 디자인 
            spawnTimer = new System.Timers.Timer(SpawnTime);
            spawnTimer.Elapsed += SpawnEvent;
            spawnTimer.AutoReset = true;
            spawnTimer.Enabled = true;

            //게임 클리어 시간 설정(90s)
            gameEndTimer = new System.Timers.Timer(90000);
            gameEndTimer.Elapsed += EndGameEvent;
            gameEndTimer.AutoReset = true;
            gameEndTimer.Enabled = true;

            //사용자 시간 동기화(1s)
            clockTimer = new System.Timers.Timer(1000);
            clockTimer.Elapsed += ClockEvent;
            clockTimer.AutoReset = true;
            clockTimer.Enabled = true;

            //몬스터 이동(0.1s)
            enemyMoveTimer = new System.Timers.Timer(100);
            enemyMoveTimer.Elapsed += EnemyMoveEvent;
            enemyMoveTimer.AutoReset = true;
            enemyMoveTimer.Enabled = true;

            //타겟 설정 타이머(0.1s) 
            tartgetResetTimer = new System.Timers.Timer(1000);
            tartgetResetTimer.Elapsed += TargetResetEvent;
            tartgetResetTimer.AutoReset = true;
            tartgetResetTimer.Enabled = true;
        }


        // 0.1초 마다 모든 적 객체 타겟 변경
        private void TargetResetEvent(Object source, ElapsedEventArgs e)
        {
            foreach (Enemy enemy in enemies.Values)
            {
                double temp = 9999999;
                Player p1 = null;

                S_EnemyTargetReset resetPacket = new S_EnemyTargetReset();
                float x = enemy.enemyInfo.PosInfo.PosX;
                float z = enemy.enemyInfo.PosInfo.PosZ;

                // 현재 위치에서 가장 가까운 플레이어를 타겟으로 설정 
                foreach (Player p in _players)
                {
                    if (p.isAlive == false) continue;
                    double dist = Math.Pow(x - p.Info.PosInfo.PosX, 2) + Math.Pow(z - p.Info.PosInfo.PosZ, 2);
                    if (temp > dist)
                    {
                        p1 = p;
                        temp = dist;
                    }
                }

                // 만약 타겟이 변경 되었다면 알려준다
                if (p1.Info.PlayerId != enemy.enemyInfo.PlayerId)
                {
                    enemy.enemyInfo.PlayerId = p1.Info.PlayerId;
                    resetPacket.PlayerId = p1.Info.PlayerId;
                    resetPacket.EnemyId = enemy.enemyInfo.EnemyId;
                    Broadcast(resetPacket);
                }
            }
        }

        // 0,1초 마다 적 이동 명령 전송 
        private void EnemyMoveEvent(Object source, ElapsedEventArgs e)
        {
            foreach (Enemy T in enemies.Values)
            {
                S_EnemyMove enemyMovePacket = new S_EnemyMove();

                EnemyPositionInfo ePos = T.enemyInfo.PosInfo;
     
                enemyMovePacket.Posinfo = ePos;
                enemyMovePacket.EnemyId = T.enemyInfo.EnemyId;

                MoveBroadcast(enemyMovePacket);
            }
        }

        // 1초마다 시간 동기화
        private void ClockEvent(Object source, ElapsedEventArgs e)
        {
            if (!isGameStart)
                return;
            S_TimeInfo timePacket = new S_TimeInfo();
            timePacket.Now = time;
            time--;

            // 스테이지 이동 구현 
            if(time == 0)
            {
                readyCount = 0;
                isReady = false;
                S_GameClear clearPacket = new S_GameClear();
                foreach (Player p in _players)
                {
                    p.Session.Send(clearPacket);
                }
            }

            Broadcast(timePacket);
        }

        // 다음 스테이지로 이동 
        public void MoveNextStage()
        {
            curStage++;
            //플레이어 새로운 좌표 랜덤 설정 
            foreach (Player p in _players)
            {
                p.Info.PosInfo.PosX = 3 + rand.Next(-10, 10);
                p.Info.PosInfo.PosZ = 3 + rand.Next(-10, 10);
            }

            //플레이어 별 패킷 전송
            foreach (Player p in _players)
            {
                S_EndStage endStagePacket = new S_EndStage();
                endStagePacket.CurStage = curStage;
                endStagePacket.Players = p.Info;
                foreach (Player otherPlayer in _players)
                {
                    if (otherPlayer != p)
                        endStagePacket.Otherplayers.Add(otherPlayer.Info);
                }
                p.Session.Send(endStagePacket);

            }
             
            //패킷 전송후 게임 룸 변수 값 초기화
            time = 30;
            enemies.Clear();
        }

        // 스폰 주기마다 몬스터 생성
        private void SpawnEvent(Object source, ElapsedEventArgs e)
        {
            if (!isReady) return;
            // 몬스터 일정 마리 수 이상일 시 생성 중단(현재 100마리)
            if (!isGameStart)
                return;
            int Count = enemies.Count;
            if (Count >= 100)
                return;
            EnemySpawn();
        }

        private void EndGameEvent(Object source, ElapsedEventArgs e)
        {
            //TODO
            //스테이지 이동 기능 구현 
        }

        public void EnterGame(Player newPlayer)
        {
            _players.Add(newPlayer);
            newPlayer.Room = this;

            //호스트 플레이어 설정 
            if(_players.Count ==1)
            {
                S_HostUser hostPacket = new S_HostUser();
                newPlayer.Session.Send(hostPacket);
                hostId = newPlayer.Info.PlayerId;
            }

            //만약 방에 모든 인원이 접속했다면 Game Start 패킷 전송
            if(_players.Count ==2)
            {
                S_GameStart gameStartPacket = new S_GameStart();
                Broadcast(gameStartPacket);
            }

        }
        
        //플레이어가 처음 방에 입장 했을때 초기화 작업
        public void StartGame(Player newPlayer)
        {
            //만약 신규 플레이어가 null로 왔다면 리턴 
            if (newPlayer == null)
                return;

            lock (_lock)
            {
                /*_players.Add(newPlayer);
                newPlayer.Room = this;*/
                
                //플레이어 초기좌표 설정 
                newPlayer.Info.PosInfo.PosX = x;
                newPlayer.Info.PosInfo.PosZ = z;
                x += 5;
                z += 5;

                //만약 사용자가 처음 입장 하였다면 시간을 90초로 설정 
                /*if (_players.Count == 1)
                {
                    time = 90;
                    S_HostUser hostPacket = new S_HostUser();
                    newPlayer.Session.Send(hostPacket);
                    hostId = newPlayer.Info.PlayerId;
                }*/


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
                    //S_EnemySpawn enemySpawnPacket = new S_EnemySpawn();

                    //foreach (Enemy e in enemies.Values)
                    //{
                    //   enemySpawnPacket.Enemys.Add(e.enemyInfo);
                    //}
                    newPlayer.Session.Send(spawnPacket);
                    //newPlayer.Session.Send(enemySpawnPacket);
                }

                // 타인에게 정보 전송 { 신규 입장한 플레이어 정보  }
                /*{
                    S_OtherPlayerSpawn spawnPacket = new S_OtherPlayerSpawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player p in _players)
                    {
                        if (newPlayer != p)
                        {
                            p.Session.Send(spawnPacket);
                        }
                    }
                }*/
                count++;
            }
        }

        public void LeaveGame(int PlayerId)
        {
            lock (_lock)
            {
                Player player = _players.Find(p => p.Info.PlayerId == PlayerId);
                if (player == null)
                    return;

                //호스트 플레이어가 나가는 경우 
                if(hostId == PlayerId)
                {
                    foreach(Player p in _players)
                    {
                        //호스트 플레이어 변경 
                        if(player!= p )
                        {
                            S_HostUser hostPacket = new S_HostUser();
                            p.Session.Send(hostPacket);
                            hostId = p.Info.PlayerId;
                            break;
                        }
                    }
                }

                //자신이 타겟인 몬스터가 있는 경우
                foreach(Enemy e in enemies.Values)
                {
                    if(e.enemyInfo.PlayerId == PlayerId)
                    {
                        S_EnemyTargetReset resetPacket = new S_EnemyTargetReset();

                        //플레이어 중 한명 선택 
                        foreach (Player p in _players)
                        {
                            //타겟 변경 
                            if (player != p)
                            {
                                resetPacket.PlayerId = p.Info.PlayerId;
                                resetPacket.EnemyId = e.enemyInfo.EnemyId;
                                Broadcast(resetPacket,PlayerId);
                                break;
                            }
                        }
                    }
                }

                //자신에게 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                //타인에게 정보 전송 
                {
                    S_PlayerDestroy despawnPacket = new S_PlayerDestroy();
                    despawnPacket.PlayerId = player.Info.PlayerId;
                    Broadcast(despawnPacket, PlayerId);
                }
                //player.Room = null;
                _players.Remove(player);
                PlayerManager.Instance.Remove(PlayerId);
                count--;
            }
        }

        // 적 생성 
        private void EnemySpawn()
        {
            // 적 좌표 랜덤 설정
            float x = 5 + rand.Next(-30, 30);
            float z = 5 + rand.Next(-30, 30);
            S_EnemySpawn enemySpawnPacket = new S_EnemySpawn();
            EnemyInfo enemyInfo = new EnemyInfo();
            EnemyPositionInfo pos = new EnemyPositionInfo();

            // 위치 설정 
            pos.PosX = x;
            pos.PosZ = z;

            // 적 정보 설정 
            enemyInfo.EnemyId = enemyId;
            enemyInfo.Type = 1;
            enemyInfo.PosInfo = pos;
            double temp = 9999999;
            Player p1 = null;
            // 생성된 위치에서 가장 가까운 플레이어를 타겟으로 설정 
            foreach (Player p in _players)
            {
                if (p.isAlive == false) continue;
                double dist = Math.Pow(x - p.Info.PosInfo.PosX, 2) + Math.Pow(z - p.Info.PosInfo.PosZ, 2);
                if (temp > dist)
                {
                    p1 = p;
                    temp = dist;
                }
            }
            enemyInfo.PlayerId = p1.Info.PlayerId;
            // enemyID, Position, playerID 정보 삽입
            Enemy enemy = new Enemy();
            enemy.enemyInfo = enemyInfo;
            enemies.Add(enemyId, enemy);
            enemySpawnPacket.Enemys.Add(enemyInfo);
            Broadcast(enemySpawnPacket);
            enemyId++;
        }

        // 패킷 전송(특정 플레이어 제외 브로드캐스팅)
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

        //호스트 제외 패킷 전송 
        public void MoveBroadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players)
                {
                    if (p.Info.PlayerId == hostId)
                        continue;
                    p.Session.Send(packet);
                }
            }
        }

        // 플레이어 좌표 변경
        public void SetLocation(PlayerInfo pInfo)
        {
            foreach(Player p in _players)
            {
                if (p.Info.PlayerId == pInfo.PlayerId)
                {
                    p.Info.PosInfo = pInfo.PosInfo;
                    return;
                }
            }
        }
        
        // 죽은 적 객체 삭제 
        public void DestroyEnemy(int enemyId)
        {
            enemies.Remove(enemyId);
        }

        // 타겟 변경 
        public void TargetReset(int playerId)
        {
            foreach(Enemy e in enemies.Values)
            {
                if(e.enemyInfo.PlayerId == playerId)
                {
                    S_EnemyTargetReset resetPacket = new S_EnemyTargetReset();
                    //플레이어 중 한명 선택 
                    foreach (Player p in _players)
                    {
                        if (p.isAlive == false) continue;
                        //타겟 변경 
                        if (playerId != p.Info.PlayerId)
                        {
                            resetPacket.PlayerId = p.Info.PlayerId;
                            resetPacket.EnemyId = e.enemyInfo.EnemyId;
                            Broadcast(resetPacket, playerId);
                            break;
                        }
                    }

                }
            }
        }

        // 플레이어 사망 처리 
        public void PlayerDead(int playerId)
        {
            foreach(Player p in _players)
            {
                if(p.Info.PlayerId == playerId)
                {
                    p.isAlive = false;
                }
            }
        }


    }
}
