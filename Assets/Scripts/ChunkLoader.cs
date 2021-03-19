using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
    public Transform player;
    public Transform terrain;
    
    public TerrainData terrainData;
    public NoiseData noiseData;
    private ResolutionData resolutionData;


    const float playerMoveThresholdForChunkUpdate = 25f;
    const float sqrPlayerMoveThresholdForChunkUpdate = playerMoveThresholdForChunkUpdate * playerMoveThresholdForChunkUpdate; 
    public int countTag; 

    public static Vector2 playerPosition;
    Vector2 playerPositionOld;
    int chunksVisibleInViewDst;
    
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    Dictionary<Vector2, int> resolutionDictionary = new Dictionary<Vector2, int>(); 
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        switch (terrainData.graphicQuality) {
            case TerrainData.GraphicQuality.Low:
                resolutionData = terrainData.lowResolution;
                break;
            case TerrainData.GraphicQuality.Medium:
                resolutionData = terrainData.mediumResolution;
                break;
            case TerrainData.GraphicQuality.High:
                resolutionData = terrainData.highResolution;
                break;
        }

        float maxViewDst = terrainData.maxViewDst * terrainData.chunkSize * terrainData.scale;
        chunksVisibleInViewDst = terrainData.maxViewDst;
        UpdateVisibleChunks(maxViewDst);
        UpdateVisibleChunks(maxViewDst);
    }

    void Update() {
        playerPosition = new Vector2(player.position.x, player.position.z);
        if ( (playerPositionOld - playerPosition).sqrMagnitude > sqrPlayerMoveThresholdForChunkUpdate) {
            playerPositionOld = playerPosition;
            UpdateVisibleChunks(terrainData.maxViewDst * terrainData.chunkSize * terrainData.scale);
        }
    }

    void UpdateVisibleChunks(float maxViewDst) {
        for ( int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(playerPosition.x / (terrainData.chunkSize * terrainData.scale));
        int currentChunkCoordY = Mathf.RoundToInt(playerPosition.y / (terrainData.chunkSize * terrainData.scale));
    
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
                    int resolution = resolutionDictionary[viewedChunkCoord]; // Resolution = the current resolution the chunk has.
                    int newResolution = terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk(terrainData.chunkSize, maxViewDst, terrainData, resolutionData, resolution);
                    resolutionDictionary[viewedChunkCoord] = newResolution; // Update the value for the chunks resolution.
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible()) {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                } else {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, terrainData.chunkSize, terrain, maxViewDst, terrainData, countTag));
                    resolutionDictionary.Add(viewedChunkCoord, 0); // Add the chunk's ChunkCoord to the dictionary.
                }
                countTag += 1;
            }
        }
    }

    public class TerrainChunk {
        ResolutionData resolutionData;
        TerrainData terrainData;
        NoiseData noiseData;
        GameObject terrainClone;
        GameObject waterClone;
        GameObject terrainObject;
        GameObject waterObject;
        GameObject a;
        GameObject b;
        Vector2 position;
        Bounds bounds;
        Bounds boundsInChunks;

        public TerrainChunk(Vector2 coord, int size, Transform terrain, float maxViewDst, TerrainData terrainData, int countTag) {
            size *= terrainData.scale;
            position = coord * size;
            bounds = new Bounds(position,Vector2.one * size);
            float playerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            playerDstFromNearestEdge = playerDstFromNearestEdge / (size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            
            terrainClone = GameObject.Find("Terrain");
            terrainObject = Instantiate (terrainClone, positionV3, Quaternion.identity);
            terrainObject.transform.localScale = new Vector3(terrainData.scale, terrainData.scale, terrainData.scale);
            terrainObject.transform.parent = GameObject.Find("Terrain Chunks").transform;
            terrainObject.transform.position = positionV3;
            terrainObject.name = "Terrain Chunk" + countTag.ToString();
            
            if (terrainData.waterVisible) {
                waterClone = GameObject.Find("Water");
                waterObject = Instantiate (waterClone, positionV3, Quaternion.identity);
                waterObject.transform.localScale = new Vector3(terrainData.scale*2, 1, terrainData.scale*2);
                waterObject.transform.parent = GameObject.Find("Terrain Chunks").transform;
                waterObject.transform.position = positionV3;
                waterObject.name = "Water Chunk" + countTag.ToString();
            }
        }

        public int UpdateTerrainChunk(int size, float maxViewDst, TerrainData terrainData, ResolutionData resolutionData, int resolution) {
            float playerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            float playerChunkDstFromNearestEdge = playerDstFromNearestEdge / (terrainData.chunkSize * terrainData.scale);
            bool visible = playerDstFromNearestEdge <= maxViewDst;

            int LODIndex = 0;
            
            for (int i = 0; i < resolutionData.resolutionLevels.Length - 1; i ++) {
                if (playerChunkDstFromNearestEdge > resolutionData.resolutionLevels[i].visibleChunksDstThreshold) {
                    LODIndex = i + 1;
                } else {
                    break;
                }
            } 
            if (resolution != resolutionData.resolutionLevels[LODIndex].resolution) { // Regenerate the chunks mesh only if the resolution of the chunk has changed.
                terrainObject.GetComponent<TerrainGenerator>().Startup(LODIndex, terrainObject.name);
            } 
            SetVisible(visible);
            return resolutionData.resolutionLevels[LODIndex].resolution; // Return the resolution of the chunk.
        }

        public void SetVisible(bool visible) {
            terrainObject.SetActive(visible);
            try {
                waterObject.SetActive(visible);
            }
            catch {}
        }

        public bool isVisible() {
            return terrainObject.activeSelf;
        }
    }
}