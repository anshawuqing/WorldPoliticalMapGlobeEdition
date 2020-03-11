using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using WPM;

public class MeshNum : MonoBehaviour
{
    public MeshFilter _meshFilter;

    public GameObject node;

    /// <summary>
    /// 顶点
    /// </summary>
    Vector3[] Vertice;

    /// <summary>
    /// 顶点索引数组
    /// </summary>
    int[] Trangle;

    /// <summary>
    ///  法线
    /// </summary>
    Vector3[] Normals;

    /// <summary>
    ///uv 数组
    /// </summary>
    Vector2[] UV;

    /// <summary>
    /// 单元格大小
    /// </summary>
    public Vector2 _cellSize = new Vector2(1, 1);

    /// <summary>
    /// 网格大小
    /// </summary>
    public Vector2 _gridSize = new Vector2(1, 1);

    private void Awake()
    {
        //UpdateMesh(GetComponent<MeshFilter>(),_cellSize,_gridSize);
    }

    public static void UpdateMesh(TileInfo ti, MeshFilter _meshFilter, Vector2 _cellSize, Vector2 _gridSize)
    {
        if (null == _meshFilter)
        {
            Debug.Log("NeedMesh");
            return;
        }

        Mesh mesh = new Mesh();

        //计算Plane大小
        Vector2 size;
        size.x = _cellSize.x * _gridSize.x;
        size.y = _cellSize.y * _gridSize.y;

        //计算Plane一半大小
        Vector2 latLonTL = ti.latlons[0];
        Vector2 latLonBR;
        int tileCode = MapLoader.instance.WorldMapGlobe.GetTileHashCode(ti.x + 1, ti.y + 1, ti.zoomLevel);
        if (MapLoader.instance.WorldMapGlobe.cachedTiles.ContainsKey(tileCode))
        {
            latLonBR = MapLoader.instance.WorldMapGlobe.cachedTiles[tileCode].latlons[0];
        }
        else
        {
            latLonBR = MapLoader.instance.WorldMapGlobe.GetLatLonFromTile(ti.x + 1, ti.y + 1, ti.zoomLevel);
        }
        
        //计算顶点及UV
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        //tileCorners[0] = Conversion.GetSpherePointFromLatLon(ti.latlons[0]);TL x0y0
        //tileCorners[1] = Conversion.GetSpherePointFromLatLon(new Vector2(ti.latlons[0].x, ti.latlons[2].y));TR x0y1
        //tileCorners[2] = Conversion.GetSpherePointFromLatLon(ti.latlons[2]);BR x1y0
        //tileCorners[3] = Conversion.GetSpherePointFromLatLon(new Vector2(ti.latlons[2].x, ti.latlons[0].y));BL x1y1

        //uv[0] = new Vector2((ti.latlons[0].y + 180) / 360f, (ti.latlons[0].x + 90) / 180f);TL.y,TL.x x0y0
        //uv[1] = new Vector2((ti.latlons[2].y + 180) / 360f, (ti.latlons[0].x + 90) / 180f);TR x0y1
        //uv[2] = new Vector2((ti.latlons[2].y + 180) / 360f, (ti.latlons[2].x + 90) / 180f);BR x1y0
        //uv[3] = new Vector2((ti.latlons[0].y + 180) / 360f, (ti.latlons[2].x + 90) / 180f);BL x1y1

        var scale = new Vector2();
        Vector2 latlon;
        var latlonY1 = latLonTL;
        var latlonY2 = latLonBR;
        for (int x = 0; x < _gridSize.x + 1; x++)
        {
            scale.x = x / (_gridSize.x);
            latlon.x = Mathf.Lerp(latLonTL.x, latLonBR.x, scale.x);
            for (int y = 0; y < _gridSize.y + 1; y++)
            {
                scale.y = y / (_gridSize.y);

                if (latlonY1.y-latlonY2.y>180)
                {
                    latlonY1.y += 360;
                }

                if (latlonY2.y-latlonY1.y>180)
                {
                    latlonY2.y += 360;
                }
                latlon.y = Mathf.Lerp(latlonY1.y, latlonY2.y, scale.y);
                vertices.Add(Conversion.GetSpherePointFromLatLon(latlon)); //添加到顶点数组
                uvs.Add(new Vector2((latlon.y + 180) / 360f, (latlon.x + 90) / 180f)); //添加到纹理坐标数组
            }
        }

        //顶点序列
        int TL = 0;
        int BL = 0;
        int BR = 0;
        int TR = 0;
        int startIndex = 0;
        int[] indexs = new int[(int) _gridSize.x * (int) _gridSize.y * 2 * 3]; //顶点序列
        for (int y = 0; y < _gridSize.y; y++)
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                //四边形四个顶点

                TL = y * ((int) _gridSize.x + 1) + x; //x0y0 TL
                BL = (y + 1) * ((int) _gridSize.x + 1) + x; //x0y1 BL
                BR = BL + 1; //x1y1 BR
                TR = TL + 1; //x1y0 TR

                //计算在数组中的起点序号
                startIndex = y * (int) _gridSize.x * 2 * 3 + x * 2 * 3;

                //左上三角形
                indexs[startIndex] = TL; //0
                indexs[startIndex + 1] = TR; //1
                indexs[startIndex + 2] = BL; //2

                //右下三角形
                indexs[startIndex + 3] = TR; //2
                indexs[startIndex + 4] = BR; //3
                indexs[startIndex + 5] = BL; //0
            }
        }

        //
        mesh.SetVertices(vertices); //设置顶点
        mesh.SetUVs(0, uvs); //设置UV
        mesh.SetIndices(indexs, MeshTopology.Triangles, 0); //设置顶点序列
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //mesh.RecalculateTangents();

        _meshFilter.sharedMesh = mesh;

        _meshFilter.transform.localScale = new Vector3(1, 1, 1);
    }

