using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMap : MonoBehaviour
{

    public GameObject selectedUnit;
    public Basic_Character_Class selectedUnitScript;
    public Tilemap tilemap;

    public TileType[] tileTypes;
    public int[,] tiles;
    Node[,] graph;
    public ClickableTile[,] clickableTiles;

    public bool movingEnemy = false;
    public bool moving = false;

    public List<ClickableTile> targetList = null;

    public bool mapCreated = false;

    int mapSizeX = 10;
    int mapSizeY = 10;

    float xOffset;
    float yOffset;

    Dictionary<string, int> tileNames = new Dictionary<string, int>(){
        {"tileGrass", 0},
        {"tileDirt", 1},
        {"tileMud", 2},
        {"tileIce", 3},
        {"tileStone", 4},
        {"tileWoodPlank", 5},
        {"tileDenseForest", 6},
        {"tileLightForest", 7},
        {"tileShallowWater", 8},
        {"tileDeepWater", 9},
        {"tileSand", 10},
        {"tileGlass", 11},
        {"tileMetal", 12},
        {"tileAshen", 13},
        {"tileMountain", 14},
        {"tileHill", 15},
        {"tileWall", 16},
        {"tileWhiteVoid", 17}
    };

    //Nodes along the path of shortest path
    public List<Node> currentPath = null;
    public List<Node> visualPath = null;
    public GameObject circleArrowPrefab;

    //Sets color to tiles

    void Start() {
        createMap();
        //GenerateMapVisual();
    }

    public void createMap(){
        UnityEngine.Debug.Log("Map Created");
        GenerateMapData();
        GenerateGraph();
        mapCreated = true;
    }

    void Update() {
        float speed = 2;
        float step = speed * Time.deltaTime;

        if (selectedUnit != null && selectedUnitScript.targeting == true)
        {
            hidePath();
        }

        if (currentPath != null){
            if (currentPath.Count > 0)
            {
                if (selectedUnit != null)
                {
                    if (selectedUnit.GetComponent<Enemy_Character_Class>())
                    {
                        movingEnemy = true;
                    }
                    moving = true;
                    int x = currentPath[0].x;
                    int y = currentPath[0].y;
                    Vector3 nextPos = TileCoordToWorldCoord(x+(int)xOffset, y+(int)yOffset);
                    if (nextPos != selectedUnit.transform.position)
                    {
                        selectedUnit.transform.position = Vector3.MoveTowards(selectedUnit.transform.position, nextPos, step);
                    }
                    else
                    {

                        clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY].characterOnTile = null;
                        //Makes the tile passable again when the unit moves off it
                        clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY].isWalkable = true;

                        selectedUnitScript.tileX = x;
                        selectedUnitScript.tileY = y;
                        //Used to apply buff/debuff to the player based on tile type stepped on
                        selectedUnitScript.tileType = tileTypes[tiles[x, y]];
                        selectedUnitScript.tile = clickableTiles[x, y];
                        clickableTiles[x, y].characterOnTile = selectedUnit;

                        //Makes the tile impassable when a character stands on it
                        clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY].isWalkable = false;

                        addTileEffect(x, y, selectedUnit);
                        currentPath.RemoveAt(0);
                    }
                }
            }
            else if (movingEnemy == true)
            {
                movingEnemy = false;
                moving = false;
                currentPath = null;
            }
            else
            {
                moving = false;
                currentPath = null;
            }
        }

        // FOR TESTING PURPOSES ----- CAN BE UNCOMMENTED TO APPLY A DEBUFF TO THE SELECTED CHARACTER BY PRESSING B OR N, PRESSING M WILL ADVANCE EVERY EFFECT IN THE PLAYER TEAM EFFECTS LIST
        /*
        if (Input.GetKeyDown(KeyCode.B))
        {
            StatusEffect newEffect = new StatusEffect();
            List<string> stats = new List<string>();
            stats.Add("attack");
            stats.Add("speed");
            stats.Add("maxHealth");
            stats.Add("movementSpeed");
            stats.Add("resistence");
            stats.Add("defense");
            stats.Add("criticalChance");
            stats.Add("accuracy");
            stats.Add("totalActions");
            List<int> amounts = new List<int>();
            amounts.Add(-10);
            amounts.Add(-10);
            amounts.Add(-10);
            amounts.Add(-10);
            amounts.Add(-10);
            amounts.Add(-10);
            amounts.Add(-10);
            amounts.Add(-10);
            amounts.Add(-5);
            newEffect.initializeStatusEffect(2, stats, "Cripple", amounts, selectedUnit, "Cripple", true, this.gameObject);
        }
        */
        if (Input.GetKeyDown(KeyCode.N))
        {
            StatusEffect newEffect = new StatusEffect();
            List<string> stats = new List<string>();
            stats.Add("speed");
            List<int> amounts = new List<int>();
            amounts.Add(-5);
            newEffect.initializeStatusEffect(5, stats, "Slow", amounts, selectedUnit, "Slow", true, this.gameObject);
        }
        /*
        if (Input.GetKeyDown(KeyCode.M))
        {
            this.gameObject.GetComponent<StatusEffectController>().playerTeamEffectsAdvance();
        }
        */
        

        

    }


    void GenerateMapDataManual() {
        //allocate map tiles
        tiles = new int[mapSizeX, mapSizeY];

        //initialize map tiles as grass
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                tiles[x,y] = 0;
            }
        }

        //map for testing
        tiles[4,4] = 2;
        tiles[5,4] = 2;
        tiles[6,2] = 2;
        tiles[7,4] = 2;
        tiles[8,4] = 2;
        tiles[7,5] = 2;
        tiles[8,5] = 2;

        tiles[4,3] = 1;
        tiles[5,3] = 1;
        tiles[3,4] = 1;
        
    }

    void GenerateMapData() {
        UnityEngine.Debug.Log(tilemap.GetUsedTilesCount());

        Transform[] allObj = tilemap.GetComponentsInChildren<Transform>();

        float[] bounds = new float[4];
        bounds[0] = allObj[0].position.x;
        bounds[1] = allObj[0].position.z;
        bounds[2] = allObj[0].position.x;
        bounds[3] = allObj[0].position.z;

        UnityEngine.Debug.Log(bounds[3]);
        foreach (Transform tile in allObj) {
            if (tileNames.ContainsKey(tile.name)) {
                if (tile.position.x < bounds[0]) {
                    bounds[0] = tile.position.x;
                }
                if (tile.position.z < bounds[1]) {
                    bounds[1] = tile.position.z;
                }
                if (tile.position.x > bounds[2]) {
                    bounds[2] = tile.position.x;
                }
                if (tile.position.z > bounds[3]) {
                    bounds[3] = tile.position.z;
                }
            }
        }
        UnityEngine.Debug.Log(bounds[0] + "," + bounds[1] + "," + bounds[2] + "," + bounds[3]);

        xOffset = bounds[0];
        yOffset = bounds[1];

        mapSizeX = (int) (bounds[2] - bounds[0] + 1);
        mapSizeY = (int) (bounds[3] - bounds[1] + 1);

        //allocate map tiles
        tiles = new int[mapSizeX, mapSizeY];
        clickableTiles = new ClickableTile[mapSizeX, mapSizeY];

        //initialize map tiles as void
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                tiles[x,y] = 17;
                clickableTiles[x, y] = null;
            }
        }

        foreach (Transform tile in allObj) {
            if (tileNames.ContainsKey(tile.name)) {
                //UnityEngine.Debug.Log((int)(tile.position.x-xOffset) + "," + (int)(tile.position.z-yOffset));
                int x = (int)(tile.position.x-xOffset);
                int y = (int)(tile.position.z-yOffset);
                tiles[x, y] = tileNames[tile.name];

                ClickableTile ct = tile.gameObject.GetComponent<ClickableTile>();
                ct.TileX = x;//(int)tile.position.x;
                ct.TileY = y;//(int)tile.position.z;
                ct.map = this;
                ct.isWalkable = tileTypes[tileNames[tile.name]].isWalkable;
                clickableTiles[x, y] = ct;

                UnityEngine.Debug.Log(clickableTiles[x, y].isWalkable);
            }
        }
        
    }

    void GenerateGraph() {
        graph = new Node[mapSizeX, mapSizeY];

        //initialize graph 
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }

        //add neighbors
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {

                if (x > 0)
                    graph[x,y].neighbors.Add(graph[x-1, y]);
                if (x < mapSizeX-1)
                    graph[x,y].neighbors.Add(graph[x+1, y]);
                if (y > 0)
                    graph[x,y].neighbors.Add(graph[x, y-1]);
                if (y < mapSizeY-1)
                    graph[x,y].neighbors.Add(graph[x, y+1]);
            }
        }
    }

    void GenerateMapVisual() {

        clickableTiles = new ClickableTile[mapSizeX, mapSizeY];

        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                TileType tt = tileTypes[tiles[x,y]];
                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x,0,y), Quaternion.identity);

                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.TileX = x;
                ct.TileY = y;
                ct.map = this;
                clickableTiles[x, y] = ct;
            }
        }
    }

    public Vector3 TileCoordToWorldCoord(int x, int y) {
        return new Vector3(x,1,y);
    }

    public void MoveSelectedUnitTo(int x, int y) {

        UnityEngine.Debug.Log(x + "," + y);

        //TEST - replace with actual movement implementation
        if (selectedUnit != null && clickableTiles[x, y].isWalkable)
        {
            if (selectedUnitScript.targeting == true)
            {
                selectedUnitScript.targeting = false;
            }
            else if (selectedUnitScript.charSelected || selectedUnit.GetComponent<Enemy_Character_Class>())
            {
                hidePath();
                generatePathTo(x, y);
                UnityEngine.Debug.Log(currentPath.Count);

                //selectedUnitScript.charSelected = false;

                //selectedUnitScript.tileX = currentPath[1].x;
                //selectedUnitScript.tileY = currentPath[1].y;
                //selectedUnit.transform.position = TileCoordToWorldCoord(currentPath[1].x,currentPath[1].y);
            }
        }

    }

    public void generatePathTo(int x, int y){

        if (selectedUnitScript.tileX == x && selectedUnitScript.tileY == y){
            currentPath = new List<Node>();
            selectedUnitScript.path = currentPath;
            return;
        }

        selectedUnitScript.path = null;
        currentPath = null;

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnitScript.tileX, selectedUnitScript.tileY];
        Node target = graph[x, y];
        dist[source] = 0;
        prev[source] = null;

        //unchecked nodes
        List<Node> unvisited = new List<Node>();

        foreach (Node n in graph){
            //Initialize to infite distance
            if (n != source){
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }

        //if there is a node in unvisited list check it
        while (unvisited.Count > 0){
            //unvisited node with shortest distance
            Node u = null;

            foreach (Node possibleU in unvisited){
                if (u == null || dist[possibleU] < dist[u]){
                    u = possibleU;
                }
            }

            if (u == target){
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbors){

                float alt = dist[u] + costToEnterTile(n.x, n.y);
                if (alt < dist[n]){
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        if (prev[target] == null){
            return;
        }

        currentPath = new List<Node>();
        Node curr = target;

        //step through current path and add it to chain
        while (curr != null){
            currentPath.Add(curr);
            curr = prev[curr];
        }
        
        currentPath.Reverse();

        selectedUnitScript.path = currentPath;

        //showPath();
    }

    public bool unitCanEnterTile(int x, int y) {

        //add section here for checking if space is occupied by other unit
        bool walkable = false;
        if (clickableTiles[x, y] != null) {
            walkable = clickableTiles[x, y].isWalkable;
        }
        return walkable;
    }

    // public void selectedChar() {
    //     if(Input.GetMouseButtonDown(0)) {
    //         selectedUnit.setCharSelected(true);
    //     }
    // }

    
    public float costToEnterTile(int x, int y) {

        if (unitCanEnterTile(x, y) == false) {
            return Mathf.Infinity;
        }
        UnityEngine.Debug.Log(x + ", " + y);
        TileType t = tileTypes[tiles[x, y]];
        float dist = t.cost;

        return dist;
    }

    public void showPath() {
        hidePath();
 
        //Create path of CircleArrows
        for (int i = 0; i < visualPath.Count; i++) {
            GameObject ca = Instantiate(circleArrowPrefab);
            ca.transform.position = new Vector3(visualPath[i].x+(int)xOffset,0.6f,visualPath[i].y+(int)yOffset);
            ca.transform.localRotation = Quaternion.Euler(90f,0,0);
            if (i != visualPath.Count - 1) {
                ca.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            } else {
                ca.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }

    public void hidePath() {
        //Delete all instances of CircleArrow
        GameObject[] existingArrows = GameObject.FindGameObjectsWithTag("CircleArrow");
        foreach (GameObject arrow in existingArrows) {
            Destroy(arrow);
        }
    }
    public void visualPathTo(int x, int y){

        if (circleArrowPrefab == null) {
            UnityEngine.Debug.LogError("circleArrowPrefab has not been assigned in the Inspector!");
            return;
        }

        if (selectedUnitScript.tileX == x && selectedUnitScript.tileY == y){
            visualPath = new List<Node>();
            selectedUnitScript.path = currentPath;
            return;
        }

        selectedUnitScript.path = null;
        visualPath = null;

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnitScript.tileX, selectedUnitScript.tileY];
        Node target = graph[x, y];
        dist[source] = 0;
        prev[source] = null;

        //unchecked nodes
        List<Node> unvisited = new List<Node>();

        foreach (Node n in graph){
            //Initialize to infite distance
            if (n != source){
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }

        //if there is a node in unvisited list check it
        while (unvisited.Count > 0){
            //unvisited node with shortest distance
            Node u = null;

            foreach (Node possibleU in unvisited){
                if (u == null || dist[possibleU] < dist[u]){
                    u = possibleU;
                }
            }

            if (u == target){
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbors){

                float alt = dist[u] + costToEnterTile(n.x, n.y);
                if (alt < dist[n]){
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        if (prev[target] == null){
            return;
        }
        visualPath = new List<Node>();
        Node curr = target;

        //step through current path and add it to chain
        while (curr != null){
            visualPath.Add(curr);
            curr = prev[curr];
        }
        
        visualPath.Reverse();

        selectedUnitScript.path = currentPath;

        showPath();
    }
    //Current placeholder function that searches for nearby characters based on a character's reach (Ex. Reach of 1 will search the tiles immediately next to the character)
    //Needs to be expanded depending on how ranged characters operate

    public void drawReach(int reach, bool targetTiles, bool targetAllies)
    {
        int width = 1;
        for (int i = 2; i <= reach; ++i)
        {
            width += 2;
        }
        int partialWidth = (width - 1) / 2;
        int storeWidth = partialWidth;

        UnityEngine.Debug.Log(width);
        UnityEngine.Debug.Log(partialWidth);
        for (int i = 1; i < reach + 1; i++)
        {
            for (int j = partialWidth; j > ((-1 * partialWidth) - 1); --j)
            {
                if (checkIndex(selectedUnitScript.tileX + i, selectedUnitScript.tileY + j) && clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j] != null)
                {
                    if (targetTiles == true)
                    {
                        clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].highlight();
                        targetList.Add(clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j]);
                    }
                    else if (clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].characterOnTile != null && clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].characterOnTile.gameObject.tag == "EnemyTeam")
                    {
                        clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].highlight();
                        targetList.Add(clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j]);
                    }

                }
            }
            partialWidth--;
        }
        partialWidth = storeWidth;
        for (int i = -1; i > -reach - 1; i--)
        {
            for (int j = partialWidth; j > ((-1 * partialWidth) - 1); --j)
            {
                if (checkIndex(selectedUnitScript.tileX + i, selectedUnitScript.tileY + j) && clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j] != null)
                {
                    if (targetTiles == true)
                    {
                        clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].highlight();
                        targetList.Add(clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j]);
                    }
                    else if (clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].characterOnTile != null && clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].characterOnTile.gameObject.tag == "EnemyTeam")
                    {
                        clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j].highlight();
                        targetList.Add(clickableTiles[selectedUnitScript.tileX + i, selectedUnitScript.tileY + j]);
                    }

                }
            }
            partialWidth--;
        }
        for (int i = 0; i <= reach; i++)
        {
            if (checkIndex(selectedUnitScript.tileX, selectedUnitScript.tileY + i) && clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY + i] != null)
            {
                if (targetTiles == true)
                {
                    clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY + i].highlight();
                    targetList.Add(clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY + i]);
                }
                else if (clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY + i].characterOnTile != null && clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY + i].characterOnTile.gameObject.tag == "EnemyTeam")
                {
                    clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY + i].highlight();
                    targetList.Add(clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY + i]);
                }
            }
        }
        for (int i = 1; i <= reach; i++)
        {
            if (checkIndex(selectedUnitScript.tileX, selectedUnitScript.tileY - i) && clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY - i] != null)
            {
                if (targetTiles == true)
                {
                    clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY - i].highlight();
                    targetList.Add(clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY - i]);
                }
                else
                {
                    if (clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY - i].characterOnTile != null && clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY - i].characterOnTile.gameObject.tag == "EnemyTeam")
                    {
                        clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY - i].highlight();
                        targetList.Add(clickableTiles[selectedUnitScript.tileX, selectedUnitScript.tileY - i]);
                    }
                }

            }
        }

    }

    public bool checkIndex(int x, int y)
    {
        if (x >= 0 && x < clickableTiles.GetLength(0))
        {
            if (y >= 0 && y < clickableTiles.GetLength(1))
            {
                return true;
            }
        }
        return false;
    }

    //Current placeholder to set the tiles back to their original colors

    public void removeReach()
    {

        for (int i = 0; i < targetList.Count; i++)
        {
            targetList[i].endHighlight();
        }
        targetList = new List<ClickableTile>();

    }

    //Use this function when changing the selectedUnit variable
    //Also sets the selectedUnitScript variable making references to it's script much easier
    
    public void updateSelectedCharacter(GameObject newCharacter)
    {
        if (newCharacter != null)
        {
            selectedUnit = newCharacter;
            selectedUnitScript = selectedUnit.GetComponent<Basic_Character_Class>();
        }
        else
        {
            selectedUnit = null;
            selectedUnitScript = null;
        }
    }

    public bool checkForTarget(GameObject selectedTarget, int reach)
    {
        for (int i = 0; i < targetList.Count; ++i)
        {
            if (targetList[i] == selectedTarget || (targetList[i].characterOnTile != null && targetList[i].characterOnTile == selectedTarget))
            {
                return true;
            }
        }
        return false;
    }

    public void swapTiles (ClickableTile previousTile, ClickableTile newTile, int tileNumber)
    {
        newTile.characterOnTile = previousTile.characterOnTile;
        newTile.map = this;
        newTile.TileX = previousTile.TileX;
        newTile.TileY = previousTile.TileY;
        tiles[previousTile.TileX, previousTile.TileY] = tileNumber;
        clickableTiles[previousTile.TileX, previousTile.TileY] = newTile;
        if (previousTile.characterOnTile != null)
        {
            Basic_Character_Class unitAffected = previousTile.characterOnTile.GetComponent<Basic_Character_Class>();
            unitAffected.tile = newTile;
            unitAffected.removeStatus(unitAffected.tileEffect, true);
            StatusEffect newEffect = new StatusEffect();
            newEffect.initializeTileEffect(newTile.statsToEffect, newTile.name, newTile.effectAmounts, unitAffected.gameObject, newTile.name + "Effect");
            unitAffected.tileType = newTile.map.tileTypes[tileNumber];
        }
        Destroy(previousTile.gameObject);

    }

    public void addTileEffect(int x, int y, GameObject unitToAffect){
        StatusEffect newEffect = new StatusEffect();
        newEffect.initializeTileEffect(tileTypes[tiles[x, y]].tileVisualPrefab.GetComponent<ClickableTile>().statsToEffect, tileTypes[tiles[x, y]].name, tileTypes[tiles[x, y]].tileVisualPrefab.GetComponent<ClickableTile>().effectAmounts, unitToAffect, tileTypes[tiles[x, y]].name + "Effect");
    }
}




