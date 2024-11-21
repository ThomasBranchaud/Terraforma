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

    public StatusEffectController statusEffectController; //Reference to the statusEffectController script, used to access the list of statusEffects/tileEffects
    public List<GameObject> enemyTeamList; //List of enemies still remaining in the level
    public List<GameObject> playerTeamList; //List of player units still remaining in the level
    public Camera camera; //Reference to the camera
    public int round = 0; //
    public int phase = 0;  //0 for player phase, 1 for enemy effects, 2 for checking the list of remaining enemies, 3 for enemy phase, 4 for player effects, 5 for resetting the round
    public TileMap map; //Reference to the map script
    public bool movingEnemy = false; //Set to true whenever the GameController is moving an enemy, otherwise false
    public int enemyCount = 0; //Amount of enemies still remaining in the level
    public int enemiesToMove = 0; //Amount of enemies that still need to be moved before moving to the next phase
    public bool targeting = false; //True if the player is currently targeting an attack or spell, false if otherwise
    public bool moving = false;
    public GameObject selectedCharacter; //Reference to the selected character, only references the GameObject, to access any variables in the Basic_Character_Class script use the characterScript variable below
    public Basic_Character_Class characterScript; //Reference to the selected character's Basic_Character_Class script, use to access variables like health, attack, etc.
    public List<GameObject> targets = new List<GameObject>(); //A list of currently selected targets for spells/attacks, if the player hasn't selected a target or is not targeting at all, will be null
    private bool endGame = false;
    private List<GameObject> heroCharacters = new List<GameObject>();
    private BuffController BuffController;

    private LayerMask mask;


    public CombatLog comlog;

    public PauseMenu pauseMenuController;
    [SerializeField] private AudioClip[] closeAtkMenu;


    void Start(){
        mask = LayerMask.GetMask("Default") | LayerMask.GetMask("BlockVisibility") | LayerMask.GetMask("UI");
        for (int i = 0; i < playerTeamList.Count; i++){
            if (playerTeamList[i].GetComponent<Hero_Character_Class>()){
                heroCharacters.Add(playerTeamList[i]);
            }
        }
        BuffController = map.gameObject.GetComponent<BuffController>();
    }

    void Update()
    {
        if (!endGame){
        //Removes all null values from the list of remaining enemy and player units
        enemyTeamList.RemoveAll(x => !x);
        playerTeamList.RemoveAll(x => !x);
        heroCharacters.RemoveAll(x => !x);

        if (enemyTeamList.Count == 0){
            gameOver(true);
        }
        else if(heroCharacters.Count == 0){
            gameOver(false);
        }

        //Checks if the selectedCharacter isn't null and if it is targeting, if true sets the GameController's targeting variable to true as well
        if (selectedCharacter != null && characterScript.targeting == true)
        {
            targeting = true;
        }
        if (characterScript != null && characterScript.isMoving){
            moving = true;
        }
        else {
            moving = false;
        }
        if (characterScript != null && characterScript.charSelected == false && selectedCharacter.GetComponent<Enemy_Character_Class>() == null){
            map.updateSelectedCharacter(null);
            map.currentPath = null;
            updateSelectedObject(null);
        }

        //Executes when the player left-clicks
        if (Input.GetMouseButtonDown(0))
        {
            //Creates a RayCast from the mouse's location, checking if it returns a hit
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, layerMask: mask))
            {
                //Checks if the player is currently targeting anything
                if (targeting == false)
                {
                    //Checks if the player has a unit selected
                    if (selectedCharacter == null)
                    {
                        //Executes this code if the player isn't currently targeting and has no unit currently selected
                        //Checks if the object returned from the hit is a Unit on the player's team
                        if (hit.collider.gameObject.GetComponent<Basic_Character_Class>() != null && phase == 0 && hit.collider.gameObject.tag == "PlayerTeam" && hit.collider.gameObject.GetComponent<Basic_Character_Class>().turnEnded == false)
                        {
                            //Changes the selectedCharacter to the new unit, calls the selectedCharacter function inside the character, and checks if the unit has ended its turn
                            updateSelectedObject(hit.collider.gameObject);
                            characterScript.selectCharacter();
                            if (characterScript.turnEnded == false)
                            {
                                //If the unit has not ended its turn, then the map's selected character is also updated with the new character
                                map.updateSelectedCharacter(selectedCharacter);
                            }
                        }
                        //If the object is not on the player team, checks if its on the enemy team
                        else if (hit.collider.gameObject.GetComponent<Basic_Character_Class>() != null && hit.collider.gameObject.tag == "EnemyTeam" && phase == 0)
                        {
                            //Changes the selectedCharacter to the new unit, calls the selectedCharacter function inside the character ---NOTE: SHOULD NEVER UPDATE THE MAP'S SELECTED CHARACTER---
                            //updateSelectedObject(hit.collider.gameObject);
                            //characterScript.selectCharacter();
                        }
                    }
                    //If the player does have a unit selected, it now checks if the returned object is another character
                    else if (selectedCharacter != null && hit.collider.gameObject.GetComponent<Basic_Character_Class>() != null && phase == 0)
                    {
                        //If the returned object was another unit and the player has already selected a unit, the unit becomes de-selected
                        //Calls the deselectCharacter function in the selectedCharacter's script, updates the map's selectedCharacter, sets the map's path to null, sets the GameController's selectedCharacter to null
                        /*
                        characterScript.deselectCharacter();
                        map.updateSelectedCharacter(null);
                        map.currentPath = null;
                        updateSelectedObject(null);
                        */
                    } 
                    //If the player has a unit selected and the returned object was either on the enemy team or had their turn ended ---NOTE: THIS COULD PROBABLY BE INCLUDED IN THE ABOVE IF ELSE STATEMENT, NEED TO REVIEW---
                    else if (selectedCharacter != null && (selectedCharacter.gameObject.tag == "EnemyTeam" || characterScript.turnEnded == true))
                    {
                        //Runs deselection
                        //Calls the deselectCharacter function in the selectedCharacter's script, updates the map's selectedCharacter, sets the map's path to null, sets the GameController's selectedCharacter to null
                        /*
                        characterScript.deselectCharacter();
                        map.updateSelectedCharacter(null);
                        map.currentPath = null;
                        updateSelectedObject(null);
                        */
                    }
                    else if (selectedCharacter != null && hit.collider.gameObject.GetComponent<Basic_Character_Class>() == null && moving == false && hit.collider.gameObject.tag != "EnemyTeam" && hit.collider.gameObject.tag != "PlayerTeam" && phase == 0)
                    {
                        /*
                        //when you click on a tile after clicking on a character (and you're not moving), it clicks off the character
                        if(selectedCharacter.GetComponent<Basic_Character_Class>().atkMenu.GetComponent<Billboard>().uiHover == false){
                            moving = false;
                            characterScript.isMoving = false;
                            characterScript.deselectCharacter();
                            map.updateSelectedCharacter(null);
                            map.currentPath = null;
                            updateSelectedObject(null);
                        }
                        //selectedCharacter.gameObject.GetComponent<Basic_Character_Class>().atkMenu.GetComponent<Billboard>().uiHover
                        */

                    }
                    //Checks if the returned object was a clickable tile, if so calling the map's MoveSelectedUnitTo function to begin moving the unit there
                    else if(selectedCharacter != null && hit.collider.gameObject.GetComponent<ClickableTile>() != null){
                        map.MoveSelectedUnitTo(hit.collider.gameObject.GetComponent<ClickableTile>().TileX, hit.collider.gameObject.GetComponent<ClickableTile>().TileY);
                    }
                }
                else if (targeting == true && characterScript.attackType == "Spell" && selectedCharacter.GetComponent<Hero_Character_Class>().selectedSpell.targeted == false){
                    List<GameObject> targets = map.targetList;
                    if (characterScript.castSpell(targets))
                        {
                            //Deselects the character as their turn is over
                            updateSelectedObject(null);
                            map.updateSelectedCharacter(null);
                            stopTargeting();
                        }
                }
                //Arrive here if the player is targeting a spell or attack and left-clicked
                //Check if the returned object from the above raycast returned is tagged on the enemy team and is within the spell/attack's reach
                else if (targeting == true && (hit.collider.gameObject.tag == "EnemyTeam" || hit.collider.gameObject.tag == "PlayerTeam" || hit.collider.gameObject.GetComponent<ClickableTile>() != null || hit.collider.gameObject.tag == "Wall") && characterScript.withinReach(hit.collider.gameObject) == true)
                {
                    //Checks if the player is targeting an Attack
                    if (characterScript.attackType == "Attack")
                    {
                        //Tries to perform the attack on the object hit from the raycast by calling the attackCharacter from the selectedCharacter's script, if successful the attack goes through
                        if (characterScript.attackCharacter(hit.collider.gameObject, characterScript.attack.moddedValue))
                        {
                            //Deselects the character as their turn is over
                            updateSelectedObject(null);
                            map.updateSelectedCharacter(null);
                            stopTargeting();
                        }
                    }
                    //Checks if the player is targeting a Spell
                    else if (characterScript.attackType == "Spell")
                    {
                        //Checks if the amount of targets selected is less than the amount of targets the spell can have
                        if (targets == null || selectedCharacter.GetComponent<Hero_Character_Class>().selectedSpell.amountOfTargets > targets.Count){
                            //If the list of targets is null, creates a new list that can be added to
                            if (targets == null){
                                targets = new List<GameObject>();
                            }
                            //Checks if the selected target can be added to the list
                            if (!targets.Contains(hit.collider.gameObject) || selectedCharacter.GetComponent<Hero_Character_Class>().selectedSpell.requireDifferentTargets == false){
                                targets.Add(hit.collider.gameObject);
                            }
                            //Checks if the required amount of targets has been selected
                            if (selectedCharacter.GetComponent<Hero_Character_Class>().selectedSpell.amountOfTargets == targets.Count){
                                //Tries to cast the spell by calling the castSpell function within the selected character's script
                                if (characterScript.castSpell(targets))
                                {
                                    //Deselects the character as their turn is over
                                    updateSelectedObject(null);
                                    map.updateSelectedCharacter(null);
                                    stopTargeting();
                                }
                            }
                        }

                    }
                }
                //Code arrives here if the player was targeting but didn't selected a valid target
                else if (targeting == true)
                {
                    //Causes the player to stop targeting the attack/spell
                    stopTargeting();
                }
            }
        }
        //Next three if statements are for deselection and cancelling of targeting for the current character
        //Executes if the player rightclicks while they have a character selected
        if (Input.GetMouseButtonDown(1) && selectedCharacter != null && phase == 0  && map.moving == false)
        {
            //Runs all relevent deselection functions in the map and character script
            characterScript.deselectCharacter();
            map.updateSelectedCharacter(null);
            map.currentPath = null;
            updateSelectedObject(null);
            stopTargeting();
        }
        //Executes if the player presses the escape key while they are targeting a spell/attack
        if (Input.GetKeyDown(KeyCode.Escape) && selectedCharacter != null && phase == 0 && targeting == true && map.moving == false)
        {
            //Stops the targeting but leaves the character selected
            stopTargeting();
            SFXController.instance.PlayRandomSFXClip(closeAtkMenu, transform, 1f);
        }
        //Executes if the player presses the escape key when they are not targeting a spell/attack
        else if (Input.GetKeyDown(KeyCode.Escape) && selectedCharacter != null && phase == 0  && map.moving == false)
        {
            //Runs all relevent deselection functions in the map and character script
            stopTargeting();
            characterScript.deselectCharacter();
            map.updateSelectedCharacter(null);
            map.currentPath = null;
            updateSelectedObject(null);
            SFXController.instance.PlayRandomSFXClip(closeAtkMenu, transform, 1f);
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && selectedCharacter == null) {
            if(pauseMenuController.GameIsPaused) {
                pauseMenuController.Resume();
            }
            else {
                pauseMenuController.Pause();
            }
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && selectedCharacter != null) {
            if(pauseMenuController.GameIsPaused) {
                pauseMenuController.Resume();
            }
            else {
                pauseMenuController.Pause();
            }
        }

        //Controls the game state and switching between the player/enemy turns
        /*
        Phase 0 --- Player Turn, if the player presses enter their turn ends and the current character is deselected
        Phase 1 --- Enemy Effect Phase, triggers all end of turn effects on the player units and advances their durations, triggers all start of turn effects on the enemy units
        Phase 2 --- Set Enemy Count, sets the count of enemies remaining in play and the amount that need to be moved
        Phase 3 --- Enemy Turn, iterates through the list of enemies, calling the takeTurn function in them
        Phase 4 --- Player Effect Phase, triggers all end of turn effects on the enemy units and advances their durations, triggers all start of turn effects on the player units
        Phase 5 --- Reset Phase, Resets the phase to 0 and starts a new round
        */
        map.setPhase(phase);
        switch (phase)
        {
            //Player's turn
            case 0:
                enemyCount = enemyTeamList.Count;
                //If the player presses the enter key on their turn, it ends their turn
                if (Input.GetKeyDown(KeyCode.Return) || checkEndOfturn())
                {
                    //End Turn Stuff
                    //UnityEngine.Debug.Log("Switching to Phase 1");
                    phase++;
                    //Checks if the player had a unit selected when ending their turn, executes if so
                    if (selectedCharacter != null)
                    {
                        //Calls the deselect character function inside the selected unit
                        characterScript.deselectCharacter();
                    }
                    //Deselects the current character
                    stopTargeting();
                    map.updateSelectedCharacter(null);
                    updateSelectedObject(null);
                    //Ends all of the player's units' turns
                    endAllPlayerTurns();
                }
                break;
            //Triggers all of the end of turn effects on the player units and advances their durations, then triggers all of the start of turn effects on enemy units, then cleans the lists of enemy and player units remaining
            case 1:
                playerTeamEndOfTurnTileEffects();
                statusEffectController.enemyTeamEffectsAdvance();
                enemyTeamList.RemoveAll(x => !x);
                playerTeamList.RemoveAll(x => !x);
                BuffController.enemyTeamBuffsAdvance();
                //UnityEngine.Debug.Log("Switching to Phase 2");
                enemyTeamStartOfTurnTileEffects();
                enemyTeamList.RemoveAll(x => !x);
                playerTeamList.RemoveAll(x => !x);
                phase++;
                comlog.addText("");
                comlog.addText("");
                comlog.addText("** Enemy Turn Start **");
                break;
            //Sets the count of enemies remaining and how many enemies need to be moved before starting the enemy turn
            case 2:
                enemyCount = enemyTeamList.Count;
                enemiesToMove = enemyCount;
                phase++;
                break;
            //Begins to move the enemies
            case 3:
                //Checks if there are still enemies that need to be moved
                if (enemiesToMove > 0)
                {
                    //Checks if an enemy is already being moved
                    if (movingEnemy == false)
                    {
                        //Sets the selected character to the next enemy in the list of enemies
                        map.updateSelectedCharacter(enemyTeamList[enemiesToMove - 1]);
                        //Calls the takeTurn function within the Enemy Class for the enemy that needs to be moved
                        enemyTeamList[enemiesToMove - 1].GetComponent<Enemy_Character_Class>().takeTurn();
                        //Sets moving enemy to true
                        movingEnemy = true;
                    }
                    //If the GameController's movingEnemy variable is true, but the map's movingEnemy variable is false, then a new enemy needs to be selected to move
                    else if (map.movingEnemy == false && movingEnemy == true)
                    {
                        //Sets moving enemy to false and counts down the enemies to move
                        movingEnemy = false;
                        enemiesToMove--;
                    }
                }
                //Upon reaching 0 enemies to move, end the phase
                else
                {
                    phase++;
                }
                break;
            //Triggers all the end of turn effects on the enemy characters and advances their durations, then triggers all start of turn effects on the player characters
            case 4:
                enemyTeamEndOfTurnTileEffects();
                statusEffectController.playerTeamEffectsAdvance();
                enemyTeamList.RemoveAll(x => !x);
                playerTeamList.RemoveAll(x => !x);
                BuffController.playerTeamBuffsAdvance();
                //Sets the map's selected character to null
                map.updateSelectedCharacter(null);
                playerTeamStartOfTurnTileEffects();
                phase++;
                break;
            //Resets the phase to 0 and starts a new round
            case 5:
                //UnityEngine.Debug.Log("End of round " + round + ". Switching to phase 0");
                round++;
                comlog.addText("");
                comlog.addText("");
                comlog.addText("Turn " + (round + 1).ToString() + " Start");
                comlog.addText("** Player Phase Start **");
                phase = 0;
                resetPlayerTeamTurns();
                
                //Resets the abilities for characters to move
                for (int i = 0; i < playerTeamList.Count; i++) {
                    playerTeamList[i].GetComponent<Basic_Character_Class>().hasWalked = false;
                }

                for (int i = 0; i < enemyTeamList.Count; i++) {
                    enemyTeamList[i].GetComponent<Basic_Character_Class>().hasWalked = false;
                }

                break;
        }
        }
    }

    //Can be called with unit to set the selectedCharacter variables and characterScript variables.
    //Also accepts null as an argument, setting both variables to null
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

    //Iterates through the list of player units and sets them to be able to take their turn again by calling the Basic Class resetTurn function
    private void resetPlayerTeamTurns()
    {
        for (int i = 0; i < playerTeamList.Count; i++)
        {
            playerTeamList[i].GetComponent<Basic_Character_Class>().resetTurn();
        }

        for (int i = 0; i < enemyTeamList.Count; i++)
        {
            enemyTeamList[i].GetComponent<Basic_Character_Class>().resetTurn();
        }
    }

    //Iterates through the list of player units and ends their turns by calling the Basic Class endTurn function
    private void endAllPlayerTurns()
    {
        for (int i = 0; i < playerTeamList.Count; i++)
        {
            playerTeamList[i].GetComponent<Basic_Character_Class>().endTurn();
        }
    }

    //Stops the current character from targeting
    private void stopTargeting(){
        targeting = false;
        targets = null;
        if (selectedCharacter != null){
            characterScript.stopTargeting();
        }
    }


    /*
    playerTeamStartOfTurnTileEffects, playerTeamEndOfTurnTileEffects, enemyTeamStartOfTurnTileEffects, and enemyTeamEndOfTurnTileEffects all function in the same way
    1. Iterate over the list of enemy or player units depending on the function called, list of enemies for the enemy function and vice versa
    2. Check if the tile the unit is standing on has a tile effect currently acting on it
    3. Call the appropriate function on the tile effect based on the function called
        -performStartOfTurnEffect -> Start of turn effects
        -performEndOfTurnEffect -> End of turn effects
    */
    private void playerTeamStartOfTurnTileEffects(){
        for (int i = 0; i < playerTeamList.Count; i++){
            if (playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile != null && playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count != 0){
                for (int j = 0; j < playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count; j++){
                    playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile[j].tileEffectPrefab.GetComponent<tileEffectActions>().performStartOfTurnEffect(playerTeamList[i].GetComponent<Basic_Character_Class>().tile);
                }
            }
        }
    }

    private void playerTeamEndOfTurnTileEffects(){
        for (int i = 0; i < playerTeamList.Count; i++){
            if (playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile != null && playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count != 0){
                for (int j = 0; j < playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count; j++){
                    playerTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile[j].tileEffectPrefab.GetComponent<tileEffectActions>().performEndOfTurnEffect(playerTeamList[i].GetComponent<Basic_Character_Class>().tile);
                }
            }
        }
    }

    private void enemyTeamStartOfTurnTileEffects(){
        for (int i = 0; i < enemyTeamList.Count; i++){
            if (enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile != null && enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count != 0){
                for (int j = 0; j < enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count; j++){
                    enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile[j].tileEffectPrefab.GetComponent<tileEffectActions>().performStartOfTurnEffect(enemyTeamList[i].GetComponent<Basic_Character_Class>().tile);
                }
            }
        }
    }

    private void enemyTeamEndOfTurnTileEffects(){
        for (int i = 0; i < enemyTeamList.Count; i++){
            if (enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile != null && enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count != 0){
                for (int j = 0; j < enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile.Count; j++){
                    enemyTeamList[i].GetComponent<Basic_Character_Class>().tile.effectsOnTile[j].tileEffectPrefab.GetComponent<tileEffectActions>().performEndOfTurnEffect(enemyTeamList[i].GetComponent<Basic_Character_Class>().tile);
                }
            }
        }
    }
    
    private bool checkEndOfturn(){
        for (int i = 0; i < playerTeamList.Count; i++){
            if (playerTeamList[i].GetComponent<Basic_Character_Class>().turnEnded == false){
                return false;
            }
        }
        return true;
    }



    public void gameOver(bool playerVictory){
        if (playerVictory){
            UnityEngine.Debug.Log("The Player has won the game");
        }
        else{
            UnityEngine.Debug.Log("The Enemy has defeated all the player characters");
        }
        endGame = true;
    }
}
