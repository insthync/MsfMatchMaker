using System.Collections;
using System.Collections.Generic;
using Barebones.MasterServer;
using Barebones.Networking;

public class MobaMatchMakerClient : QueueMatchMakerClient, ILobbyListener
{
    public UIMobaCharacterSelectionLobby uiCharacterSelectionLobby;

    protected override void Awake()
    {
        base.Awake();

        if (uiCharacterSelectionLobby == null)
            uiCharacterSelectionLobby = FindObjectOfType<UIMobaCharacterSelectionLobby>();
    }

    protected override void DisplayLobbyWindowIfInLobby()
    {
        var lastLobby = Msf.Client.Lobbies.LastJoinedLobby;
        if (lastLobby != null && !lastLobby.HasLeft)
        {
            lastLobby.SetListener(this);
            uiLobby.gameObject.SetActive(true);
        }
        uiCharacterSelectionLobby.gameObject.SetActive(false);
    }

    protected virtual void DisplayCharacterSelectionLobbyWindowIfInLobby()
    {
        var lastLobby = Msf.Client.Lobbies.LastJoinedLobby;
        if (lastLobby != null && !lastLobby.HasLeft)
        {
            lastLobby.SetListener(this);
            uiCharacterSelectionLobby.gameObject.SetActive(true);
        }
        uiLobby.gameObject.SetActive(false);
    }

    #region ILobbyListener integration
    public void Initialize(JoinedLobby lobby)
    {
        uiLobby.Initialize(lobby);
        uiCharacterSelectionLobby.Initialize(lobby);
    }

    public void OnMemberPropertyChanged(LobbyMemberData member, string property, string value)
    {
        uiLobby.OnMemberPropertyChanged(member, property, value);
        uiCharacterSelectionLobby.OnMemberPropertyChanged(member, property, value);
    }

    public void OnMemberJoined(LobbyMemberData member)
    {
        uiLobby.OnMemberJoined(member);
        uiCharacterSelectionLobby.OnMemberJoined(member);
    }

    public void OnMemberLeft(LobbyMemberData member)
    {
        uiLobby.OnMemberLeft(member);
        uiCharacterSelectionLobby.OnMemberLeft(member);
    }

    public void OnLobbyLeft()
    {
        uiLobby.OnLobbyLeft();
        uiCharacterSelectionLobby.OnLobbyLeft();
    }

    public void OnChatMessageReceived(LobbyChatPacket packet)
    {
        uiLobby.OnChatMessageReceived(packet);
        uiCharacterSelectionLobby.OnChatMessageReceived(packet);
    }

    public void OnLobbyPropertyChanged(string property, string value)
    {
        uiLobby.OnLobbyPropertyChanged(property, value);
        uiCharacterSelectionLobby.OnLobbyPropertyChanged(property, value);
        if (property.Equals(MobaCharacterSelectionLobby.PROPERTY_MOBA_LOBBY_STATE_KEY))
        {
            var state = (MobaCharacterSelectionLobby.MobaLobbyState)short.Parse(value);
            switch (state)
            {
                case MobaCharacterSelectionLobby.MobaLobbyState.WaitingPlayersToReady:
                    DisplayLobbyWindowIfInLobby();
                    break;
                case MobaCharacterSelectionLobby.MobaLobbyState.CharacterSelection:
                    DisplayCharacterSelectionLobbyWindowIfInLobby();
                    break;
            }
        }
    }

    public void OnMasterChanged(string masterUsername)
    {
        uiLobby.OnMasterChanged(masterUsername);
        uiCharacterSelectionLobby.OnMasterChanged(masterUsername);
    }

    public void OnMemberReadyStatusChanged(LobbyMemberData member, bool isReady)
    {
        uiLobby.OnMemberReadyStatusChanged(member, isReady);
        uiCharacterSelectionLobby.OnMemberReadyStatusChanged(member, isReady);
    }

    public void OnMemberTeamChanged(LobbyMemberData member, LobbyTeamData team)
    {
        uiLobby.OnMemberTeamChanged(member, team);
        uiCharacterSelectionLobby.OnMemberTeamChanged(member, team);
    }

    public void OnLobbyStatusTextChanged(string statusText)
    {
        uiLobby.OnLobbyStatusTextChanged(statusText);
        uiCharacterSelectionLobby.OnLobbyStatusTextChanged(statusText);
    }

    public void OnLobbyStateChange(LobbyState state)
    {
        uiLobby.OnLobbyStateChange(state);
        uiCharacterSelectionLobby.OnLobbyStateChange(state);
    }
    #endregion
}