//
//    public static void UpdateMesh(MeshFilter _meshFilter, Vector2 _cellSize, Vector2 _gridSize)
//    {
//        if (null == _meshFilter)
//        {
//            Debug.Log("NeedMesh");
//            return;
//        }
//
//        Mesh mesh = new Mesh();
//
//        //计算Plane大小
//        Vector2 size;
//        size.x = _cellSize.x * _gridSize.x;
//        size.y = _cellSize.y * _gridSize.y;
//
//        //计算Plane一半大小
//        Vector2 halfSize = size / 2;
//
//        //计算顶点及UV
//        List<Vector3> vertices = new List<Vector3>();
//        List<Vector2> uvs = new List<Vector2>();
//
//        Vector3 vertice = Vector3.zero;
//        Vector2 uv = Vector3.zero;
//
//        for (int y = 0; y < _gridSize.y + 1; y++)
//        {
//            vertice.z = y * _cellSize.y - halfSize.y; //计算顶点Y轴
//            uv.y = y * _cellSize.y / size.y; //计算顶点纹理坐标V
//
//            for (int x = 0; x < _gridSize.x + 1; x++)
//            {
//                vertice.x = x * _cellSize.x - halfSize.x; //计算顶点X轴
//                uv.x = x * _cellSize.x / size.x; //计算顶点纹理坐标U
//
//                vertices.Add(vertice); //添加到顶点数组
//                uvs.Add(uv); //添加到纹理坐标数组
//            }
//        }
//
//        //顶点序列
//        int a = 0;
//        int b = 0;
//        int c = 0;
//        int d = 0;
//        int startIndex = 0;
//        int[] indexs = new int[(int) _gridSize.x * (int) _gridSize.y * 2 * 3]; //顶点序列
//        for (int y = 0; y < _gridSize.y; y++)
//        {
//            for (int x = 0; x < _gridSize.x; x++)
//            {
//                //四边形四个顶点
//                //a = y * ((int)_gridSize.x + 1) + x;//0
//                //b = (y + 1) * ((int)_gridSize.x + 1) + x;//1
//                //c = b + 1;//2
//                //d = a + 1;//3
//
//
//                a = y * ((int) _gridSize.x + 1) + x; //0
//                b = (y + 1) * ((int) _gridSize.x + 1) + x; //1
//                c = b + 1; //2
//                d = a + 1; //3
//
//                //计算在数组中的起点序号
//                startIndex = y * (int) _gridSize.x * 2 * 3 + x * 2 * 3;
//
//                //左上三角形
//                indexs[startIndex] = a; //0
//                indexs[startIndex + 1] = b; //1
//                indexs[startIndex + 2] = c; //2
//
//                //右下三角形
//                indexs[startIndex + 3] = c; //2
//                indexs[startIndex + 4] = d; //3
//                indexs[startIndex + 5] = a; //0
//            }
//        }
//
//        //
//        mesh.SetVertices(vertices); //设置顶点
//        mesh.SetUVs(0, uvs); //设置UV
//        mesh.SetIndices(indexs, MeshTopology.Triangles, 0); //设置顶点序列
//        mesh.RecalculateNormals();
//        mesh.RecalculateBounds();
//        //mesh.RecalculateTangents();
//
//        _meshFilter.mesh = mesh;
//
//        _meshFilter.transform.localScale = new Vector3(1, 1, 1);
//    }
}