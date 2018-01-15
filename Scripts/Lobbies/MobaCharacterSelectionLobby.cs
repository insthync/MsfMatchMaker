using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;
using Barebones.MasterServer;

public class MobaCharacterSelectionLobby : BaseLobby
{
    public float waitSeconds = 10;

    public MobaCharacterSelectionLobby(int lobbyId, IEnumerable<LobbyTeam> teams, LobbiesModule module, LobbyConfig config) : base(lobbyId, teams, module, config)
    {
        config.EnableReadySystem = false;
        config.EnableManualStart = false;
        config.PlayAgainEnabled = false;
        config.EnableGameMasters = false;
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
