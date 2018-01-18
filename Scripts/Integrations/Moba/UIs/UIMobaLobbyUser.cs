using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Barebones.MasterServer;

public class UIMobaLobbyUser : LobbyUserUi
{
    public Image characterIcon;

    public void SelectCharacter(MobaCharacterData data)
    {
        if (characterIcon != null)
            characterIcon.sprite = data.icon;
    }
}
