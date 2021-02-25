using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{
    #pragma warning disable 0649 
    [Header("Map size and scale")]
    [SerializeField] int width = 32;
    [SerializeField] int height = 32;
    [SerializeField] float tileScale = 5f;
    [Header("Noise parameters")]
    [SerializeField] int seed = 0;
    [SerializeField] bool useRandomSeed = true;
    [SerializeField] float noiseScale = 5f;
    [Tooltip("Controls how high the height points are")]
    [SerializeField] float startingAmplitude = 1f;
    [Tooltip("Controls how often the high points occur")]
    [SerializeField] float startingFrequency = 1f;
    [Tooltip("Controls the decrease in amplitude of octaves")]
    [SerializeField] float persistance = 5f;
    [Tooltip("Controls the increase in frequency of octaves")]
    [SerializeField] float lacunarity = 5f;
    [Tooltip("How many noise passes with decreasingly effect to use.")]
    [SerializeField] int octaves = 3;
    [Header("Image map to influence the noise")]
    [SerializeField] bool useGradient = true;
    public Texture2D circleGradient;
    [Header("")]
    [SerializeField] Material meshMaterial;
    [SerializeField] Vector2 offset = Vector2.zero;
    [Header("Terrain Colors")]
    [SerializeField] TerrainColors[] colors;
    [SerializeField] float snowCutoff = 0.9f;
    [SerializeField] float mountainCutoff = 0.6f;
    [SerializeField] float forestCutoff = 0.5f;
    [SerializeField] float grassCutoff = 0.4f;
    Dictionary<ColorNames, Color> colorDict;
    [SerializeField] bool makeWater = false;
    #pragma warning restore 0649
    GameObject terrain;
    MeshFilter meshFilter;
    Mesh mesh;

    List<Vector3> verts = new List<Vector3>();
    List<Vector3> targetVertPositions = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector3> norms = new List<Vector3>();
    bool animate = true;
    

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        if (!makeWater)
            DestroyOldTerrains();
        CreateTerrain();

        if (animate)
            StartCoroutine(LerpNoiseScale());
    }

    public void DestroyOldTerrains() {
        foreach(GameObject t in GameObject.FindGameObjectsWithTag("Terrain")) {
            DestroyImmediate(t);
        }
    }

    public void CreateTerrain()
    {
        SetUp();

        CreateMesh();
        if(!makeWater)
            CreateTexture();

    }

    public void UpdateMesh() {

        verts = new List<Vector3>();
        tris = new List<int>();
        norms = new List<Vector3>();
        
        CreateMesh();
        CreateTexture();

       
    }

    public IEnumerator LerpNoiseScale() {
        float waitTime = 0.1f;
        float lerpIncrement = 0.1f;
        WaitForSeconds wait = new WaitForSeconds(waitTime);
        float targetNoise = 7.0f;
        noiseScale = 1f;
        while(noiseScale < targetNoise) {
            noiseScale += lerpIncrement;
            UpdateMesh();
            
            yield return wait;

        }
    }

    void CreateTexture()
    {
        int meshWidth = (int)width * (int)tileScale;
        int meshHeight = (int)height * (int)tileScale;

        Texture2D texture = new Texture2D(width, height);
   
        foreach (Vector3 vertex in mesh.vertices)
        {
            AssignColor(texture, vertex);

        }
        texture.Apply();

        terrain.GetComponent<Renderer>().sharedMaterial.SetTexture("_TerrainTex", texture);
        CreateUV(meshWidth, meshHeight);
    }

    private void AssignColor(Texture2D texture, Vector3 vertex)
    {
        float normalizedY = Mathf.InverseLerp(mesh.bounds.min.y, mesh.bounds.max.y, vertex.y);
        float randomValue = Random.Range(-0.03f, 0.03f);
        if(snowCutoff + randomValue < normalizedY)
        {
            texture.SetPixel((int)vertex.x / (int)tileScale, (int)vertex.z / (int)tileScale, colorDict[ColorNames.Snow]);
        }
        else if (mountainCutoff + randomValue < normalizedY)
        {
            texture.SetPixel((int)vertex.x / (int)tileScale, (int)vertex.z / (int)tileScale, colorDict[ColorNames.Mountain]);
        }
        else if (forestCutoff + randomValue < normalizedY)
        {
            texture.SetPixel((int)vertex.x / (int)tileScale, (int)vertex.z / (int)tileScale, colorDict[ColorNames.Forest]);
        }
        else if (grassCutoff + randomValue < normalizedY)
        {
            texture.SetPixel((int)vertex.x / (int)tileScale, (int)vertex.z / (int)tileScale, colorDict[ColorNames.Grass]);
        }
        else 
        {
            texture.SetPixel((int)vertex.x / (int)tileScale, (int)vertex.z / (int)tileScale, colorDict[ColorNames.Sand]);
        }
       
    }

    private void CreateUV(int meshWidth, int meshHeight)
    {
        Vector2[] uvs = new Vector2[verts.Count];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(verts[i].x / meshWidth, verts[i].z / meshHeight);
        }

        mesh.uv = uvs;
    }

    private void CreateMesh()
    {
        Vector2[] octavesOffset = CreateOctaveOffsets();

        CreateVertices(octavesOffset);

        CreateTriangles();

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        MeshCollider mc = terrain.AddComponent<MeshCollider>();
    }

    private void CreateVertices(Vector2[] octavesOffset)
    {
        if (useRandomSeed)
            seed = UnityEngine.Random.Range(-100000, 100000);
        for (int row = 0; row < width; row++)
        {
            for (int col = 0; col < height; col++)
            {
                
                float noiseHeight = 0;

                float frequency = startingFrequency;
                float amplitude = startingAmplitude;

                for (int o = 0; o < octaves; o++)
                {
                    float nx = row / noiseScale * frequency + octavesOffset[o].x;
                    float nz = col / noiseScale * frequency + octavesOffset[o].y;

                    float ny = Noise.GetNoise(seed, nx, nz);
                    float gradientValue = GetPixelValue(circleGradient, row, col);
                    if(useGradient)
                        noiseHeight += (ny + gradientValue) * amplitude; 
                    else
                        noiseHeight += ny * amplitude;


                    amplitude *= persistance;
                    frequency *= lacunarity;

                }
                verts.Add(new Vector3(row * tileScale, (noiseHeight * noiseScale) * tileScale, col * tileScale));
            }
        }
    }

    private Vector2[] CreateOctaveOffsets()
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        Vector2[] octavesOffset = new Vector2[octaves];
        Random.InitState(seed);
        for (int i = 0; i < octaves; i++)
        {
            float offsetX;
            float offsetY;
            // each octave offset get's it's own noise to make it look more interesting
            if (useRandomSeed)
            {
                offsetX = Random.Range(-100000, 100000) + offset.x / halfWidth;
                offsetY = Random.Range(-100000, 100000) + offset.y / halfHeight;
            } else
            {
                offsetX = seed + offset.x / halfWidth;
                offsetY = seed + offset.y / halfHeight;
            }
            octavesOffset[i] = new Vector2(offsetX, offsetY);
        }

        return octavesOffset;
    }

    private void CreateTriangles()
    {
        // Populate triangles list two triangles at a time.
        for (int row = 0; row < width - 1; row++)
        {
            for (int col = 0; col < height - 1; col++)
            {
                tris.Add(height * row + col);
                tris.Add(height * row + col + 1);
                tris.Add(height * (row + 1) + col);

                tris.Add(height * (row + 1) + col);
                tris.Add(height * row + col + 1);
                tris.Add(height * (row + 1) + col + 1);
            }
        }
    }

    public float GetPixelValue(Texture2D tex, int x, int y)
    {
        float percentX = (float) x / width;
        float percentY = (float) y / height;

        int textureX = Convert.ToInt32(tex.width * percentX);
        int textureY = Convert.ToInt32(tex.height * percentY);

        Color pixelColor = tex.GetPixel(textureX, textureY);


        float pixelValue = pixelColor.r + pixelColor.g + pixelColor.b + pixelColor.a;
        float maxPixelValue = 4f;
        float greyscale = (pixelValue / maxPixelValue) * 2 -1;
        return -1f * greyscale;
    }

    private void SetUp()
    {

        colorDict = new Dictionary<ColorNames, Color>();
        foreach(TerrainColors c in colors)
        {
            colorDict.Add(c.name, c.color);
        }

        verts = new List<Vector3>();
        tris = new List<int>();
        norms = new List<Vector3>();

        if(makeWater) {
            // terrain.GetComponent<MeshFilter>();
            terrain = new GameObject("Water");
        } else {
            terrain = new GameObject("terrain");
            terrain.tag = "Terrain";
            terrain.layer = 8;
        }
        terrain.AddComponent<MeshRenderer>();
        MeshRenderer renderer = terrain.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = meshMaterial;
        meshFilter = terrain.AddComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

    }

    public void OnDrawGizmos()
    {
        //if (verts.Count > 0)
        //{
        //    foreach (Vector3 vert in verts)
        //    {
        //        //Gizmos.DrawSphere(vert, 0.1f);
        //        int pos = (int)verts.IndexOf(vert);
        //        String vectorString = $"{vert.x}, {vert.z}, {pos}";
        //        drawString($"{pos}", vert);
        //    }
        //}

        //if (tris.Count > 0)
        //{
        //    foreach (int i in tris)
        //    {
        //        drawString($"tri: {i}", verts[i]);
        //    }
        //}

    }

    //static void drawString(string text, Vector3 worldPos, Color? colour = null)
    //{
    //    UnityEditor.Handles.BeginGUI();
    //    if (colour.HasValue) GUI.color = colour.Value;
    //    var view = UnityEditor.SceneView.currentDrawingSceneView;
    //    Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
    //    Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
    //    GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
    //    UnityEditor.Handles.EndGUI();
    //}
}
