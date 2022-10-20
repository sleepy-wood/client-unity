using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UserInput : MonoBehaviour
{
    public Text currnetStateText;
    public Text clickText;

#if UNITY_STANDALONE
    private const string MoveNameX = "Horizontal";
    private const string MoveNameZ = "Vertical";
    private const string LeftClickName = "Fire1";
    private const string RightClickName = "Fire2";
    private const string MouseXName = "Mouse X";

#elif UNITY_IOS || UNITY_ANDROID
    /// <summary>
    /// Touch State 설정
    /// </summary>
    enum TouchState
    {
        None,
        Move,
        Touch
    }
    TouchState currentState = TouchState.None;
#endif
    /// <summary>
    /// InputCotrol = true => Control InputSystem
    /// </summary>
    public bool InputControl { get;private set; }

    /// <summary>
    /// X축으로 이동
    /// </summary>
    public float MoveX { get; private set; }
    /// <summary>
    /// Z축으로 이동
    /// </summary>
    public float MoveZ { get; private set; }

    /// <summary>
    /// Zoom In / Out 관련
    /// </summary>
    public float Zoom { get; private set; }

    /// <summary>
    /// Mouse Interact 마우스 좌클릭, 모바일 터치
    /// </summary>
    public bool Interact { get; private set; }

    /// <summary>
    /// Mouse Drag -> Rotate
    /// </summary>
    public float Rotate { get; private set; }


    private void Update()
    {
        if (!InputControl)
        {
#if UNITY_STANDALONE
            #region 이동 
            MoveX = Input.GetAxisRaw(MoveNameX);
            MoveZ = Input.GetAxisRaw(MoveNameZ);
            #endregion

            #region interact
            Interact = Input.GetButtonDown(LeftClickName);
            #endregion

            #region 줌인/줌아웃
            Zoom = Input.GetAxis("Mouse ScrollWheel");
            #endregion

            #region 회전 
            if (Input.GetButton(RightClickName))
            {
                Rotate = Input.GetAxis(MouseXName);
            }
            else
            {
                Rotate = 0;
            }
            #endregion

#elif UNITY_IOS || UNITY_ANDROID
            //현재 Touch를 한손가락으로 했을 경우 Move 상태인지, Touch상태인지 확인
            //Move 상태: 약간의 움직임이 감지가 된다면 Move
            //Touch 상태: 움직임이 감지가 되지 않은 상태에서 손가락을 뗐을 때
            currnetStateText.text = "currentState: " + currentState.ToString();
            if (Input.touchCount == 1)
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (Input.touches[i].phase == TouchPhase.Moved)
                {
                    if (currentState == TouchState.None)
                        currentState = TouchState.Move;
                }
                else if (Input.touches[i].phase == TouchPhase.Ended)
                {
                    currentState = currentState == TouchState.None ? TouchState.Touch : TouchState.None;
                }
            }

            #region 이동 
            if (currentState == TouchState.Move)
            {
                    clickText.text = "\n클릭여부: 노노";
                    Vector3 mousePos = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        //맞은 곳과 플레이어의 방향 벡터 구하기
                        Vector3 dir = hit.point - transform.position;
                        dir.Normalize();
                        MoveX = dir.x;
                        MoveZ = dir.z;
                            currnetStateText.text = $"hit.point = {hit.point} \n transform.position = {transform.position}" +
                        $"\n dir = {dir}\n  MoveX = {MoveX} | MoveZ = {MoveZ}";
                        }
                }
            }
            else
            {
                MoveX = 0;
                MoveZ = 0;
            }
        }
            #endregion

            #region interact

        if (currentState == TouchState.Touch)
        {
                clickText.text = "\n클릭여부: 클릭";
                Interact = true;
            currentState = TouchState.None;
        }
        else
            Interact = false;
            #endregion


        if (Input.touchCount == 2)
        {
            #region 줌인/줌아웃
            Touch touchFirstFinger = Input.GetTouch(0);
            Touch touchSecondFinger = Input.GetTouch(1);

            //움직이기 전 손가락 위치 구하기
            Vector2 touchFirstFingerPos = touchFirstFinger.position - touchFirstFinger.deltaPosition;
            Vector2 touchSecondFingerPos = touchSecondFinger.position - touchSecondFinger.deltaPosition;

            float prevTwoFingerDist = (touchFirstFingerPos - touchSecondFingerPos).magnitude;
            float curTwoFingerDist = (touchFirstFinger.position - touchSecondFinger.position).magnitude;

                //두 손가락의 간격이 달라졌을 경우 -> Zoom
                //currnetStateText.text = Mathf.Abs(prevTwoFingerDist - curTwoFingerDist).ToString();
                if (Mathf.Abs(prevTwoFingerDist - curTwoFingerDist) > 6) {

                Zoom = (prevTwoFingerDist - curTwoFingerDist) * - 0.006f;

            }
            #endregion

            #region 회전
            //두 손가락의 간격이 같은 경우 -> Roate
            else
            {
                for (int i = 0; i < Input.touches.Length; i++)
                {
                    if (Input.touches[i].phase == TouchPhase.Moved)
                    {
                        Rotate = (touchFirstFingerPos.x - touchFirstFinger.position.x) * 0.07f; 
                    }
                }
            }
            #endregion

        }
        else
        {
            Zoom = 0;
            Rotate = 0;
        }

#endif
        }
    }
}
