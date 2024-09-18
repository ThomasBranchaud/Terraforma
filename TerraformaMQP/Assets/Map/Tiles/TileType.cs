using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType
{
    public string name;
    public GameObject tileVisualPrefab;

    public int cost = 1;

    public bool isWalkable  = true;

    void Start()
    {

    }

}
