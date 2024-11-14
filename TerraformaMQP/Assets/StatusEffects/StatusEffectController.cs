using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    [SerializeField]
    public List<StatusEffect> playerTeamEffects;
    public List<StatusEffect> enemyTeamEffects;

    public void playerTeamEffectsAdvance(){
        if (playerTeamEffects != null){
            List<StatusEffect> effectsToRemove = new List<StatusEffect>();
            for (int i = 0; i < playerTeamEffects.Count; i++){
                if (!playerTeamEffects[i].reduceDuration())
                {
                    effectsToRemove.Add(playerTeamEffects[i]);
                }
            }
            for (int i = 0; i < effectsToRemove.Count; i++)
            {
                playerTeamEffects.Remove(effectsToRemove[i]);
            }
        }
    }

    public void enemyTeamEffectsAdvance() { 
        if (enemyTeamEffects != null)
        {
            List<StatusEffect> effectsToRemove = new List<StatusEffect>();
            for (int i = 0; i < enemyTeamEffects.Count; i++)
            {
                if (!enemyTeamEffects[i].reduceDuration())
                {
                    effectsToRemove.Add(playerTeamEffects[i]);
                }
            }
            for (int i = 0; i < effectsToRemove.Count; i++)
            {
                enemyTeamEffects.Remove(effectsToRemove[i]);
            }
        }
    }
}
