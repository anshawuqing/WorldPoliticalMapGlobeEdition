using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class HeightMapTools : MonoBehaviour
{
    //根据数据生成Mesh
    public static Mesh SetMeshHeight(Mesh deformingMesh, Texture2D heightmap, float3x2 room, bool isStatic = false,
        bool RecalculateNormals = false)
    {
        var uvs = deformingMesh.uv;
        var originalVertices = deformingMesh.vertices;
        var displacedVertices = new Vector3[originalVertices.Length];
        var normalVerts = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            normalVerts[i] = Vector3.Normalize(deformingMesh.normals[i]);
        }

        for (int i = 0; i < originalVertices.Length; i++)
        {
            int u = Mathf.FloorToInt(deformingMesh.uv[i].x * (room.c1.x -room.c0.x )*
                                     math.lerp(room.c0.x, room.c1.x, i * 1f / originalVertices.Length));
            int v = Mathf.FloorToInt(deformingMesh.uv[i].y * (room.c1.z -room.c0.z )*
                                     math.lerp(room.c0.z, room.c1.z, i * 1f / originalVertices.Length));

            //int u = Mathf.FloorToInt (uvs [i].x * heightmap.width * stretchX);
            //int v = Mathf.FloorToInt (uvs [i].y * heightmap.height * stretchZ);
            
            //float newx = originalVertices[i].x;
            //float newy = normalVerts[i].y * heightmap.GetPixel(u, v).grayscale * room.c0.y;
            //float newz = originalVertices[i].z;

            float multiplier = Mathf.Lerp(room.c0.y, room.c1.y, heightmap.GetPixel(u, v).grayscale);

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