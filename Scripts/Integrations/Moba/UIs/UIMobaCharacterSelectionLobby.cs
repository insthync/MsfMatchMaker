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

    protected readonly Dictionary<string, MobaCharacterData> CharacterData = new Dictionary<string, MobaCharacterData>();
    protected readonly Dictionary<string, UIMobaLobbyCharacter> Characters = new Dictionary<string, UIMobaLobbyCharacter>();
    protected readonly Dictionary<string, UIMobaLobbyCharacter> CharactersByUsers = new Dictionary<string, UIMobaLobbyCharacter>();

    public override void Initialize(JoinedLobby lobby)
    {
        base.Initialize(lobby);

        for (var i = charactersContainer.childCount - 1; i >= 0; --i)
            Destroy(charactersContainer.GetChild(i).gameObject);

        CharacterData.Clear();
        Characters.Clear();
        CharactersByUsers.Clear();

        foreach (var character in MobaGameDatabase.Singleton.characters)
        {
            CharacterData[character.name] = character;
            Characters[character.name] = CreateCharacterView(character);
        }

        var properties = lobby.Properties;

        if (uiChat != null)
            uiChat.Clear();
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
        LobbyMemberData member;
        if (!JoinedLobby.Members.TryGetValue(username, out member))
            return;

        if (member.Team.Equals(CurrentTeam))
        {
            UIMobaLobbyCharacter character;
            if (CharactersByUsers.TryGetValue(username, out character)) {
                character.SetSelected(false);
                character.SetSelectedByCurrentUser(false);
            }

            if (Characters.TryGetValue(value, out character))
            {
                character.SetSelected(true);
                if (username.Equals(CurrentUser))
                    character.SetSelectedByCurrentUser(true);
                CharactersByUsers[username] = character;
            }
        }

        MobaCharacterData characterData;
        UIMobaLobbyUser user;
        if (Users.TryGetValue(username, out user) && CharacterData.TryGetValue(value, out characterData))
            user.SelectCharacter(characterData);
    }
}
