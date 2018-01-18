using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIMobaLobbyCharacter : MonoBehaviour
{
    public Text title;
    public Text description;
    public Image icon;
    public GameObject selected;
    public GameObject selectedByCurrentUser;
    private MobaCharacterData data;
    private bool isSelected;

    public virtual void Reset()
    {
        if (title != null)
            title.text = "";

        if (description != null)
            description.text = "";

        if (icon != null)
            icon.sprite = null;

        SetSelected(false);
        SetSelectedByCurrentUser(false);
    }

    public virtual void Setup(MobaCharacterData data)
    {
        this.data = data;

        if (title != null)
            title.text = data.title;

        if (description != null)
            description.text = data.description;

        if (icon != null)
            icon.sprite = data.icon;
    }

    public void SetSelected(bool isSelected)
    {
        if (selected != null)
            selected.SetActive(isSelected);

        this.isSelected = isSelected;
    }

    public void SetSelectedByCurrentUser(bool isSelectedByCurrentUser)
    {
        if (selectedByCurrentUser != null)
            selectedByCurrentUser.SetActive(isSelectedByCurrentUser);
    }

    public void OnClickSelectCharacter()
    {
        var lobby = GetComponentInParent<UIMobaCharacterSelectionLobby>();
        if (lobby != null && !isSelected)
            lobby.JoinedLobby.SetLobbyProperty(MobaCharacterSelectionLobby.PROPERTY_CHARACTER_KEY_PREFIX + lobby.CurrentUser, data.name);
    }
}
