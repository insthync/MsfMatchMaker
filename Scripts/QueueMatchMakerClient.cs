using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Barebones.MasterServer;
using Barebones.Networking;

public class QueueMatchMakerClient : MonoBehaviour
{
    public List<GameObject> enableObjectsOnLogIn;
    public List<GameObject> disableObjectsOnLogout;
    public event System.Action<int> MatchMakingLobbyCreated;
    public LobbyUi uiLobby;

    protected virtual void Awake()
    {
        Msf.Client.Auth.LoggedIn += OnLoggedIn;
        Msf.Client.Auth.LoggedOut += OnLoggedOut;

        if (Msf.Client.Auth.IsLoggedIn)
            OnLoggedIn();
        
        Msf.Client.SetHandler((short)QueueMatchMakerOpCodes.matchMakingLobbyCreated, OnMatchMakingLobbyCreated);
        
        if (uiLobby == null)
            uiLobby = FindObjectOfType<LobbyUi>();
    }

    protected virtual void Start()
    {
        DisplayLobbyWindowIfInLobby();
    }

    protected virtual void OnDestroy()
    {
        Msf.Client.Auth.LoggedIn -= OnLoggedIn;
        Msf.Client.Auth.LoggedOut -= OnLoggedOut;
    }

    protected virtual void DisplayLobbyWindowIfInLobby()
    {
        var lastLobby = Msf.Client.Lobbies.LastJoinedLobby;
        if (lastLobby != null && !lastLobby.HasLeft)
        {
            lastLobby.SetListener(uiLobby);
            uiLobby.gameObject.SetActive(true);
        }
    }

    protected virtual void OnLoggedIn()
    {
        foreach (var obj in enableObjectsOnLogIn)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    protected virtual void OnLoggedOut()
    {
        foreach (var obj in disableObjectsOnLogout)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    protected virtual void OnMatchMakingLobbyCreated(IIncommingMessage message)
    {
        var lobbyId = message.AsInt();
        var loadingPromise = Msf.Events.FireWithPromise(Msf.EventNames.ShowLoading, "Joining lobby");
        Msf.Client.Lobbies.JoinLobby(lobbyId, (lobby, error) =>
        {
            loadingPromise.Finish();

            if (lobby == null)
            {
                Msf.Events.Fire(Msf.EventNames.ShowDialogBox, DialogBoxData.CreateError(error));
                return;
            }
            DisplayLobbyWindowIfInLobby();
        });

        if (MatchMakingLobbyCreated != null)
            MatchMakingLobbyCreated.Invoke(lobbyId);
    }
}
