using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class InfTerrain : MonoBehaviour {

    const float chunkUpdateThreshold = 25f;
    const float squareChunkUpdateThreshhold = chunkUpdateThreshold * chunkUpdateThreshold;
    public DetailInfo[] detailLevels;
    public static float maxViewDist;
    
    public Material mapMaterial;

    Transform viewer;
    static Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    static MapGenerator mapGen;
    int chunkSize;
    int chunksVisible;

    Dictionary<Vector2, TerrainChunk> chunkDict = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> chunksUpdated = new List<TerrainChunk>();

    private void Start() {
        mapGen = FindObjectOfType<MapGenerator>();
        // set the chunk generator center to the first player if one exists
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0) {
            viewer = players[0].transform;
        } else {
            // or to the map generator object's position (0,0,0) otherwise
            viewer = mapGen.transform;
        }
        // set the maxviewdist to be the last distance defined in the 
        // detailLevels inspector window
        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        // 1 less triangle on each side than there is vertices
        chunkSize = MapGenerator.mapChunkSize - 1;
        // calculate the visible radius
        chunksVisible = Mathf.RoundToInt(maxViewDist / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update() {
        // every frame, update which chunks are visible to the player
        // position terrain generator around the first player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0) {
            viewer = players[0].transform;
        }
        // update the player's position
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        // if the viewer has moved a threshhold amount, update all
        // visible chunks and reset the threshhold
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > squareChunkUpdateThreshhold) {
            UpdateVisibleChunks();
            viewerPositionOld = viewerPosition;
        }
    }

    void UpdateVisibleChunks() {
        // set all chunks from last update to not visible
        foreach (TerrainChunk t in chunksUpdated) {
            t.SetVisible(false);
        }
        // clear list for this frame
        chunksUpdated.Clear();

        // get x,y of current chunk on the chunk grid (steps of mapChunkSize-1)
        int currentChunkX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        // sweep a square around the player of chunks that should be currently loaded
        for (int chunkGridY = -chunksVisible; chunkGridY <= chunksVisible; chunkGridY++) {
            for (int chunkGridX = -chunksVisible; chunkGridX <= chunksVisible; chunkGridX++) {
                // chunk grid coords of selected chunk
                Vector2 viewedChunkVec = new Vector2(
                        currentChunkX + chunkGridX, 
                        currentChunkY + chunkGridY);

                if (chunkDict.ContainsKey(viewedChunkVec)) {
                    // the selected chunk has already been generated
                    chunkDict[viewedChunkVec].UpdateChunk();   
                } else {
                    // the selected chunk is new and needs to be generated and added to the dictionary
                    chunkDict.Add(
                        viewedChunkVec, 
                        new TerrainChunk(viewedChunkVec, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }
    
    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        DetailInfo[] detailLevels;
        DetailMesh[] detailMeshes;

        MapData mapData;
        bool mapDataReceived;
        int prevDetailIndex = -1;

        public TerrainChunk(Vector2 chunkGridVec, int size, DetailInfo[] detailLevels, Transform parent, Material mat) {
            // constructor
            this.detailLevels = detailLevels;

            // actual position in unity coordinates
            position = chunkGridVec * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;

            SetVisible(false);

            // initialize mesh generators for the various levels of detail (does not actually 
            // generate the meshes until later)
            detailMeshes = new DetailMesh[detailLevels.Length]; 
            for (int i = 0; i < detailLevels.Length; i++) {
                detailMeshes[i] = new DetailMesh(detailLevels[i].detailLevel, UpdateChunk);
            }

            // generate the noiseMap and colorMap for this chunk, then call OnMapDataReceived
            mapGen.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            // function called by thread when mapData has been retrieved for a chunk
            this.mapData = mapData;
            mapDataReceived = true; // I guess this is better than checking if this.mapData == Null/equivalent

            // generate the texture from the mapdata colorMap element and set it
            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateChunk();
        }

        public void UpdateChunk() {
            // only do all of this if we have mapData for this chunk!
            if (mapDataReceived) { 
                // checks if the viewer's distance to the nearest edge of this chunk is within
                // view distance. If it is, set this chunk to visible
                float viewDistToEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewDistToEdge <= maxViewDist;

                if (visible) {
                    // determine the detail level at which to display this chunk
                    // based on distance to player
                    int detailIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++) {
                        if (viewDistToEdge > detailLevels[i].visibleDistThreshold) {
                            detailIndex = i + 1;
                        } else break;
                    }
                    // if the detailIndex has changed (the player moved through a threshhold)
                    if (detailIndex != prevDetailIndex) {
                        // select the mesh generator for the correct detail level
                        DetailMesh detailMesh = detailMeshes[detailIndex];
                        // if the mesh has already been generated then just set it and update the
                        // collider mesh
                        if (detailMesh.hasMesh) {
                            prevDetailIndex = detailIndex;
                            meshFilter.mesh = detailMesh.mesh;
                            meshCollider.sharedMesh = meshFilter.mesh;
                            meshCollider.enabled = true;
                        } else if (!detailMesh.requestedMesh) {
                            // the mesh has not yet been generated, so do so now (since we need it now)
                            // this function call will result in an UpdateChunk() callback
                            // which will follow the above branch
                            detailMesh.RequestMesh(mapData);
                        }
                    }
                    // if the chunk became visible, add it to the list, so it can be set invisble 
                    // once it leaves the view distance bounds
                    chunksUpdated.Add(this);

                }
                // activates the meshObject in the scene for this chunk
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible) {
            // sets the visibility of the chunk
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            // returns the visibility of the chunk
            return meshObject.activeSelf;
        }
    }

    class DetailMesh {
        public Mesh mesh;
        public bool requestedMesh;
        public bool hasMesh;
        int detailLevel;
        System.Action updateCallback;

        public DetailMesh(int detail, System.Action updateCallback) {
            // instantiate a generator for a specific detail level
            detailLevel = detail;
            this.updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData) {
            // request a mesh, using the detail level and the noiseMap from mapData
            requestedMesh = true;
            // this is done in a threaded manner, will callback OnMeshDataReceived when complete
            mapGen.RequestMeshData(mapData, detailLevel, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData) {
            // turn the vertices and triangles calculated into a unity mesh, also recalculates normals
            mesh = meshData.CreateMesh();
            hasMesh = true;

            // run the callback function (probably UpdateChunk)
            updateCallback();
        }
    }

    [System.Serializable]
    public struct DetailInfo {
        public int detailLevel;
        public float visibleDistThreshold;
    }
}
