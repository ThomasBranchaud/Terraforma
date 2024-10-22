using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    //Relevant to lighting up the tile held over
    private Renderer tileRenderer;
    private Color originalColor;
    public float highlightMultiplier = 1.2f; 

    public int TileX;
    public int TileY;
    public TileMap map;
    public bool isWalkable = true;
    //Determines how the tile affects characters that stand on it
    public List<string> statsToEffect;
    public List<int> effectAmounts;
    public GameObject characterOnTile = null;
    public Color color;

    public List<TileEffect> effectsOnTile;
    
    void Start() {
        tileRenderer = GetComponent<Renderer>();
        originalColor = tileRenderer.sharedMaterial.color;
        color = GetComponent<Renderer>().material.color;
    }




    void OnMouseEnter() {

        Color highlightColor;
        if (map.selectedUnit != null && map.selectedUnitScript.targeting == true && map.targetList.Contains(this))
        {
            float darkerHighlight = highlightMultiplier + 0.1f;
            highlightColor = originalColor * darkerHighlight;
            tileRenderer.material.color = highlightColor;
        }
        else
        {
            highlightColor = originalColor * highlightMultiplier;
            tileRenderer.material.color = highlightColor;
        }

        //Highlight Path
        if (map.selectedUnit != null && map.selectedUnit.gameObject.tag != "EnemyTeam" && map.selectedUnitScript.turnEnded == false && map.selectedUnitScript.targeting == false && map.moving == false && ((map.selectedUnit.GetComponent<Hero_Character_Class>() != null && map.selectedUnit.GetComponent<Hero_Character_Class>().pickingSpell == false) || map.selectedUnit.GetComponent<Hero_Character_Class>() == null)) {
            map.visualPathTo(TileX,TileY);
            //UnityEngine.Debug.Log(currentPath[0].x)
        }
    }

    public void highlight()
    {
        Color highlightColor = originalColor * highlightMultiplier;
        tileRenderer.material.color = highlightColor;
    }

    public void endHighlight()
    {

        GetComponent<Renderer>().material.color = color;
    }

    void OnMouseExit() {
        if (map.selectedUnit != null && map.selectedUnitScript.targeting == true && map.targetList.Contains(this))
        {
            Color highlightColor = originalColor * highlightMultiplier;
            tileRenderer.material.color = highlightColor;
            return;
        }
        tileRenderer.material.color = originalColor;
    }

    public void addEffectToTile(TileEffect effect){
        effectsOnTile.Add(effect);
        for (int i = 0; i < effect.statToEffect.Count; i++){
            if (statsToEffect.Contains(effect.statToEffect[i])){
                int statLoc = checkList(effect.statToEffect[i]);
                effectAmounts[statLoc] += effect.amountToEffect[i];
            }
            else{
                statsToEffect.Add(effect.statToEffect[i]);
                effectAmounts.Add(effect.amountToEffect[i]);
            }
        }
        if (characterOnTile!= null){
            updateTileEffect();
        }
    }

    public void removeEffectFromTile(TileEffect effect){
        effectsOnTile.Remove(effect);
        for (int i = 0; i < effect.statToEffect.Count; i++){
            int statLoc = checkList(effect.statToEffect[i]);
            effectAmounts[statLoc] -= effect.amountToEffect[i];
            if (effectAmounts[statLoc] == 0){
                effectAmounts.Remove(effectAmounts[statLoc]);
                statsToEffect.Remove(statsToEffect[statLoc]);
            }
        }
        if (characterOnTile!= null){
            updateTileEffect();
        }
    }

    public void updateTileEffect(){
        StatusEffect newEffect = new StatusEffect();
        newEffect.initializeTileEffect(statsToEffect, name, effectAmounts, characterOnTile, name + " Effect");
    }

    public int checkList(string statToEffect){
        for (int i = 0; i < statsToEffect.Count; i++){
            if (statsToEffect[i] == statToEffect){
                return i;
            }
        }
        return -99;
    }

}
