using UnityEngine;
using UnityEngine.UI;

public class NumberView : MonoBehaviour
{
    [SerializeField] Image onesPalce;
    [SerializeField] Image tensPalce;

    [SerializeField] Sprite[] numberSprites;

    int currentNumber = 0;

    public void SetNumber(int number)
    {
        if (number < 0 || number > 99) return;
        currentNumber = number;
        int ones = number % 10;
        int tens = number / 10;
        if (numberSprites == null || numberSprites.Length < 10)
        {
            Debug.LogError("NumberView: numberSpritesには0～9の10個のSpriteを設定してください。", this);
            return;
        }
        onesPalce.sprite = numberSprites[ones];
        tensPalce.sprite = numberSprites[tens];
    }

    public void SetTensView(bool isActive)
    {
        tensPalce.gameObject.SetActive(isActive);
    }
}
