using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UserInput : MonoBehaviour
{
    private const string MoveNameX = "Horizontal";
    private const string MoveNameZ = "Vertical";
    private const string LeftClickName = "Fire1";
    private const string RightClickName = "Fire2";
    private const string MouseXName = "Mouse X";

    public Text currnetStateText;
    public Text clickText;

#if  UNITY_IOS || UNITY_ANDROID
    /// <summary>
    /// Touch State ����
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
    /// X������ �̵�
    /// </summary>
    public float MoveX { get; private set; }
    /// <summary>
    /// Z������ �̵�
    /// </summary>
    public float MoveZ { get; private set; }

    /// <summary>
    /// Zoom In / Out ����
    /// </summary>
    public float Zoom { get; private set; }

    /// <summary>
    /// Mouse Interact ���콺 ��Ŭ��, ����� ��ġ
    /// </summary>
    public bool Interact { get; private set; }

    /// <summary>
    /// Mouse Drag -> Rotate
    /// </summary>
    public float Rotate { get; private set; }

    private void Update()
    {
#if UNITY_STANDALONE
        #region �̵� 
        MoveX = Input.GetAxisRaw(MoveNameX);
        MoveZ = Input.GetAxisRaw(MoveNameZ);
        #endregion

        #region interact
        Interact = Input.GetButtonDown(LeftClickName);
        #endregion

        #region ����/�ܾƿ�
        Zoom = Input.GetAxis("Mouse ScrollWheel");
        #endregion

        #region ȸ�� 
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
        //���� Touch�� �Ѽհ������� ���� ��� Move ��������, Touch�������� Ȯ��
        //Move ����: �ణ�� �������� ������ �ȴٸ� Move
        //Touch ����: �������� ������ ���� ���� ���¿��� �հ����� ���� ��
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
        }

        #region �̵� 
        if (currentState == TouchState.Move)
        {
            clickText.text = "Ŭ������: ���";
            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
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
        #endregion

        #region interact

        if (currentState == TouchState.Touch)
        {
            clickText.text = "Ŭ������: Ŭ��";
            Interact = true;
            currentState = TouchState.None;
        }
        else
            Interact = false;
        #endregion

        
        if (Input.touchCount == 2)
        {
        #region ����/�ܾƿ�
            Touch touchFirstFinger = Input.GetTouch(0); 
            Touch touchSecondFinger = Input.GetTouch(1); 

            Vector2 touchFirstFingerPos = touchFirstFinger.position - touchFirstFinger.deltaPosition;
            Vector2 touchSecondFingerPos = touchSecondFinger.position - touchSecondFinger.deltaPosition;

            float prevTouchDeltaMag = (touchFirstFingerPos - touchSecondFingerPos).magnitude;
            float touchDeltaMag = (touchFirstFinger.position - touchSecondFinger.position).magnitude;

            Zoom = prevTouchDeltaMag - touchDeltaMag;
        #endregion

        #region ȸ�� 

        #endregion

        }
        else
        {
            Zoom = 0;
        }

#endif
    }
}
