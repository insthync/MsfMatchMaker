using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueMatchMakerMatch
{
    public int SpawnId;
    public List<QueueMatchMakerPlayer> Players;
    public string MachineId { get; set; }
    public string SpawnCode { get; set; }
    public int AssignedPort { get; set; }

    public QueueMatchMakerMatch(int spawnId, List<QueueMatchMakerPlayer> players)
    {
        SpawnId = spawnId;
        Players = players;
        MachineId = string.Empty;
        SpawnCode = string.Empty;
    }
}
