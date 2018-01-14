using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;
using Barebones.Networking;

public abstract class BaseQueueMatchMakerGameMode : ScriptableObject
{
    public int playersPerMatch = 1;
    public abstract Dictionary<string, string> GenerateSceneSpawnInfo(Dictionary<string, string> matchDetails);
    public abstract ILobby GenerateLobby(LobbiesModule module, List<QueueMatchMakerPlayer> players);
}
