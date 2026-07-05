using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [Header("ゲームオブジェクト")]
    [SerializeField] private GameObject BastetIcon;
    [SerializeField] private GameObject BastetOutLineIcon;
    [SerializeField] private GameObject DetailTextImage;
    [SerializeField] private GameObject DistanceImage;
    [SerializeField] private GameObject NumberView;

    [Header("スプライト")]
    [SerializeField] private Sprite Moveing;
    [SerializeField] private Sprite Pinch;

    int Distance = 100;

    private void Awake()
    {
        BastetOutLineIcon.SetActive(false);
        DetailTextImage.SetActive(false);
    }

    private void FixedUpdate()
    {
     
    }


    void ViewSetting(int i)
    {
        switch(i)
        {
            case 0:
                {
                }
            break;
            case 1:
                {
                }
            break;
            case 2:
                {

                }
            break;
        }
        return;
    }

}
