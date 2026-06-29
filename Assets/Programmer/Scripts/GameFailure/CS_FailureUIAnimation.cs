using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CS_FailureUIAnimation : MonoBehaviour
{
    // アニメーションの遷移
    private enum AnimationPhase
    {
        FallFailureMessage
    }
    AnimationPhase phase = AnimationPhase.FallFailureMessage;

    // アニメーションに使用するオブジェクトの種類
    public enum ImageKind
    {
        FailureMessageBack,
        FailureMessage,
    }


    // 外部から格納するUI情報
    [Serializable]
    public class InputUIData
    {
        [Tooltip("アニメーションに使用するオブジェクト")] public GameObject imageObject;  // アニメーションに使用するオブジェクト
        [Tooltip("アニメーションの種類")] public ImageKind kind;          // アニメーションの種類

    }
    [Header("外部から格納するUI情報"), SerializeField] private InputUIData[] inputDatas = null;

    // 内部で使用するUIのMap
    struct UIData
    {
        public Image image;
        public RectTransform rectTransform;
    }
    private Dictionary<ImageKind, UIData> datas = new Dictionary<ImageKind, UIData>();

    // 経過時間計測
    float timer = 0.0f;

    private void Awake()
    {
        // データ登録
        UIData data;
        foreach (var item in inputDatas)
        {
            data.image = item.imageObject.GetComponent<Image>();
            data.rectTransform = item.imageObject.GetComponent<RectTransform>();

            datas[item.kind] = data;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        switch (phase)
        {
            case AnimationPhase.FallFailureMessage:
                PhaseFallFailureMessage();
                break;
            default:
                break;
        }
    }

    private void PhaseFallFailureMessage()
    {

    }
}
