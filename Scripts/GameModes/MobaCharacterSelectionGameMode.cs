using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;
using Barebones.MasterServer;

public class MobaCharacterSelectionGameMode : BaseQueueMatchMakerGameMode
{
    public int playersPerTeam = 2;
    public int waitToReadySeconds = 10;
    public int waitSeconds = 30;
    public override int PlayersPerMatch { get { return playersPerTeam * 2; } }

    protected override ILobby GenerateLobbyWithPlayers(LobbiesModule module, List<QueueMatchMakerPlayer> players)
    {
        var teamA = new LobbyTeam("Team A")
        {
            MaxPlayers = playersPerTeam,
            MinPlayers = playersPerTeam
        };
        var teamB = new LobbyTeam("Team B")
        {
            MaxPlayers = playersPerTeam,
            MinPlayers = playersPerTeam
        };

        var config = new LobbyConfig();
        var lobby = new MobaCharacterSelectionLobby(module.GenerateLobbyId(), new[] { teamA, teamB }, module, config);
        lobby.waitToReadySeconds = waitToReadySeconds;
        lobby.waitSeconds = waitSeconds;
        lobby.StartAutomation();

        return lobby;
    }
}
