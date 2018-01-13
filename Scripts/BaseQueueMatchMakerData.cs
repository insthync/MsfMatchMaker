using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseQueueMatchMakerData : ScriptableObject
{
    public abstract Dictionary<string, string> GenerateSceneSpawnInfo(Dictionary<string, string> matchDetails);
}
