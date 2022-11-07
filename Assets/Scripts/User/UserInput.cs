using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UserInput : MonoBehaviour
{

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
        Touch,
        LongTouch
    }
    TouchState currentState = TouchState.None;
    private float curTime = 0;
#endif
    /// <summary>
    /// InputCotrol = true => Control InputSystem
    /// </summary>
    public bool InputControl { get; set; }

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
    /// Long Touch - 마우스 좌클릭, 모바일 터치
    /// </summary>
    public bool LongInteract { get; private set; }

    /// <summary>
    /// Mouse Drag -> Rotate
    /// </summary>c
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
            LongInteract = Input.GetKey(KeyCode.LeftAlt);
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
            if (Input.touchCount == 1)
            {
                curTime += Time.deltaTime;
                //LongTouch 설정
                if (curTime > 1.5f && currentState == TouchState.None)
                    currentState = TouchState.LongTouch;
                
                for (int i = 0; i < Input.touches.Length; i++)
                {
                    if (Input.touches[i].phase == TouchPhase.Began)
                    {
                        curTime = 0;
                    }
                    else if (Input.touches[i].phase == TouchPhase.Moved)
                    {
                        if (currentState == TouchState.None)
                            currentState = TouchState.Move;
                    }
                    else if (Input.touches[i].phase == TouchPhase.Ended)
                    {
                        currentState = currentState == TouchState.None ? TouchState.Touch : TouchState.None;
                        curTime = 0;
                    }
                }

            #region 이동 
                if (currentState == TouchState.Move)
                {
                    Vector3 mousePos = Input.mousePosition;
                    Ray ray = Camera.main.ScreenPointToRay(mousePos);
                    RaycastHit hit;
                    LayerMask layer = 1 << LayerMask.NameToLayer("Portal");
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layer))
                    {
                        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                            hit.transform.gameObject.layer == LayerMask.NameToLayer("Bridge"))
                        {
                            //맞은 곳과 플레이어의 방향 벡터 구하기
                            Vector3 dir = hit.point - transform.position;
                            dir.Normalize();
                            MoveX = dir.x;
                            MoveZ = dir.z;
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
                Interact = true;
                currentState = TouchState.None;
            }
            else if (currentState == TouchState.LongTouch)
            {
                LongInteract = true;
                currentState = TouchState.None;
            }
            else
            {
                LongInteract = false;
                Interact = false;
            }
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
                if (Mathf.Abs(prevTwoFingerDist - curTwoFingerDist) > 6)
                {

                    Zoom = (prevTwoFingerDist - curTwoFingerDist) * -0.006f;

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

