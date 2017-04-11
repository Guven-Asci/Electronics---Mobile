using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class curvedLine : MonoBehaviour {

    LineRenderer line;
    int pointCount=20;
    public Vector3 firstProbe;
    public bool isDone=false;
    Vector3 mousePosition;
    Ray resRay;
    RaycastHit hit;

	// Use this for initialization
	void Start () {
        line = GetComponent<LineRenderer>();
        GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        line.SetVertexCount(pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            line.SetPosition(i, Vector3.zero);
        }
        //line.numPositions = pointCount;
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


            float yMax = 1f;
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < pointCount; i++)
            {

                float x = ((mousePosition.x - firstProbe.x) / (pointCount - 1)) * i,
                    z = ((mousePosition.z - firstProbe.z) / (pointCount - 1)) * i,
                    y = (-1 * x * x + (mousePosition.x - firstProbe.x) * x) + (-1 * z * z + (mousePosition.z - firstProbe.z) * z);

                if (y > yMax)
                    yMax = y;
                points.Add(new Vector3(firstProbe.x + x, y, firstProbe.z + z));
            }
            for (int i = 0; i < pointCount; i++)
            {
                line.SetPosition(i, new Vector3(points[i].x, points[i].y / (yMax / 2), points[i].z));
                
            }



           
        }
	
	}
    public void finish(Vector3 finalPosition)
    {
        if (firstProbe == finalPosition)
            Destroy(transform.gameObject);
        else
        {
            float yMax = 1f;
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < pointCount; i++)
            {

                float x = ((finalPosition.x - firstProbe.x) / (pointCount - 1)) * i,
                    z = ((finalPosition.z - firstProbe.z) / (pointCount - 1)) * i,
                    y = (-1 * x * x + (finalPosition.x - firstProbe.x) * x) + (-1 * z * z + (finalPosition.z - firstProbe.z) * z);

                if (y > yMax)
                    yMax = y;
                points.Add(new Vector3(firstProbe.x + x, y, firstProbe.z + z));
            }
            for (int i = 0; i < pointCount; i++)
            {
                line.SetPosition(i, new Vector3(points[i].x, points[i].y / (yMax / 2), points[i].z));
                if (i != pointCount - 1)
                    addColliderToLine(new Vector3(points[i].x, points[i].y / (yMax / 2), points[i].z),
                        new Vector3(points[i + 1].x, points[i + 1].y / (yMax / 2), points[i + 1].z));
            }
            isDone = true;
        }
    }

    private void addColliderToLine(Vector3 startPos,Vector3 endPos)
    {
        BoxCollider col = new GameObject("Collider").AddComponent<BoxCollider> ();
        col.transform.tag = "cable";
        col.transform.parent = line.transform; // Collider is added as child object of line
        float lineLength = Vector3.Distance (startPos, endPos)+0.1f; // length of line
        col.size = new Vector3 (lineLength, 0.1f, 0.1f); // size of collider is set where X is length of line, Y is width of line, Z will be set as per requirement
        Vector3 midPoint = (startPos + endPos)/2;
        col.transform.position = midPoint; // setting position of collider object
        // Following lines calculate the angle between startPos and endPos
        float angle = (Mathf.Abs (startPos.y - endPos.y) / Mathf.Abs (startPos.x - endPos.x));
        if((startPos.y<endPos.y && startPos.x>endPos.x) || (endPos.y<startPos.y && endPos.x>startPos.x))
        {
            angle*=-1;
        }
        angle = Mathf.Rad2Deg * Mathf.Atan (angle);
        col.transform.Rotate (0, 0, angle);
    }

}
