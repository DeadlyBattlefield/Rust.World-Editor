using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

    public enum ArrowAxis {
        X = 0,
        Y = 1,
        Z = 2,
    }

    public ArrowAxis axis;

    private float dist;
    private bool dragging = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Camera.main.GetComponent<MainCamera>().selection == null)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = true;
        }
        /*if (dragging) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 raypoint = ray.GetPoint(dist);
            if (axis == ArrowAxis.X)
            {
                Camera.main.GetComponent<MainCamera>().selection.transform.position = new Vector3(raypoint.x, Camera.main.GetComponent<MainCamera>().selection.transform.position.y, Camera.main.GetComponent<MainCamera>().selection.transform.position.z);
            }
            if (axis == ArrowAxis.Y)
            {
                Camera.main.GetComponent<MainCamera>().selection.transform.position = new Vector3(Camera.main.GetComponent<MainCamera>().selection.transform.position.x, raypoint.y, Camera.main.GetComponent<MainCamera>().selection.transform.position.z);
            }
            if (axis == ArrowAxis.Z)
            {
                Camera.main.GetComponent<MainCamera>().selection.transform.position = new Vector3(Camera.main.GetComponent<MainCamera>().selection.transform.position.x, Camera.main.GetComponent<MainCamera>().selection.transform.position.y, raypoint.z);
            }
        }*/
    }

    private void OnMouseDown()
    {
        //dist = Vector3.Distance(Camera.main.GetComponent<MainCamera>().selection.transform.position, Camera.main.transform.position);
        //dragging = true;
    }

    private void OnMouseUp()
    {
        //dragging = false;
    }

    /*private void OnMouseDrag()
    {
        if (axis == ArrowAxis.X)
        {
            Camera.main.GetComponent<MainCamera>().X();
        }
        if (axis == ArrowAxis.Y)
        {
            Camera.main.GetComponent<MainCamera>().Y();
        }
        if (axis == ArrowAxis.Z)
        {
            //Vector3 pos2 = Camera.main.ScreenToWorldPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z))) + offset;
            //Camera.main.GetComponent<MainCamera>().selection.transform.position = pos2;
            Camera.main.GetComponent<MainCamera>().Z();
        }
    }*/
}
