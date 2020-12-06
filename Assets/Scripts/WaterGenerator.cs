using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterGenerator : MonoBehaviour {
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    MeshCollider meshCollider;
 
    public int xSize;
    public int zSize;
    public float frequency = 0.3f;
    public float amplitude = 0.5f;
    public int octaves = 1;
    public float lacinarity;
    public float persistance;

    public Gradient gradient;
    public AnimationCurve meshHeightCurve;
    public float water;
    float minTerrianHeight = 1000;
    float maxTerrianHeight = -1000;

    // Start is called before the first frame update
    void Start() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GenerateTerrain();
        UpdateMesh();
    }

    void GenerateTerrain() {
        vertices = new Vector3[(xSize+1) * (zSize+1)];
        float y = 1;

            for (int i = 0, z = 0; z <= zSize; z++) {
                for (int x = 0; x <= xSize; x++) {
                    
                        y = (vertices[i][1]);
                        y += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
                        
                            if (y > maxTerrianHeight) {
                                maxTerrianHeight = y;
                            }

                            if (y < minTerrianHeight) {
                                minTerrianHeight = y;
                            }
                            water = (maxTerrianHeight - minTerrianHeight) * 0.15f;
                        
                    
                    vertices[i] = new Vector3(x, y, z);
                    i++;
                };
            };

		triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++) {
            for (int x = 0; x < xSize; x++) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        };
        colours = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++) {
            for (int x = 0; x <= xSize; x++) {
                float height = Mathf.InverseLerp(minTerrianHeight, maxTerrianHeight, vertices[i].y);
                colours[i] = gradient.Evaluate(height);
                i++;
            }
        };
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colours;
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
