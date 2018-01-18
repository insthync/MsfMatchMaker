using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;
using Barebones.Utils;
using System;

public class UIMobaCharacterSelectionLobby : BaseUIMobaLobby
{
    public UIMobaLobbyCharacter characterPrefab;
    public UIMobaLobbyChat uiChat;
    public Transform charactersContainer;

    protected readonly Dictionary<string, UIMobaLobbyCharacter> Characters = new Dictionary<string, UIMobaLobbyCharacter>();

    public override void Initialize(JoinedLobby lobby)
    {
        base.Initialize(lobby);

        for (var i = charactersContainer.childCount - 1; i >= 0; --i)
            Destroy(charactersContainer.GetChild(i).gameObject);

        Characters.Clear();

        foreach (var character in MobaGameDatabase.Singleton.characters)
            Characters.Add(character.name, CreateCharacterView(character));

        if (uiChat != null)
        {
            uiChat.Clear();
        }
    }

    public UIMobaLobbyCharacter CreateCharacterView(MobaCharacterData data)
    {
        var character = Instantiate(characterPrefab);
        character.Reset();
        character.gameObject.SetActive(true);
        character.Setup(data);

        character.transform.SetParent(charactersContainer, false);
        character.transform.SetAsLastSibling();

        return character;
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
