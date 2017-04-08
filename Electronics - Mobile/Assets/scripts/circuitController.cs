using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class circuitController : MonoBehaviour {
    public GameObject hole;
    public GameObject resistor;
    public GameObject cable;
    static public GameObject resistorOnMouse;
    static public GameObject cableOnMouse;
    Transform oldHole;
    public float temp = 9.5f;

    Color defaultHoleColor;
    public GameObject startHole;
    Vector3 offset ;
    Vector3 holeStartPoint;
    float xGap=0.22f,zGap=0.25f;
    public GameObject holeParent;

    Ray mouseRay;
    RaycastHit hit;
    Vector3 mousePosition;

    bool isCableDrawing = false, isResDrawing=false;
    public Text onHand;

    string selectedComponent = "cable";
	// Use this for initialization
	void Start () {

        defaultHoleColor=hole.GetComponent<SpriteRenderer>().color;

        
        offset = startHole.transform.position;
        holeStartPoint = offset;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 60; j++)
            {
                GameObject tempHole=Instantiate(hole, holeStartPoint, hole.transform.rotation) as GameObject;
                tempHole.transform.SetParent(holeParent.transform);
                tempHole.name = i.ToString() +"-"+ j.ToString();
                holeStartPoint.x += xGap;
            }
            holeStartPoint.x = offset.x;
            holeStartPoint.z -= zGap;
        }
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.touchCount == 1)
        {
            mouseRay = Camera.main.ScreenPointToRay(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, temp));


            mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, temp));
            mousePosition.y = 0.3f;


            if (Physics.Raycast(mouseRay, out hit))
            {

                if (hit.transform.tag == "Hole")
                {
                    if (oldHole != null)
                        oldHole.GetComponent<SpriteRenderer>().color = defaultHoleColor;
                    hit.transform.GetComponent<SpriteRenderer>().color = Color.green;
                    oldHole = hit.transform;
                }
                else
                {
                    if (oldHole != null)
                        oldHole.GetComponent<SpriteRenderer>().color = defaultHoleColor;
                    oldHole = null;
                }

            }

            if (Input.GetTouch(0).phase==TouchPhase.Began)
            {
                if (Physics.Raycast(mouseRay, out hit))
                {
                    if (hit.transform.tag == "Hole")
                    {
                        if (selectedComponent=="cable")
                        {
                            
                            cableOnMouse = Instantiate(cable);
                            cableOnMouse.GetComponent<curvedLine>().firstProbe = hit.transform.position;
                        }

                        if (selectedComponent == "res")
                        {
                            resistorOnMouse = Instantiate(resistor, mousePosition, resistor.transform.rotation) as GameObject;
                            resistorOnMouse.GetComponent<resScrpt>().firstProbe = hit.transform.position;
                            
                        }
                            

                    }
                }

            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (selectedComponent == "cable"&&cableOnMouse!=null)
                {
                    cableOnMouse.GetComponent<curvedLine>().finish(nearestHole(mousePosition).transform.position);
                    cableOnMouse = null;
                        
                }
                else if (selectedComponent == "res"&&resistorOnMouse!=null)
                {
                    resistorOnMouse.GetComponent<resScrpt>().secondProbe = nearestHole(mousePosition).transform.position;
                    resistorOnMouse = null;
                        
                }
                
            }
        }
	}

    GameObject nearestHole(Vector3 position)
    {
        int coordinateX =(int)Mathf.Round((position.x - holeStartPoint.x) / xGap);
        int coordinateZ = 10 - (int)Mathf.Round(((position.z - holeStartPoint.z) / zGap));
        Debug.Log(coordinateZ.ToString() + "-" + coordinateX.ToString());

        return GameObject.Find(coordinateZ.ToString() + "-" + coordinateX.ToString());
    }

    public void selectRes()
    {
        if (selectedComponent != "res")
        {
            selectedComponent = "res";
            onHand.text = "Resistor";
        }
    }

    public void selectCable()
    {
        if (selectedComponent != "cable")
        {
            selectedComponent = "cable";
            onHand.text = "Cable";
        }
    }
}
