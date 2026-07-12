using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NumberView : MonoBehaviour
{
    [SerializeField] Image onesPalce;
    [SerializeField] Image tensPalce;

    [SerializeField] Sprite[] numberSprites;

    [Header("1桁ずつ増減させる際の間隔（秒）")]
    [SerializeField] private float stepInterval = 0.15f;

    [Header("1桁分のスライドアニメーションの再生時間（秒）")]
    [SerializeField] private float slideDuration = 0.12f;

    [Header("スライドの移動距離（ピクセル）")]
    [SerializeField] private float slideDistance = 40f;

    int currentNumber = 0;
    int animateTargetNumber = 0;
    bool hasAnimatedOnce = false;
    Coroutine animateRoutine;

    public void SetNumber(int number)
    {
        if (animateRoutine != null)
        {
            StopCoroutine(animateRoutine);
            animateRoutine = null;
        }

        ApplyNumber(number);
        currentNumber = number;
        animateTargetNumber = number;
    }

    /// <summary>
    /// 現在の表示数から目標の数まで、1つずつスライドしながら増減させて表示する処理。
    /// 初回表示時はアニメーションせずそのまま反映する。
    /// </summary>
    public void AnimateToNumber(int number)
    {
        if (number < 0 || number > 99)
        {
            SetNumber(number);
            return;
        }

        animateTargetNumber = number;

        if (!hasAnimatedOnce)
        {
            hasAnimatedOnce = true;
            SetNumber(number);
            return;
        }

        if (animateRoutine == null)
        {
            animateRoutine = StartCoroutine(AnimateRoutine());
        }
    }

    private IEnumerator AnimateRoutine()
    {
        while (currentNumber != animateTargetNumber)
        {
            int step = animateTargetNumber > currentNumber ? 1 : -1;
            int nextNumber = currentNumber + step;

            yield return StartCoroutine(PlayStepSlide(currentNumber, nextNumber));

            currentNumber = nextNumber;

            if (currentNumber != animateTargetNumber)
            {
                yield return new WaitForSecondsRealtime(stepInterval);
            }
        }

        animateRoutine = null;
    }

    private IEnumerator PlayStepSlide(int from, int to)
    {
        bool up = to > from;
        int toOnes = to % 10;
        int toTens = to / 10;
        int fromTens = from / 10;

        // 一の位は毎回スライドさせる
        yield return StartCoroutine(SlideDigit(onesPalce, toOnes, up));

        // 十の位が変わる場合（繰り上がり／繰り下がり）はそちらもスライドさせる
        if (fromTens != toTens && tensPalce != null && tensPalce.gameObject.activeSelf)
        {
            yield return StartCoroutine(SlideDigit(tensPalce, toTens, up));
        }
    }

    private IEnumerator SlideDigit(Image digitImage, int newDigitValue, bool up)
    {
        if (digitImage == null) yield break;
        if (numberSprites == null || newDigitValue < 0 || newDigitValue >= numberSprites.Length) yield break;

        RectTransform rt = digitImage.rectTransform;
        Vector2 basePos = rt.anchoredPosition;
        float direction = up ? 1f : -1f;
        float half = Mathf.Max(0.01f, slideDuration * 0.5f);

        // 前半：現在の数字が奥へスライドして消える
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            rt.anchoredPosition = basePos - new Vector2(0, direction * slideDistance * t);
            yield return null;
        }

        // 数字を切り替えて反対側から戻す
        digitImage.sprite = numberSprites[newDigitValue];
        rt.anchoredPosition = basePos + new Vector2(0, direction * slideDistance);

        // 後半：新しい数字が元の位置へスライドして戻る
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            rt.anchoredPosition = Vector2.Lerp(basePos + new Vector2(0, direction * slideDistance), basePos, t);
            yield return null;
        }

        rt.anchoredPosition = basePos;
    }

    private void ApplyNumber(int number)
    {
        if (number < 0 || number > 99)
        {
            onesPalce.sprite = numberSprites[10];
            tensPalce.sprite = numberSprites[10];
            return;
        }

        if (numberSprites == null || numberSprites.Length < 10)
        {
            Debug.LogError("NumberView: numberSpritesには0～9の10個のSpriteを設定してください。", this);
            return;
        }

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
