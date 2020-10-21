using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skins DataBase", menuName = "Assets/Scriptable/Skins")]
public class SkinData : ScriptableObject
{
    public List<Skin> skins = new List<Skin>(); 
}
