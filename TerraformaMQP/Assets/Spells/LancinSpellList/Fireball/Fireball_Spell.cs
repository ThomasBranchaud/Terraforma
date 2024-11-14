using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

[SerializeField]
public class Fireball_Spell : MonoBehaviour, Cast_Spell
{
    public int spread;
    public GameObject tileFireGrass;

    public void castSpell(List<GameObject> targets, GameObject caster)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            ClickableTile previousTile = targets[i].GetComponent<Basic_Character_Class>().tile;
            GameObject newTile = (GameObject)Instantiate(tileFireGrass, new Vector3(previousTile.gameObject.transform.position.x, 0, previousTile.gameObject.transform.position.z), Quaternion.identity);
            previousTile.map.swapTiles(previousTile, newTile.GetComponent<ClickableTile>(), 18);
            targets[i].GetComponent<Basic_Character_Class>().takeMagicDamage(caster.GetComponent<Hero_Character_Class>().magic.moddedValue, "Fire");

        }
    }
}
