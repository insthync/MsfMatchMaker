using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.MasterServer;
using Barebones.Networking;

public class QueueMatchMakerClient : MonoBehaviour
{
    public static QueueMatchMakerClient Singleton { get; private set; }
    public List<GameObject> enableObjectsOnLogIn;
    public List<GameObject> disableObjectsOnLogout;
    public event System.Action<int> MatchMakingLobbyCreated;
    public event System.Action<JoinedLobby> MatchMakingLobbyJoined;

    protected virtual void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject);

        Msf.Client.Auth.LoggedIn += OnLoggedIn;
        Msf.Client.Auth.LoggedOut += OnLoggedOut;

        if (Msf.Client.Auth.IsLoggedIn)
            OnLoggedIn();
        
        Msf.Client.SetHandler((short)QueueMatchMakerOpCodes.MatchMakingLobbyCreated, OnMatchMakingLobbyCreated);
    }

    protected virtual void OnDestroy()
    {
        Msf.Client.Auth.LoggedIn -= OnLoggedIn;
        Msf.Client.Auth.LoggedOut -= OnLoggedOut;
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
        // When match making lobby created, join the lobby
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
            OnMatchMakingLobbyJoined(lobby);
        });

        if (MatchMakingLobbyCreated != null)
            MatchMakingLobbyCreated.Invoke(lobbyId);
    }

    protected virtual void OnMatchMakingLobbyJoined(JoinedLobby lobby)
    {
        if (MatchMakingLobbyJoined != null)
            MatchMakingLobbyJoined.Invoke(lobby);
    }
}
