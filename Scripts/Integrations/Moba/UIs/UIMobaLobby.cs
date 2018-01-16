using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;
using Barebones.Utils;
using System;

public class UIMobaLobby : LobbyUi
{
    public Text gameStatus;
    [Header("Player Waiting to Ready")]
    public LobbyUserUi waitingPlayerPrefab;

    public override void OnLobbyPropertyChanged(string property, string value)
    {
        base.OnLobbyPropertyChanged(property, value);
    }
}
