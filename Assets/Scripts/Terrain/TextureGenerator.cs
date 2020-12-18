using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {
    
    // Generate a 2d texture given a colormap and its dimensions
    public static Texture2D TextureFromColorMap(Color32[] colorMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels32(colorMap);
        texture.Apply();
        return texture;
    }

    // Generate a 2d texture given a noise/heightmap
    public static Texture2D TextureFromHeightMap(float[,] heightMap) {
        // get dimensions of the heightMap
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color32[] colorMap = new Color32[width * height];
        for (int y = 0; y<height; y++) {
            for (int x = 0; x<width; x++) {
                // color of the texture should be a gradient from black to white depending on height
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        // since the above creates a colormap, reuse existing code
        return TextureFromColorMap(colorMap, width, height);
    }
}
