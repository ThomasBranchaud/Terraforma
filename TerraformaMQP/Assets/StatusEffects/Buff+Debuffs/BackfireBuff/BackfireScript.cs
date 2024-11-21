using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackfireScript : BuffActions
{
    public override void endOfDurationEffect(Basic_Character_Class character){
        UnityEngine.Debug.Log("Backfire has ended dealing 2 fire damage");
        character.takeMagicDamage(4, "Fire");
    }
}
