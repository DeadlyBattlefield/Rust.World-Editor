//Credit to Facepunch and https://github.com/JasonM97

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public class WorldExample : MonoBehaviour
{
    public static Dictionary<uint, string> prefabsListUint = new Dictionary<uint, string>();
    public static Dictionary<string, uint> prefabsListString = new Dictionary<string, uint>();
    public static Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
    public static GameManifest manifest;

    public string prefabsPath;
    public GameObject terrain;
    public GameObject water;
    public GameObject empty;
    public Transform prefabs;

    private bool norustbundles = false;
    private bool loading = false;
    private bool restart = false;
    private string filename = string.Empty;
	private string result = string.Empty;
    private WorldSerialization world;
    private Terrain ter;
    private Terrain ter2;
    private TerrainData data;
    private TerrainData data2;
    private List<int> spawned = new List<int>();
    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    private List<GameObject> preSpawnedPrefabs = new List<GameObject>();
    private Vector2 scrollpos = Vector2.zero;
    private string dir;
    private string png;
    private Rect winrect = new Rect(50, 50, 200, 300);
    private Rect winrect2 = new Rect(250, 50, 200, 300);
    private int tToolSelected;
    private int modeSelected;
    private int tModeSelected;
    private int oModeSelected;

    private void Awake()
    {
        dir = Application.dataPath;
        //Debug.Log(File.Exists(Application.dataPath + "/editor.config"));
        if (File.Exists(Application.dataPath + "/editor.config"))
        {
            string[] data = File.ReadAllLines(Application.dataPath + "/editor.config");
            if (data.Length > 0)
            {
                if (File.Exists(data[0]))
                {
                    prefabsPath = data[0];
                    if (FileSystem.iface == null)
                    {
                        FileSystem.iface = new FileSystem_AssetBundles(prefabsPath);
                    }
                }
                else
                {
                    norustbundles = true;
                }
            }
            else
            {
                norustbundles = true;
            }
        }
        else
        {
            norustbundles = true;
        }
    }

    private WorldSerialization LoadWorld(string filename)
	{
		var blob = new WorldSerialization();

		blob.Load(filename);

		return blob;
	}

	private string GetInfo(WorldSerialization blob)
	{
		// Resolution of the terrain height and water maps
		var meshResolution = Mathf.NextPowerOfTwo((int)(blob.world.size * 0.50f)) + 1;

		// Resolution of the terrain splat, topology, biome and alpha maps
		var textureResolution = Mathf.NextPowerOfTwo((int)(blob.world.size * 0.50f));

		// The dimensions of the terrain object, Y always goes from -500 to +500, X and Z from -extents to +extents
		var terrainSize = new Vector3(blob.world.size, 1000, blob.world.size);

		// The position of the terrain object, chosen so world origin is always at the center of the terrain bounds
		var terrainPosition = -0.5f * terrainSize;

        /*for (int i = 0; i < blob.world.prefabs.Count; i++)
        {
            //Debug.Log(blob.world.prefabs[i].id);
            if (blob.world.prefabs[i].id == 84019840) {
                Debug.Log(blob.world.prefabs[i].id);
                Debug.Log(blob.world.prefabs[i].category);
            }
        }*/

		// Terrain mesh height values (16 bit)
		// Indexed [z, x]
		var terrainMap = new TerrainMap<short>(blob.GetMap("terrain").data, 1);

		// World height values (16 bit)
		// Indexed [z, x]
		// Used to sample the height at which to spawn grass and decor at
		// Can include both terrain and other meshes like for example cliffs
		var heightMap = new TerrainMap<short>(blob.GetMap("height").data, 1);

		// Water map (16 bit)
		// Indexed [z, x]
		// Includes both the ocean plane at zero level and any rivers
		var waterMap = new TerrainMap<short>(blob.GetMap("water").data, 1);

		// Alpha map (8 bit)
		// Indexed [z, x]
		// Zero to render parts of the terrain invisible
		var alphaMap = new TerrainMap<byte>(blob.GetMap("alpha").data, 1);

		// Splat map (8 bit, 8 channels)
		// Indexed [c, z, x] (see TerrainSplat class)
		// Sum of all channels should be normalized to 255
		var splatMap = new TerrainMap<byte>(blob.GetMap("splat").data, 8);

		// Biome map (8 bit, 4 channels)
		// Indexed [c, z, x] (see TerrainBiome class)
		// Sum of all channels should be normalized to 255
		var biomeMap = new TerrainMap<byte>(blob.GetMap("biome").data, 4);

		// Topology map (32 bit)
		// Indexed [z, x] (see TerrainTopology class)
		// Used as a bit mask, multiple topologies can be set in one location
		var topologyMap = new TerrainMap<int>(blob.GetMap("topology").data, 1);

		int x = 0;
		int z = 0;

		var sb = new StringBuilder();

		sb.AppendLine("Info");
		sb.Append("\tPosition: ");
		sb.AppendLine(terrainPosition.ToString());
		sb.Append("\tSize: ");
		sb.AppendLine(terrainSize.ToString());
		sb.Append("\tMesh Resolution: ");
		sb.AppendLine(meshResolution.ToString());
		sb.Append("\tTexture Resolution: ");
		sb.AppendLine(textureResolution.ToString());

		sb.AppendLine();

		sb.AppendLine("Terrain Map");
		sb.Append("\t");
		sb.AppendLine(BitUtility.Short2Float(terrainMap[z, x]).ToString());

		sb.AppendLine();

		sb.AppendLine("Height Map");
		sb.Append("\t");
		sb.AppendLine(BitUtility.Short2Float(heightMap[z, x]).ToString());

		sb.AppendLine();

		sb.AppendLine("Water Map");
		sb.Append("\t");
		sb.AppendLine(BitUtility.Short2Float(waterMap[z, x]).ToString());

		sb.AppendLine();

		sb.AppendLine("Alpha Map");
		sb.Append("\t");
		sb.AppendLine(BitUtility.Byte2Float(alphaMap[z, x]).ToString());

		sb.AppendLine();

		sb.AppendLine("Splat Map");
		sb.Append("\tDirt: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.DIRT_IDX, z, x]).ToString());
		sb.Append("\tSnow: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.SNOW_IDX, z, x]).ToString());
		sb.Append("\tSand: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.SAND_IDX, z, x]).ToString());
		sb.Append("\tRock: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.ROCK_IDX, z, x]).ToString());
		sb.Append("\tGrass: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.GRASS_IDX, z, x]).ToString());
		sb.Append("\tForest: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.FOREST_IDX, z, x]).ToString());
		sb.Append("\tStones: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.STONES_IDX, z, x]).ToString());
		sb.Append("\tGravel: ");
		sb.AppendLine(BitUtility.Byte2Float(splatMap[TerrainSplat.GRAVEL_IDX, z, x]).ToString());

		sb.AppendLine();

		sb.AppendLine("Biome Map");
		sb.Append("\tArid: ");
		sb.AppendLine(BitUtility.Byte2Float(biomeMap[TerrainBiome.ARID_IDX, z, x]).ToString());
		sb.Append("\tTemperate: ");
		sb.AppendLine(BitUtility.Byte2Float(biomeMap[TerrainBiome.TEMPERATE_IDX, z, x]).ToString());
		sb.Append("\tTundra: ");
		sb.AppendLine(BitUtility.Byte2Float(biomeMap[TerrainBiome.TUNDRA_IDX, z, x]).ToString());
		sb.Append("\tArctic: ");
		sb.AppendLine(BitUtility.Byte2Float(biomeMap[TerrainBiome.ARCTIC_IDX, z, x]).ToString());

		sb.AppendLine();

		sb.AppendLine("Topology Map");
		sb.Append("\tField: ");
		sb.AppendLine((topologyMap[z, x] & TerrainTopology.FIELD) != 0 ? "yes" : "no");
		sb.Append("\tBeach: ");
		sb.AppendLine((topologyMap[z, x] & TerrainTopology.BEACH) != 0 ? "yes" : "no");
		sb.Append("\tForest: ");
		sb.AppendLine((topologyMap[z, x] & TerrainTopology.FOREST) != 0 ? "yes" : "no");
		sb.Append("\tOcean: ");
		sb.AppendLine((topologyMap[z, x] & TerrainTopology.OCEAN) != 0 ? "yes" : "no");
		sb.Append("\tLake: ");
		sb.AppendLine((topologyMap[z, x] & TerrainTopology.LAKE) != 0 ? "yes" : "no");

		sb.AppendLine();
		sb.AppendLine("Paths");
		sb.Append("\t");
		sb.Append(blob.world.paths.Count);

		return sb.ToString();
	}

	protected void OnGUI()
	{
        if (norustbundles) {
            GUIStyle skin = new GUISkin().button;
            skin.wordWrap = true;
            if (GUI.Button(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)), "enter bundles path, e.g. \"<path to steamlibrary>/steamapps/common/Rust/Bundles/Bundles\" in \"<path to _Data folder on Win/Linux, Content on Mac>/editor.config\" (click for unity documentation)", skin)) Application.OpenURL("https://docs.unity3d.com/ScriptReference/Application-dataPath.html");
        }
        else
        {
            const float padding = 10;

            GUILayout.BeginArea(new Rect(padding, padding, Screen.width - padding - padding, Screen.height - padding - padding));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (world == null)
            {
                GUILayout.Label("Map File");
                filename = GUILayout.TextField(filename, GUILayout.MinWidth(100));
#if UNITY_EDITOR
                if (GUILayout.Button("Browse")) filename = UnityEditor.EditorUtility.OpenFilePanel("Select Map File", filename, "map");
#endif
                winrect = GUI.Window(0, winrect, BrowseFiles, "File Browser");
                winrect2 = GUI.Window(1, winrect2, Credits, "Credits");
                if (GUILayout.Button("Load"))
                {
                    world = LoadWorld(filename);
                    result = GetInfo(world);
                    //StartCoroutine(GetPrefabs(world));
                    SpawnMap(world);
                }
            }
            else {

            }
            if (GUILayout.Button("Save"))
            {
                SaveMap(world);
            }
            restart = GUILayout.Toggle(restart, "Restart?");
            if (GUILayout.Button("Restart") && restart) SceneManager.LoadScene("SampleScene");
            /*if (GUILayout.Button("Add Prefab 84019840 (debug)")) {
                world.AddPrefab("Decor", 84019840, new Vector3(0, 0, 0), Quaternion.Euler(Vector3.zero), Vector3.one);
                world.Save(filename + ".new");
            }*/
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            modeSelected = GUILayout.SelectionGrid(modeSelected, new string[] { "Object", "Terrain" }, 2);
            Camera.main.GetComponent<MainCamera>().mode = (MainCamera.EditorMode)modeSelected;
            GUILayout.EndHorizontal();
            if (Camera.main.GetComponent<MainCamera>().mode == MainCamera.EditorMode.Object)
            {
                GUILayout.BeginHorizontal();
                oModeSelected = GUILayout.SelectionGrid(oModeSelected, new string[] { "Position", "Rotation", "Scale" }, 3);
                Camera.main.GetComponent<MainCamera>().oMode = (MainCamera.ObjectMode)oModeSelected;
                GUILayout.EndHorizontal();
            }
            if (Camera.main.GetComponent<MainCamera>().mode == MainCamera.EditorMode.Terrain)
            {
                GUILayout.BeginHorizontal();
                tModeSelected = GUILayout.SelectionGrid(tModeSelected, new string[] { "Height Map", "Water Map", "Alpha Map" }, 3);
                Camera.main.GetComponent<MainCamera>().tMode = (MainCamera.TerrainMode)tModeSelected;
                GUILayout.EndHorizontal();
            }
            if (Camera.main.GetComponent<MainCamera>().mode == MainCamera.EditorMode.Terrain) {
                GUILayout.BeginHorizontal();
                tToolSelected = GUILayout.SelectionGrid(tToolSelected, new string[] { "Raise", "Lower", "Smooth", "Flatten" }, 4);
                Camera.main.GetComponent<MainCamera>().tTool = (MainCamera.TerrainTool)tToolSelected;
                GUILayout.EndHorizontal();
            }
            if (Camera.main.GetComponent<MainCamera>().mode == MainCamera.EditorMode.Terrain && (Camera.main.GetComponent<MainCamera>().tTool == MainCamera.TerrainTool.Raise || Camera.main.GetComponent<MainCamera>().tTool == MainCamera.TerrainTool.Lower || Camera.main.GetComponent<MainCamera>().tTool == MainCamera.TerrainTool.Smooth)) {
                GUILayout.BeginHorizontal();
                Camera.main.GetComponent<MainCamera>().tRadius = (int)GUILayout.HorizontalSlider(Camera.main.GetComponent<MainCamera>().tRadius, 1, 100, GUILayout.MinWidth(100));
                GUILayout.Label("Radius: " + Camera.main.GetComponent<MainCamera>().tRadius);
                Camera.main.GetComponent<MainCamera>().tChange = GUILayout.HorizontalSlider(Camera.main.GetComponent<MainCamera>().tChange, 0, 0.01f, GUILayout.MinWidth(100));
                GUILayout.Label("Amount of change: " + Camera.main.GetComponent<MainCamera>().tChange);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(".png file path");
                png = GUILayout.TextField(png, GUILayout.MaxWidth(100));
                if (GUILayout.Button("Load from .png")) {
                    /*if (File.Exists(png))
                        LoadFromPNG(png, data, world);
                    else if (File.Exists("\\" + png))*/
                    LoadFromPNG("\\" + png, ter, ter2, world);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (loading)
                GUILayout.Label("Loading...");
            //GUILayout.TextArea(result);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
	}

    private void Credits(int id)
    {
        if (GUI.Button(new Rect(20, 20, 160, 30), "Facepunch")) Application.OpenURL("https://github.com/Facepunch/Rust.World");
        if (GUI.Button(new Rect(20, 55, 160, 30), "VirtualBrightPlayz")) Application.OpenURL("https://steamcommunity.com/id/virtualbrightplayz/");
        if (GUI.Button(new Rect(20, 90, 160, 30), "JasonM97")) Application.OpenURL("https://github.com/JasonM97/Rust-Map-Editor");
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void BrowseFiles(int id)
    {

        GUI.Label(new Rect(0, 20, 40, 20), "Path:");
        string olddir = dir;
        dir = GUI.TextField(new Rect(40, 20, 160, 20), dir);

        if (!Directory.Exists(dir)) {
            dir = olddir;
        }

        scrollpos = GUI.BeginScrollView(new Rect(20, 40, 170, 200), scrollpos, new Rect(Vector2.zero, new Vector2(200, 200)));


        for (int i = 0; i < Directory.GetFiles(dir).Length; i++) {
            string[] file = Directory.GetFiles(dir)[i].Split(new char[] { '\\' });
            if (GUI.Button(new Rect(0, i * 35, 150, 30), file[file.Length - 1])) {
                filename = Directory.GetFiles(dir)[i];
            }
        }


        GUI.EndScrollView();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void Update()
    {
        if (world != null)
        {
            for (int i = 0; i < preSpawnedPrefabs.Count; i++)
            {
                if (preSpawnedPrefabs[i].name.ToLower().Contains("monument")) {
                    if (Vector3.Distance(preSpawnedPrefabs[i].transform.position, Camera.main.transform.position) < 100 && !spawned.Contains(i))
                    {
                        int j = preSpawnedPrefabs[i].GetComponent<PrefabData>().idInList;
                        Destroy(preSpawnedPrefabs[i]);
                        preSpawnedPrefabs.RemoveAt(i);
                        spawned.Add(i);
                        StartCoroutine(SpawnPrefab(world.world.prefabs[j], j));
                    }
                }
                else if (!preSpawnedPrefabs[i].name.ToLower().Contains("decor"))
                {
                    if (Vector3.Distance(preSpawnedPrefabs[i].transform.position, Camera.main.transform.position) < 50 && !spawned.Contains(i))
                    {
                        int j = preSpawnedPrefabs[i].GetComponent<PrefabData>().idInList;
                        Destroy(preSpawnedPrefabs[i]);
                        preSpawnedPrefabs.RemoveAt(i);
                        spawned.Add(i);
                        StartCoroutine(SpawnPrefab(world.world.prefabs[j], j));
                    }
                }
            }
        }
    }

    private IEnumerator SpawnPrefab(WorldSerialization.PrefabData prefabData, int i)
    {
        if (true) {
            //Debug.Log(prefabData.id + "-" + prefabData.category);
            FileSystem.Operation g2 = FileSystem.LoadAsync(StringPool.Get((prefabData.id)));
            /*yield return new WaitUntil(() => {
                return g2.isDone;
            });*/
            while (!g2.isDone)
            {
                yield return null;
                loading = true;
            }

            GameObject g3 = g2.Load<GameObject>();

            GameObject g = Instantiate(g3, prefabData.position, prefabData.rotation);
            g.transform.localScale = prefabData.scale;
            g.SetActive(true);
            g.layer = 9;
            string name = g.name;
            g.name = prefabData.id.ToString();
            g.AddComponent<EditableObject>();
            BoxCollider box = g.AddComponent<BoxCollider>();
            Bounds b = new Bounds(Vector3.zero, Vector3.zero);
            /*Renderer r2 = g.GetComponent<Renderer>();
            if (r2 != null)
            {
                b.Encapsulate(r2.bounds);
            }*/
            Transform[] ts = g.GetComponentsInChildren<Transform>();
            foreach (Transform t in ts)
            {
                Renderer r = t.GetComponent<Renderer>();
                BoxCollider bc = t.GetComponent<BoxCollider>();
                ParticleSystem p = t.GetComponent<ParticleSystem>();
                t.gameObject.layer = 9;
                if (r != null) {
                    //box.bounds.Encapsulate(r.bounds);
                    //Vector3.Lerp(b.center, r.center, 0.2f);
                    //r.enabled = false;
                }
                if (bc != null)
                {
                    //b.Encapsulate(bc.bounds);
                    //Vector3.Lerp(b.center, bc.center, 0.2f);
                    bc.enabled = false;
                }
                if (p != null) {
                    var em = p.emission;
                    em.enabled = false;
                }
                //box.center = b.center - g.transform.position;
                //box.size = b.size;
            }
            box.enabled = true;
            g.AddComponent<PrefabData>().idInList = i;
            g.GetComponent<PrefabData>().prefabName = name;
            g.AddComponent<LineRenderer>().material = Rainbow.material;
            g.GetComponent<LineRenderer>().useWorldSpace = false;
            Vector3[] poss = new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, -1, 0) };
            g.GetComponent<LineRenderer>().SetPositions(poss);
            spawnedPrefabs.Add(g);
            loading = false;
        }
    }

    private void LoadFromPNG(string path, Terrain ter1, Terrain ter2_1, WorldSerialization blob) {
        var data1 = ter1.terrainData;
        var data2_1 = ter2_1.terrainData;
        WWW www = new WWW("file://" + path);
        Texture2D tex = www.texture;
        data1.heightmapResolution = Mathf.NextPowerOfTwo((int)(tex.height * 0.50f)) + 1;
        data2_1.heightmapResolution = Mathf.NextPowerOfTwo((int)(tex.height * 0.50f)) + 1;
        float[,] height = new float[Mathf.NextPowerOfTwo((int)(tex.height * 0.50f)) + 1, Mathf.NextPowerOfTwo((int)(tex.height * 0.50f)) + 1];
        data1.size = new Vector3(tex.height, 1000, tex.height);
        data2_1.size = new Vector3(tex.height, 1000, tex.height);
        ter1.transform.position = -0.5f * data1.size;
        ter2_1.transform.position = -0.5f * data2_1.size;
        blob.world.size = (uint)data1.size.x;
        for (int y = 0; y < Mathf.NextPowerOfTwo((int)(tex.height * 0.50f)) + 1; y++)
        {
            for (int x = 0; x < Mathf.NextPowerOfTwo((int)(tex.height * 0.50f)) + 1; x++)
            {
                height[y, x] = tex.GetPixel(x, y).grayscale;
            }
        }
        data1.SetHeights(0, 0, height);
    }

    /*private IEnumerator GetPrefabs(WorldSerialization blob)
    {
        for (int i = 0; i < blob.world.prefabs.Count; i++) {
            yield return null;
            if ((blob.world.prefabs[i].category.ToLower().Contains("monument") || blob.world.prefabs[i].category.ToLower().Contains("river")) || blob.world.prefabs[i].category.ToLower().Contains("decor")) {
                Debug.Log(blob.world.prefabs[i].id + "-" + blob.world.prefabs[i].category);
                FileSystem.Operation g2 = FileSystem.LoadAsync(StringPool.Get((blob.world.prefabs[i].id)));
                /*yield return new WaitUntil(() => {
                    return g2.isDone;
                });*
                while (!g2.isDone) {
                    yield return null;
                }

                GameObject g3 = g2.Load<GameObject>();

                GameObject g = Instantiate(g3, blob.world.prefabs[i].position, blob.world.prefabs[i].rotation);
                g.transform.localScale = blob.world.prefabs[i].scale;
                g.SetActive(true);
                g.AddComponent<EditableObject>();
                g.AddComponent<BoxCollider>();
                yield return null;
                Debug.Log("Loaded " + i + "/" + blob.world.prefabs.Count);
            }
            else
            {
                yield return null;
                Debug.Log("Skipping " + i + "/" + blob.world.prefabs.Count);
            }
        }
    }*/

    private void SaveMap(WorldSerialization blob)
    {
        // Resolution of the terrain height and water maps
        var meshResolution = Mathf.NextPowerOfTwo((int)(blob.world.size * 0.50f)) + 1;

        // Resolution of the terrain splat, topology, biome and alpha maps
        var textureResolution = Mathf.NextPowerOfTwo((int)(blob.world.size * 0.50f));

        // The dimensions of the terrain object, Y always goes from -500 to +500, X and Z from -extents to +extents
        var terrainSize = new Vector3(blob.world.size, 1000, blob.world.size);

        // The position of the terrain object, chosen so world origin is always at the center of the terrain bounds
        var terrainPosition = -0.5f * terrainSize;

        // Terrain mesh height values (16 bit)
        // Indexed [z, x]
        var terrainMap = new TerrainMap<short>(blob.GetMap("terrain").data, 1);

        // World height values (16 bit)
        // Indexed [z, x]
        // Used to sample the height at which to spawn grass and decor at
        // Can include both terrain and other meshes like for example cliffs
        var heightMap = new TerrainMap<short>(blob.GetMap("height").data, 1);

        // Water map (16 bit)
        // Indexed [z, x]
        // Includes both the ocean plane at zero level and any rivers
        var waterMap = new TerrainMap<short>(blob.GetMap("water").data, 1);

        // Alpha map (8 bit)
        // Indexed [z, x]
        // Zero to render parts of the terrain invisible
        var alphaMap = new TerrainMap<byte>(blob.GetMap("alpha").data, 1);

        // Splat map (8 bit, 8 channels)
        // Indexed [c, z, x] (see TerrainSplat class)
        // Sum of all channels should be normalized to 255
        var splatMap = new TerrainMap<byte>(blob.GetMap("splat").data, 8);

        // Biome map (8 bit, 4 channels)
        // Indexed [c, z, x] (see TerrainBiome class)
        // Sum of all channels should be normalized to 255
        var biomeMap = new TerrainMap<byte>(blob.GetMap("biome").data, 4);

        // Topology map (32 bit)
        // Indexed [z, x] (see TerrainTopology class)
        // Used as a bit mask, multiple topologies can be set in one location
        var topologyMap = new TerrainMap<int>(blob.GetMap("topology").data, 1);

        float[,] terrainMap2 = data.GetHeights(0, 0, meshResolution, meshResolution);
        for (int y = 0; y < meshResolution; y++)
        {
            for (int x = 0; x < meshResolution; x++)
            {
                terrainMap[y, x] = BitUtility.Float2Short(terrainMap2[y, x]);
            }
        }
        int t = 0;
        for (; t < blob.world.maps.Count; t++)
        {
            if (blob.world.maps[t].name == "terrain") {
                break;
            }
        }
        blob.world.maps[t].data = terrainMap.ToByteArray();

        int wh = 0;
        for (; wh < blob.world.maps.Count; wh++)
        {
            if (blob.world.maps[wh].name == "height")
            {
                break;
            }
        }
        blob.world.maps[wh].data = terrainMap.ToByteArray();

        float[,,] splatMap2 = data.GetAlphamaps(0, 0, textureResolution, textureResolution);
        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                splatMap[0, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 0]);
                splatMap[1, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 1]);
                splatMap[2, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 2]);
                splatMap[3, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 3]);
                splatMap[4, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 4]);
                splatMap[5, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 5]);
                splatMap[6, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 6]);
                splatMap[7, y, x] = BitUtility.Float2Byte(splatMap2[y, x, 7]);
            }
        }
        int s = 0;
        for (; s < blob.world.maps.Count; s++)
        {
            if (blob.world.maps[s].name == "splat")
            {
                break;
            }
        }
        blob.world.maps[s].data = splatMap.ToByteArray();

        float[,] waterMap2 = data2.GetHeights(0, 0, meshResolution, meshResolution);
        for (int y = 0; y < meshResolution; y++)
        {
            for (int x = 0; x < meshResolution; x++)
            {
                waterMap[y, x] = BitUtility.Float2Short(waterMap2[y, x]);
            }
        }
        int w = 0;
        for (; w < blob.world.maps.Count; w++)
        {
            if (blob.world.maps[w].name == "water")
            {
                break;
            }
        }
        blob.world.maps[w].data = waterMap.ToByteArray();

        //blob.world.prefabs.Clear();
        for (int i = 0; i < blob.world.prefabs.Count; i++)
        {
            for (int j = 0; j < spawnedPrefabs.Count; j++)
            {
                if (spawnedPrefabs[j].GetComponent<PrefabData>().idInList == i) {
                    blob.world.prefabs[i].position = spawnedPrefabs[j].transform.position;
                    blob.world.prefabs[i].rotation = spawnedPrefabs[j].transform.rotation.eulerAngles;
                    blob.world.prefabs[i].scale = spawnedPrefabs[j].transform.localScale;
                }
            }
        }


        blob.Save(filename + ".new");
    }

    private void SpawnMap(WorldSerialization blob)
    {
        // Resolution of the terrain height and water maps
        var meshResolution = Mathf.NextPowerOfTwo((int)(blob.world.size * 0.50f)) + 1;

        // Resolution of the terrain splat, topology, biome and alpha maps
        var textureResolution = Mathf.NextPowerOfTwo((int)(blob.world.size * 0.50f));

        // The dimensions of the terrain object, Y always goes from -500 to +500, X and Z from -extents to +extents
        var terrainSize = new Vector3(blob.world.size, 1000, blob.world.size);

        // The position of the terrain object, chosen so world origin is always at the center of the terrain bounds
        var terrainPosition = -0.5f * terrainSize;

        // Terrain mesh height values (16 bit)
        // Indexed [z, x]
        var terrainMap = new TerrainMap<short>(blob.GetMap("terrain").data, 1);

        // World height values (16 bit)
        // Indexed [z, x]
        // Used to sample the height at which to spawn grass and decor at
        // Can include both terrain and other meshes like for example cliffs
        var heightMap = new TerrainMap<short>(blob.GetMap("height").data, 1);

        // Water map (16 bit)
        // Indexed [z, x]
        // Includes both the ocean plane at zero level and any rivers
        var waterMap = new TerrainMap<short>(blob.GetMap("water").data, 1);

        // Alpha map (8 bit)
        // Indexed [z, x]
        // Zero to render parts of the terrain invisible
        var alphaMap = new TerrainMap<byte>(blob.GetMap("alpha").data, 1);

        // Splat map (8 bit, 8 channels)
        // Indexed [c, z, x] (see TerrainSplat class)
        // Sum of all channels should be normalized to 255
        var splatMap = new TerrainMap<byte>(blob.GetMap("splat").data, 8);

        // Biome map (8 bit, 4 channels)
        // Indexed [c, z, x] (see TerrainBiome class)
        // Sum of all channels should be normalized to 255
        var biomeMap = new TerrainMap<byte>(blob.GetMap("biome").data, 4);

        // Topology map (32 bit)
        // Indexed [z, x] (see TerrainTopology class)
        // Used as a bit mask, multiple topologies can be set in one location
        var topologyMap = new TerrainMap<int>(blob.GetMap("topology").data, 1);

        GameObject obj = Instantiate(terrain);
        obj.layer = 10;
        ter = obj.GetComponent<Terrain>();
        Camera.main.GetComponent<MainCamera>().height = ter.gameObject;
        data = obj.GetComponent<Terrain>().terrainData;
        GameObject obj2 = Instantiate(water);
        obj2.layer = 10;
        ter2 = obj2.GetComponent<Terrain>();
        Camera.main.GetComponent<MainCamera>().water = ter2.gameObject;
        data2 = obj2.GetComponent<Terrain>().terrainData;
        //data.heightmapResolution = meshResolution;
        obj.transform.position = terrainPosition;
        data.heightmapResolution = (int)meshResolution;
        data.alphamapResolution = textureResolution;
        data.size = terrainSize;
        obj2.transform.position = terrainPosition;
        data2.heightmapResolution = (int)meshResolution;
        data2.alphamapResolution = textureResolution;
        data2.size = terrainSize;

        float[,] terrainMap2 = new float[(int)meshResolution, (int)meshResolution];
        for (int y = 0; y < meshResolution; y++)
        {
            for (int x = 0; x < meshResolution; x++)
            {
                terrainMap2[y, x] = BitUtility.Short2Float(terrainMap[y, x]);
            }
        }
        data.SetHeights(0, 0, terrainMap2);
        float[,,] splatMap2 = new float[(int)textureResolution, (int)textureResolution, 8];
        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                splatMap2[y, x, 0] = BitUtility.Byte2Float(splatMap[0, y, x]);
                splatMap2[y, x, 1] = BitUtility.Byte2Float(splatMap[1, y, x]);
                splatMap2[y, x, 2] = BitUtility.Byte2Float(splatMap[2, y, x]);
                splatMap2[y, x, 3] = BitUtility.Byte2Float(splatMap[3, y, x]);
                splatMap2[y, x, 4] = BitUtility.Byte2Float(splatMap[4, y, x]);
                splatMap2[y, x, 5] = BitUtility.Byte2Float(splatMap[5, y, x]);
                splatMap2[y, x, 6] = BitUtility.Byte2Float(splatMap[6, y, x]);
                splatMap2[y, x, 7] = BitUtility.Byte2Float(splatMap[7, y, x]);
            }
        }
        data.SetAlphamaps(0, 0, splatMap2);
        float[,] waterMap2 = new float[(int)meshResolution, (int)meshResolution];
        for (int y = 0; y < meshResolution; y++)
        {
            for (int x = 0; x < meshResolution; x++)
            {
                waterMap2[y, x] = BitUtility.Short2Float(waterMap[y, x]);
            }
        }
        data2.SetHeights(0, 0, waterMap2);
        float[,,] waterMap3 = new float[(int)textureResolution, (int)textureResolution, 1];
        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                waterMap3[y, x, 0] = 1;
            }
        }
        data2.SetAlphamaps(0, 0, waterMap3);

        for (int i = 0; i < blob.world.prefabs.Count; i++) {
            var prefab = blob.world.prefabs[i];
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.position = prefab.position;
            g.transform.rotation = prefab.rotation;
            g.transform.localScale = prefab.scale;
            g.name = prefab.category.ToString();
            g.layer = 9;
            g.AddComponent<EditableObject>();
            g.AddComponent<LineRenderer>().material = Rainbow.material;
            g.GetComponent<LineRenderer>().useWorldSpace = false;
            string[] pool = StringPool.Get(prefab.id).Split('/');
            g.AddComponent<PrefabData>().prefabName = pool[pool.Length - 1];
            g.GetComponent<PrefabData>().idInList = i;
            Vector3[] poss = new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, -1, 0) };
            g.GetComponent<LineRenderer>().SetPositions(poss);
            preSpawnedPrefabs.Add(g);
        }

        Camera.main.GetComponent<MainCamera>().isOpen = true;

        //StartCoroutine(GetPrefabs(blob));

        /*foreach (var prefab in blob.world.prefabs)
        {
            GameObject obj3 = Instantiate((GameObject)bundles[prefabsListUint[prefab.id]].mainAsset, prefabs);
            obj3.transform.name = bundles[prefabsListUint[prefab.id]].name + " - " + prefab.id + " - " + prefab.category;
            obj3.transform.position = prefab.position;
            obj3.transform.rotation = prefab.rotation;
            obj3.transform.localScale = prefab.scale;
        }*/
    }
}
