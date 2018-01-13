using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueMatchMakerDataList : MonoBehaviour {
    public static QueueMatchMakerDataList Singleton { get; private set; }
    public BaseQueueMatchMakerData[] dataList;
    public readonly Dictionary<string, BaseQueueMatchMakerData> DataList = new Dictionary<string, BaseQueueMatchMakerData>();

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);

        foreach (var data in dataList)
        {
            DataList[data.name] = data;
        }
    }
}
