using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MapGenerator : MonoBehaviour {

    // Editor visible variables
    public bool flatShading;

    public const int mapChunkSize = 97;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;
    public int seed;
    public Vector2 offset;

    public bool autoUpdate;
    public TerrainType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    PhotonView PV;

    void Awake() {
        PV = GetComponent<PhotonView>();
    }

    public void RequestMapData( Vector2 center, Action<MapData> callback) {
        // starts thread which will retrieve mapData
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback) {
        // thread to generate map data (noisemap, colormap)
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int detailLevel, Action<MeshData> callback) {
        // starts thread which will retrieve meshData
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, detailLevel, callback);
        };

        new Thread(threadStart).Start();
    }
    
    public void MeshDataThread(MapData mapData, int detailLevel, Action<MeshData> callback) {
        // thread to generate mesh data from mapdata (mesh vertices, etc)
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(
            mapData.heightMap, meshHeightMultiplier, meshHeightCurve, detailLevel, flatShading);
        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update() {
        // every frame, check if any threads have completed and put their data in the queue
        // if so, process the callback
        processThreadInfo<MapData>(mapDataThreadInfoQueue);
        processThreadInfo<MeshData>(meshDataThreadInfoQueue);
    }

    
    private void processThreadInfo<T>(Queue<MapThreadInfo<T>> q) {
        // generic function to process callback for any MapThreadInfo-typed Queue
        if (q.Count > 0) {
            // for each object in the queue, process its callback
            for (int i = 0; i < q.Count; i++) {
                MapThreadInfo<T> mtinfo = q.Dequeue();
                mtinfo.callback(mtinfo.param);
            }
        }
    }

    MapData GenerateMapData(Vector2 center) {
        // return the noiseMap and colorMap (currently always the same regardless of chunk)
        float[,] noiseMap = Noise.GenerateNoiseMap(
                mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence,
                lacunarity, center + offset);

        // Generate colors based on terrain height
        Color32[] colorMap = new Color32[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                // determine color based on which region the height is in
                foreach (TerrainType region in regions) {
                    if (currentHeight >= region.height) {
                        colorMap[y * mapChunkSize + x] = region.color;
                    } else break;
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }

    // check that inspector variables set to valid values
    void OnValidate() {
        if (noiseScale < 1) noiseScale = 1;
        if (octaves < 1) octaves = 1;
        if (lacunarity < 1) lacunarity = 1;
    }

    // transfer struct for threaded data
    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T param;

        public MapThreadInfo(Action<T> callback, T param) {
            this.callback = callback;
            this.param = param;
        }
    }
}

// inspector variables
[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

// transfer struct for data from which a map can be generated and drawn
public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color32[] colorMap;

    public MapData(float[,] heightMap, Color32[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
