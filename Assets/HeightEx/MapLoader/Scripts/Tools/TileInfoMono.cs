using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPM;

public class TileInfoMono : MonoBehaviour
{
    public TileInfo TileInfo;
    public Texture2D heightTexture;
    public static int loadIndex=20;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (loadIndex<0)
            {
                return;
            }

            loadIndex--;
            Debug.Log(TileInfo.x+" "+TileInfo.y+" "+TileInfo.zoomLevel);
            var uv=TileInfo.gameObject.GetComponent<MeshFilter>().mesh.uv;
            for (int i = 0; i < uv.Length; i++)
            {
                Debug.Log(uv[i]);
            }
        }
    }
}
