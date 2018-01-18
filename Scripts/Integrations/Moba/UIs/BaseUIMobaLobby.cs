using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

public class BaseUIMobaLobby : MonoBehaviour
{
    public UIMobaLobbyUser userPrefab;
    public Text gameStatus;
    public Button readyButton;
    public Transform alliesContainer;
    public Transform enemiesContainer;
    
    protected readonly Dictionary<string, UIMobaLobbyUser> Users = new Dictionary<string, UIMobaLobbyUser>();
    public string CurrentUser { get; protected set; }
    public string CurrentTeam { get; protected set; }
    public JoinedLobby JoinedLobby { get; protected set; }
    
    public virtual void Initialize(JoinedLobby lobby)
    {
        JoinedLobby = lobby;
        CurrentUser = lobby.Data.CurrentUserUsername;
        CurrentTeam = lobby.Members[CurrentUser].Team;

        for (var i = alliesContainer.childCount - 1; i >= 0; --i)
            Destroy(alliesContainer.GetChild(i).gameObject);

        for (var i = enemiesContainer.childCount - 1; i >= 0; --i)
            Destroy(enemiesContainer.GetChild(i).gameObject);

        Users.Clear();

        foreach (var player in lobby.Members)
            Users.Add(player.Key, CreateMemberView(player.Value));

        UpdateReadyButton();
    }

    protected virtual UIMobaLobbyUser CreateMemberView(LobbyMemberData data)
    {
        // Get user view
        var user = Instantiate(userPrefab);
        user.Reset();
        user.gameObject.SetActive(true);
        user.Setup(data);
        user.IsCurrentPlayer = data.Username == CurrentUser;

        // Add user to team
        var teamContainer = data.Team.Equals(CurrentTeam) ? alliesContainer : enemiesContainer;
        user.transform.SetParent(teamContainer, false);
        user.transform.SetAsLastSibling();

        user.SetReadyStatusVisibility(AreUserReadyStatesVisible());

        // Generate username text
        user.Username.text = data.Username;

        return user;
    }

    /// <summary>
    /// Returns true if user ready states should be visible
    /// </summary>
    /// <returns></returns>
    protected bool AreUserReadyStatesVisible()
    {
        return JoinedLobby.State == LobbyState.Preparations && JoinedLobby.Data.EnableReadySystem;
    }

    public virtual void OnMemberJoined(LobbyMemberData member)
    {
        Users.Add(member.Username, CreateMemberView(member));
    }

    public virtual void OnMemberLeft(LobbyMemberData member)
    {
        UIMobaLobbyUser user;
        if (!Users.TryGetValue(member.Username, out user))
            return;

        Destroy(user.gameObject);
        Users.Remove(member.Username);
    }


    public virtual void OnMemberReadyStatusChanged(LobbyMemberData member, bool isReady)
    {
        UIMobaLobbyUser user;
        if (!Users.TryGetValue(member.Username, out user))
            return;

        user.SetReady(isReady);

        UpdateReadyButton();
    }

    public virtual void OnMemberTeamChanged(LobbyMemberData member, LobbyTeamData team)
    {
        UIMobaLobbyUser user;
        if (!Users.TryGetValue(member.Username, out user))
            return;

        // Player changed teams
        var teamContainer = team.Name.Equals(CurrentTeam) ? alliesContainer : enemiesContainer;
        user.transform.SetParent(teamContainer, false);

        UpdateReadyButton();
    }

    public virtual void OnLobbyLeft()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnLobbyStatusTextChanged(string statusText)
    {
        gameStatus.text = statusText;
    }

    protected virtual void UpdateReadyButton()
    {
        var user = Users[CurrentUser];

        if (readyButton != null)
            readyButton.enabled = !JoinedLobby.Members[CurrentUser].IsReady;
    }

    public virtual void OnClickReady()
    {
        // Can ready only once
        if (!JoinedLobby.Members[CurrentUser].IsReady)
            JoinedLobby.SetReadyStatus(true);

        UpdateReadyButton();
    }
}
