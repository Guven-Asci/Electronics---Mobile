using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using SharpCircuit;

public class circuitController : MonoBehaviour {
    Circuit sim = new Circuit();

    public GameObject hole;
    public GameObject resistor;
    public GameObject cable;
    public GameObject probe;
    public GameObject resistorOnMouse;
    public GameObject cableOnMouse;
    public GameObject probeOnMouse;
    Transform oldHole;
    float temp = 9.5f;

    Color defaultHoleColor;
    public GameObject startHole;
    Vector3 offset ;
    Vector3 holeStartPoint;
    float xGap=0.44f,zGap=0.5f;
    int xCount = 30, zCount = 5;
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
    public List<string> parallelRes = new List<string>();
    public List<string> serialRes = new List<string>();

    public Text parallelAndSerialText;
    public Image redProbeButton;
    public Image blackProbeButton;

    public string redProbePin="",blackProbePin="";

    Dictionary<string, Resistor> resSharpRef = new Dictionary<string, Resistor>();

    //List<Comp> deneme = new List<Comp>();

	// Use this for initialization
	void Start () {

        HoleInitialization();
        resList.Add("R0-A1-A2-1000");
        resList.Add("R1-A1-A3-1000");
        resList.Add("R2-A2-A3-1000");
        resList.Add("R3-A3-A5-100");

        breadToTopology();

        
        //Cap cap = new Cap();
        //cap.name = "a";
        //cap.capacity = 50f;
        //deneme.Add(cap);


        //Res res=new Res();
        //res.name = "b";
        //res.resistance = 10f;
        //deneme.Add(res);

        //foreach (Comp c in deneme)
        //{
        //    if (c is Res)
        //    {
        //        Debug.Log(c);
        //    }
        //}
	}
	
