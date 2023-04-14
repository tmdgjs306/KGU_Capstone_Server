using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyPlayer == null)
            return;
        if (clientSession.MyPlayer.Room == null)
            return;

        // 서버에서 좌표이동 
        //PlayerInfo info = clientSession.MyPlayer.Info;
        //info.PosInfo = movePacket.PosInfo;

        // 다른 플레이어한테도 알려준다
        S_Move resMovePacket = new S_Move();
        resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resMovePacket.PosInfo = movePacket.PosInfo;

        clientSession.MyPlayer.Room.Broadcast(resMovePacket);
    }
    public static void C_ActionHandler(PacketSession session, IMessage packet)
    {
        C_Action actPacket = packet as C_Action;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyPlayer == null)
            return;
        if (clientSession.MyPlayer.Room == null)
            return;

        // 다른 플레이어들에게 액션 처리 전송
        S_Action resActionPacket = new S_Action();
        resActionPacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resActionPacket.ActInfo = actPacket.ActInfo;

        clientSession.MyPlayer.Room.Broadcast(resActionPacket);

    }
}