using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class BurningTileEffect : tileEffectActions
{
    public List<string> burnableTiles;
    public List<string> reactableTiles;

    public override void performEndOfTurnEffect(ClickableTile tile){
        UnityEngine.Debug.Log("Proccing Burning");
        if (tile.GetComponent<ClickableTile>().characterOnTile != null){
            tile.GetComponent<ClickableTile>().characterOnTile.GetComponent<Basic_Character_Class>().takeMagicDamage(2, "Fire");
        }
    }

    /*
    public override void react(List<TileEffect> effectsOnTile, ClickableTile tile, TileEffect thisEffect){
        UnityEngine.Debug.Log("We Overriding with this one");
        if (effectsOnTile != null){
            for (int i = 0; i < effectsOnTile.Count; i++){
                switch (effectsOnTile[i].name){
                    case "Soaked":
                        tile.removeEffectFromTile(effectsOnTile[i]);
                        tile.removeEffectFromTile(thisEffect);
                        break;
                }
            }
        }
        if (tile.effectsOnTile.Contains(thisEffect)){
            switch(tile.gameObject.name){
                case "tileMud":
                    UnityEngine.Debug.Log("Hit deep water");
                    break;
            }
        }
    }
    */

    public override void endOfDurationEffect(ClickableTile tile){
        switch(tile.gameObject.name){
            case "tileGrass":
                tile.map.swapTiles(tile, 1, true);
                break;
        }
    }

    public override void performStepOnEffect(ClickableTile tile){
        tile.characterOnTile.GetComponent<Basic_Character_Class>().takeMagicDamage(2, "Fire");
    }
}
