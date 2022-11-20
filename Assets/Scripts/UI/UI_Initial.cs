using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Broccoli.Factory;
using Broccoli.Pipe;
using Broccoli.Generator;
using System;

public class UI_Initial : MonoBehaviour
{
    #region Variable

    [Header("Menu Bar Canvas")]
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private GameObject shareCanvas;
    [SerializeField] private GameObject marketCanvas;

    
    [Space]

    
    public GameObject profileUI;
    public GameObject myCollectionUI;
    public GameObject screenShotCam;
    public GameObject sleepDataUI;
    public GameObject plantNameUI;
    public GameObject bottomUI;
    public GameObject landCanvas;
    public GameObject customCanvas;

    public RectTransform developerUI;
    public RectTransform dayCountFlag;
    public Transform previewTreePos;

    public Text txtAge;
    public Text treeName;
    public Text txtTreeBirth;

    public Button btnPlantName;

    public InputField inputPlantName;
    public InputField inputYear;
    public InputField inputMonth;
    public InputField inputDay;
    public InputField inputHour;
    public InputField inputSeed;

    public TimeManager timeManager;

    #endregion

    

    private void Start()
    {
        inputPlantName.onValueChanged.AddListener(onValueChanged);
        //if (!PhotonNetwork.IsMasterClient)
        //{
        //    gameObject.SetActive(false);
        //}
        //developerUI.anchoredPosition = new Vector2(-570, -211);

    }

    /// <summary>
    /// 건강기록 창 켜기
    /// </summary>
    public void OnClickHealthActive()
    {
        healthCanvas.SetActive(true);
    }
    /// <summary>
    /// 건강기록 창 끄기
    /// </summary>
    public void OnClickHealthNotActive()
    {
        healthCanvas.SetActive(false);
    }

    /// <summary>
    /// 공유하기 창 켜기
    /// </summary>
    public void OnClickShareActive()
    {
        shareCanvas.SetActive(true);
    }
    /// <summary>
    /// 공유하기 창 끄기
    /// </summary>
    public void OnClickShareNotActive()
    {
        shareCanvas.SetActive(false);
    }
    /// <summary>
    /// 마켓 창 켜기
    /// </summary>
    public void OnClickMarketActive()
    {
        marketCanvas.SetActive(true);
    }
    /// <summary>
    /// 마켓 창 끄기
    /// </summary>
    public void OnClickMarketNotActive()
    {
        marketCanvas.SetActive(false);
    }

