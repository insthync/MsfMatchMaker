using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;
using Barebones.MasterServer;

public class MobaCharacterSelectionLobby : BaseLobby
{
    public const string PROPERTY_PREFIX_CHARACTER = "character_";
    public float waitSeconds = 10;

    public MobaCharacterSelectionLobby(int lobbyId, IEnumerable<LobbyTeam> teams, LobbiesModule module, LobbyConfig config) : base(lobbyId, teams, module, config)
    {
        config.EnableReadySystem = false;
        config.EnableManualStart = false;
        config.PlayAgainEnabled = false;
        config.EnableGameMasters = false;
    }

    protected override void OnPlayerAdded(LobbyMember member)
    {
        base.OnPlayerAdded(member);
        
        // Add controls for player's character (May add skills)
        AddControl(new LobbyPropertyData()
        {
            Label = "",
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

    public void StartAutomation()
    {
        BTimer.Instance.StartCoroutine(StartTimer());
    }

    protected IEnumerator StartTimer()
    {
        float timeToWait = waitSeconds;

        var initialState = State;

        while (State == LobbyState.Preparations || State == initialState)
        {
            yield return new WaitForSeconds(1f);

            if (IsDestroyed)
                break;

            // Reduce the time to wait by one second
            timeToWait -= 1;

            StatusText = "Starting game in " + timeToWait;

            if (timeToWait <= 0)
            {
                StartGame();
                Destroy();
                break;
            }
        }
    }
}
