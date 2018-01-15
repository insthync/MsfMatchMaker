using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;
using Barebones.Networking;

public abstract class BaseQueueMatchMakerGameMode : ScriptableObject
{
    public abstract int PlayersPerMatch { get; }
    protected abstract ILobby GenerateLobbyWithPlayers(LobbiesModule module, List<QueueMatchMakerPlayer> players);

    public virtual ILobby CreateLobbyWithPlayers(LobbiesModule module, List<QueueMatchMakerPlayer> players)
    {
        var lobby = GenerateLobbyWithPlayers(module, players);

        if (AddPlayersToLobby(lobby, players))
            return lobby;

        return null;
    }

    protected virtual bool AddPlayersToLobby(ILobby lobby, List<QueueMatchMakerPlayer> players)
    {
        if (lobby == null)
            return false;

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
            if (!lobby.AddPlayer(user, out error))
            {
                problemOccurs = true;
                break;
            }

            lobbyPackets.Add(player.Peer, lobby.GenerateLobbyData(user));
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
        else
        {
            foreach (var player in players)
            {
                var user = GetOrCreateLobbiesExtension(player.Peer);
                lobby.RemovePlayer(user);
            }
            return false;
        }
    }

    protected virtual LobbyUserExtension GetOrCreateLobbiesExtension(IPeer peer)
    {
        var extension = peer.GetExtension<LobbyUserExtension>();

        if (extension == null)
        {
            extension = new LobbyUserExtension(peer);
            peer.AddExtension(extension);
        }

        return extension;
    }
}
