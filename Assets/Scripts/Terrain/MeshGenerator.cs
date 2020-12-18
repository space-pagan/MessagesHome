using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, 
            float heightMultiplier, AnimationCurve _heightCurve,
            int detailLevel, bool useFlatShading) {

        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int detailInc = detailLevel == 0 ? 1 : detailLevel * 2;
        int vertWidth = (width - 1) / detailInc + 1;

        // keeps the center of the map at 0, 0
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        MeshData meshData = new MeshData(vertWidth, vertWidth, useFlatShading);
        int vertexIndex = 0;

        // add triangles to mesh for every x,y
        for (int y = 0; y < height; y += detailInc) {
            for (int x = 0; x < width; x += detailInc) {
                // add the vertex to the mesh, offset by topleftx/z
                meshData.vertices[vertexIndex] = 
                    new Vector3(topLeftX + x, 
                        heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier,
                        topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                
                // triangles do not need to be generated for right column or bottom row
                if (x < width - 1 && y < height - 1) {
                    /*   i              i - i+1
                     * |    \            \  |
                     * i+w - i+w+1        i+w+1
                     */
                    meshData.AddTriangle(vertexIndex, vertexIndex + vertWidth + 1, vertexIndex + vertWidth);
                    meshData.AddTriangle(vertexIndex + vertWidth + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }
        return meshData;
    }
}

// class to store mesh
public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    bool useFlatShading;
    int triangleIndex;

    // constructor
    public MeshData(int meshWidth, int meshHeight, bool useFlatShading) {
        this.useFlatShading = useFlatShading;
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshWidth * meshHeight];
    }

    // appends a triangle to the triangle array
    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    void FlatShading() {
        Vector3[] flatShadedVerts = new Vector3[triangles.Length];
        Vector2[] flatShadedUVs = new Vector2[triangles.Length];
        for (int i = 0; i < triangles.Length; i++) {
            flatShadedVerts[i] = vertices[triangles[i]];
            flatShadedUVs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }
        vertices = flatShadedVerts;
        uvs = flatShadedUVs;
    }

    public Mesh CreateMesh() {
        if (useFlatShading) {
            FlatShading();
        }
        Mesh mesh = new Mesh {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}