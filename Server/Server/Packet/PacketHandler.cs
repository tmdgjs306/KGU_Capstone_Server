using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    #region 이동 핸들러
    public static void C_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        C_PlayerMove movePacket = packet as C_PlayerMove;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.MyPlayer == null)
            return;
        if (clientSession.MyPlayer.Room == null)
            return;

        //서버 내부 좌표이동 
        PlayerInfo info = clientSession.MyPlayer.Info;
        info.PosInfo = movePacket.PosInfo;
        clientSession.MyPlayer.Room.SetLocation(info);

        // 다른 플레이어에게 이동 좌표 전송 
        S_PlayerMove resMovePacket = new S_PlayerMove();
        resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resMovePacket.PosInfo = movePacket.PosInfo;
       
        clientSession.MyPlayer.Room.Broadcast(resMovePacket,resMovePacket.PlayerId);
    }
    #endregion
    #region 액션 핸들러
    public static void C_PlayerActionHandler(PacketSession session, IMessage packet)
    {
        C_PlayerAction actPacket = packet as C_PlayerAction;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyPlayer == null)
            return;
        if (clientSession.MyPlayer.Room == null)
            return;

        // 다른 플레이어들에게 액션 처리 전송
        S_PlayerAction resActionPacket = new S_PlayerAction();
        resActionPacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resActionPacket.ActInfo = actPacket.ActInfo;
        clientSession.MyPlayer.Room.Broadcast(resActionPacket,resActionPacket.PlayerId);
    }
    #endregion

    #region 적 이동 핸들러
    public static void C_EnemyMoveHandler(PacketSession session, IMessage packet)
    {
        // Host Player 로 부터 받은 몬스터 좌표로 좌표 설정 
        C_EnemyMove eMovePacket = packet as C_EnemyMove;
        ClientSession clientSession = session as ClientSession;
        Enemy enemy;
        clientSession.MyPlayer.Room.enemies.TryGetValue(eMovePacket.EnemyId, out enemy);
        if (enemy == null)
            return;
        enemy.enemyInfo.PosInfo = eMovePacket.Posinfo;
    }
    #endregion

    #region 플레이어 삭제 핸들러 
    public static void C_PlayerDestroyHandler(PacketSession session, IMessage packet)
    {
        C_PlayerDestroy cPlayerDestroyPacket = packet as C_PlayerDestroy;
        ClientSession clientSession = session as ClientSession;
        S_PlayerDestroy sPlayerDestroyPacket = new S_PlayerDestroy();

        clientSession.MyPlayer.Room.TargetRest(cPlayerDestroyPacket.PlayerId);
        sPlayerDestroyPacket.PlayerId = cPlayerDestroyPacket.PlayerId;

        clientSession.MyPlayer.Room.Broadcast(sPlayerDestroyPacket, cPlayerDestroyPacket.PlayerId);
        clientSession.MyPlayer.Room.count--;
        clientSession.MyPlayer.Room.PlayerDead(cPlayerDestroyPacket.PlayerId);
    }
    #endregion

    #region 적 삭제 핸들러
    public static void C_EnemyDestroyHandler(PacketSession session, IMessage packet)
    {
        C_EnemyDestroy eDestroyPaceket = packet as C_EnemyDestroy;
        S_EnemyDestroy sEnemyDestroyPacekt = new S_EnemyDestroy();
        ClientSession clientSession = session as ClientSession;
        sEnemyDestroyPacekt.EnemyId = eDestroyPaceket.EnemyId;
        clientSession.MyPlayer.Room.Broadcast(sEnemyDestroyPacekt,clientSession.MyPlayer.Info.PlayerId);
        clientSession.MyPlayer.Room.DestroyEnemy(eDestroyPaceket.EnemyId);
    }
    #endregion

    #region 적 히트 핸들러
    public static void C_EnemyHitHandler(PacketSession session, IMessage packet)
    {
        C_EnemyHit eHitInfoPacket = packet as C_EnemyHit;
        S_EnemyHit eHitPacket = new S_EnemyHit();
        ClientSession clientSession = session as ClientSession;
        eHitPacket.EnemyId = eHitInfoPacket.EnemyId;
        eHitPacket.CurHp = eHitInfoPacket.CurHp;
        clientSession.MyPlayer.Room.Broadcast(eHitPacket, clientSession.MyPlayer.Info.PlayerId);
    }
    #endregion

    #region 플레이어 히트 핸들러
    public static void C_PlayerHitHandler(PacketSession session, IMessage packet)
    {
        C_PlayerHit eHitInfoPacket = packet as C_PlayerHit;
        S_PlayerHit eHitPacket = new S_PlayerHit();
        ClientSession clientSession = session as ClientSession;
        eHitPacket.PlayerId = eHitInfoPacket.PlayerId;
        eHitPacket.CurHp = eHitInfoPacket.CurHp;
        clientSession.MyPlayer.Room.Broadcast(eHitPacket, clientSession.MyPlayer.Info.PlayerId);
    }
    #endregion

    #region 채팅 핸들러
    public static void C_PlayerChatHandler(PacketSession session, IMessage packet)
    {
        C_PlayerChat cPlayerChat = packet as C_PlayerChat;
        S_PlayerChat sPlayerChat = new S_PlayerChat();
        ClientSession clientSession = session as ClientSession;
        sPlayerChat.Chat = cPlayerChat.Chat;
        clientSession.MyPlayer.Room.Broadcast(sPlayerChat, clientSession.MyPlayer.Info.PlayerId);
    }
    #endregion

    #region 플레이어 선택 핸들러
    public static void C_PlayerSelectHandler(PacketSession session, IMessage packet)
    {
        C_PlayerSelect cPlayerSelectPacket = packet as C_PlayerSelect;
        ClientSession clientSession = session as ClientSession;
        
        if (clientSession.MyPlayer.Room.selects.Contains(cPlayerSelectPacket.PlayerCode))
        {
            S_PlayerAlreadySelected sPlayerAlreadySelectedPacket = new S_PlayerAlreadySelected();
            sPlayerAlreadySelectedPacket.PlayerCode = cPlayerSelectPacket.PlayerCode;
            clientSession.Send(sPlayerAlreadySelectedPacket);
        }
        
        else
        {
            clientSession.MyPlayer.Room.selects.Add(cPlayerSelectPacket.PlayerCode);
            clientSession.MyPlayer.Info.PlayerType = cPlayerSelectPacket.PlayerCode;
            S_GameReady gameReadyPacket = new S_GameReady();
            clientSession.Send(gameReadyPacket);
            clientSession.MyPlayer.Room.selectCount++;
            if (clientSession.MyPlayer.Room.selectCount == 2)
            {
                S_MainGameStart mainGameStartPacket = new S_MainGameStart();
                clientSession.MyPlayer.Room.Broadcast(mainGameStartPacket);
                clientSession.MyPlayer.Room.isGameStart = true;
                clientSession.MyPlayer.Room.time = 90;
            }
        }
    }
    #endregion

    #region 게임 시작 핸들러
    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame cEnterGamePacket = packet as C_EnterGame;
        ClientSession clientSession = session as ClientSession;
        clientSession.MyPlayer.Room.StartGame(clientSession.MyPlayer);
    }
    #endregion
}
