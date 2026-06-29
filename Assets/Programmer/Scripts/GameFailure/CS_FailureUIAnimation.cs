using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CS_FailureUIAnimation : CS_ResultUIAnimationBase
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
        [Tooltip("アニメーションの種類")] public ImageKind kind;          // アニメーションの種類
        [Tooltip("アニメーション用データ登録")] public TransitionData data;
    }
    [Header("外部から格納するUI情報"), SerializeField] private InputUIData[] inputDatas = null;


    // 内部で使用するUIのMap
    struct UIData
    {
        public Image image;
        public RectTransform rectTransform;
    }
    private Dictionary<ImageKind, TransitionData> datas = new Dictionary<ImageKind, TransitionData>();

    // 経過時間計測
    float timer = 0.0f;

    private void Awake()
    {
        // データ登録
        foreach (var item in inputDatas)
        {
            datas[item.kind] = item.data;
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
