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
    RectTransform rectTransform;
    NumberView numberView;
    int Distance = 100;
    bool isDestroyCheck = false;

    Vector3 scale;

    private void Awake()
    {
        BastetOutLineIcon.SetActive(false);
        DetailTextImage.SetActive(false);
        numberView = NumberView.GetComponent<NumberView>();
        rectTransform = GetComponent<RectTransform>();
        scale =rectTransform.localScale;
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
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scale, Time.deltaTime * 5f);
            BastetIcon.SetActive(false);
            BastetOutLineIcon.SetActive(false);
            DetailTextImage.SetActive(false);
            DistanceImage.SetActive(false);
            NumberView.SetActive(false);
            FrameIcon.SetActive(true);
            return;
        }

        FrameIcon.SetActive(false);

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
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scale * 1.25f, Time.deltaTime * 5f);
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
