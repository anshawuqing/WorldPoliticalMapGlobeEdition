using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Networking;
using Hash128 = Unity.Entities.Hash128;

//将渲染队列中的数据赋值给渲染组件
public class LoadingQueue : JobComponentSystem
{
    private Queue<LoadCommand> LoadInfos; //读取队列
    private int DeQueueIndex = 15; //限流值
    private EndSimulationEntityCommandBufferSystem bufferSystem;
     

    protected override void OnCreate()
    {
        bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var Concurrent = bufferSystem.CreateCommandBuffer().ToConcurrent();
        inputDeps = Entities.WithName("LoadQueue")
            .ForEach((Entity entity, int entityInQueryIndex, ref LoadCommand command) =>
            {
                //加载判定
                if (command.State == LoadState.Loaded || command.State == LoadState.Loading)
                {
                    //TODO:此处移至加载完成
                    Concurrent.RemoveComponent<LoadCommand>(entityInQueryIndex, entity);
                    return;
                }
                //TODO:限流
                //if (DeQueueIndex < 0) return;
                //DeQueueIndex--;
                command.State = LoadState.Loading;
                //Loading
                string HeightMapUrl="www.";
                var url = HeightMapUrl + "/" + command.zoomLevel + "/" + command.x + "/" + command.y;
                
            }).Schedule(inputDeps);
        return inputDeps;
    }
}


public struct LoadCommand : IComponentData
{
    public int x, y, zoomLevel; //位置及索引
    public int subquadIndex; //父节点下的索引
    public LoadState State; //状态
}

public enum LoadState
{
    Inactive = 0,
    InQueue = 1,
    Loading = 2,
    Loaded = 3
}