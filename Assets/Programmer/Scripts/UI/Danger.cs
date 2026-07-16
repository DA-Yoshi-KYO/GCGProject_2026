using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Danger : MonoBehaviour
{
    [SerializeField] private GameObject frameTop;
    [SerializeField] private GameObject frameBottom;
    [SerializeField] private GameObject Text;
    [SerializeField] private Volume volume;
    private Vignette vignette;

    private Image textImage;

    private void Awake()
    {
        frameTop.SetActive(false);
        frameBottom.SetActive(false);
        Text.SetActive(false);
        textImage = Text.GetComponent<Image>();
        if (!volume)
        {
            Debug.LogError("Volumeがアタッチされていません");
            return;
        }
        if (!volume.profile.TryGet<Vignette>(out vignette))
        {
            Debug.LogError("ボリューム内にVignetteがありません");
            return;
        }
    }

    private void Update()
    {
        if (textImage == null) return;
        // 点滅表示
        float alpha = Mathf.PingPong(Time.time, 1f);
        Color color = textImage.color;
        color.a = alpha;
        textImage.color = color;

        const float minIntensity = 0f;
        const float maxIntensity = 0.2f;
        float actualValue = Mathf.Lerp(minIntensity, maxIntensity, alpha);
        if (vignette != null) vignette.intensity.value = actualValue;
    }



    public void SetFrame(bool isActive)
    {
        frameBottom.SetActive(isActive);
        frameTop.SetActive(isActive);
        Text.SetActive(isActive);
        vignette.active = isActive;
    }
}
