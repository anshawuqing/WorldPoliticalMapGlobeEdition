using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshCreater : MonoBehaviour
{
    private Texture textureGray; //灰度图
    private Texture textureGrass; //草地贴图
    private int tGrayWidth = 0, tGrayHeight = 0; //灰度图的宽和高
    private bool bCreate = false; //是否完成创建
    public List<GameObject> meshList; //mesh集合
    private Texture2D texture2dGray;
    public float scale = 2; //高度参数
    public Vector3 size = new Vector3(1,0.25f,1);//尺寸
    [Tooltip("传入mesh使用的材质")] public Material meshMaterial;

    void Start()
    {
        StartCoroutine(loadImage("IGray.jpg", (t) => textureGray = t));
        StartCoroutine(loadImage("IGrass.png", (t) => textureGrass = t));
        meshList = new List<GameObject>();
    }

    void Update()
    {
        if (textureGray != null && textureGrass != null)
        {
            if (bCreate == false)
            {
                tGrayWidth = textureGray.width;
                tGrayHeight = textureGray.height;
                meshMaterial.mainTexture = textureGrass; //设置材质贴图
                //mesh顶点数目最大65000，则取mes为250*250=62500
                int xNum = 1 + tGrayWidth / 250; //x方向mesh个数
                int zNum = 1 + tGrayHeight / 250; //z方向mesh个数
                texture2dGray = (Texture2D) textureGray;
                //根据灰度图创建mesh
                for (int i = 0; i < xNum; i++)
                {
                    for (int j = 0; j < zNum; j++)
                    {
                        if (i < xNum - 1 && j < zNum - 1)
                        {
                            meshList.Add(
                                createMesh("meshX" + i.ToString() + "Z" + j.ToString(), 251, 251,
                                    i * new Vector3(scale, 0, 0) + j * new Vector3(0, 0, scale),
                                    (i + 1) * new Vector3(scale, 0, 0) + (j + 1) * new Vector3(0, 0, scale) +
                                    new Vector3(scale/250f, 0, scale/250f),
                                    i * new Vector2(250, 0) + j * new Vector2(0, 250),
                                    (i + 1) * new Vector2(250, 0) + (j + 1) * new Vector2(0, 250) + new Vector2(1, 1)));
                        }
                        else if (i == xNum - 1 && j < zNum - 1)
                        {
                            meshList.Add(createMesh("meshX" + i.ToString() + "Z" + j.ToString(), tGrayWidth % 250, 251,
                                i * new Vector3(scale, 0, 0) + j * new Vector3(0, 0, scale),
                                i * new Vector3(scale, 0, 0) + new Vector3(scale/250f * (tGrayWidth % 250), 0, scale/250f) +
                                (j + 1) * new Vector3(0, 0, scale),
                                i * new Vector2(250, 0) + j * new Vector2(0, 250),
                                i * new Vector2(250, 0) + new Vector2(tGrayWidth % 250, 1) +
                                (j + 1) * new Vector2(0, 250)));
                        }
                        else if (i < xNum - 1 && j == zNum - 1)
                        {
                            meshList.Add(createMesh("meshX" + i.ToString() + "Z" + j.ToString(), 251, tGrayHeight % 250,
                                i * new Vector3(scale, 0, 0) + j * new Vector3(0, 0, scale),
                                (i + 1) * new Vector3(scale, 0, 0) + j * new Vector3(0, 0, scale) +
                                new Vector3(scale/250f, 0, scale/250f * (tGrayHeight % 250)),
                                i * new Vector2(250, 0) + j * new Vector2(0, 250),
                                (i + 1) * new Vector2(250, 0) + j * new Vector2(0, 150) +
                                new Vector2(1, tGrayHeight % 250)));
                        }
                        else if (i == xNum - 1 && j == zNum - 1)
                        {
                            meshList.Add(createMesh("meshX" + i.ToString() + "Z" + j.ToString(), tGrayWidth % 250,
                                tGrayHeight % 250,
                                i * new Vector3(scale, 0, 0) + j * new Vector3(0, 0, scale),
                                i * new Vector3(scale, 0, 0) + j * new Vector3(0, 0, scale) +
                                new Vector3(scale/250f * (tGrayWidth % 250), 0, scale/250f * (tGrayHeight % 250)),
                                i * new Vector2(250, 0) + j * new Vector2(0, 250),
                                i * new Vector2(250, 0) + j * new Vector2(0, 250) +
                                new Vector2(tGrayWidth % 250, tGrayHeight % 250)));
                        }
                    }
                }

                bCreate = true;
            }
        }
    }

    //加载图片
    IEnumerator loadImage(string imagePath, System.Action<Texture> action)
    {
        WWW www = new WWW("file://" + Application.streamingAssetsPath + "/" + imagePath);
        yield return www;
        if (www.error == null)
        {
            action(www.texture);
        }
    }


    /// <summary>
    ///创建mesh
    /// </summary>
    /// <param name="meshName">mesh名称</param>
    /// <param name="row">行数</param>
    /// <param name="col">列数</param>
    /// <param name="minPoint">最小点位置</param>
    /// <param name="maxPoint">最大点位置</param>
    /// <param name="minImgPosition">最小点灰度图位置</param>
    /// <param name="maxImgPosition">最大点灰度图位置</param>
    /// <returns></returns>
    ///
    private GameObject createMesh(string meshName, int row, int col, Vector3 minPoint, Vector3 maxPoint,
        Vector2 minImgPosition, Vector2 maxImgPosition)
    {
        GameObject meshObject = new GameObject(meshName);

        int verticeNum = row * col;
        Vector3[] vertices = new Vector3[verticeNum]; //顶点数组大小
        int[] triangles = new int[verticeNum * 3 * 2]; //三角集合数组,保存顶点索引
        // Vector3[] normals = new Vector3[verticeNum];//顶点法线数组大小
        Vector2[] uvs = new Vector2[verticeNum];
        float rowF = (float) row;
        float colF = (float) col;
        Vector3 xStep = new Vector3((maxPoint.x - minPoint.x) / rowF, 0, 0);
        Vector3 zSetp = new Vector3(0, 0, (maxPoint.z - minPoint.z) / colF);
        int k = 0;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                float tempZ = texture2dGray.GetPixel((int) minImgPosition.x + i, (int) minImgPosition.y + j).grayscale;
                vertices[i + j * row] = minPoint + xStep * i + zSetp * j + new Vector3(0, tempZ*size.y*scale, 0);

                uvs[i + j * row] = new Vector2((float) i / rowF, (float) j / colF);

                if (j < col - 1 && i < row - 1)
                {
                    triangles[k++] = j * row + i;
                    triangles[k++] = j * row + i + row;
                    triangles[k++] = j * row + i + 1;

                    triangles[k++] = j * row + i + row;
                    triangles[k++] = j * row + i + row + 1;
                    triangles[k++] = j * row + i + 1;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        // mesh.normals = normals;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshObject.AddComponent<MeshFilter>();
        meshObject.AddComponent<MeshRenderer>();
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = meshMaterial;

        return meshObject;
    }
}