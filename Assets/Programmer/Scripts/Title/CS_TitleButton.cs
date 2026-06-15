using UnityEngine;
using UnityEngine.UI;

public class CS_TitleButton : MonoBehaviour
{
    [Header("非選択時のテクスチャ")][SerializeField] Sprite offTexture;
    [Header("選択時のテクスチャ")][SerializeField] Sprite onTexture;
    Image currentImage;

    void Awake()
    {
        currentImage = GetComponent<Image>();
        currentImage.sprite = offTexture;
    }

    public void ChangeTexture(bool isSelect)
    {
        if (currentImage != null)
            currentImage.sprite = isSelect ? onTexture : offTexture;
    }
}
