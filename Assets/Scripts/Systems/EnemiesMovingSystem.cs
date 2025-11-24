using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct EnemiesMovingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        EnemiesMoveJob enemiesMoveJob = new EnemiesMoveJob
        {
            playerLocation = SystemAPI.GetComponent<LocalTransform>(player).Position,
            DeltaTime = SystemAPI.Time.DeltaTime,
            ecb = ecb.AsParallelWriter()
        };

        enemiesMoveJob.ScheduleParallel(state.Dependency).Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        EntityCommandBuffer ecb2 = new EntityCommandBuffer(Allocator.TempJob);

        GetPathJob getPathJob = new GetPathJob
        {
            ecb = ecb2.AsParallelWriter()
        };

        getPathJob.ScheduleParallel(state.Dependency).Complete();

        ecb2.Playback(state.EntityManager);
        ecb2.Dispose();

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[BurstCompile]
public partial struct EnemiesMoveJob : IJobEntity
{
    public float3 playerLocation;
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter ecb;

    [BurstCompile]
    public void Execute([EntityIndexInQuery] int entityInQueryIndex, EnabledRefRO<MovingEnemy> movingEnemy, ref DynamicBuffer<VerticesEnemies> verticesEnemies, ref DynamicBuffer<NodesPath> path, ref EnemyImportantNodes eImportantNodes, ref LocalTransform localTransform, ref EnemyTargetPosition enemyTargetPosition, in EnemyRange enemyRange, in EnemySpeed enemySpeed, Entity entity)
    {
        if (math.distance(localTransform.Position, playerLocation) <= enemyRange.attackRange)
        {
            //ataco
        }
        else
        {
            if (math.distance(verticesEnemies[eImportantNodes.currentNode.y * 29 + eImportantNodes.currentNode.x].value, verticesEnemies[eImportantNodes.targetNode.y * 29 + eImportantNodes.targetNode.x].value) < 0.1f)
            {
                //he llegado al nodo target
                eImportantNodes.currentNode = eImportantNodes.targetNode;

                if (math.distance(localTransform.Position, playerLocation) <= enemyRange.range)
                {
                    //veo al jugador
                    if (math.distance(playerLocation, verticesEnemies[eImportantNodes.endNode.y * 29 + eImportantNodes.endNode.x].value) < 5f)
                    {
                        //estoy yendo hacia el jugador -> siguiente nodo
                        if (path.Length > 0)
                        {
                            eImportantNodes.targetNode = path[path.Length - 1].value;
                            path.RemoveAt(path.Length - 1);
                        }
                    }
                    else
                    {
                        //pido camino hacia el jugador
                        enemyTargetPosition.position = playerLocation;
                        ecb.SetComponentEnabled<MovingEnemy>(entityInQueryIndex, entity, false);
                        ecb.SetComponentEnabled<RequestPath>(entityInQueryIndex, entity, true);
                    }
                }
                else
                {
                    //no veo al jugador
                    if (eImportantNodes.endNode.Equals(eImportantNodes.originNode))
                    {
                        //estoy yendo a casa
                        if (path.Length > 0)
                        {
                            eImportantNodes.targetNode = path[path.Length - 1].value;
                            path.RemoveAt(path.Length - 1);
                        }
                    }
                    else
                    {
                        //pido camino a casa
                        enemyTargetPosition.position = verticesEnemies[eImportantNodes.originNode.y * 29 + eImportantNodes.originNode.x].value;
                        ecb.SetComponentEnabled<MovingEnemy>(entityInQueryIndex, entity, false);
                        ecb.SetComponentEnabled<RequestPath>(entityInQueryIndex, entity, true);
                    }
                }
            }
            else
            {
                //mover enemigo
                float3 verticeTarget = verticesEnemies[eImportantNodes.targetNode.y * 29 + eImportantNodes.targetNode.x].value;
                float3 direction = verticeTarget - localTransform.Position;
                float3 normalizedDirection = math.normalize(direction);

                //Debug.Log(normalizedDirection);

                //localTransform.Position.xyz += normalizedDirection * enemySpeed.speed * DeltaTime;
                float3 forward = new float3(normalizedDirection.x, 0f, normalizedDirection.z);

                localTransform = new LocalTransform { Position = new float3(localTransform.Position.x + normalizedDirection.x * enemySpeed.speed * DeltaTime, localTransform.Position.y + normalizedDirection.y * enemySpeed.speed * DeltaTime, localTransform.Position.z + normalizedDirection.z * enemySpeed.speed * DeltaTime), Rotation = quaternion.LookRotation(forward, math.up()), Scale = 1f };

                //float3 forward = new float3(normalizedDirection.x, 0f, normalizedDirection.z);
                //localTransform.Rotation = quaternion.LookRotation(forward, math.up());
            }
        }
    }
}

