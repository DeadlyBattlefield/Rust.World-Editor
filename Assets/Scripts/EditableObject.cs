using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditableObject : MonoBehaviour {

    //public Material mat;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Camera.main.GetComponent<MainCamera>().selection == this.gameObject && this.GetComponent<BoxCollider>() != null) {
            this.gameObject.layer = 2;
            this.GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            this.gameObject.layer = 9;
            if (this.GetComponent<BoxCollider>() != null) {
                this.GetComponent<BoxCollider>().enabled = true;
            }
        }
	}

    /*private void OnMouseOver()
    {
        if (Input.GetButtonDown("Fire2"))
            Camera.main.GetComponent<MainCamera>().selection = this.gameObject;
    }*/

    /*private void OnPostRender()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        GL.modelview = Camera.main.worldToCameraMatrix;
        GL.Begin(GL.TRIANGLES);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(0, 1, 0);
        GL.End();
        GL.PopMatrix();
    }*/
}
