using UnityEngine;
using UnityEngine.UI;

public class Danger : MonoBehaviour
{
    [SerializeField] private GameObject frameTop;
    [SerializeField] private GameObject frameBottom;
    [SerializeField] private GameObject Text;

    private Image textImage;

    private void Awake()
    {
        frameTop.SetActive(false);
        frameBottom.SetActive(false);
        Text.SetActive(false);
        textImage = Text.GetComponent<Image>();
    }

    private void Update()
    {
        if (textImage == null) return;
        // 点滅表示
        float alpha = Mathf.PingPong(Time.time, 1f);
        Color color = textImage.color;
        color.a = alpha;
        textImage.color = color;
    }



    public void SetFrame(bool isActive)
    {
        frameBottom.SetActive(isActive);
        frameTop.SetActive(isActive);
        Text.SetActive(isActive);
    }
}
