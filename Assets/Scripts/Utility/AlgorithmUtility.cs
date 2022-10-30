using Cysharp.Threading.Tasks;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AlgorithmUtility
{
    public static List<int> BFS(List<BridgeFromTo> bridgeFroms, int startId, int endId, int landCount)
    {
        bool[] visited = new bool[landCount + 1];
        int[] parent  = new int[landCount + 1];

        List<int> paths = new List<int>();
        List<List<int>> nodes = new List<List<int>>();
        //초기화
        for (int i = 0; i < landCount + 1; i++)
        {
            List<int> list = new List<int>();
            nodes.Add(list);
        }
        //parent는 자기 자신으로 초기화
        for(int i = 0; i < landCount +1; i++)
        {
            parent[i] = i;
        }

        //연결 노드 간의 관계 설정 
        for(int i = 0; i < bridgeFroms.Count; i++)
        {
            nodes[bridgeFroms[i].fromLandId].Add(bridgeFroms[i].toLandId);
            nodes[bridgeFroms[i].toLandId].Add(bridgeFroms[i].fromLandId);
        }

        //BFS를 사용한 길찾기
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startId);
        visited[startId] = true;
        parent[startId] = startId;

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            for (int i = 0; i < nodes[node].Count; i++)
            {
                if (visited[nodes[node][i]] == false)
                {
                    visited[nodes[node][i]] = true;
                    queue.Enqueue(nodes[node][i]);
                    parent[nodes[node][i]] = node;
                }
            }
        }

        //Calculate Path
        int nodeId = endId;
        while (parent[nodeId] != nodeId)
        {
            paths.Add(nodeId);
            nodeId = parent[nodeId];
        }
        paths.Add(nodeId);
        paths.Reverse();

        return paths;
    }
    
}
