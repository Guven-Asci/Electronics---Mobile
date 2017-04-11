using UnityEngine;
using System.Collections;

public class resScrpt : MonoBehaviour {
    public Vector3 firstProbe,secondProbe;
    
    Ray resRay;
    RaycastHit hit;
    Vector3 mousePosition;
    int clickCount=0;
    bool isDone = false;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (!isDone)
        {
            resRay = Camera.main.ScreenPointToRay(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 100f));

            if (Physics.Raycast(resRay, out hit))
            { 
                mousePosition = hit.point;
            }
            

            if (firstProbe == Vector3.zero)
            {
                transform.position = mousePosition;
            }
            else if (secondProbe == Vector3.zero)
            {
                transform.position = firstProbe;
                Vector3 difference = firstProbe - mousePosition;
                if (difference.x >= 0)
                    transform.rotation = Quaternion.Euler(transform.rotation.x, -1 * (Mathf.Atan(difference.z / difference.x) * Mathf.Rad2Deg + 90f), transform.rotation.z);
                else
                    transform.rotation = Quaternion.Euler(transform.rotation.x, -1 * (Mathf.Atan(difference.z / difference.x) * Mathf.Rad2Deg - 90f), transform.rotation.z);

                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Mathf.Sqrt(Mathf.Pow(difference.x, 2f) + Mathf.Pow(difference.z, 2f)) * 80);

            }
            else
            {
                finish(secondProbe);
            }
        }
        
	}
    void finish(Vector3 finalPosition)
    {
        if (firstProbe == finalPosition)
            Destroy(transform.gameObject);
        else
        {
            Vector3 difference = firstProbe - finalPosition;
            if (difference.x >= 0)
                transform.rotation = Quaternion.Euler(transform.rotation.x, -1 * (Mathf.Atan(difference.z / difference.x) * Mathf.Rad2Deg + 90f), transform.rotation.z);
            else
                transform.rotation = Quaternion.Euler(transform.rotation.x, -1 * (Mathf.Atan(difference.z / difference.x) * Mathf.Rad2Deg - 90f), transform.rotation.z);

            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Mathf.Sqrt(Mathf.Pow(difference.x, 2f) + Mathf.Pow(difference.z, 2f)) * 80);
            isDone = true;
        }

    }
}
