using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{

    public StatusEffectController statusEffectController;
    public List<GameObject> enemyTeamList;
    public List<GameObject> playerTeamList;
    public List<GameObject> playerTeamTileEffects;
    public List<GameObject> enemyTeamTileEffects;
    public Camera camera;
    public int round = 0;
    public int phase = 0;  //0 for player phase, 1 for enemy effects, 2 for enemy phase, 3 for player effects
    public TileMap map;
    public bool movingEnemy = false;
    private int enemyCount = 0;
    public bool lockPlayer = false;
    public bool targeting = false;
    public GameObject selectedCharacter;
    public Basic_Character_Class characterScript;

    void Update()
    {

        enemyTeamList.RemoveAll(x => !x);
        playerTeamList.RemoveAll(x => !x);


        if (selectedCharacter != null)
        {
            if (characterScript.targeting == true)
            {
                targeting = true;
            }
        }
        /*
        if (map.selectedUnit == null)
        {
            updateSelectedObject(null);
        }
        */
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (targeting == false)
                {
                    if (selectedCharacter == null)
                    {
                        if (hit.collider.gameObject.GetComponent<Basic_Character_Class>() != null && phase == 0 && hit.collider.gameObject.tag == "PlayerTeam")
                        {
                            updateSelectedObject(hit.collider.gameObject);
                            characterScript.selectCharacter();
                            if (characterScript.turnEnded == false)
                            {
                                map.updateSelectedCharacter(selectedCharacter);
                            }
                        }
                        else if (hit.collider.gameObject.GetComponent<Basic_Character_Class>() != null && hit.collider.gameObject.tag == "EnemyTeam" && phase == 0)
                        {
                            updateSelectedObject(hit.collider.gameObject);
                            characterScript.selectCharacter();
                        }
                    }
                    else if (selectedCharacter != null && hit.collider.gameObject.GetComponent<Basic_Character_Class>() != null && phase == 0)
                    {
                        characterScript.deselectCharacter();
                        map.updateSelectedCharacter(null);
                        map.currentPath = null;
                        updateSelectedObject(null);
                    } 
                    else if (selectedCharacter != null && (selectedCharacter.gameObject.tag == "EnemyTeam" || characterScript.turnEnded == true))
                    {
                        characterScript.deselectCharacter();
                        map.updateSelectedCharacter(null);
                        map.currentPath = null;
                        updateSelectedObject(null);
                    }
                }
                else if (targeting == true && hit.collider.gameObject.tag == "EnemyTeam" && characterScript.withinReach(hit.collider.gameObject) == true)
                {
                    if (characterScript.attackType == "Attack")
                    {
                        if (characterScript.attackCharacter(hit.collider.gameObject, characterScript.attack.moddedValue))
                        {
                            updateSelectedObject(null);
                            map.updateSelectedCharacter(null);
                        }
                    }
                    else if (characterScript.attackType == "Spell")
                    {
                        List<GameObject> targets = new List<GameObject>();
                        targets.Add(hit.collider.gameObject);
                        if (characterScript.castSpell(targets))
                        {
                            updateSelectedObject(null);
                            map.updateSelectedCharacter(null);
                        }
                    }
                    targeting = false;

                }
                else if (targeting == true)
                {
                    targeting = false;
                    characterScript.stopTargeting();
                }
            }
        }
        if (Input.GetMouseButtonDown(1) && selectedCharacter != null && phase == 0)
        {
            characterScript.deselectCharacter();
            map.updateSelectedCharacter(null);
            map.currentPath = null;
            updateSelectedObject(null);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && selectedCharacter != null && phase == 0 && targeting == true)
        {
            characterScript.stopTargeting();
            targeting = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && selectedCharacter != null && phase == 0)
        {
            characterScript.deselectCharacter();
            map.updateSelectedCharacter(null);
            map.currentPath = null;
            updateSelectedObject(null);
        }


        switch (phase)
        {
            case 0:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    //End Turn Stuff
                    UnityEngine.Debug.Log("Switching to Phase 1");
                    phase++;
                    if (selectedCharacter != null)
                    {
                        UnityEngine.Debug.Log("Made it here");
                        characterScript.deselectCharacter();
                        characterScript.stopTargeting();
                    }
                    targeting = false;
                    map.updateSelectedCharacter(null);
                    updateSelectedObject(null);
                    endAllPlayerTurns();
                }
                break;

            case 1:
                statusEffectController.playerTeamEffectsAdvance();
                UnityEngine.Debug.Log("Switching to Phase 2");
                phase++;
                enemyCount = enemyTeamList.Count;
                break;

            case 2:
                if (enemyCount > 0)
                {
                    if (movingEnemy == false)
                    {
                        map.updateSelectedCharacter(enemyTeamList[enemyCount - 1]);
                        enemyTeamList[enemyCount - 1].GetComponent<Enemy_Character_Class>().takeTurn();
                        movingEnemy = true;
                    }
                    else if (map.movingEnemy == false && movingEnemy == true)
                    {
                        movingEnemy = false;
                        enemyCount--;
                    }
                }
                else
                {
                    phase++;
                }
                break;

            case 3:
                UnityEngine.Debug.Log("End of round " + round + ". Switching to phase 0");
                statusEffectController.enemyTeamEffectsAdvance();
                round++;
                phase = 0;
                map.updateSelectedCharacter(null);
                resetPlayerTeamTurns();
                break;
        }
    }

    private void updateSelectedObject(GameObject newObject)
    {
        if (newObject != null)
        {
            selectedCharacter = newObject;
            characterScript = newObject.GetComponent<Basic_Character_Class>();
        }
        else
        {
            selectedCharacter = null;
            characterScript = null;
        }

    }

    private void resetPlayerTeamTurns()
    {
        for (int i = 0; i < playerTeamList.Count; i++)
        {
            playerTeamList[i].GetComponent<Basic_Character_Class>().resetTurn();
        }
    }

    private void endAllPlayerTurns()
    {
        for (int i = 0; i < playerTeamList.Count; i++)
        {
            playerTeamList[i].GetComponent<Basic_Character_Class>().endTurn();
        }
    }

}
