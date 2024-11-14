using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType
{
    public string name;
    public GameObject tileVisualPrefab;
    public bool isWalkable;

    public int cost = 1;

    void Start()
    {

    }

}
