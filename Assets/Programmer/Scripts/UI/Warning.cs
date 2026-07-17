using System.Collections;
using UnityEngine;

public class Warning : MonoBehaviour
{
    [SerializeField] private GameObject warningTextObject;
    [SerializeField] private float warningDuration = 2f;

    [Header("回転アニメーション設定")]
    [SerializeField] private float rotateInDuration = 0.3f;  // UIの裏から表に回転してくる時間
    [SerializeField] private float rotateOutDuration = 0.3f; // 表からUIの裏に回転して収納される時間
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // 回転させる軸（裏に回り込む向き）
    [SerializeField] private float hiddenAngle = 90f; // UIの裏に隠れているときの回転角度

    private RectTransform warningRectTransform;
    private CanvasGroup warningCanvasGroup;
    private Quaternion shownRotation;
    private Quaternion hiddenRotation;
    private Coroutine warningRoutine;

    private void Awake()
    {
        if (warningTextObject == null)
        {
            Debug.LogError("Warning Text Object is not assigned.");
            return;
        }

        warningRectTransform = warningTextObject.GetComponent<RectTransform>();
        shownRotation = warningRectTransform != null ? warningRectTransform.localRotation : Quaternion.identity;
        hiddenRotation = shownRotation * Quaternion.Euler(rotationAxis * hiddenAngle);

        warningCanvasGroup = warningTextObject.GetComponent<CanvasGroup>();
        if (warningCanvasGroup == null)
        {
            warningCanvasGroup = warningTextObject.AddComponent<CanvasGroup>();
        }

        warningTextObject.SetActive(false);
    }

    public void ShowWarning()
    {
        if (warningTextObject == null)
        {
            Debug.LogWarning("Warning Text Object is not assigned.", this);
            return;
        }

        if (warningRoutine != null)
        {
            StopCoroutine(warningRoutine);
        }
        warningRoutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        warningTextObject.SetActive(true);
        if (warningCanvasGroup != null) warningCanvasGroup.alpha = 1f;

        // UIの裏側から回転しながら表に出てくる
        yield return RotateRoutine(hiddenRotation, shownRotation, rotateInDuration, 1f, 1f);

        yield return new WaitForSecondsRealtime(warningDuration);

        // 回転しながら透明度を下げつつUIの裏に収納される
        yield return RotateRoutine(shownRotation, hiddenRotation, rotateOutDuration, 1f, 0f);

        warningTextObject.SetActive(false);
        warningRoutine = null;
    }

    private IEnumerator RotateRoutine(Quaternion from, Quaternion to, float duration, float alphaFrom, float alphaTo)
    {
        if (warningRectTransform == null || duration <= 0f)
        {
            if (warningRectTransform != null) warningRectTransform.localRotation = to;
            if (warningCanvasGroup != null) warningCanvasGroup.alpha = alphaTo;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // イーズアウト

            warningRectTransform.localRotation = Quaternion.Slerp(from, to, eased);
            if (warningCanvasGroup != null) warningCanvasGroup.alpha = Mathf.Lerp(alphaFrom, alphaTo, eased);

            yield return null;
        }

        warningRectTransform.localRotation = to;
        if (warningCanvasGroup != null) warningCanvasGroup.alpha = alphaTo;
    }
}
