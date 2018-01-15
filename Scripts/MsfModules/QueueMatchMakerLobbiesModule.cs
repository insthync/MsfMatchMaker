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
                GameModes.Add(gameMode.name, gameMode);
        }
    }

    public bool CreateGameLobby(string gameModeName, List<QueueMatchMakerPlayer> players)
    {
        BaseQueueMatchMakerGameMode gameMode;
        if (!GameModes.TryGetValue(gameModeName, out gameMode))
            return false;

        var newLobby = gameMode.GenerateLobby(this, players);
        if (AddLobby(newLobby))
        {
            var problemOccurs = false;
            var lobbyPackets = new Dictionary<IPeer, LobbyDataPacket>();
            foreach (var player in players)
            {
                var user = GetOrCreateLobbiesExtension(player.Peer);
                if (user.CurrentLobby != null)
                {
                    problemOccurs = true;
                    break;
                }

                string error;
                if (!newLobby.AddPlayer(user, out error))
                {
                    problemOccurs = true;
                    break;
                }

                lobbyPackets.Add(player.Peer, newLobby.GenerateLobbyData(user));
            }
            
            if (!problemOccurs)
            {
                // send lobby data to players
                foreach (var lobbyPacket in lobbyPackets)
                {
                    lobbyPacket.Key.SendMessage((short)QueueMatchMakerOpCodes.matchMakingLobbyCreated, lobbyPacket.Value);
                }
                return true;
            }
        }

        return false;
    }
}
