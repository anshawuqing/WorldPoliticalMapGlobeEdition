using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class HeightMapTools : MonoBehaviour
{
    public Texture2D HeightMap;
    public Vector3 size = new Vector3(1, 1, 0.25f);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            SetMeshHeight(GetComponent<MeshFilter>().mesh, HeightMap,new float3x2(Vector3.zero,size) );
        }
    }

    //根据数据生成Mesh
    public static Mesh SetMeshHeight(Mesh deformingMesh, Texture2D heightmap, float3x2 room, bool RecalculateNormals = false)
    {
        var originalVertices = deformingMesh.vertices;
        var displacedVertices = new Vector3[originalVertices.Length];
        var uvs = deformingMesh.uv;
        var normalVerts = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            normalVerts[i] = Vector3.Normalize(deformingMesh.normals[i]);
        }

        for (int i = 0; i < originalVertices.Length; i++)
        {
            int u = Mathf.FloorToInt(heightmap.width * math.lerp(room.c0.x, room.c1.x, uvs[i].x));
            int v = Mathf.FloorToInt(heightmap.height * math.lerp(room.c0.y, room.c1.y, uvs[i].y));

            float multiplier = math.lerp(room.c0.z, room.c1.z, heightmap.GetPixel(u, v).grayscale);

            float newx = originalVertices[i].x + normalVerts[i].x * multiplier;
            float newy = originalVertices[i].y + normalVerts[i].y * multiplier;
            float newz = originalVertices[i].z + normalVerts[i].z * multiplier;

            Vector3 pos = new Vector3(newx, newy, newz);
            displacedVertices[i] = pos;
        }

        deformingMesh.vertices = displacedVertices;
        if (RecalculateNormals)
        {
            deformingMesh.RecalculateNormals();
        }

        return deformingMesh;
    }
}