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
            Vector3 mousePos = Input.mousePosition;
            Ray ray = GetComponent<Camera>().ScreenPointToRay(mousePos);
            Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.NameToLayer("Ground")))
            {
                GameObject Land = userInteract.OnLand();
                Debug.Log(Land);
                if (Land && 
                    int.Parse(hit.transform.name[hit.transform.name.Length - 1].ToString()) != int.Parse(Land.name[Land.name.Length - 1].ToString()))
                {
                    ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>("LandData");
                    List<int> path = AlgorithmUtility.BFS(
                        arrayLandData.bridgeInfo, int.Parse(Land.name[Land.name.Length - 1].ToString()),
                        int.Parse(hit.transform.name[hit.transform.name.Length - 1].ToString()),
                        LandDataManager.Instance.transform.childCount - 1);

                    for (int i = 0; i < path.Count; i++)
                    {
                        print(path[i]);
                    }
                    ShowPath(path);
                }
            }
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