	// Update is called once per frame
	void Update () {
        printParallelandSerials();
        sim.doTick();

        //Debug.Log(resSharpRef.Count);
        //Debug.Log(resSharpRef["R3-A3-A5-100"].getVoltageDelta());
        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).position.x<50)
                Camera.main.transform.Translate(-0.08f,0,0);
            if (Screen.width-Input.GetTouch(0).position.x <50)
                Camera.main.transform.Translate(0.08f, 0, 0);
            if (Input.GetTouch(0).position.y <75)
                Camera.main.transform.Translate(0, -0.04f, 0);
            if (Screen.height - Input.GetTouch(0).position.y < 75)
                Camera.main.transform.Translate(0, 0.04f, 0);
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
                    if (hit.transform.tag == "probe")
                    {
                        probeOnMouse = hit.transform.gameObject;
                        probeOnMouse.GetComponent<probeControl>().enabled = true;
                        selectedComponent = "probe";
                        if (probeOnMouse.GetComponent<Renderer>().material.color == Color.red)
                            redProbePin = "";
                        else if (probeOnMouse.GetComponent<Renderer>().material.color == Color.black)
                            blackProbePin = "";


                    }
                    else if (selectedComponent=="cable")
                    {
                        int coordinateX = (int)Mathf.Round((mousePosition.x - holeStartPoint.x) / xGap);
                        int coordinateZ = zCount - (int)Mathf.Round(((mousePosition.z - holeStartPoint.z) / zGap));

                        if(coordinateX>-2 && coordinateX<xCount+2 && coordinateZ>-2 && coordinateZ<zCount+2)
                        {
                            cableOnMouse = Instantiate(cable);
                            cableOnMouse.GetComponent<curvedLine>().firstProbe = nearestHole(mousePosition).transform.position;
                            firstPinTemp = nearestHole(mousePosition).transform.name.Substring(1);
                        }
                            
                    }

                    else if (selectedComponent == "res")
                    {
                        int coordinateX = (int)Mathf.Round((mousePosition.x - holeStartPoint.x) / xGap);
                        int coordinateZ = zCount - (int)Mathf.Round(((mousePosition.z - holeStartPoint.z) / zGap));

                        if (coordinateX > -2 && coordinateX < xCount+2 && coordinateZ > -2 && coordinateZ < zCount+2)
                        {
                            resistorOnMouse = Instantiate(resistor, hit.transform.position, resistor.transform.rotation) as GameObject;
                            resistorOnMouse.GetComponent<resScrpt>().firstProbe = nearestHole(mousePosition).transform.position;
                            resistorOnMouse.name = "R" + resCount.ToString();
                            firstPinTemp = nearestHole(mousePosition).transform.name;
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
                    cableOnMouse.name = firstPinTemp + "-" + nearestHole(mousePosition).name.Substring(1);
                    cableOnMouse = null;
                    makeHoleEq(firstPinTemp,nearestHole(mousePosition).name.Substring(1));
                        
                }
                else if (selectedComponent == "res" && resistorOnMouse!=null)
                {
                    resistorOnMouse.GetComponent<resScrpt>().secondProbe = nearestHole(mousePosition).transform.position;
                    resList.Add(resistorOnMouse.name+"-"+firstPinTemp + "-"+nearestHole(mousePosition).transform.name+"-1000");
                    resistorOnMouse = null;
                }
                else if (selectedComponent == "probe"&& probeOnMouse!=null)
                {
                    probeOnMouse.transform.position = nearestHole(mousePosition).transform.position;
                    if (probeOnMouse.GetComponent<Renderer>().material.color == Color.red)
                        redProbePin = nearestHole(mousePosition).name.Substring(1);
                    else if (probeOnMouse.GetComponent<Renderer>().material.color == Color.black)
                        blackProbePin = nearestHole(mousePosition).name.Substring(1); 
                    probeOnMouse.GetComponent<probeControl>().enabled = false;
                    probeOnMouse = null;
                    selectCable();
                }
                findParallelRes();
                findSerialRes();
                
            }
        }
	}

    private void HoleInitialization()
    {
        defaultHoleColor = hole.GetComponent<SpriteRenderer>().color;


        offset = startHole.transform.position;
        holeStartPoint = offset;
        for (int i = 0; i < zCount; i++)
        {
            for (int j = 0; j < xCount; j++)
            {
                GameObject tempHole = Instantiate(hole, holeStartPoint, hole.transform.rotation) as GameObject;
                tempHole.transform.SetParent(holeParent.transform);
                tempHole.name = (char)(i + 65) + (j + 1).ToString();
                holeStartPoint.x += xGap;
            }
            holeStartPoint.x = offset.x;
            holeStartPoint.z -= zGap;
        }
        
    }

    GameObject nearestHole(Vector3 position)
    {
        int coordinateX =(int)Mathf.Round((position.x - holeStartPoint.x) / xGap);
        int coordinateZ = zCount - (int)Mathf.Round(((position.z - holeStartPoint.z) / zGap));

        if (coordinateX < 0)
            coordinateX = 0;
        if (coordinateX > xCount)
            coordinateX = xCount-1;
        if (coordinateZ < 0)
            coordinateZ = 0;
        if (coordinateZ > zCount)
            coordinateZ = zCount-1;

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

    public void reset()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void openSpecifications() {
        if(hit.transform.tag=="resistor"){
            foreach (string res in resList.ToArray())
            {
                if (res.StartsWith(hit.transform.name))
                {
                    resList.Remove(res);
                    Destroy(hit.transform.gameObject);
                }
                    
            }
            
        }

        else if (hit.transform.tag == "cable")
        {
            string[] strArray = hit.transform.parent.name.Split("-"[0]);
            holeEquivalent.Remove(strArray[0]+"-"+strArray[1]);
            holeEquivalent.Remove(strArray[1] + "-" + strArray[0]);
            Destroy(hit.transform.parent.gameObject);
        }

        findParallelRes();
        findSerialRes();
    }

    void findParallelRes()
    {
        parallelRes=new List<string>();
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
                    parallelRes.Add(strArray[0] + "//" + strArray2[0]);
                }
            }
            
        }
    }

    void findSerialRes()
    {
        serialRes=new List<string>();
        for (int i = 0; i < resList.Count; i++)
        {
            string[] strArray = resList[i].Split("-"[0]);
            for (int j = i + 1; j < resList.Count; j++)
            {
                
                string[] strArray2 = resList[j].Split("-"[0]);
                if (shortCircuit(strArray[1].Substring(1), strArray2[1].Substring(1)) || shortCircuit(strArray[2].Substring(1), strArray2[1].Substring(1)))
                {
                    bool areThereMore = false;
                    for (int k = 0; k < resList.Count; k++)
                    {
                        string[] strArray3 = resList[k].Split("-"[0]);
                        if (shortCircuit(strArray3[1].Substring(1), strArray2[1].Substring(1)) || 
                            shortCircuit(strArray3[2].Substring(1), strArray2[1].Substring(1)))
                        {
                            if (!(strArray3[0] == strArray[0] || strArray3[0] == strArray2[0]))
                                areThereMore = true;
                        }
                    }
                    if (!areThereMore)
                    {
                        bool areTheyParallel = false;
                        foreach (string parallel in parallelRes)
                        {
                            string[] strArray4 = parallel.Split(new string[] { "//" }, StringSplitOptions.None);
                            if ((strArray4[0] == strArray[0] &&
                                strArray4[1] == strArray2[0])||
                                (strArray4[1] == strArray[0] &&
                                strArray4[0] == strArray2[0]))
                            {
                                areTheyParallel = true;
                            }
                        }
                        if (!areTheyParallel)
                            serialRes.Add(strArray[0] + "+" + strArray2[0]);

                    }
                }
                else if (shortCircuit(strArray[1].Substring(1), strArray2[2].Substring(1)) || shortCircuit(strArray[2].Substring(1), strArray2[2].Substring(1)))
                {
                    bool areThereMore = false;
                    for (int k = 0; k < resList.Count; k++)
                    {
                        string[] strArray3 = resList[k].Split("-"[0]);
                        if (shortCircuit(strArray3[1].Substring(1), strArray2[2].Substring(1)) ||
                            shortCircuit(strArray3[2].Substring(1), strArray2[2].Substring(1)))
                        {
                            if (!(strArray3[0] == strArray[0] || strArray3[0] == strArray2[0]))
                                areThereMore = true;
                        }
                    }
                    if (!areThereMore)
                    {
                        bool areTheyParallel = false;
                        foreach (string parallel in parallelRes)
                        {
                            string[] strArray4 = parallel.Split(new string[] { "//" }, StringSplitOptions.None);
                            
                            if ((strArray4[0] == strArray[0] &&
                                strArray4[1] == strArray2[0]) ||
                                (strArray4[1] == strArray[0] &&
                                strArray4[0] == strArray2[0]))
                            {
                                
                                areTheyParallel = true;
                            }
                        }
                        if (!areTheyParallel)
                            serialRes.Add(strArray[0] + "+" + strArray2[0]);

                    }
                }
                
                    
                
            }
        }

    }

    bool areTheyParallel(string firstRes,string secondRes)
    {
        foreach (string parallel in parallelRes)
        {
            string[] strArray4 = parallel.Split(new string[] { "//" }, StringSplitOptions.None);
            if ((strArray4[0] == firstRes &&
                strArray4[1] == secondRes) ||
                (strArray4[1] == firstRes &&
                strArray4[0] == secondRes))
            {
                return true;
            }
        }
        return false;

    }

    bool areTheySerial(string firstRes,string secondRes)
    {
        foreach (string serial in serialRes)
        {
            string[] strArray4 = serial.Split('+');
            if ((strArray4[0] == firstRes &&
                strArray4[1] == secondRes) ||
                (strArray4[1] == firstRes &&
                strArray4[0] == secondRes))
            {
                return true;
            }
        }
        return false;

    
    }

    void printParallelandSerials()
    {
        parallelAndSerialText.text = "<b>Serial Resistors:\n</b>";
        if (serialRes.Count == 0)
            parallelAndSerialText.text += "---\n";
        else
        {
            foreach (string serial in serialRes)
            {
                parallelAndSerialText.text += serial + "\n";
            }
        }
        parallelAndSerialText.text += "<b>Parallel Resistors:\n</b>";
        if (parallelRes.Count == 0)
            parallelAndSerialText.text += "---\n";
        else
        {
            foreach (string parallel in parallelRes)
            {
                parallelAndSerialText.text += parallel + "\n";
            }
        }
        parallelAndSerialText.text += "<b>Equivalent Resistance:\n</b>";
        if (redProbePin != "" && blackProbePin != "")
            parallelAndSerialText.text += (findEquivalentRes(redProbePin, blackProbePin)).ToString()+"\n";
        else
            parallelAndSerialText.text += "None\n";

        parallelAndSerialText.text += "<b>Short Circuit:\n</b>";
        if (redProbePin != "" && blackProbePin != "")
            parallelAndSerialText.text += shortCircuit(redProbePin, blackProbePin).ToString();
        else
            parallelAndSerialText.text += "None\n";

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

    public List<string> tempList = new List<string>();
    int crashControl = 0;
    float equivalentRes(string firstPin, string secondPin)
    {
        if (crashControl == 0) { tempList = new List<string>(resList); }
        crashControl++;
        if (crashControl > 20) { Debug.Log("Crash"); return 0f; }
        List<float> parallelList = new List<float>();

        foreach (string first in tempList.ToArray())
        {
            string[] tempArray = first.Split('-');
            
            if (shortCircuit(tempArray[1].Substring(1), firstPin) && shortCircuit(tempArray[2].Substring(1), secondPin) ||
                shortCircuit(tempArray[2].Substring(1), firstPin) && shortCircuit(tempArray[1].Substring(1), secondPin))
            {
                
                parallelList.Add(float.Parse(tempArray[3]));
                tempList.Remove(first);
                
            }
        }
        foreach (string first in tempList.ToArray())
        {
            string[] tempArray = first.Split('-');
            if (shortCircuit(tempArray[1].Substring(1), firstPin))
            {
                float a = equivalentRes(firstPin, tempArray[2].Substring(1));
                tempList.Remove(first);
                parallelList.Add(a + equivalentRes(tempArray[2].Substring(1), secondPin));
            }
            else if (shortCircuit(tempArray[2].Substring(1), firstPin))
            {
                float a = equivalentRes(firstPin, tempArray[1].Substring(1));
                tempList.Remove(first);
                parallelList.Add(a + equivalentRes(tempArray[1].Substring(1), secondPin));
            }
            
        }
        double sum=0f;
        foreach (float resValue in parallelList)
        {
            sum += 1/resValue;
        }
        sum = 1 / sum;

        return (float)sum;
    }

    float findEquivalentRes(string firstPin, string secondPin)
    {
        float result;
        tempList = new List<string>(resList);
        result = equivalentRes(firstPin, secondPin);
        crashControl = 0;
        return result;
        
        
    }

    public void newProbe(bool redORblack)
    {
        if (!redORblack && redProbePin == "")
        {
            redProbeButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
            probeOnMouse = Instantiate(probe);
            probeOnMouse.GetComponent<Renderer>().material.color = Color.red;
            probeOnMouse.GetComponent<probeControl>().enabled = true;
            selectedComponent = "probe";
        }
        else if(blackProbePin == "")
        {
            blackProbeButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
            probeOnMouse = Instantiate(probe);
            probeOnMouse.GetComponent<Renderer>().material.color = Color.black;
            probeOnMouse.GetComponent<probeControl>().enabled = true;
            selectedComponent = "probe";
        }


        

    }

    public void probeIsback(bool redORblack)
    {
        if (probeOnMouse!=null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };

            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                if (results[0].gameObject.name == "redProbe"&&
                    probeOnMouse.GetComponent<Renderer>().material.color == Color.red)
                {
                    redProbeButton.GetComponent<Image>().color = Color.white;
                    Destroy(probeOnMouse);
                    probeOnMouse = null;
                }
                else if (results[0].gameObject.name == "blackProbe" &&
                    probeOnMouse.GetComponent<Renderer>().material.color == Color.black)
                {
                    blackProbeButton.GetComponent<Image>().color = Color.white;
                    Destroy(probeOnMouse);
                    probeOnMouse = null;
                }
            } 
        }
        

    }

    string voltageLead = "1";
    string groundLead = "5";

    private void breadToTopology()
    {
        tempList = new List<string>(resList);
        
        var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
        volt0.maxVoltage = 5f;
        var ground0 = sim.Create<Ground>();
        connectComponent("1");
        Debug.Log(resultList.Count);
        foreach (string comp in resultList.ToArray())
        {
            Debug.Log(comp);
            string[] tempArray = comp.Split('-');
            if (!resSharpRef.ContainsKey(comp))
            {
                var res = sim.Create<Resistor>(float.Parse(tempArray[3]));
                resSharpRef.Add(comp, res);
            }
            if (tempArray[1].Substring(1) == voltageLead)
                sim.Connect(resSharpRef[comp], 0, volt0, 0);
            else if (tempArray[2].Substring(1) == voltageLead)
                sim.Connect(resSharpRef[comp], 1, volt0, 0);

            if (tempArray[1].Substring(1) == groundLead)
                sim.Connect(resSharpRef[comp], 0, ground0, 0);
            else if (tempArray[2].Substring(1) == groundLead)
                sim.Connect(resSharpRef[comp], 1, ground0, 0);
            
            resultList.Remove(comp);
            foreach (string comp2 in resultList.ToArray())
            {
                string[] tempArray2 = comp2.Split('-');
                if (!resSharpRef.ContainsKey(comp2))
                {
                    var res = sim.Create<Resistor>(float.Parse(tempArray2[3]));
                    resSharpRef.Add(comp2, res);
                }

                if (tempArray[1].Substring(1) == tempArray2[1].Substring(1))
                {
                    sim.Connect(resSharpRef[comp], 0, resSharpRef[comp2], 0);
                }
                else if (tempArray[1].Substring(1) == tempArray2[2].Substring(1))
                {
                    sim.Connect(resSharpRef[comp], 0, resSharpRef[comp2], 1);
                }
                else if (tempArray[2].Substring(1) == tempArray2[1].Substring(1))
                {
                    sim.Connect(resSharpRef[comp], 1, resSharpRef[comp2], 0);
                }
                else if (tempArray[2].Substring(1) == tempArray2[2].Substring(1))
                {
                    sim.Connect(resSharpRef[comp], 1, resSharpRef[comp2], 1);
                }
                
            }
        }

        sim.doTick();
    }

    public List<string> resultList = new List<string>();
    private bool connectComponent(string firstLead)
    {
        bool groundReached = false;
        
        crashControl++;
        if (crashControl > 20) { Debug.Log("Crash"); return false; }
        if (firstLead == groundLead)
            return true;
        else
        {
            foreach (string comp in tempList.ToArray())
            {
                string[] tempArray = comp.Split('-');
                if (tempArray[1].Substring(1) == firstLead)
                {
                    tempList.Remove(comp);
                    if (connectComponent(tempArray[2].Substring(1)))
                    {
                        if(!resultList.Contains(comp))
                            resultList.Add(comp);
                        groundReached = true;
                        
                        
                    }
                    tempList.Add(comp);
                    

                }
                else if (tempArray[2].Substring(1) == firstLead)
                {
                    tempList.Remove(comp);
                    if (connectComponent(tempArray[1].Substring(1)))
                    {
                        if (!resultList.Contains(comp))
                            resultList.Add(comp);
                        groundReached = true;
                        
                        
                    }
                    tempList.Add(comp);
                }
                
            }
            if (groundReached)
                return true;
        }
        return false;
    }


}

class Comp
{
    public string name;
}

class Res : Comp
{
    public float resistance;
    public List<string> pin;
}

class Cap : Comp
{
    public float capacity;
}