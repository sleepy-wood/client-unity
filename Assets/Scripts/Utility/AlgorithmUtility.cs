using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AlgorithmUtility
{
    public static List<int> DFS(List<BridgeFromTo> bridgeFroms, int startId, int endId, int landCount)
    {
        bool[] visited = new bool[landCount];

        List<int> path = new List<int>();
        path.Add(startId);

        for (int i = 0; i < bridgeFroms.Count; i++)
        {
            //if()
        }
        return null;
    }
}
