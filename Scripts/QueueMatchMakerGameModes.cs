using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueMatchMakerGameModes : MonoBehaviour {
    public static QueueMatchMakerGameModes Singleton { get; private set; }
    public BaseQueueMatchMakerGameMode[] gameModes;
    public readonly Dictionary<string, BaseQueueMatchMakerGameMode> GameModes = new Dictionary<string, BaseQueueMatchMakerGameMode>();

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);

        foreach (var data in gameModes)
        {
            GameModes[data.name] = data;
        }
    }
}
