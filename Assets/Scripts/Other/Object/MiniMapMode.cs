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
    public Camera cam;
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
        rect = transform.GetChild(3).GetComponent<RawImage>().rectTransform.rect;
        texture = transform.GetChild(3).GetComponent<RawImage>().texture;
    }

    /// <summary>
    /// Screen pointer 이벤트 함수
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 curosr = new Vector2(0, 0);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.GetChild(3).GetComponent<RawImage>().rectTransform,
            eventData.pressPosition, eventData.pressEventCamera, out curosr))
        {

            Texture texture = transform.GetChild(3).GetComponent<RawImage>().texture;
            Rect rect = transform.GetChild(3).GetComponent<RawImage>().rectTransform.rect;

            float coordX = Mathf.Clamp(0, (((curosr.x - rect.x) * texture.width) / rect.width), texture.width);
            float coordY = Mathf.Clamp(0, (((curosr.y - rect.y) * texture.height) / rect.height), texture.height);

            float calX = coordX / texture.width;
            float calY = coordY / texture.height;


            curosr = new Vector2(calX, calY);

            CastRayToWorld(curosr);
        }
    }
    /// <summary>
    /// Ray를 World에 쏘는 함수
    /// </summary>
    /// <param name="vec"></param>
    private void CastRayToWorld(Vector2 vec)
    {
        Ray MapRay = cam.ScreenPointToRay(new Vector2(vec.x * cam.pixelWidth,
            vec.y * cam.pixelHeight));

        RaycastHit hit;

        if (Physics.Raycast(MapRay, out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                GameObject Land = userInteract.OnLand();
                if (Land &&
                    int.Parse(hit.transform.name[hit.transform.name.Length - 1].ToString()) != int.Parse(Land.name[Land.name.Length - 1].ToString()))
                {
                    ArrayLandData arrayLandData = FileManager.LoadDataFile<ArrayLandData>("LandData");

                    List<int> path = AlgorithmUtility.BFS(
                        arrayLandData.bridgeInfo, int.Parse(Land.name[Land.name.Length - 1].ToString()),
                        int.Parse(hit.transform.name[hit.transform.name.Length - 1].ToString()),
                        LandDataManager.Instance.transform.childCount - 1);

                    //길을 찾았다면
                    if (path.Count > 1)
                    {
                        ShowPath(path);
                    }
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
        //기존의 기록이 있다면 초기화
        for (int i = 0; i < LandDataManager.Instance.transform.childCount - 1; i++)
        {
            LandDataManager.Instance.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
        }
        lineRenderer.SetPositions(new Vector3[10]);

        //다음 기록 그리기
        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            LandDataManager.Instance.transform.GetChild(path[i] - 1).GetChild(0).gameObject.SetActive(true);
            positions[i] = LandDataManager.Instance.transform.GetChild(path[i] - 1).position + new Vector3(0, 10, 0);
        }
        lineRenderer.SetPositions(positions);
    }
}
