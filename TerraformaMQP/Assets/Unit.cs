using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int tileX = 0;
    public int tileY = 0;
    public TileType tile;

    public bool charSelected = false;
    public bool charHover  = false;

    public List<Node> path = null;

    public Camera camera;
    public Renderer renderer;


    public void SetTile(int x, int y) {

    }

    // void Start() {
    //     renderer = GetComponent<Renderer>();
    // }

    void Update() {
        if(charHover) {
            if(Input.GetMouseButtonDown(0)) {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                //Debug.Log("Check 1");

                if(Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    //Debug.Log("Check 2");
                    if(hitInfo.collider.gameObject.GetComponent<Unit>());
                    {
                        charSelected = true;
                        Debug.Log("Clicked on Unit");

                    }
                }
            }
        }

        if(Input.GetKeyUp("escape") && charSelected == true) {
            charSelected = false;
            Debug.Log("Character UnSelected");
        }
    }

    //Recolors when mouse is hovering over a unit
    public void mouseEnter() {
        Debug.Log("Mouse Entered");
        renderer.material.color = Color.blue;
        charHover = true;
    }

    //Resets when mouse has stopped hovering over a unit
    public void mouseExit() {
        Debug.Log("Mouse Exited");
        renderer.material.color = Color.white;
        charHover = false;
    }

}
