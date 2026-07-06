using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [Header("ゲームオブジェクト")]
    [SerializeField] private GameObject BastetIcon;
    [SerializeField] private GameObject BastetOutLineIcon;
    [SerializeField] private GameObject DetailTextImage;
    [SerializeField] private GameObject DistanceImage;
    [SerializeField] private GameObject NumberView;
    [SerializeField] private GameObject FrameIcon;

    [Header("スプライト")]
    [SerializeField] private Sprite Moveing;
    [SerializeField] private Sprite Pinch;

    CS_VisionTarget visionTarget;
    NumberView numberView;
    int Distance = 100;
    bool isDestroyCheck = false;

    private void Awake()
    {
        BastetOutLineIcon.SetActive(false);
        DetailTextImage.SetActive(false);
        numberView = NumberView.GetComponent<NumberView>();
    }

    private void FixedUpdate()
    {
        if (visionTarget == null)
        {
            if (!isDestroyCheck)
            {
                isDestroyCheck = true;
                return;
            }
            Debug.Log("宝物が破棄されました。");
            BastetIcon.SetActive(false);
            BastetOutLineIcon.SetActive(false);
            DetailTextImage.SetActive(false);
            DistanceImage.SetActive(false);
            NumberView.SetActive(false);
            FrameIcon.SetActive(true);
            return;
        }

        isDestroyCheck = false;

        // 宝物が盗まれているかどうかを確認

        if (visionTarget.read_IsStolenMoveing)
        {
            SetViewMoveing();
        }
        else
        {
            BastetIcon.SetActive(true);
            BastetOutLineIcon.SetActive(false);
            DetailTextImage.SetActive(false);
        }

        Distance = visionTarget.read_ExitDistance;

        // 距離表示

        if (numberView != null)
        {
            numberView.SetNumber(Distance);
        }
    }


    void SetViewMoveing()
    {
        BastetIcon.SetActive(false);
        BastetOutLineIcon.SetActive(true);
        DetailTextImage.SetActive(true);

        if (Distance <= 10)
        {
            DetailTextImage.GetComponent<UnityEngine.UI.Image>().sprite = Pinch;
        }
        else
        {
            DetailTextImage.GetComponent<UnityEngine.UI.Image>().sprite = Moveing;
        }
    }
    
    public void SetCS(CS_VisionTarget target)
    {
        visionTarget = target;
    }

}
