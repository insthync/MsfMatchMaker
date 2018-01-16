using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Logging;
using Barebones.MasterServer;
using Barebones.Networking;

public class QueueMatchMakerLobbiesModule : LobbiesModule
{
    public BaseQueueMatchMakerGameMode[] gameModes;
    public readonly Dictionary<string, BaseQueueMatchMakerGameMode> GameModes = new Dictionary<string, BaseQueueMatchMakerGameMode>();

    public override void Initialize(IServer server)
    {
        base.Initialize(server);

        // Use game mode to generate lobby
        foreach (var gameMode in gameModes)
        {
            if (!GameModes.ContainsKey(gameMode.name))
            {
                gameMode.Initialize(server);
                GameModes.Add(gameMode.name, gameMode);
            }
        }
    }

    public bool CreateGameLobby(string gameModeName, List<QueueMatchMakerPlayer> players)
    {
        BaseQueueMatchMakerGameMode gameMode;
        if (!GameModes.TryGetValue(gameModeName, out gameMode))
            return false;

        var newLobby = gameMode.CreateLobbyWithPlayers(this, players);
        return newLobby != null && AddLobby(newLobby);
    }
}
