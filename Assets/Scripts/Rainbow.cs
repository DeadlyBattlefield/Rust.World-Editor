using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rainbow : MonoBehaviour {

    public static Material material;

    public Material mat;
    Color color = Color.red;
    int id = 0;
    int timesran = 0;

	// Use this for initialization
	void Start () {
        if (material == null) {
            material = mat;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (id == 0) {
            color = Color.Lerp(color, Color.green, 0.2f);
            mat.color = color;
            timesran++;
            if (timesran >= 14) {
                id = 1;
                timesran = 0;
            }
        }
        if (id == 1)
        {
            color = Color.Lerp(color, Color.blue, 0.2f);
            mat.color = color;
            timesran++;
            if (timesran >= 14)
            {
                id = 2;
                timesran = 0;
            }
        }
        if (id == 2)
        {
            color = Color.Lerp(color, Color.red, 0.2f);
            mat.color = color;
            timesran++;
            if (timesran >= 14)
            {
                id = 0;
                timesran = 0;
            }
        }
    }
}
