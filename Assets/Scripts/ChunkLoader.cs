﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
    public Transform player;
    public Transform terrain;
    
    public TerrainData terrainData;
    public NoiseData noiseData;

    const float playerMoveThresholdForChunkUpdate = 25f;
    const float sqrPlayerMoveThresholdForChunkUpdate = playerMoveThresholdForChunkUpdate * playerMoveThresholdForChunkUpdate; 

    public static Vector2 playerPosition;
    Vector2 playerPositionOld;
    int chunksVisibleInViewDst;
    
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        float maxViewDst = terrainData.maxViewDst;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / terrainData.chunkSize);
        UpdateVisibleChunks(maxViewDst);
        UpdateVisibleChunks(maxViewDst);
    }

    void Update() {
        playerPosition = new Vector2(player.position.x, player.position.z);

        if ( (playerPositionOld - playerPosition).sqrMagnitude > sqrPlayerMoveThresholdForChunkUpdate) {
            playerPositionOld = playerPosition;
            UpdateVisibleChunks(terrainData.maxViewDst);
        }
    }

void UpdateVisibleChunks(float maxViewDst) {
        for ( int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(playerPosition.x / terrainData.chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(playerPosition.y / terrainData.chunkSize);
    
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk(maxViewDst, terrainData);
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible()) {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                } else {
                    terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk(viewedChunkCoord, terrainData.chunkSize, terrainData.resolution, terrain, maxViewDst, terrainData));
                }
            }
        }
    }

    public class TerrainChunk {
        TerrainData terrainData;
        NoiseData noiseData;
        GameObject terrainClone;
        GameObject meshObject;
        GameObject a;
        GameObject b;
        Vector2 position;
        Bounds bounds;
        Bounds boundsInChunks;

        public TerrainChunk(Vector2 coord, int size, int resolution, Transform terrain, float maxViewDst, TerrainData terrainData) {
            position = coord * size;
            bounds = new Bounds(position,Vector2.one * size);
            float playerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            playerDstFromNearestEdge = playerDstFromNearestEdge / (size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            terrainClone = GameObject.Find("Terrain");
            meshObject = Instantiate (terrainClone, positionV3, Quaternion.identity);
            meshObject.transform.parent = GameObject.Find("Terrain Chunks").transform;
            meshObject.transform.position = positionV3;
            SetVisible(false);
        }

        public void UpdateTerrainChunk(float maxViewDst, TerrainData terrainData) {
            float playerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            float playerChunkDstFromNearestEdge = playerDstFromNearestEdge / terrainData.chunkSize;
            bool visible = playerDstFromNearestEdge <= maxViewDst;

            if (visible) {
                int LODIndex = 0;
                for ( int i = 0; i < terrainData.resolutionLevels.Length - 1; i ++) {
                    if (playerChunkDstFromNearestEdge > terrainData.resolutionLevels[i].visibleChunksDstThreshold) {
                        LODIndex = i + 1;
                    } else {
                        break;
                    }
                } 
                meshObject.GetComponent<TerrainGenerator>().Start_(LODIndex);
            }

            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool isVisible() {
            return meshObject.activeSelf;
        }
    }
}