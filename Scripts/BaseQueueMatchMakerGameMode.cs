using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;
using Barebones.Networking;

public abstract class BaseQueueMatchMakerGameMode : ScriptableObject
{
    public abstract int PlayersPerMatch { get; }
    public abstract ILobby GenerateLobby(LobbiesModule module, List<QueueMatchMakerPlayer> players);
}
