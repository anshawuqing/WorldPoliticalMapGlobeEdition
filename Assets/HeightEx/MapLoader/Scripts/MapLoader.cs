using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WPM;

public class MapLoader : MonoBehaviour
{
    public static MapLoader instance;
    public Camera mainCamera;
    public WorldMapGlobe WorldMapGlobe;
    internal  Dictionary<int, TileInfo> TileInfos;
    private  EntityManager manager;
    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        mainCamera = Camera.main;
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        TileInfos = WorldMapGlobe.cachedTiles;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void AddLoadEx(TileInfo ti)
    {
        var index =WorldMapGlobe.GetTileHashCode(ti.x , ti.y, ti.zoomLevel);
        HeightMapIndex mapIndex = new HeightMapIndex();
        mapIndex.hasCode = index;
        var meshnum = ti.gameObject.AddComponent<MeshNum>();
        meshnum._cellSize = Vector2.one / 4;
        meshnum._gridSize = Vector2.one * 4;
        manager.AddComponentData<HeightMapIndex>(manager.CreateEntity(), mapIndex);
    }
    
}