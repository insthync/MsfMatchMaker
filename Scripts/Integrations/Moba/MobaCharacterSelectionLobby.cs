using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;
using Barebones.MasterServer;

public class MobaCharacterSelectionLobby : BaseLobby
{
    public const string PROPERTY_PREFIX_CHARACTER = "character_";
    public int waitToReadySeconds = 10;
    public int waitSeconds = 30;
    private readonly Dictionary<string, QueueMatchMakerPlayer> PlayablePlayers = new Dictionary<string, QueueMatchMakerPlayer>();
    private bool isPlayersReady;
    private int timeToWait;

    public MobaCharacterSelectionLobby(int lobbyId, IEnumerable<LobbyTeam> teams, LobbiesModule module, LobbyConfig config) : base(lobbyId, teams, module, config)
    {
        config.EnableReadySystem = true;
        config.EnableManualStart = false;
        config.EnableTeamSwitching = false;
        config.PlayAgainEnabled = false;
        config.EnableGameMasters = false;
    }

    public void SetPlayablePlayers(List<QueueMatchMakerPlayer> players)
    {
        foreach (var player in players)
            PlayablePlayers[player.Username] = player;
    }

    protected override void OnPlayerAdded(LobbyMember member)
    {
        // Don't add this player
        if (!PlayablePlayers.ContainsKey(member.Username))
        {
            RemovePlayer(member.Extension);
            return;
        }

        base.OnPlayerAdded(member);
        
        // Add controls for player's character (May add skills)
        AddControl(new LobbyPropertyData()
        {
            Label = member.Username + "'s Character",
            Options = new List<string>(),
            PropertyKey = PROPERTY_PREFIX_CHARACTER + member.Username,
        }, "");
    }

    public override bool SetProperty(LobbyUserExtension setter, string key, string value)
    {
        LobbyMember member;
        if (MembersByPeerId.TryGetValue(setter.Peer.Id, out member))
        {
            // Cannot set another player's character
            if (key.StartsWith(PROPERTY_PREFIX_CHARACTER) && !key.Substring(PROPERTY_PREFIX_CHARACTER.Length).Equals(member.Username))
                return false;
        }
        return base.SetProperty(setter, key, value);
    }

    protected override void OnAllPlayersReady()
    {
        if (!isPlayersReady)
        {
            Broadcast(MessageHelper.Create((short)MobaMatchMakerOpCodes.AllPlayersReady));
            timeToWait = waitSeconds;
            isPlayersReady = true;
        }
    }

    public void StartAutomation()
    {
        BTimer.Instance.StartCoroutine(StartTimer());
    }

    protected IEnumerator StartTimer()
    {
        timeToWait = waitToReadySeconds;
        isPlayersReady = false;

        var initialState = State;
        while (State == LobbyState.Preparations || State == initialState)
        {
            if (IsDestroyed)
                break;

            yield return new WaitForSeconds(1f);

            // Reduce the time to wait by one second
            timeToWait -= 1;

            if (!isPlayersReady)
            {
                StatusText = "Waiting for players " + timeToWait;
                if (timeToWait <= 0)
                {
                    Destroy();
                    break;
                }
            }
            else
            {
                StatusText = "Starting game in " + timeToWait;
                if (timeToWait <= 0)
                {
                    StartGame();
                    break;
                }
            }
        }
    }
}
