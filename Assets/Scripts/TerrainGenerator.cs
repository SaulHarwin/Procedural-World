using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour {

    public float radius;
	public Vector2 regionSize = Vector2.one;
	public int rejectionSamples;
	public float displayRadius;

	List<Vector2> points;


    private Biome[] heatType;
    private Biome biomeType;

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

    [SerializeField] private TerrainType[] heightTerrainTypes;
    [SerializeField] private TerrainType[] heatTerrainTypes;
    [SerializeField] private TerrainType[] moistureTerrainTypes;
    // [SerializeField] private TerrainType[] biomeTerrainTypes;
    [SerializeField] private VisualizationMode visualizationMode;
    enum VisualizationMode {Shaded, Heat, Moisture, Biomes}

    public void Startup(int LODIndex) {
        int resolution = terrainData.resolutionLevels[LODIndex].resolution;
        int resolutionDevisionNum = (resolution ==0)?1:resolution*2;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        float maxValue = CalculateMaxAndMinValues();
        float distanceFromZero = GenerateTerrain(maxValue, resolutionDevisionNum);
        transform.position = new Vector3 ( transform.position.x, -distanceFromZero, transform.position.z);
        UpdateMesh();

        points = TreeGeneration.GeneratePoints(radius, regionSize, rejectionSamples);

        // Gizmos.DrawWireCube(regionSize/2,regionSize);
		if (points != null) {
            foreach (Vector2 point in points) {
				// Gizmos.DrawSphere(point, displayRadius);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(displayRadius, displayRadius, displayRadius);
                float height = 999999999;
                float height1 = 0;
                float height2 = 0;
                for (int i = 0; i < heightMap.Length; i ++) {
                    height = 999999999;
                    height1 = 0;
                    height2 = 0;
                    // Debug.Log(heightMap[3720]);
                    int incrementY = (terrainData.chunkSize) / resolutionDevisionNum + 1;
                    int incrementX = 1;
                    // Debug.Log("-");
                    // Debug.Log(incrementX);
                    // Debug.Log(incrementY);
                    // Debug.Log("\n");
                    // if ( i+incrementY > (terrainData.chunkSize*terrainData.chunkSize) / (resolutionDevisionNum*resolutionDevisionNum)) {
                    //     incrementY = -incrementY; 
                    // }
                    // if ( i+incrementX > (terrainData.chunkSize*terrainData.chunkSize) / (resolutionDevisionNum*resolutionDevisionNum)) {
                    //     incrementX = -incrementX; 
                    // }
                    if (point[0] == heightMap[i][0]) {
                        if (point[1] == heightMap[i][2]) {
                            // Debug.Log(heightMap[i][2]);
                            // Debug.Log(heightMap[i+incrementY][2]);
                            height = heightMap[i][1]; 
                            // Debug.Log(i);
                            // Debug.Log(height);
                            break;
                        } 
                        // else {
                        //     if (point[1] >= heightMap[i][2] && point[1] <= heightMap[i+incrementY][2]) {
                        //         // X == point[0]
                        //         // Y Middle of...
                        //         // Debug.Log(heightMap[i+terrainData.chunkSize+4]);
                        //         Debug.Log("1");
                        //         break;
                        //     }
                        // }
                    }
                    // if (point[1] == heightMap[i][2]) {
                    //     if (point[0] == heightMap[i][0]) {
                    //         height = heightMap[i][1];
                    //         Debug.Log("2");
                    //         break;
                    //     } 
                    // }
                        // else {
                    //         if (point[0] >= heightMap[i][0] && point[0] <= heightMap[i+incrementX][0]) {
                    //             // X Middle of...
                    //             // Y == point[1]
                    //             height = (heightMap[i][1] + heightMap[i+incrementX][1]) / ((heightMap[i][0]+heightMap[i+incrementX][0]) / point[0]);
                    //             Debug.Log("3");
                    //             break;
                    //         }
                    //     }
                // if (height == 999999999) {
                //     for (int n = 0; n < heightMap.Length; n ++) {
                    if (point[0] == heightMap[i][0]) {
                        // Debug.Log(heightMap[i]);
                        // Debug.Log(heightMap[i+incrementY]);
                        // Debug.Log(point);
                        if (point[1] > heightMap[i][2] && point[1] < heightMap[i+incrementY][2]) {
                            height = (heightMap[i][1] + heightMap[i+incrementY][1]) / ((heightMap[i][2]+heightMap[i+incrementY][2]) / point[1]);
                            break;
                        }
                    }
                    if (point[1] == heightMap[i][2]) {
                        if (point[0] > heightMap[i][0] && point[0] < heightMap[i+incrementX][0]) {
                            height = (heightMap[i][1] + heightMap[i+incrementX][1]) / ((heightMap[i][0]+heightMap[i+incrementX][0]) / point[0]);
                            break;
                        }
                    }
                //     }
                // } 
                // if (height == 999999999) {
                //     for (int m = 0; m < heightMap.Length; m ++) {
                    if (point[0] > heightMap[i][0] && point[0] < heightMap[i+incrementX][0]) {
                        // Debug.Log("4");
                        if (point[1] > heightMap[i][2] && point[1] < heightMap[i+incrementY][2]) {
                            // XDiv ((heightMap[i][0]+heightMapi+incrementY][2]) / point[1])
                            height1 = (heightMap[i][1] + heightMap[i+incrementX][1]) / ((heightMap[i][0]+heightMap[i+incrementX][0]) / point[0]);
                            height2 = (heightMap[i+incrementY][1] + heightMap[i+incrementX+incrementY][1]) / ((heightMap[i+incrementY][0]+heightMap[i+incrementX+incrementY][0]) / point[0]);
                            // Debug.Log(height1);
                            // Debug.Log(height2);
                            height = (height1 + height2)/ ((heightMap[i][2]+heightMap[i+incrementY][2]) / point[1]);
                            break;
                        }
                    }
                //     }
                // }
            }
            // Debug.Log(height);
            cube.transform.Translate(point[0], height-distanceFromZero, point[1]);
			}
		}
    }

    public float GenerateTerrain(float maxValue, int resolutionDevisionNum) {
        Color[] colours;
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

                float offSetX = (transform.position.x * noiseData.frequency) / terrainData.scale;
                float offSetZ = (transform.position.z * noiseData.frequency) / terrainData.scale;

                float landMassOffSetX = (transform.position.x * noiseData.landMassFrequency) / terrainData.scale;
                float landMassOffSetZ = (transform.position.z * noiseData.landMassFrequency) / terrainData.scale;

                float heatMapOffSetX = (transform.position.x * noiseData.heatMapFrequency) / terrainData.scale;
                float heatMapOffSetZ = (transform.position.z * noiseData.heatMapFrequency) / terrainData.scale;

                float moistureMapOffSetX = (transform.position.x * noiseData.moistureMapFrequency) / terrainData.scale;
                float moistureMapOffSetZ = (transform.position.z * noiseData.moistureMapFrequency) / terrainData.scale;

                for (int o = 1; o <= noiseData.octaves; o++, newSeed += 500, newFrequency *= noiseData.lacinarity, newAmplitude *= noiseData.persistance) {

                    if (o == 1) {
                        heightMapValue = Mathf.PerlinNoise(x  * newFrequency + (newSeed - (-offSetX)), z * newFrequency + (newSeed - (-offSetZ)));
                        heightMapValue = terrainData.meshHeightCurve.Evaluate(heightMapValue);
                        heightMapValue = heightMapValue * newAmplitude;
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
                float continentValue = Mathf.PerlinNoise(x * noiseData.landMassFrequency + (newSeed - (-landMassOffSetX)), z * noiseData.landMassFrequency + (newSeed - (-landMassOffSetZ)));
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
        float maxValue = 800f;
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
        mesh.colors = colours;
        // mesh.normals = CalculateNormals(); 
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

	void OnValidate() {
		// points = TreeGeneration.GeneratePoints(radius, regionSize, rejectionSamples);
	}

	// void OnDrawGizmos() {
	// 	Gizmos.DrawWireCube(regionSize/2,regionSize);
	// 	if (points != null) {
    //         foreach (Vector2 point in points) {
    //             Vector3 point = new Vector3(point[0], 0f, point[1]);
    //             // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //             // cube.transform.Translate(point[0], 0, point[1]);
                
    //             // for (int i = 0; i < mesh.vertices.Length; i ++) {
    //             //     if (mesh.vertices[i][0] == point[0] && mesh.vertices[i][2] == point[1]) {
    //             //         Debug.Log(mesh.vertices[i]);
    //             //     }
    //             // }
	// 		}
	// 	}
	// }
    void OnDrawGizmos() {
        for (int i = 0; i >= heightMap.Length; i++) {
            Gizmos.DrawSphere(heightMap[i], 0.5f);
        }
    }

    public struct Maps {
        public Vector3[] heightMap;
        public Vector3[] heatMap;
        public Vector3[] moistureMap;
    }
    [SerializeField]
	private BiomeRow[] biomes;
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
    public float threshold; 
    public Color color;
}

[System.Serializable]
public class BiomeRow {
    public string name;
    public float threshold; 
    public Biome[] biomes;
}