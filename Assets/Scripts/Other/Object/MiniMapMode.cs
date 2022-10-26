using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapMode : MonoBehaviour
{
    private UserInteract userInteract;
    private UserInput userInput;
    private ArrayLandData arrayLandData;
    private LineRenderer lineRenderer;

    private void Start()
    {
        userInteract = GameManager.Instance.User.GetComponent<UserInteract>();
        userInput = GameManager.Instance.User.GetComponent<UserInput>();
        arrayLandData = FileManager.LoadDataFile<ArrayLandData>("LandData");
        lineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
        lineRenderer.SetPositions(new Vector3[10]);
    }
    private void Update()
    {
        if (userInput.LongInteract)
        {
            List<int> path = AlgorithmUtility.BFS(arrayLandData.bridgeInfo, 1, 4, LandDataManager.Instance.transform.childCount - 1);
            ShowPath(path);
            //GameObject Land = userInteract.OnLand();
            //if (Land)
            //{
            //    Debug.Log("시작2");
            //    ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>("LandData");
            //    List<int> path = AlgorithmUtility.Start_DFS(arrayLandData.bridgeInfo, 1, 4, 5);
            //    for(int i = 0; i < path.Count; i++)
            //    {
            //        print(path[i]);
            //    }
            //}
        }
    }
    /// <summary>
    /// Path를 보여주는 함수
    /// LineRenderer On
    /// </summary>
    /// <param name="path"></param>
    void ShowPath(List<int> path)
    {
        lineRenderer.SetPositions(new Vector3[10]);
        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            //Mark 켜기
            LandDataManager.Instance.transform.GetChild(path[i] - 1).GetChild(0).gameObject.SetActive(true);
            positions[i] = LandDataManager.Instance.transform.GetChild(path[i] - 1).position + new Vector3(0, 10, 0);
            //print(path[i]);
        }
        //for(int i = 0; i < positions.Length; i++)
        //{
        //    print("p =" + positions[i]);
        //}
        lineRenderer.SetPositions(positions);
    }
}
