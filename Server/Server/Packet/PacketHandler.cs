﻿using Google.Protobuf;
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

        //Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosZ})");

        if (clientSession.MyPlayer == null)
            return;
        if (clientSession.MyPlayer.Room == null)
            return;

        // 서버에서 좌표이동 
        PlayerInfo info = clientSession.MyPlayer.Info;
        info.PosInfo = movePacket.PosInfo;

        // 다른 플레이어한테도 알려준다
        S_Move resMovePacket = new S_Move();
        resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resMovePacket.PosInfo = movePacket.PosInfo;

        clientSession.MyPlayer.Room.Broadcast(resMovePacket);
    }
}