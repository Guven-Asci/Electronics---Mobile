using UnityEngine;
using System.Collections;

public class probeControl : MonoBehaviour {
    Ray resRay;
    RaycastHit hit;
    Vector3 mousePosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount == 1)
        {
            resRay = Camera.main.ScreenPointToRay(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 100f));

            if (Physics.Raycast(resRay, out hit))
            {
                if(hit.transform.tag!="probe")
                    mousePosition = hit.point;
            }

            transform.position = new Vector3(mousePosition.x, 1.56f , mousePosition.z);
        }
	}
}
