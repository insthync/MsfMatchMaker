using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;

public class QueueMatchMakerPlayer
{
    public IPeer Peer { get; protected set; }
    public string Username { get; protected set; }
    public string GameModeName { get; protected set; }
    public float Time { get; protected set; }

    protected Dictionary<string, string> Properties;

    public QueueMatchMakerPlayer(IPeer peer, string username, string gameModeName, float time, Dictionary<string, string> properties = null)
    {
        Peer = peer;
        Username = username;
        GameModeName = gameModeName;
        Time = time;
        Properties = (properties == null) ? new Dictionary<string, string>() : properties;
    }

    public void SetProperty(string key, string value)
    {
        Properties[key] = value;
    }

    public string GetProperty(string key)
    {
        string result;

        Properties.TryGetValue(key, out result);
        return result;
    }
}
