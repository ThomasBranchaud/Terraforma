using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]

public class Basic_Spell_Class : ScriptableObject
{

    public int manaCost;
    public string spellName;
    public string elementType;
    public GameObject spellPrefab;
    public string description;
    public bool targeted;
    public int range;
    public bool targetTiles;
    public GameObject target;



}

public interface Cast_Spell
{
    public void castSpell(List<GameObject> targets, GameObject caster)
    {
        UnityEngine.Debug.Log("Cast Spell");
    }
}
