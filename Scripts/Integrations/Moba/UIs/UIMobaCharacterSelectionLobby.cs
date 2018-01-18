using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;
using Barebones.Utils;
using System;

public class UIMobaCharacterSelectionLobby : BaseUIMobaLobby
{
    public UIMobaLobbyChat uiChat;

    public override void Initialize(JoinedLobby lobby)
    {
        base.Initialize(lobby);

        if (uiChat != null)
        {
            uiChat.Clear();
        }
    }

    public virtual void OnChatMessageReceived(LobbyChatPacket packet)
    {
        LobbyMemberData member;
        if (JoinedLobby.Members.TryGetValue(packet.Sender, out member) && member.Team.Equals(CurrentTeam))
            uiChat.OnMessageReceived(packet);
    }

    public virtual void OnCharacterChanged(string username, string value)
    {

    }
}
