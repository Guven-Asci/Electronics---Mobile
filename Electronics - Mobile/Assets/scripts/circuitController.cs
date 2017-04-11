using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
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
    string firstPinTemp;

    public List<string> resList=new List<string>();
    int resCount;
    public List<string> holeEquivalent = new List<string>();
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
                tempHole.name = (char)(i+65) +(j+1).ToString();
                holeStartPoint.x += xGap;
            }
            holeStartPoint.x = offset.x;
            holeStartPoint.z -= zGap;
        }
	
	}
	
	// Update is called once per frame
	void Update () {
        findParallelRes();
        //Debug.Log(shortCircuit("3", "5"));
        if (Input.touchCount == 1)
        {
            mouseRay = Camera.main.ScreenPointToRay(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, temp));

            if (Physics.Raycast(mouseRay, out hit))
            {
                mousePosition = hit.point;
                
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
                            firstPinTemp = hit.transform.name.Substring(1);
                        }

                        if (selectedComponent == "res")
                        {
                            resistorOnMouse = Instantiate(resistor, hit.transform.position, resistor.transform.rotation) as GameObject;
                            resistorOnMouse.GetComponent<resScrpt>().firstProbe = hit.transform.position;
                            resistorOnMouse.name = "R" + resCount.ToString();
                            firstPinTemp = hit.transform.name;
                            resCount++;
                            
                        }
                            

                    }
                }

            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (selectedComponent == "cable" && cableOnMouse!=null)
                {
                    cableOnMouse.GetComponent<curvedLine>().finish(nearestHole(mousePosition).transform.position);
                    cableOnMouse = null;
                    makeHoleEq(firstPinTemp,nearestHole(mousePosition).name.Substring(1));
                        
                }
                else if (selectedComponent == "res" && resistorOnMouse!=null)
                {
                    resistorOnMouse.GetComponent<resScrpt>().secondProbe = nearestHole(mousePosition).transform.position;
                    resList.Add(resistorOnMouse.name+"-"+firstPinTemp + "-"+nearestHole(mousePosition).transform.name);
                    resistorOnMouse = null;
                    
                        
                }
                
            }
        }
	}

    GameObject nearestHole(Vector3 position)
    {
        int coordinateX =(int)Mathf.Round((position.x - holeStartPoint.x) / xGap);
        int coordinateZ = 10 - (int)Mathf.Round(((position.z - holeStartPoint.z) / zGap));
        //Debug.Log(coordinateZ.ToString() + "-" + coordinateX.ToString());

        return GameObject.Find((char)(coordinateZ+65) + (coordinateX+1).ToString());
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

    public void openSpecifications() {
        Debug.Log(hit.transform.name);
    }

    void findParallelRes()
    {
        for (int i=0;i<resList.Count;i++)
        {
            string[] strArray = resList[i].Split("-"[0]);
            //Debug.Log(strArray[1]);
            for (int j = i+1; j < resList.Count; j++)
            {
                string[] strArray2 = resList[j].Split("-"[0]);
                if ((shortCircuit(strArray[1].Substring(1),strArray2[1].Substring(1))
                    &&shortCircuit(strArray[2].Substring(1),strArray2[2].Substring(1))) ||
                    (shortCircuit(strArray[2].Substring(1), strArray2[1].Substring(1))
                    && shortCircuit(strArray[1].Substring(1), strArray2[2].Substring(1))))
                {
                    Debug.Log(strArray[0] + "+" + strArray2[0]);
                }
            }
            
        }
    }

    void makeHoleEq(string a, string b)
    {

        string result = a + "-" + b;

        if (!holeEquivalent.Contains(result))
        {
            holeEquivalent.Add(result);
            holeEquivalent.Add(b +"-"+ a);
        }


    }

    bool shortCircuit(string a, string b)
    {
        if (a == b)
            return true;
        else
        {
            List<string> tempNodeEq = new List<string>(holeEquivalent);
            List<string> cache = new List<string>();
            cache.Add(a);
            do
            {
                foreach (string eq in tempNodeEq.ToArray())
                {
                    if (eq.StartsWith(cache[0].ToString()))
                    {
                        if (eq.Split("-"[0])[1] == b)
                            return true;
                        else
                        {
                            cache.Add(eq.Split("-"[0])[1]);
                            tempNodeEq.Remove(eq);
                        }

                    }

                }
                cache.RemoveAt(0);
            } while (!(cache.Count == 0));
        }
            return false;
    }

}
