using UnityEngine;
using UnityEngine.UI;

public class NumberView : MonoBehaviour
{
    [SerializeField] Image onesPalce;
    [SerializeField] Image tensPalce;

    [SerializeField] Sprite[] numberSprites;

    public void SetNumber(int number)
    {
        if (number <= -1)
        {
            onesPalce.sprite = numberSprites[10];
            tensPalce.sprite = numberSprites[10];
            return;
        }
        if (number < 0 || number > 99) return;
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