    /// <summary>
    /// 홈 버튼 ( = 모든 UI 끄기 )
    /// </summary>
    public void OnClickHome()
    {
        healthCanvas.SetActive(false);
        shareCanvas.SetActive(false);
        //marketCanvas.SetActive(false);
        myCollectionUI.SetActive(false);
        // Bottom UI 색 Home 제외 모두 원래대로
        for (int i = 0; i < 5; i++)
        {
            if (i==2)
            {
                // 원래 이미지
                bottomUI.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                // 선택했을 때의 이미지
                bottomUI.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                // 원래 이미지
                bottomUI.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                // 선택했을 때의 이미지
                bottomUI.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 커스텀 UI 활성화, Land UI 비활성화
    /// </summary>
    public void OnClickCustomActive()
    {
        customCanvas.SetActive(true);
    }
    /// <summary>
    /// 커스텀 UI 비활성화, Land UI 활성화
    /// </summary>
    public void OnClickCustomNotActive()
    {
        customCanvas.SetActive(false);
    }


    /// <summary>
    /// 프로필 UI 활성화
    /// </summary>
    public void onProfile()
    {
        profileUI.gameObject.SetActive(true);
        for(int i = 0; i < profileUI.transform.GetChild(1).childCount; i++)
        {
            if (profileUI.transform.GetChild(1).GetChild(i).name == DataTemporary.MyUserData.avatar)
            {
                profileUI.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                profileUI.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
            }
        }
        // Tree Age 변경
        txtAge.text = $"{timeManager.totalPlantDay}세";
        //screenShotCam.transform.parent = previewTreePos;
        //screenShotCam.transform.localPosition = new Vector3(0.42f, 13.73f, -26.06f);
    }
    /// <summary>
    /// 프로필 UI 비활성화
    /// </summary>
    public void onProfileOff()
    {
        profileUI.gameObject.SetActive(false);
    }


    /// <summary>
    /// TreeList UI 활성화
    /// </summary>
    public void OnMyCollection()
    {
        myCollectionUI.gameObject.SetActive(true);
    }
    /// <summary>
    /// TreeList UI 비활성화
    /// </summary>
    public Transform imgCollection;
    public void OnMyCollectionOff()
    {
        myCollectionUI.gameObject.SetActive(false);
        // Bottom UI에서 컬렉션 메뉴 색상 변경
        imgCollection.GetChild(1).gameObject.SetActive(false);
        imgCollection.GetChild(0).gameObject.SetActive(true);
    }

    /// <summary>
    /// MyWorld Scene으로 이동
    /// </summary>
    public void OnClickToWorld()
    {
        PhotonNetwork.LoadLevel("MyWorld");
    }

    /// <summary>
    /// 수면데이터 분석 UI 활성화
    /// </summary>
    public void OnSleepData()
    {
        sleepDataUI.SetActive(true);
    }
    /// <summary>
    /// 수면데이터 분석 UI 비활성화
    /// </summary>
    public void OnSleepDataOff()
    {
        sleepDataUI.SetActive(false);
    }

    /// <summary>
    /// 나무 이름 입력하면 다음 버튼 활성화
    /// </summary>
    /// <param name="s">식물 이름</param>
    void onValueChanged(string s)
    {
        btnPlantName.interactable = true;
    }

    /// <summary>
    /// 나무 이름 결정 버튼 누르면 나무 이름 저장 & UI 비활성화
    /// </summary>
    public void onConfirmPlantName()
    {
        // 나무 이름
        treeName.text = inputPlantName.text;
        GameManager.Instance.treeController.treeName = inputPlantName.text;
        // 나무 탄생일
        DateTime birth = GameManager.Instance.timeManager.firstPlantDate.dateTime;
        string date = $"{birth.Year.ToString()} / {birth.Month.ToString()} / {birth.Day.ToString()}";
        txtTreeBirth.text = date;
        print(txtTreeBirth.text);
        // TreeData 저장
        GameManager.Instance.treeController.SaveTreeData();
        plantNameUI.SetActive(false);
    }

    #region 개발자 모드

    /// <summary>
    /// 개발자 모드 버튼 왔다갔다하는 버튼
    /// </summary>
    bool once;
    public void OnDevloperUI()
    {
        if (!once)
        {
            Vector2 pos = developerUI.anchoredPosition;
            StartCoroutine(UILerp(pos, new Vector2(120, pos.y), 2, developerUI));
            once = true;
        }
        else
        {
            Vector2 pos = developerUI.anchoredPosition;
            StartCoroutine(UILerp(pos, new Vector2(-160, pos.y), 2, developerUI));
            once = false;
        }
        
    }
    /// <summary>
    /// UI Lerp 이동
    /// </summary>
    /// <param name="from"> UI 본래 위치 </param>
    /// <param name="to"> UI가 이동할 위치 </param>
    /// <param name="speed"> UI 이동 속도 </param>
    /// <param name="rect"> 움직일 UI의 RectTransform </param>
    /// <returns></returns>
    IEnumerator UILerp(Vector2 from, Vector2 to, float speed, RectTransform rect)
    {
        float t = 0;
        while (t<1)
        {
            t += speed * Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        developerUI.anchoredPosition = to;
    }
    /// <summary>
    /// 개발자 모드에서 시작 시간 입력 후 확인 버튼 눌렀을 시 호출
    /// </summary>
    public int year;
    public int month;
    public int day;
    public int hour;
    public void OnStartTime()
    {
        bool cond1 = inputYear.text.Length == 4;
        bool cond2 = inputMonth.text.Length > 0;
        bool cond3 = inputDay.text.Length > 0;
        bool cond4 = inputHour.text.Length > 0;
        if (cond1 && cond2 && cond3 && cond4)
        {
            year = int.Parse(inputYear.text);
            month = int.Parse(inputMonth.text);
            day = int.Parse(inputDay.text);
            hour = int.Parse(inputHour.text);

            // 시작 시간 저장
            GameManager.Instance.timeManager.firstPlantDate = new DateTime(year, month, day, hour, 0, 0);

            inputYear.text = "";
            inputMonth.text = "";
            inputDay.text = "";
            inputHour.text = "";
        }

    }
    /// <summary>
    /// 개발자 모드에서 랜덤 시드 번호 입력 후 입력 버튼 눌렀을 시 호출
    /// </summary>
    public void OnRandomSeed()
    {
        Pipeline pipe = GameManager.Instance.treeController.treePipeline;
        pipe.seed = int.Parse(inputSeed.text);
        inputSeed.text = "";
        GameManager.Instance.treeController.PipelineReload();
    }

    /// <summary>
    /// 개발자 모드에서 일차 수 선택했을 때 실행되는 함수들
    /// </summary>
    public void OnClickDay1()
    {
        // 나무 변화
        GameManager.Instance.treeController.SetTree(1);
        StartCoroutine(PlusDayFlag(2, 1));
    }
    public void OnClickDay2()
    {
        GameManager.Instance.treeController.SetTree(2);
        StartCoroutine(PlusDayFlag(2, 2));
    }
    public void OnClickDay3()
    {
        GameManager.Instance.treeController.SetTree(3);
        StartCoroutine(PlusDayFlag(2, 3));
    }
    public void OnClickDay4()
    {
        GameManager.Instance.treeController.SetTree(4);
        StartCoroutine(PlusDayFlag(2, 4));
    }
    public void OnClickDay5()
    {
        GameManager.Instance.treeController.SetTree(5);
        StartCoroutine(PlusDayFlag(2, 5));
    }
    IEnumerator PlusDayFlag(float speed, float day)
    {
        float t = 0;
        while (t < 1)
        {
            t += speed * Time.deltaTime;
            dayCountFlag.sizeDelta = Vector2.Lerp(dayCountFlag.sizeDelta, new Vector2(day*20, dayCountFlag.sizeDelta.y), t);
            yield return null;
        }
        dayCountFlag.sizeDelta = new Vector2(day * 20, dayCountFlag.sizeDelta.y);
    }
    
    /// <summary>
    /// 개발자 모드 창 닫기
    /// </summary>
    public void OnCloseDeveloperMode()
    {
        Vector2 pos = developerUI.anchoredPosition;
        StartCoroutine(UILerp(pos, new Vector2(-570, pos.y), 2, developerUI));
    }
# endregion

    public void OnClickBottomUI(int idx)
    {
        for (int i=0; i<5; i++)
        {
            if (idx == i)
            {
                // 원래 이미지
                bottomUI.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                // 선택했을 때의 이미지
                bottomUI.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                // 원래 이미지
                bottomUI.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                // 선택했을 때의 이미지
                bottomUI.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
            }
        }
    }
}
