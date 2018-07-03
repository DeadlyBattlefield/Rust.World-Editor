using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Arrow;

public class MainCamera : MonoBehaviour {

    public enum EditorMode
    {
        Object = 0,
        Terrain = 1,
        Splat = 2,
    }

    public enum ObjectMode
    {
        Position = 0,
        Rotation = 1,
        Scale = 2,
    }

    public enum TerrainMode
    {
        HeightMap = 0,
        WaterMap = 1,
        AlphaMap = 2,
    }

    public enum TerrainTool
    {
        Raise = 0,
        Lower = 1,
        Smooth = 2,
        Flat = 3,
    }

    public EditorMode mode;
    public ObjectMode oMode;
    public TerrainMode tMode;
    public TerrainTool tTool;
    public float speed = 10;
    public float sense = 10;
    public int tRadius = 0;
    public float tChange = 0;
    public GameObject arrows;
    public GameObject rings;
    public GameObject terrainEdit;
    public GameObject height;
    public GameObject water;
    //public GameObject alpha;
    public GameObject selection;
    public bool isOpen = false;
    private float dist;
    private ArrowAxis axis;
    private bool drag;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        if (selection != null) {
            arrows.transform.position = selection.transform.position;
            arrows.transform.rotation = selection.transform.rotation;
            rings.transform.position = selection.transform.position;
            rings.transform.rotation = selection.transform.rotation;
        }
        if (Input.GetButton("Fire3"))
        {
            transform.position += transform.forward * Time.deltaTime * vert * speed;
            transform.position += transform.right * Time.deltaTime * horiz * speed;
            transform.localRotation = ClampRotationAroundXAxis(Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(-mouseY * sense, mouseX * sense, 0)));
            transform.localRotation = Quaternion.Euler(new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0));
            Cursor.lockState = CursorLockMode.Locked;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            drag = false;
        }
        if (isOpen)
        {
            if (mode == EditorMode.Object)
            {
                if (oMode == ObjectMode.Position || oMode == ObjectMode.Scale) {
                    arrows.SetActive(true);
                    rings.SetActive(false);
                }
                else
                {
                    arrows.SetActive(false);
                    rings.SetActive(true);
                }
                terrainEdit.SetActive(false);
                if (Input.GetButton("Fire2"))
                {
                    RaycastHit hit;
                    Debug.DrawRay(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).origin, GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).direction * GetComponent<Camera>().farClipPlane, Color.red);
                    if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit, GetComponent<Camera>().farClipPlane, 1 << 9))
                    {
                        Debug.Log(hit.collider.gameObject);
                        if (hit.collider.transform.GetComponent<EditableObject>() == null)
                        {
                            selection = hit.collider.transform.GetComponentInParent<EditableObject>().gameObject;
                        }
                        else
                        {
                            selection = hit.collider.gameObject;
                        }
                    }
                }
                if (Input.GetButton("Fire1") && selection != null)
                {
                    RaycastHit hit;
                    Debug.DrawRay(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).origin, GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).direction * GetComponent<Camera>().farClipPlane, Color.red);
                    if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit, GetComponent<Camera>().farClipPlane, 1 << 8))
                    {
                        if (Input.GetButtonDown("Fire1"))
                        {
                            dist = Vector3.Distance(Camera.main.GetComponent<MainCamera>().selection.transform.position, Camera.main.transform.position);
                            axis = hit.collider.GetComponent<Arrow>().axis;
                            drag = true;
                        }
                    }
                    if (drag)
                    {
                        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                        Vector3 raypoint = ray.GetPoint(dist);
                        if (axis == ArrowAxis.X)
                        {
                            Vector3 pos = new Vector3(raypoint.x, Camera.main.GetComponent<MainCamera>().selection.transform.position.y, Camera.main.GetComponent<MainCamera>().selection.transform.position.z);
                            //selection.transform.InverseTransformDirection(pos);
                            //selection.transform.position = pos;
                            if (oMode == ObjectMode.Position)
                                selection.transform.Translate((pos - selection.transform.position).x, 0, 0);
                            else if (oMode == ObjectMode.Rotation)
                                selection.transform.Rotate(-(pos - selection.transform.position).x, 0, 0);
                        }
                        if (axis == ArrowAxis.Y)
                        {
                            Vector3 pos = new Vector3(Camera.main.GetComponent<MainCamera>().selection.transform.position.x, raypoint.y, Camera.main.GetComponent<MainCamera>().selection.transform.position.z);
                            //selection.transform.InverseTransformDirection(pos);
                            //selection.transform.position = pos;
                            if (oMode == ObjectMode.Position)
                                selection.transform.Translate(0, (pos - selection.transform.position).y, 0);
                            else if (oMode == ObjectMode.Rotation)
                                selection.transform.Rotate(0, -(pos - selection.transform.position).y, 0);
                        }
                        if (axis == ArrowAxis.Z)
                        {
                            Vector3 pos = new Vector3(Camera.main.GetComponent<MainCamera>().selection.transform.position.x, Camera.main.GetComponent<MainCamera>().selection.transform.position.y, raypoint.z);
                            //selection.transform.InverseTransformDirection(pos);
                            //selection.transform.position = pos;
                            if (oMode == ObjectMode.Position)
                                selection.transform.Translate(0, 0, (pos - selection.transform.position).z);
                            else if (oMode == ObjectMode.Rotation)
                                selection.transform.Rotate(0, 0, -(pos - selection.transform.position).z);
                        }
                    }
                }
            }
            else if (mode == EditorMode.Terrain && tMode != TerrainMode.AlphaMap)
            {
                if (tMode == TerrainMode.HeightMap) {
                    height.SetActive(true);
                    water.SetActive(false);
                }
                else if (tMode == TerrainMode.WaterMap)
                {
                    height.SetActive(false);
                    water.SetActive(true);
                }
                terrainEdit.SetActive(true);
                selection = null;
                RaycastHit hit2;
                if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit2, GetComponent<Camera>().farClipPlane, 1 << 10)) {
                    terrainEdit.transform.position = hit2.point;
                }
                if (Input.GetButton("Fire2"))
                {
                    RaycastHit hit;
                    Debug.DrawRay(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).origin, GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).direction * GetComponent<Camera>().farClipPlane, Color.red);
                    if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit, GetComponent<Camera>().farClipPlane, 1 << 10))
                    {
                        if (hit.collider.GetComponent<Terrain>() != null)
                        {
                            var data = hit.collider.GetComponent<Terrain>().terrainData;
                            Vector3 pos = hit.collider.transform.InverseTransformPoint(hit.point);
                            pos.x = (int)(pos.x * 0.50f) + 1;
                            pos.z = (int)(pos.z * 0.50f) + 1;
                            Debug.Log(pos);
                            float[,] heights = data.GetHeights(0, 0, data.heightmapWidth, data.heightmapHeight);
                            for (int y = (int)Mathf.Clamp(pos.z - tRadius, 0, data.heightmapHeight); y < (int)Mathf.Clamp(pos.z + tRadius, 0, data.heightmapHeight); y++)
                            {
                                for (int x = (int)Mathf.Clamp(pos.x - tRadius, 0, data.heightmapWidth); x < (int)Mathf.Clamp(pos.x + tRadius, 0, data.heightmapWidth); x++) {
                                    Vector2Int pos2 = new Vector2Int(Mathf.Clamp(((int)x + (int)(tRadius * 1.5f)), 0, data.heightmapWidth), Mathf.Clamp(((int)y + (int)(tRadius * 1.5f)), 0, data.heightmapHeight));
                                    Debug.Log(pos2);
                                    if (tTool == TerrainTool.Raise) {
                                        heights[pos2.y, pos2.x] += Mathf.Clamp((-Vector2Int.Distance(new Vector2Int((int)pos.x, (int)pos.z), new Vector2Int(x, y)) + tRadius) * tChange * Time.deltaTime, 0, Mathf.Infinity);
                                    }
                                    else if (tTool == TerrainTool.Lower) {
                                        heights[pos2.y, pos2.x] += Mathf.Clamp(-(-Vector2Int.Distance(new Vector2Int((int)pos.x, (int)pos.z), new Vector2Int(x, y)) + tRadius) * tChange * Time.deltaTime, -Mathf.Infinity, 0);
                                    }
                                }
                            }
                            //heights[(int)pos.z, (int)pos.x] += 0.01f;
                            data.SetHeights(0, 0, heights);
                        }
                    }
                }
            }
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, -89, 89);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    public void X() {
        selection.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, (selection.transform.position.y - Camera.main.transform.position.y), (selection.transform.position.z - Camera.main.transform.position.z))).x, selection.transform.position.y, selection.transform.position.z);
    }

    public void Y()
    {
        selection.transform.position = new Vector3(selection.transform.position.x, Camera.main.ScreenToWorldPoint(new Vector3((selection.transform.position.x - Camera.main.transform.position.x), Input.mousePosition.y, (selection.transform.position.z - Camera.main.transform.position.z))).y, selection.transform.position.z);
    }

    public void Z()
    {
        selection.transform.position = new Vector3(selection.transform.position.x, selection.transform.position.y, Camera.main.ScreenToWorldPoint(new Vector3((selection.transform.position.x - Camera.main.transform.position.x), (selection.transform.position.y - Camera.main.transform.position.y), Input.mousePosition.z)).z);
    }
}
