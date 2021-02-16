using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour {

    // public float radius;
	// public Vector2 regionSize = Vector2.one;
	// public int rejectionSamples;
	// public float displayRadius;

	List<Vector2> points;

    public bool foliage;

    Biome[] heatType;
    Biome biomeType;

    public static Mesh mesh;
    public Maps maps;
    MeshCollider meshCollider;
    Vector3[] heightMap;
    Vector3[] heatMap;
    Vector3[] moistureMap;
    // Vector3[] Maps;
    int[] triangles; 
    Color[] colours;
    Texture[] textures;
    float offSetX;
    float offSetZ;

    public NoiseData noiseData;
    public TerrainData terrainData;
    public TreeData treeData;

    [SerializeField] private TerrainType[] heightTerrainTypes;
    [SerializeField] private TerrainType[] heatTerrainTypes;
    [SerializeField] private TerrainType[] moistureTerrainTypes;
    // [SerializeField] private TerrainType[] biomeTerrainTypes;

    public void Startup(int LODIndex, string chunkName) {
        int resolution = terrainData.resolutionLevels[LODIndex].resolution;
        int resolutionDevisionNum = (resolution == 1)?1:resolution*2;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        float maxValue = CalculateMaxAndMinValues();
        float distanceFromZero = GenerateTerrain(maxValue, resolutionDevisionNum);
        transform.position = new Vector3 ( transform.position.x, -distanceFromZero, transform.position.z);
        if (foliage) {
            points = TreeGeneration.GeneratePoints(treeData.radius, treeData.regionSize, treeData.rejectionSamples);
            FindNearestPoints(points, maxValue);
        }
        UpdateMesh();
    }
    public void FindNearestPoints(List<Vector2> points, float maxValue) {
        if (transform.childCount == 0) { // If the chunk has already got tree on it don't try and spawn more.
            if (points != null) {
                foreach (Vector2 point_ in points) {
                    
                    // Point of object.
                    Vector3 p = new Vector3(point_.x, 0, point_.y);
                    Vector3 i = new Vector3(300,300,300); 
                    Vector3 j = new Vector3(300,300,300);
                    Vector3 k = new Vector3(300,300,300); 
                    float dst1 = 300;
                    float dst2 = 300;
                    float dst3 = 300;

                    float newDst;
                    foreach (Vector3 point in heightMap) {
                        newDst = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(point.x, point.z));
                        newDst *= newDst;
                        if (newDst < dst1) {
                            dst1 = newDst;
                            i = point;
                        }
                    }
                    foreach (Vector3 point in heightMap) {
                        if (point != i) {
                            newDst = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(point.x, point.z));
                            newDst *= newDst;
                            if (newDst >= dst1 && newDst < dst2) {
                                dst2 = newDst;
                                j = point;
                            }
                        }
                    }
                    foreach (Vector3 point in heightMap) {
                        if (point != i && point != j) {
                            newDst = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(point.x, point.z));
                            newDst *= newDst;
                            if (newDst >= dst2 && newDst < dst3) {
                                dst3 = newDst;
                                k = point;
                            }
                        }
                    }
                    float treeDensity = FindBiome(i, heatMap, moistureMap, biomes);
                    float x = UnityEngine.Random.Range(0f,1f);
                    if (x < treeDensity) {
                        p = FindY(i, j, k, p);
                        if (p.y > maxValue / 8) { // If the tree will be under water then don't place it.  
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.parent = transform;
                            cube.transform.localScale = new Vector3(treeData.displayRadius, treeData.displayRadius, treeData.displayRadius);
                            cube.transform.Translate(transform.position + p * terrainData.scale);
                        }
                    }
                }
            }
        }
    }

    public float FindBiome(Vector3 point, Vector3[] heatMap, Vector3[] moistureMap, BiomeRow[] biomes) {
        int i = 0;
        float treeDensity;
        Color biomeColor = new Color(0f, 0f, 0f);
        for(i = 0; i < heightMap.Length; i ++) {
            if (heightMap[i] == point) {
                break;
            }
        }
        colours = new Color[heatMap.Length];
        float heat = Mathf.InverseLerp(0, 1, heatMap[i].y);
        float moisture = Mathf.InverseLerp(0, 1, moistureMap[i].y);  
        // BiomeRow[] heatType = ChooseBiomeType (heat, biomes);
        // Biome biomeType = ChooseBiomeType (moisture, heatType);
        foreach (BiomeRow biomeRow in biomes) {
            if (heat < biomeRow.threshold) {   
                heatType = biomeRow.biomes;
                break;
            } else {
                heatType = biomes [biomes.Length - 1].biomes;
            }
        }
        foreach (Biome biome in heatType) {
            if (moisture < biome.threshold) {   
                biomeType = biome;
                break;
            } else {
                heatType = biomes [biomes.Length - 1].biomes;
            }
        }
        treeDensity = biomeType.treeDensity;
        return treeDensity;
    }

    public Vector3 FindY(Vector3 i, Vector3 j, Vector3 k, Vector3 p) {
        Vector3 L1 = j - i;
        Vector3 L2 = k - j;
        float m;
        float n; 
        if ( L1.x == 0f) { // if l1.x == 0 
            n = (p.x - i.x) / L2.x;
            m = (p.z - i.z - n*(L2.z)) / L1.z;
        }
        else if ( L2.x == 0f) { // if l2.x == 0
            m = (p.x - i.x) / L1.x;
            n = (p.z - i.z - m*(L1.z)) / L2.z; 
        }
        else if ( L1.z == 0f) {
            n = (p.z - i.z) / L2.z;
            m = (p.x - i.x - n*(L2.x)) / L1.x;
        }
        else if (L2.z == 0f) {
            m = (p.z - i.z) / L1.z;
            n = (p.x - i.x - m*(L1.x)) / L2.x;
        }
        else { // l1.x, l2.x, l1.z, 12.z != 0
            n = (p.x * L1.z - p.z * L1.x + i.z * L1.x - i.x * L1.z) / (L1.z * L2.x - L1.x * L2.z);
            m = (p.x - i.x - n*(L2.x)) / L1.x;
        }
        // Debug.Log(i + m*(L1) + n*(L2));
        return i + m*(L1) + n*(L2); // Vector of p.
    }

    public float GenerateTerrain(float maxValue, int resolutionDevisionNum) {
        Maps maps = GenerateheightMap(resolutionDevisionNum);
        int[] triangles = GenerateTriangles(resolutionDevisionNum);
        switch (this.visualizationMode) {
            case VisualizationMode.Shaded:
                colours = GenerateColours(maps.heightMap, resolutionDevisionNum, maxValue, this.heightTerrainTypes);
                break;
            case VisualizationMode.Heat:
                colours = GenerateColours(maps.heatMap, resolutionDevisionNum, maxValue, this.heatTerrainTypes);
                break;
            case VisualizationMode.Moisture:
                colours = GenerateColours(maps.moistureMap, resolutionDevisionNum, maxValue, this.moistureTerrainTypes);
                break;
            case VisualizationMode.Biomes:
                colours = GenerateBiomes(maps.heatMap, maps.moistureMap, resolutionDevisionNum, maxValue, this.biomes);
                break;
        }
        // This is only a prediction and a poor one at that because the heighest value won't be close to the max possible value attainable.
        float distanceFromZero = ((maxValue) / 8); 
        return distanceFromZero; 
    }

    public Maps GenerateheightMap(int resolutionDevisionNum) {
        heightMap = new Vector3[((terrainData.chunkSize / resolutionDevisionNum)+1) * ((terrainData.chunkSize / resolutionDevisionNum)+1)];
        heatMap = new Vector3[((terrainData.chunkSize / resolutionDevisionNum)+1) * ((terrainData.chunkSize / resolutionDevisionNum)+1)];
        moistureMap = new Vector3[((terrainData.chunkSize / resolutionDevisionNum)+1) * ((terrainData.chunkSize / resolutionDevisionNum)+1)];

        float heightMapValue = 1;

        for (int i = 0, z = 0; z <= terrainData.chunkSize; z += resolutionDevisionNum) {
            for (int x = 0; x <= terrainData.chunkSize; x += resolutionDevisionNum) {
                
                float newAmplitude = noiseData.amplitude;
                float newFrequency = noiseData.frequency;
                float normalization = 0;
                int newSeed = noiseData.seed;
                int newLandMassSeed = noiseData.seedLandMass;

                float offSetX = (transform.position.x * noiseData.frequency) / terrainData.scale;
                float offSetZ = (transform.position.z * noiseData.frequency) / terrainData.scale;

                float landMassOffSetX = (transform.position.x * noiseData.landMassFrequency) / terrainData.scale;
                float landMassOffSetZ = (transform.position.z * noiseData.landMassFrequency) / terrainData.scale;

                float heatMapOffSetX = (transform.position.x * noiseData.heatMapFrequency) / terrainData.scale;
                float heatMapOffSetZ = (transform.position.z * noiseData.heatMapFrequency) / terrainData.scale;

                float moistureMapOffSetX = (transform.position.x * noiseData.moistureMapFrequency) / terrainData.scale;
                float moistureMapOffSetZ = (transform.position.z * noiseData.moistureMapFrequency) / terrainData.scale;

                for (int o = 1; o <= noiseData.octaves; o++, newSeed += 500, newLandMassSeed += 500, newFrequency *= noiseData.lacinarity, newAmplitude *= noiseData.persistance) {
                    if (o == 1) {
                        heightMapValue = Mathf.PerlinNoise(x  * newFrequency + (newSeed + offSetX), z * newFrequency + (newSeed + offSetZ));
                        heightMapValue = terrainData.meshHeightCurve.Evaluate(heightMapValue);
                        heightMapValue = (heightMapValue * newAmplitude);
                    }
                    else {
                        offSetX *= noiseData.lacinarity;
                        offSetZ *= noiseData.lacinarity;
                        float newHeightMapValue = Mathf.PerlinNoise(x * newFrequency + (newSeed - (-offSetX)), z * newFrequency + (newSeed - (-offSetZ)));
                        newHeightMapValue = terrainData.meshHeightCurve.Evaluate(newHeightMapValue);
                        newHeightMapValue = newHeightMapValue * newAmplitude;
                        heightMapValue += newHeightMapValue;
                    }
                };
                // Continent Script 
                float continentValue = Mathf.PerlinNoise(x * noiseData.landMassFrequency + (newLandMassSeed - (-landMassOffSetX)), z * noiseData.landMassFrequency + (newLandMassSeed - (-landMassOffSetZ)));
                continentValue = terrainData.landMassHeightCurve.Evaluate(continentValue);
                continentValue = continentValue * 2 -1; // Centering around Zero.
                continentValue = continentValue * noiseData.landMassAmplitude;
                heightMapValue += continentValue;
                
                heightMap[i] = new Vector3(x, heightMapValue, z);
                
                // HeatMap Generation
                float heatMapValue = Mathf.PerlinNoise(x * noiseData.heatMapFrequency + (noiseData.seedHeat - (-heatMapOffSetX)), z * noiseData.heatMapFrequency + (noiseData.seedHeat - (-heatMapOffSetZ)));
                heatMap[i] = new Vector3(x, heatMapValue, z);
            
                // MoistureMap Generation
                float moistureMapValue = Mathf.PerlinNoise(x * noiseData.moistureMapFrequency + (noiseData.seedMoisture - (-moistureMapOffSetX)), z * noiseData.moistureMapFrequency + (noiseData.seedMoisture - (-moistureMapOffSetZ)));
                moistureMap[i] = new Vector3(x, moistureMapValue, z);

                i++;
            }
        };
        maps.heightMap = heightMap;
        maps.heatMap = heatMap;
        maps.moistureMap = moistureMap;
        return maps;
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

    public Color[] GenerateColours(Vector3[] map, int resolutionDevisionNum, float maxValue, TerrainType[] terrainTypes) {
        colours = new Color[map.Length];
        float height;
        for (int i = 0, z = 0; z <= terrainData.chunkSize; z += resolutionDevisionNum) {
            for (int x = 0; x <= terrainData.chunkSize; x += resolutionDevisionNum) {
                if (this.visualizationMode == VisualizationMode.Shaded) {
                    height = Mathf.InverseLerp(0, maxValue, map[i].y);
                } else {
                    height = Mathf.InverseLerp(0, 1, map[i].y); 
                }
                TerrainType terrainType = ChooseTerrainType (height, terrainTypes);
                colours[i] = terrainType.colour;
                i++;
            }
        };
        return colours;
    }
	
    public Color[] GenerateBiomes(Vector3[] heatMap, Vector3[] moistureMap,int resolutionDevisionNum,  float maxValue, BiomeRow[] biomes) {
        colours = new Color[heatMap.Length];
        for (int i = 0, z = 0; z <= terrainData.chunkSize; z += resolutionDevisionNum) {
            for (int x = 0; x <= terrainData.chunkSize; x += resolutionDevisionNum) {
                float heat = Mathf.InverseLerp(0, 1, heatMap[i].y);
                float moisture = Mathf.InverseLerp(0, 1, moistureMap[i].y);  
                // BiomeRow[] heatType = ChooseBiomeType (heat, biomes);
                // Biome biomeType = ChooseBiomeType (moisture, heatType);
                foreach (BiomeRow biomeRow in biomes) {
                    if (heat < biomeRow.threshold) {   
                        heatType = biomeRow.biomes;
                        break;
                    } else {
                        heatType = biomes [biomes.Length - 1].biomes;
                    }
                }
                foreach (Biome biome in heatType) {
                    if (moisture < biome.threshold) {   
                        biomeType = biome;
                        break;
                    } else {
                        heatType = biomes [biomes.Length - 1].biomes;
                    }
                }
                colours[i] = biomeType.color;
                i++;
            }
        }
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

    // BiomeRow[] ChooseBiomeType(float y, BiomeRow[] BiomeTypes) {
    //     foreach (BiomeRow biomeRow in BiomeTypes) {
    //         if (y < biomeRow.threshold) {
    //             return biomeRow;
    //         }
    //     }
    //     return BiomeTypes [BiomeTypes.Length - 1];
    // }

    float CalculateMaxAndMinValues() {
        float x = 1;
        float newX = 1;

        // Can't need to work out away of predicting maxHeight value
        float maxValue = 250f;
        return maxValue;
    }
    
    Vector3[] CalculateNormals() {
        Vector3[] vertexNormals = new Vector3[heightMap.Length];
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
        Vector3 pointA = heightMap [indexA];
        Vector3 pointB = heightMap [indexB];
        Vector3 pointC = heightMap [indexC];

        // To Calculate the surface normal from these point I will use the cross product.
        
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = heightMap;
        mesh.triangles = triangles;
        Debug.Log(colours.Length);
        mesh.colors = colours;
        // mesh.normals = CalculateNormals(); 
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public struct Maps {
        public Vector3[] heightMap;
        public Vector3[] heatMap;
        public Vector3[] moistureMap;
    }
    [SerializeField]
	private BiomeRow[] biomes;

    [SerializeField] private VisualizationMode visualizationMode;
    enum VisualizationMode {Shaded, Heat, Moisture, Biomes}
}

[System.Serializable]
public class TerrainType {
	public string name;
	public Color colour;
	public float threshold;
	public int index;
}

[System.Serializable]
public class Biome {
    public string name;
    public string biomeName;
    public float treeDensity;
    public float threshold; 
    public Color color;
}

[System.Serializable]
public class BiomeRow {
    public string name;
    public float threshold; 
    public Biome[] biomes;
}
