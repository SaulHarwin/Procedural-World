// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class MapGeneration : MonoBehaviour {

// 	[SerializeField]
// 	private int mapWidthInTiles, mapDepthInTiles;

// 	[SerializeField]
// 	public GameObject Terrain;

// 	[SerializeField]
// 	private float centerVertexZ, maxDistanceZ;

// 	void Start() {
// 		GenerateMap ();
// 	}

// 	void GenerateMap() {
// 		Terrain = GameObject.Find("Terrain");
// 		// get the tile dimensions from the tile Prefab
// 		Vector3 tileSize = Terrain.GetComponent<MeshRenderer>().bounds.size;
// 		int tileWidth = 200;
// 		int tileDepth = 200;
// 		// Debug.Log(tileWidth);

// 		// for each Tile, instantiate a Tile in the correct position
// 		for (int xTileIndex = 0; xTileIndex < mapWidthInTiles; xTileIndex++) {
// 			for (int zTileIndex = 0; zTileIndex < mapDepthInTiles; zTileIndex++) {
// 				// calculate the tile position based on the X and Z indices
// 				Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + xTileIndex * tileWidth, 
// 					this.gameObject.transform.position.y, 
// 					this.gameObject.transform.position.z + zTileIndex * tileDepth);
// 				// instantiate a new Tile
// 				GameObject terrainClone = Instantiate (Terrain, tilePosition, Quaternion.identity) as GameObject;
// 				// generate the Tile texture
// 				terrainClone.GetComponent<TerrainGenerator>().Start();
// 			}
// 		}
// 	}
// }
