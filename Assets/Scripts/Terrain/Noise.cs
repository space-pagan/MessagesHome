using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    public static float[,] GenerateNoiseMap(
                           int mapWidth, int mapHeight, int seed, float scale, 
                           int octaves, float persistence, float lacunarity,
                           Vector2 offset) {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        // sample each octave from a different point, given a seed;
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        // scale should not be 0, set it to a minimal value if it is
        if (scale <= 0) {
            scale = 0.0001f;
        }

        // use so that scale zooms into the middle of the map rather than the top right corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // generate noise from scale, octaves, persistence, lacunarity for each x,y
        for (int y = 0; y < mapHeight; y++) { 
            for (int x = 0; x < mapWidth; x++) {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int oct = 0; oct < octaves; oct++) {
                    float sampleX = (x - halfWidth + octaveOffsets[oct].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[oct].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // set the noiseMap value to the generated height
                noiseMap[x, y] = noiseHeight;
            }
        }

        // normalize the matrix to [0, 1] for each x,y
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.75f);
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0f, int.MaxValue);
            }
        }

        return noiseMap;
    }
}
