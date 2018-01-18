using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobaGameDatabase : MonoBehaviour
{
    public static MobaGameDatabase Singleton { get; private set; }
    public MobaCharacterData[] characters;

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }
}