[BurstCompile]
public partial struct GetPathJob : IJobEntity
{

    public EntityCommandBuffer.ParallelWriter ecb;

    [BurstCompile]
    public void Execute([EntityIndexInQuery] int entityInQueryIndex, EnabledRefRO<RequestPath> requestPath, ref DynamicBuffer<VerticesEnemies> verticesEnemies, ref DynamicBuffer<NodesPath> nodesPath, ref EnemyImportantNodes eImportantNodes, in EnemyTargetPosition enemyTargetPosition, Entity entity)
    {
        NativeArray<float> gCostArray = new NativeArray<float>(29 * 29, Allocator.Temp);
        NativeArray<float> hCostArray = new NativeArray<float>(29 * 29, Allocator.Temp);
        NativeArray<float> fCostArray = new NativeArray<float>(29 * 29, Allocator.Temp);
        NativeArray<int2> parentsArray = new NativeArray<int2>(29 * 29, Allocator.Temp);

        NativeList<int2> openList = new NativeList<int2>(29 * 29, Allocator.Temp);
        NativeList<int2> closedList = new NativeList<int2>(29 * 29, Allocator.Temp);

        float3 targetLocation = enemyTargetPosition.position;

        openList.Add(eImportantNodes.currentNode);

        float3 endLocation = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
        int2 endNode = eImportantNodes.currentNode;

        for (int y = 0; y < 29; y++) 
        {
            for (int x = 0; x < 29; x++) 
            {
                gCostArray[y * 29 + x] = float.MaxValue;
                hCostArray[y * 29 + x] = math.distancesq(verticesEnemies[y * 29 + x].value, targetLocation);
                parentsArray[y * 29 + x] = new int2(-2, -2);
                if (math.distancesq(verticesEnemies[y * 29 + x].value, targetLocation) < math.distancesq(targetLocation, endLocation))
                {
                    endLocation = verticesEnemies[y * 29 + x].value;
                    endNode = new int2(x, y);
                }
            }
        }

        eImportantNodes.endNode = endNode;

        gCostArray[eImportantNodes.currentNode.y * 29 + eImportantNodes.currentNode.x] = 0;
        fCostArray[eImportantNodes.currentNode.y * 29 + eImportantNodes.currentNode.x] = gCostArray[eImportantNodes.currentNode.y * 29 + eImportantNodes.currentNode.x] + hCostArray[eImportantNodes.currentNode.y * 29 + eImportantNodes.currentNode.x];

        while (!openList.IsEmpty)
        {
            //Check Lowest fNode
            int2 lowestFCostNode = openList[0];
            int indexLowest = 0;
            for (int i = 1; i < openList.Length; i++)
            {
                if ( fCostArray[openList[i].y * 29 + openList[i].x] < fCostArray[lowestFCostNode.y * 29 + lowestFCostNode.x])
                {
                    lowestFCostNode = openList[i];
                    indexLowest = i;
                }
            }

            if (lowestFCostNode.Equals(eImportantNodes.endNode))
            {
                //Final Node Reached
                NativeList<NodesPath> listPath = new NativeList<NodesPath>(Allocator.Temp);

                listPath.Add(new NodesPath { value = lowestFCostNode });
                int2 auxNode = lowestFCostNode;
                while (!parentsArray[auxNode.y * 29 + auxNode.x].Equals(new int2(-2, -2)))
                {
                    listPath.Add(new NodesPath { value = parentsArray[auxNode.y * 29 + auxNode.x] });
                    auxNode = parentsArray[auxNode.y * 29 + auxNode.x];
                }

                eImportantNodes.targetNode = listPath[listPath.Length - 1].value;
                listPath.RemoveAt(listPath.Length - 1);

                ecb.SetBuffer<NodesPath>(entityInQueryIndex, entity).CopyFrom(listPath.AsArray());
                ecb.SetComponentEnabled<MovingEnemy>(entityInQueryIndex, entity, true);
            }

            openList.RemoveAt(indexLowest);
            closedList.Add(lowestFCostNode);

            //Check Neighbours
            NativeList<int2> neighboursList = new NativeList<int2>(Allocator.Temp);

            if (lowestFCostNode.x - 1 >= 0 && lowestFCostNode.y - 1 >= 0) //TopLeft
            {
                neighboursList.Add(new int2(lowestFCostNode.x - 1, lowestFCostNode.y - 1));
            }
            if (lowestFCostNode.y - 1 >= 0) //Top
            {
                neighboursList.Add(new int2(lowestFCostNode.x, lowestFCostNode.y - 1));
            }
            if (lowestFCostNode.x + 1 < 29 && lowestFCostNode.y - 1 >= 0) //TopRight
            {
                neighboursList.Add(new int2(lowestFCostNode.x + 1, lowestFCostNode.y - 1));
            }
            if (lowestFCostNode.x - 1 >= 0) //Left
            {
                neighboursList.Add(new int2(lowestFCostNode.x - 1, lowestFCostNode.y));
            }
            if (lowestFCostNode.x + 1 < 29) //Right
            {
                neighboursList.Add(new int2(lowestFCostNode.x + 1, lowestFCostNode.y));
            }
            if (lowestFCostNode.x - 1 >= 0 && lowestFCostNode.y + 1 < 29) //BottomLeft
            {
                neighboursList.Add(new int2(lowestFCostNode.x - 1, lowestFCostNode.y + 1));
            }
            if (lowestFCostNode.y + 1 < 29) //Bottom
            {
                neighboursList.Add(new int2(lowestFCostNode.x, lowestFCostNode.y + 1));
            }
            if (lowestFCostNode.x + 1 < 29 && lowestFCostNode.y + 1 < 29) //BottomRight
            {
                neighboursList.Add(new int2(lowestFCostNode.x + 1, lowestFCostNode.y + 1));
            }

            foreach (int2 neighbourNode in neighboursList)
            {
                if (closedList.Contains(neighbourNode)) continue;

                float newGCost = gCostArray[lowestFCostNode.y * 29 + lowestFCostNode.x] + math.distancesq(verticesEnemies[lowestFCostNode.y * 29 + lowestFCostNode.x].value , verticesEnemies[neighbourNode.y * 29 + neighbourNode.x].value);
                if (newGCost < gCostArray[neighbourNode.y * 29 + neighbourNode.x])
                {
                    parentsArray[neighbourNode.y * 29 + neighbourNode.x] = lowestFCostNode;
                    gCostArray[neighbourNode.y * 29 + neighbourNode.x] = newGCost;
                    fCostArray[neighbourNode.y * 29 + neighbourNode.x] = gCostArray[neighbourNode.y * 29 + neighbourNode.x] + hCostArray[neighbourNode.y * 29 + neighbourNode.x];

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }

            }

        }
        //Out of Nodes

    }
}
