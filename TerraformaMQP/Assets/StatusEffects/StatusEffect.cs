using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : ScriptableObject
{
    public int duration;
    public List<string> statToEffect;
    public string source;
    public List<int> amount;
    public GameObject selectedObject;
    public string name;
    public bool playerTeam; 

    public void initializeStatusEffect(int newDuration, List<string> newStatToEffect, string newSource, List<int> newAmount, GameObject newSelectedObject, string newName, bool newPlayerTeam, GameObject statusEffectController)
    {
        duration = newDuration;
        statToEffect = newStatToEffect;
        source = newSource;
        amount = newAmount;
        selectedObject = newSelectedObject;
        name = newName;
        playerTeam = newPlayerTeam;
        selectedObject.GetComponent<Basic_Character_Class>().addStatus(this, false);
        if (playerTeam == true)
        {
            statusEffectController.GetComponent<StatusEffectController>().playerTeamEffects.Add(this);
        }
        else
        {
            statusEffectController.GetComponent<StatusEffectController>().enemyTeamEffects.Add(this);
        }

    }

    public void initializeTileEffect(List<string> newStatToEffect, string newSource, List<int> newAmount, GameObject newSelectedObject, string newName)
    {
        statToEffect = newStatToEffect;
        source = newSource;
        amount = newAmount;
        selectedObject = newSelectedObject;
        name = newName;
        selectedObject.GetComponent<Basic_Character_Class>().addStatus(this, true);
    }

    public bool reduceDuration()
    {
            if (duration > 1)
            {
                duration--;
                return true;
            }
            else
            {
                selectedObject.GetComponent<Basic_Character_Class>().removeStatus(this, false);
                return false;
            }
    }

}
