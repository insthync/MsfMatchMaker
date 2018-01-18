using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MobaCharacterData", menuName = "MobaGameData/MobaCharacterData")]
public class MobaCharacterData : ScriptableObject
{
    public string title;
    public string description;
    public Sprite icon;
}
