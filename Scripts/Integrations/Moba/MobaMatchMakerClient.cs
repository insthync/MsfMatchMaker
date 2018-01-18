using System.Collections;
using System.Collections.Generic;
using Barebones.MasterServer;
using Barebones.Networking;

public class MobaMatchMakerClient : QueueMatchMakerClient, ILobbyListener
{
    public UIMobaPlayersReadyLobby uiPlayersReadyLobby;
    public UIMobaCharacterSelectionLobby uiCharacterSelectionLobby;

    protected override void Awake()
    {
        base.Awake();

        if (uiPlayersReadyLobby == null)
            uiPlayersReadyLobby = FindObjectOfType<UIMobaPlayersReadyLobby>();

        if (uiCharacterSelectionLobby == null)
            uiCharacterSelectionLobby = FindObjectOfType<UIMobaCharacterSelectionLobby>();
    }

    protected override void OnMatchMakingLobbyJoined(JoinedLobby lobby)
    {
        if (lobby != null && !lobby.HasLeft)
        {
            lobby.SetListener(this);
            uiPlayersReadyLobby.gameObject.SetActive(true);
            uiCharacterSelectionLobby.gameObject.SetActive(false);
        }
    }

    #region ILobbyListener integration
    public void Initialize(JoinedLobby lobby)
    {
        uiPlayersReadyLobby.Initialize(lobby);
        uiCharacterSelectionLobby.Initialize(lobby);
    }

    public void OnMemberPropertyChanged(LobbyMemberData member, string property, string value)
    {
    }

    public void OnMemberJoined(LobbyMemberData member)
    {
        uiPlayersReadyLobby.OnMemberJoined(member);
        uiCharacterSelectionLobby.OnMemberJoined(member);
    }

    public void OnMemberLeft(LobbyMemberData member)
    {
        uiPlayersReadyLobby.OnMemberLeft(member);
        uiCharacterSelectionLobby.OnMemberLeft(member);
    }

    public void OnLobbyLeft()
    {
        uiPlayersReadyLobby.OnLobbyLeft();
        uiCharacterSelectionLobby.OnLobbyLeft();
    }

    public void OnChatMessageReceived(LobbyChatPacket packet)
    {
        uiCharacterSelectionLobby.OnChatMessageReceived(packet);
    }

    public void OnLobbyPropertyChanged(string property, string value)
    {
        if (property.Equals(MobaCharacterSelectionLobby.PROPERTY_MOBA_LOBBY_STATE_KEY))
        {
            var state = (MobaCharacterSelectionLobby.MobaLobbyState)short.Parse(value);
            switch (state)
            {
                case MobaCharacterSelectionLobby.MobaLobbyState.WaitingPlayersToReady:
                    uiPlayersReadyLobby.gameObject.SetActive(true);
                    uiCharacterSelectionLobby.gameObject.SetActive(false);
                    break;
                case MobaCharacterSelectionLobby.MobaLobbyState.CharacterSelection:
                    uiPlayersReadyLobby.gameObject.SetActive(false);
                    uiCharacterSelectionLobby.gameObject.SetActive(true);
                    break;
            }
        }
        else if (property.StartsWith(MobaCharacterSelectionLobby.PROPERTY_CHARACTER_KEY_PREFIX))
        {
            uiCharacterSelectionLobby.OnCharacterChanged(property.Substring(MobaCharacterSelectionLobby.PROPERTY_CHARACTER_KEY_PREFIX.Length), value);
        }
    }

    public void OnMasterChanged(string masterUsername)
    {
    }

    public void OnMemberReadyStatusChanged(LobbyMemberData member, bool isReady)
    {
        uiPlayersReadyLobby.OnMemberReadyStatusChanged(member, isReady);
        uiCharacterSelectionLobby.OnMemberReadyStatusChanged(member, isReady);
    }

    public void OnMemberTeamChanged(LobbyMemberData member, LobbyTeamData team)
    {
        uiPlayersReadyLobby.OnMemberTeamChanged(member, team);
        uiCharacterSelectionLobby.OnMemberTeamChanged(member, team);
    }

    public void OnLobbyStatusTextChanged(string statusText)
    {
        uiPlayersReadyLobby.OnLobbyStatusTextChanged(statusText);
        uiCharacterSelectionLobby.OnLobbyStatusTextChanged(statusText);
    }

    public void OnLobbyStateChange(LobbyState state)
    {
    }
    #endregion
}
