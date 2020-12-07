using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour {

    public static Mesh mesh;
    MeshCollider meshCollider;
    Vector3[] vertices;
    int[] triangles; 
    Color[] colours;
    Texture[] textures;
    float offSetX;
    float offSetZ;

    public NoiseData noiseData;
    public TerrainData terrainData;

    [SerializeField] private TerrainType[] heightTerrainTypes;
    [SerializeField] private TerrainType[] heatTerrainTypes;
    [SerializeField] private TerrainType[] moistureTerrainTypes;
    [SerializeField] private VisualizationMode visualizationMode;
    enum VisualizationMode {Shaded, Heat, Moisture}

    public void Startup(int LODIndex) {
        int resolution = terrainData.resolutionLevels[LODIndex].resolution;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        float maxValue = CalculateMaxAndMinValues();
        float distanceFromZero = GenerateTerrain(maxValue, resolution);
        transform.position = new Vector3 ( transform.position.x, -distanceFromZero, transform.position.z);
        UpdateMesh();
    }

    public float GenerateTerrain(float maxValue, int resolution) {
        int resolutionDevisionNum = (resolution ==0)?1:resolution*2;
        Color[] colours;
        Vector3[] vertices = GenerateVertices(resolutionDevisionNum);
        int[] triangles    = GenerateTriangles(resolutionDevisionNum);
        switch (this.visualizationMode) {
            case VisualizationMode.Shaded:
                colours = GenerateColours(vertices, resolutionDevisionNum, maxValue, this.heightTerrainTypes);
                break;
            case VisualizationMode.Moisture:
                colours = GenerateColours(vertices, resolutionDevisionNum, maxValue, this.moistureTerrainTypes);
                break;
        }

        // This is only a prediction and a poor one at that because the heighest value won't be close to the max possible value attainable.
        float distanceFromZero = ((maxValue) / 8); 
        return distanceFromZero; 
    }

    public Vector3[] GenerateVertices(int resolutionDevisionNum) {
        vertices = new Vector3[((terrainData.chunkSize / resolutionDevisionNum)+1) * ((terrainData.chunkSize / resolutionDevisionNum)+1)];
        float y = 1;

        for (int i = 0, z = 0; z <= terrainData.chunkSize; z += resolutionDevisionNum) {
            for (int x = 0; x <= terrainData.chunkSize; x += resolutionDevisionNum) {
                
                float newAmplitude = noiseData.amplitude;
                float newFrequency = noiseData.frequency;
                float normalization = 0;
                int newSeed = noiseData.seed;

                float offSetX = transform.position.x * noiseData.frequency;
                float offSetZ = transform.position.z * noiseData.frequency;

                float landMassOffSetX = transform.position.x * noiseData.landMassFrequency;
                float landMassOffSetZ = transform.position.z * noiseData.landMassFrequency;

                for (int o = 1; o <= noiseData.octaves; o++, newSeed += 500, newFrequency *= noiseData.lacinarity, newAmplitude *= noiseData.persistance) {

                    if (o == 1) {
                        y = Mathf.PerlinNoise(x  * newFrequency + (newSeed - (-offSetX)), z * newFrequency + (newSeed - (-offSetZ)));
                        y = terrainData.meshHeightCurve.Evaluate(y);
                        y = y * newAmplitude;
                    }
                    else {
                        offSetX *= noiseData.lacinarity;
                        offSetZ *= noiseData.lacinarity;
                        float newY = Mathf.PerlinNoise(x * newFrequency + (newSeed - (-offSetX)), z * newFrequency + (newSeed - (-offSetZ)));
                        newY = terrainData.meshHeightCurve.Evaluate(newY);
                        newY = newY * newAmplitude;
                        y += newY;
                    }
                };
                // Continent Script 
                float continentValue = Mathf.PerlinNoise(x * noiseData.landMassFrequency + (newSeed - (-landMassOffSetX)), z * noiseData.landMassFrequency + (newSeed - (-landMassOffSetZ)));
                continentValue = terrainData.landMassHeightCurve.Evaluate(continentValue);
                continentValue = continentValue * 2 -1; // Centering around Zero.
                continentValue = continentValue * noiseData.landMassAmplitude;
                y += continentValue;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        };
        return vertices;
    }

    public int[] GenerateTriangles(int resolutionDevisionNum) {
        triangles = new int[(terrainData.chunkSize / resolutionDevisionNum) * (terrainData.chunkSize / resolutionDevisionNum) * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < terrainData.chunkSize; z += resolutionDevisionNum) {
            for (int x = 0; x < terrainData.chunkSize; x += resolutionDevisionNum) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + (terrainData.chunkSize / resolutionDevisionNum) + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + (terrainData.chunkSize / resolutionDevisionNum) + 1;
                triangles[tris + 5] = vert + (terrainData.chunkSize / resolutionDevisionNum) + 2;
                vert++;
                tris += 6;
            }
            vert++;
        };
        return triangles;
    }

    public Color[] GenerateColours(Vector3[] vertices, int resolutionDevisionNum, float maxValue, TerrainType[] terrainTypes) {
        colours = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= terrainData.chunkSize; z += resolutionDevisionNum) {
            for (int x = 0; x <= terrainData.chunkSize; x += resolutionDevisionNum) {
                float height = Mathf.InverseLerp(0, maxValue, vertices[i].y);
                TerrainType terrainType = ChooseTerrainType (height, terrainTypes);
                colours[i] = terrainData.gradient.Evaluate(height);
                i++;
            }
        };
        return colours;
    }
	
    TerrainType ChooseTerrainType(float height, TerrainType[] terrainTypes) {
        foreach (TerrainType terrainType in terrainTypes) {
            if (height < terrainType.threshold) {
                return terrainType;
            }
        }
        return terrainTypes [terrainTypes.Length - 1];
    }

    float CalculateMaxAndMinValues() {
        float x = 1;
        float newX = 1;

        // Can't need to work out away of predicting maxHeight value
        float maxValue = 800f;
        return maxValue;
    }
    
    Vector3[] CalculateNormals() {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        
        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles [normalTriangleIndex];
            int vertexIndexB = triangles [normalTriangleIndex + 1];
            int vertexIndexC = triangles [normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }


    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = vertices [indexA];
        Vector3 pointB = vertices [indexB];
        Vector3 pointC = vertices [indexC];

        // To Calculate the surface normal from these point I will use the cross product.
        
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colours;
        // mesh.normals = CalculateNormals(); 
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}

[System.Serializable]
public class TerrainType {
	public string name;
	public Color colour;
    public Texture texture;
	public float threshold;
	public int index;
}