using UnityEngine;
using UnityEngine.UI;

public class CS_TitleButton : MonoBehaviour
{
    [Header("非選択時のテクスチャ")][SerializeField] Sprite offTexture;
    [Header("選択時のテクスチャ")][SerializeField] Sprite onTexture;
    Image currentImage;

    void Start()
    {
        currentImage = GetComponent<Image>();
        currentImage.sprite = offTexture;
    }

    public void ChangeTexture(bool isSelect)
    {
        currentImage.sprite = isSelect ? onTexture : offTexture;
    }
}
