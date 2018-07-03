using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowsScale : MonoBehaviour {

    public float offset = 70;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float dist = Vector3.Distance(transform.position, Camera.main.transform.position) + offset;
        transform.localScale = new Vector3(dist, dist, dist);
    }
}
