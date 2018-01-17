using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Barebones.MasterServer;
using Barebones.Networking;

public class MobaMatchMakerClient : QueueMatchMakerClient
{
    public event System.Action AllPlayersReady;

    protected override void Awake()
    {
        base.Awake();

        Msf.Client.SetHandler((short)MobaMatchMakerOpCodes.AllPlayersReady, OnAllPlayersReady);

        if (uiLobby == null)
            uiLobby = FindObjectOfType<UIMobaLobby>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (uiLobby != null && !(uiLobby is UIMobaLobby))
        {
            Debug.LogWarning("`uiLobby` for `MobaMatchMakerClient` must be `UIMobaLobby`");
            uiLobby = null;
            EditorUtility.SetDirty(this);
        }
    }
#endif

    protected virtual void OnAllPlayersReady(IIncommingMessage message)
    {
        if (AllPlayersReady != null)
            AllPlayersReady.Invoke();
    }
}
