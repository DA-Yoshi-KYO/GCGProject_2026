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
        onesPalce.sprite = numberSprites[ones];
        tensPalce.sprite = numberSprites[tens];
    }

    public void SetTensView(bool isActive)
    {
        tensPalce.gameObject.SetActive(isActive);
    }
}
