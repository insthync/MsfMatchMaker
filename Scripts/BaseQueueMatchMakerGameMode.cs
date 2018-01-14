using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseQueueMatchMakerGameMode : ScriptableObject
{
    public int playersPerMatch = 1;
    public abstract Dictionary<string, string> GenerateSceneSpawnInfo(Dictionary<string, string> matchDetails);
}
