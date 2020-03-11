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
        private EndSimulationEntityCommandBufferSystem BufferSystem;
        private EntityQuery _entityQuery;
        private float height = 0.1f;

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
            int solution = 16;
            ti.heightTexture = new Texture2D(solution, solution);
            ti.heightTexture.filterMode = FilterMode.Bilinear;
            ImageConversion.LoadImage(ti.heightTexture, bb);
            if (ti.heightTexture.width <= 4)
            {
                // Invalid texture in local cache, retry
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return false;
            }

//            if (ti.zoomLevel % 3 != 0)
//            {
//                return false;
//            }

            //生成Mesh
            var meshFilter = ti.gameObject.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                return false;
            }


//            ti.heightTexture.filterMode = FilterMode.Bilinear;
//            ti.heightTexture.Resize(solution, solution);
//            ti.heightTexture.Apply();
            MeshNum.UpdateMesh(ti, meshFilter, Vector2.one / (solution+1f), Vector2.one *  (solution+1f));
            Debug.Log(solution);
            Vector3 size = new Vector3(1, 1, Mathf.Pow(0.5f,2));
            meshFilter.mesh =
                HeightMapTools.SetMeshHeight(meshFilter.mesh, ti.heightTexture, new float3x2(Vector3.zero, size));
            ti.gameObject.GetComponent<TileInfoMono>().heightTexture = ti.heightTexture;
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