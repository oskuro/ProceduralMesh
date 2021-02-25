using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Noise;

public class TerrainPlacer : MonoBehaviour
{
    #pragma warning disable 0649 
    [SerializeField] GameObject rockPrefab;
    [SerializeField] GameObject treePrefab;
    #pragma warning restore 0649 
    int stepIncrement = 1;
    float rockCutoff = 0.85f;
    float treeCutoff = 0.75f;
    int mapsize = 10;
    void Start()
    {
        PlaceGeometry();
    }

    // Update is called once per frame
    void Update()
    {
            
    }

    

    private void PlaceGeometry() {
        float seed = Random.Range(-100000, 100000);

        GameObject rockParent = new GameObject("rocks");
        GameObject treeParent = new GameObject("trees");
        int infinityCount = 0;

        for (int x = 0; x < mapsize; x+=stepIncrement)
        {
            for (int z = 0; z < mapsize; z+=stepIncrement) {
                float y = Noise.GetNoise(seed, x, z);
                if (y > rockCutoff)
                {
                    PlaceGeometry(rockParent, rockPrefab, x, z);
                } else if(y > treeCutoff)
                {
                    PlaceGeometry(treeParent, treePrefab, x, z);
                }


                infinityCount++;
                if (infinityCount > 10000)
                {
                    Debug.Log("Pretty sure this is an infinityloop, breaking out.");
                    break;
                }
            }
        }
    }

    private void PlaceGeometry(GameObject parent, GameObject prefab, int x, int z)
    {
        if (rockPrefab == null) { return; }

        float scale = (float)Random.Range(0.1f, 1.3f);
        Quaternion geometryRotation = Quaternion.Euler(0, Random.Range(0, 180), 0);
        GameObject go = Instantiate(prefab, new Vector3(x, Mathf.Abs(scale), z), geometryRotation, parent.transform);
        go.transform.localScale = go.transform.localScale * scale;
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null) { rb.mass = scale; };
    }

}
