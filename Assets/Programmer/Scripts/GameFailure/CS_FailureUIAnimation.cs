using UnityEngine;

public class CS_FailureUIAnimation : CS_ResultUIAnimationBase
{
    [Header("外部から格納するUI情報"), SerializeField] private TransitionData[] inputDatas =
    {
        new TransitionData()
        {
            transitionPos = new Vector3Data() { init = new Vector3(0f, 0f, 0f), target = new Vector3(0f, 0f, 0f) },
            transitionScale = new Vector3Data() { init = new Vector3(1f, 1f, 1f), target = new Vector3(1f, 1f, 1f) },
            transitionRotate = new Vector3Data() { init = new Vector3(0f, 0f, 0f), target = new Vector3(0f, 0f, 0f) },
            transitionColor = new ColorData() { init = new Color(1f, 1f, 1f, 1f), target = new Color(1f, 1f, 1f, 1f) },

            useInitPos = true,
            useInitScale = true,
            useInitRotate = true,
            useInitColor = true,

            posEaseKind = Easing.EaseKind.Linear,
            scaleEaseKind = Easing.EaseKind.Linear,
            rotateEaseKind = Easing.EaseKind.Linear,
            colorEaseKind = Easing.EaseKind.Linear,

            imageObject = null
        }
    };

    void Start()
    {
        
    }

    void Update()
    {
        if (phase.Length <= progressIndex)
            return;

        float duration = phase[progressIndex].animationDuration;
        timer += Time.deltaTime;

        foreach (int index in phase[progressIndex].sameTimeAnimationIndex)
        {
            TransitionData data = inputDatas[index];
            GameObject imageObject = data.imageObject;
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            if (!data.useInitPos)
            {
                rectTransform.anchoredPosition = data.transitionPos.init +
                (data.transitionPos.target - data.transitionPos.init) *
                Easing.Ease(data.posEaseKind, timer, duration);
            }
            if (!data.useInitScale)
            {
                rectTransform.localScale = data.transitionScale.init +
                (data.transitionScale.target - data.transitionScale.init) *
                Easing.Ease(data.scaleEaseKind, timer, duration);
            }
            if (!data.useInitRotate)
            {
                Vector3 initRotateRadian = new Vector3(
                    data.transitionRotate.init.x * Mathf.Deg2Rad,
                    data.transitionRotate.init.y * Mathf.Deg2Rad,
                    data.transitionRotate.init.z * Mathf.Deg2Rad);
                Vector3 targetRotateRadian = new Vector3(
                    data.transitionRotate.target.x * Mathf.Deg2Rad,
                    data.transitionRotate.target.y * Mathf.Deg2Rad,
                    data.transitionRotate.target.z * Mathf.Deg2Rad);
                rectTransform.Rotate(initRotateRadian +
                (targetRotateRadian - initRotateRadian) *
                Easing.Ease(data.rotateEaseKind, timer, duration));
            }
            if (!data.useInitColor)
            {
                UnityEngine.UI.Image image = imageObject.GetComponent<UnityEngine.UI.Image>();
                image.color = data.transitionColor.init +
                (data.transitionColor.target - data.transitionColor.init) *
                Easing.Ease(data.colorEaseKind, timer, duration);
            }
        }

        if (timer > duration)
        {
            progressIndex++;
            timer = 0f;
        }
    }
}
