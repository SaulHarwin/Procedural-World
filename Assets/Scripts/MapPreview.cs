using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour {
    public Renderer renderer;
    public NoiseData noiseData;
    public TerrainData terrainData;

    public void DrawNoiseMap(float[,] noiseMap) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        Color[] colours = new Color[(width) * (height)];
        float maxValue = noiseData.amplitude * 0.8f; 
        for (int y = 0; y < width; y ++) {
            for (int x = 0; x < height; x ++) {
                float value = Mathf.InverseLerp(0, maxValue, noiseMap[x, y]);
                colours[y * width + x] = Color.Lerp(Color.white, Color.black, value);
            }
        }    
        texture.SetPixels(colours);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        
        renderer.transform.localScale = new Vector3((terrainData.chunkSize*terrainData.scale)/10, 1, (terrainData.chunkSize*terrainData.scale)/10);
        renderer.sharedMaterial.mainTexture = texture;
    }
}
