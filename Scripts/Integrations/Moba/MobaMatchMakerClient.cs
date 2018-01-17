using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;
using Barebones.Networking;

public class MobaMatchMakerClient : QueueMatchMakerClient
{
    public event System.Action AllPlayersReady;

    protected override void Awake()
    {
        base.Awake();

        Msf.Client.SetHandler((short)MobaMatchMakerOpCodes.AllPlayersReady, OnAllPlayersReady);
    }

    protected virtual void OnAllPlayersReady(IIncommingMessage message)
    {
        if (AllPlayersReady != null)
            AllPlayersReady.Invoke();
    }
}
