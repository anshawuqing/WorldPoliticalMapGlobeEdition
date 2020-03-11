using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using WPM;

namespace HeightEx
{
    public class Loader_S : JobComponentSystem
    {
        public string cachePath;
        public int _tileSize;
        const int TILE_MIN_ZOOM_LEVEL = 5; // min zoom level to show
        private EndSimulationEntityCommandBufferSystem BufferSystem;
        private EntityQuery _entityQuery;

        protected override void OnCreate()
        {
            BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _entityQuery = GetEntityQuery(typeof(HeightMapIndex));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer.Concurrent concurrent = BufferSystem.CreateCommandBuffer().ToConcurrent();
            var dic = MapLoader.instance.TileInfos;
            Entities.WithName("Loader_S")
                .WithoutBurst()
                .ForEach((Entity entity, int entityInQueryIndex, in HeightMapIndex mapIndex) =>
                {
                    LoadTileContentBackground(dic[mapIndex.hasCode]);
                }).Run();

            NativeArray<Entity> entityArray = _entityQuery.ToEntityArray(Allocator.TempJob);
            for (int i = 0; i < entityArray.Length; i++)
            {
                EntityManager.DestroyEntity(entityArray[i]);
            }

            entityArray.Dispose(inputDeps);
            return inputDeps;
        }

        internal bool LoadTileContentBackground(TileInfo ti, string key = null)
        {
            string filePath;

            //GetUrl
            filePath = WebHelper.GetLocalFilePathForUrl(ti);
            //校验
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            //添加Key
            if (key != null)
            {
                filePath += key;
            }

            //读取本地数据
            byte[] bb = System.IO.File.ReadAllBytes(filePath);
            ti.heightTexture = new Texture2D(256, 256);
            ti.heightTexture.LoadImage(bb);
            if (ti.heightTexture.width <= 16)
            {
                // Invalid texture in local cache, retry
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return false;
            }

            //生成Mesh
            var meshFilter = ti.gameObject.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                return false;
            }

            MeshNum.UpdateMesh(ti, meshFilter, Vector2.one / 4, Vector2.one * 4);
            //meshFilter.mesh = HeightMapTools.SetMeshHeight(meshFilter.mesh, ti.heightTexture,
            //    new float3x2(Vector3.zero, Vector3.one));

            ti.loadStatus = TILE_LOAD_STATUS.Loaded;
            //TODO：处理极点
            return true;
        }
    }
}

public struct HeightMapIndex : IComponentData
{
    public int hasCode;
}

public enum MapStyle
{
    simaple,
    height,
    road
}