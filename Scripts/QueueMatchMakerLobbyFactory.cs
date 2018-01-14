using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;
using Barebones.Networking;


public class QueueMatchMakerLobbyFactory : ILobbyFactory
{
    private LobbiesModule _module;
    private readonly LobbyCreationFactory _factory;

    public delegate ILobby LobbyCreationFactory(LobbiesModule module, Dictionary<string, string> properties);

    public QueueMatchMakerLobbyFactory(string id, LobbiesModule module, LobbyCreationFactory factory)
    {
        Id = id;
        _factory = factory;
        _module = module;
    }

    public ILobby CreateLobby(Dictionary<string, string> properties, IPeer creator)
    {
        var lobby = _factory.Invoke(_module, properties);

        // Add the lobby type if it's not set by the factory method
        if (lobby != null && lobby.Type == null)
            lobby.Type = Id;

        return lobby;
    }

    public string Id { get; private set; }
}
