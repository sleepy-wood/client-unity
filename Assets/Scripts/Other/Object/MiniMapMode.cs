using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniMapMode : MonoBehaviour, IPointerClickHandler
{
    private UserInteract userInteract;
    private UserInput userInput;
    private ArrayLandData arrayLandData;
    private LineRenderer lineRenderer;
    private Camera cam;
    Texture texture;
    Rect rect;
    Vector2 curosr = new Vector2(0, 0);

    private void Start()
    {
        userInteract = GameManager.Instance.User.GetComponent<UserInteract>();
        userInput = GameManager.Instance.User.GetComponent<UserInput>();
        arrayLandData = FileManager.LoadDataFile<ArrayLandData>("LandData");
        lineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
        lineRenderer.SetPositions(new Vector3[10]);
        cam = GetComponent<Camera>();
        rect = GetComponent<RawImage>().rectTransform.rect;
        texture = GetComponent<RawImage>().texture;
    }
    private void Update()
    {
        
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (userInput.LongInteract)
        {
            Debug.Log("롱");
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RawImage>().rectTransform,
                eventData.pressPosition, eventData.pressEventCamera, out curosr))
            {
                float coordX = Mathf.Clamp(0, (((curosr.x - rect.x) * texture.width) / rect.width), texture.width);
                float coordY = Mathf.Clamp(0, (((curosr.y - rect.y) * texture.height) / rect.height), texture.height);

                float calX = coordX / texture.width;
                float calY = coordY / texture.height;

                curosr = new Vector2(calX, calY);

                Ray MapRay = cam.ScreenPointToRay(new Vector2(curosr.x * cam.pixelWidth,
                    curosr.y * cam.pixelHeight));

                RaycastHit hit;

                if (Physics.Raycast(MapRay, out hit, Mathf.Infinity, LayerMask.NameToLayer("Ground")))
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

        /*
         * Vector3 mousePos = Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(mousePos);
        Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity, Color.red);
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
    else
    {

        //Camera.main.gameObject.SetActive(true);
    }
        */

    }
}
