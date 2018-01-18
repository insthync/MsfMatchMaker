using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMobaLobbyCharacter : MonoBehaviour
{
    public Text title;
    public Text description;
    public Image icon;

    public virtual void Reset()
    {
        if (title != null)
            title.text = "";

        if (description != null)
            description.text = "";

        if (icon != null)
            icon.sprite = null;
    }

    public virtual void Setup(MobaCharacterData data)
    {
        if (title != null)
            title.text = data.title;

        if (description != null)
            description.text = data.description;

        if (icon != null)
            icon.sprite = data.icon;
    }
}
