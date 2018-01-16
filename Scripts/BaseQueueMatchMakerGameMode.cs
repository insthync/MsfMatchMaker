using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;
using Barebones.Networking;

public abstract class BaseQueueMatchMakerGameMode : ScriptableObject
{
    public abstract int PlayersPerMatch { get; }
    protected abstract ILobby GenerateLobbyWithPlayers(LobbiesModule module, List<QueueMatchMakerPlayer> players);

    /// <summary>
    /// Initialize function, this will be called when lobbies module initialize, you may add server messages handlers here
    /// </summary>
    /// <param name="server"></param>
    public virtual void Initialize(IServer server)
    {

    }

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
        
        foreach (var player in players)
        {
            player.Peer.SendMessage((short)QueueMatchMakerOpCodes.matchMakingLobbyCreated, lobby.Id);
        }
        return true;
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
