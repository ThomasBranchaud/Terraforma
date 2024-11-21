using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportFlames_Spell : MonoBehaviour, Cast_Spell
{

    public BuffClass SupportFlamesBuff;

    public void castSpell(List<GameObject> targets, GameObject caster){
        for (int i = 0; i < targets.Count; i++){
            if (targets[i].GetComponent<ClickableTile>().characterOnTile != null){
                GameObject characterOnTile = targets[i].GetComponent<ClickableTile>().characterOnTile;
                if (characterOnTile.GetComponent<Basic_Character_Class>().buffs.Count > 0){
                    for (int j = 0; j < characterOnTile.GetComponent<Basic_Character_Class>().buffs.Count; j++){
                        if (characterOnTile.GetComponent<Basic_Character_Class>().buffs[j].name == "Support Flames"){
                            characterOnTile.GetComponent<Basic_Character_Class>().buffs[j].duration += 1;
                            return;
                        }
                    }
                }
                BuffClass newBuff = Instantiate(SupportFlamesBuff);
                newBuff.createBuff(true, characterOnTile.GetComponent<Basic_Character_Class>());
            }
        }
    }

}
