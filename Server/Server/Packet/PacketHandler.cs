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
    public static void C_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        C_PlayerMove movePacket = packet as C_PlayerMove;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.MyPlayer == null)
            return;
        if (clientSession.MyPlayer.Room == null)
            return;

        // 서버에서 좌표이동 
        //PlayerInfo info = clientSession.MyPlayer.Info;
        //info.PosInfo = movePacket.PosInfo;

        // 다른 플레이어한테도 알려준다
        S_PlayerMove resMovePacket = new S_PlayerMove();
        resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resMovePacket.PosInfo = movePacket.PosInfo;
       
        clientSession.MyPlayer.Room.Broadcast(resMovePacket,resMovePacket.PlayerId);
    }
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
    public static void C_EnemyMoveHandler(PacketSession session, IMessage packet)
    {

    }
}