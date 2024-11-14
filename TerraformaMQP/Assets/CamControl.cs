using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    public Vector3 startPos;
    public float tooFarHorizontal = 10;
    public float tooFarVertical = 10;

    public TileMap map;

    float speed = 4f;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Unit is actively moving
        if (map.currentPath != null) {
            transform.position = new Vector3(map.selectedUnit.transform.position.x, 7, map.selectedUnit.transform.position.z - 2.3f);
        } else if (map.selectedUnit != null) { //Unit is selected but isn't moving, restrict camera so that they are always within shot.
            float hInput = Input.GetAxis("Horizontal");
            float vInput = Input.GetAxis("Vertical");
            GameObject unit = map.selectedUnit;
            Vector3 unitPos = unit.transform.position;

            //Set restrictions
            float northWall = unitPos.z + 4;
            float southWall = unitPos.z - 4;
            float eastWall = unitPos.x + 3;
            float westWall = unitPos.x - 5;

            //Set movement to 0 if too far in wrong direction and also teleport them back to that position
            if (transform.position.x <= westWall && hInput < 0) {
                hInput = 0;
                transform.position = new Vector3(westWall,transform.position.y,transform.position.z);
            } else if (transform.position.x >= eastWall && hInput > 0) {
                hInput = 0;
                transform.position = new Vector3(eastWall,transform.position.y,transform.position.z);
            }

            if (transform.position.z <= southWall && vInput < 0) {
                vInput = 0;
                transform.position = new Vector3(transform.position.x,transform.position.y,southWall);
            } else if (transform.position.z >= northWall && vInput > 0) {
                vInput = 0;
                transform.position = new Vector3(transform.position.x,transform.position.y,northWall);
            }

            transform.position = transform.position + new Vector3(speed * hInput * Time.deltaTime, 0, speed * vInput * Time.deltaTime);    
        } else {
            float hInput = Input.GetAxis("Horizontal");
            float vInput = Input.GetAxis("Vertical");
            
            //Set movement to 0 if too far in wrong direction and also teleport them back to that position
            if (transform.position.x <= 0 && hInput < 0) {
                hInput = 0;
                transform.position = new Vector3(0,transform.position.y,transform.position.z);
            } else if (transform.position.x >= tooFarHorizontal && hInput > 0) {
                hInput = 0;
                transform.position = new Vector3(tooFarHorizontal,transform.position.y,transform.position.z);
            }

            if (transform.position.z <= -5 && vInput < 0) {
                vInput = 0;
                transform.position = new Vector3(transform.position.x,transform.position.y,-5);
            } else if (transform.position.z >= tooFarVertical && vInput > 0) {
                vInput = 0;
                transform.position = new Vector3(transform.position.x,transform.position.y,tooFarVertical);
            }

            transform.position = transform.position + new Vector3(speed * hInput * Time.deltaTime, 0, speed * vInput * Time.deltaTime);    
        }
    }
}
