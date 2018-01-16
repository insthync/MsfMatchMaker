using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;
using Barebones.MasterServer;

[CreateAssetMenu(fileName = "MobaCharacterSelectionGameMode", menuName = "GameMode/MobaCharacterSelectionGameMode")]
public class MobaCharacterSelectionGameMode : BaseQueueMatchMakerGameMode
{
    public const string TEAM_A = "A";
    public const string TEAM_B = "B";
    public int playersPerTeam = 2;
    public int waitToReadySeconds = 10;
    public int waitSeconds = 30;
    public override int PlayersPerMatch { get { return playersPerTeam * 2; } }

    protected override ILobby GenerateLobbyWithPlayers(LobbiesModule module, List<QueueMatchMakerPlayer> players)
    {
        var teamA = new LobbyTeam(TEAM_A)
        {
            MaxPlayers = playersPerTeam,
            MinPlayers = playersPerTeam
        };
        var teamB = new LobbyTeam(TEAM_B)
        {
            MaxPlayers = playersPerTeam,
            MinPlayers = playersPerTeam
        };

        var config = new LobbyConfig();
        var lobby = new MobaCharacterSelectionLobby(module.GenerateLobbyId(), new[] { teamA, teamB }, module, config);
        lobby.Name = "#" + lobby.Id;
        lobby.waitToReadySeconds = waitToReadySeconds;
        lobby.waitSeconds = waitSeconds;
        lobby.StartAutomation();

        return lobby;
    }
}
