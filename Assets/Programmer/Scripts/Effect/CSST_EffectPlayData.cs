/*
+=====================================
 ファイル名 : CSST_EffectPlayData.cs
 概要     : Effectの再生に必要なデータクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

using UnityEngine;

/// <summary>
/// Effectの再生に必要なデータです。
/// 呼び出し側で必要な情報だけ設定して、Effect再生処理へ渡します。
/// Inspector上では「指定するかどうか」と「値」を編集します。
/// </summary>
[System.Serializable]
public struct CSST_EffectPlayData
{
    [Header("再生時間")]
    [SerializeField]
    private bool b_UsePlayTime;

    [SerializeField]
    private float f_InspectorPlayTime;

    [Header("終了時間")]
    [SerializeField]
    private bool b_UseEndTime;

    [SerializeField]
    private float f_InspectorEndTime;

    [Header("ループ設定")]
    [SerializeField]
    private bool b_UseLoopFlag;

    [SerializeField]
    private bool b_InspectorLoopFlag;

    [Header("終了時非表示")]
    [SerializeField]
    private bool b_UseHideOnEnd;

    [SerializeField]
    private bool b_InspectorHideOnEnd;

    [Header("自動終了時間")]
    [SerializeField]
    private bool b_UsePlayEndTime;

    [SerializeField]
    private float f_InspectorPlayEndTime;

    /// <summary>
    /// Effectを再生する位置です。
    /// 位置は基本Transformを使うため、Inspectorからは編集しません。
    /// </summary>
    private Vector3? v3_OverridePosition;

    /// <summary>
    /// Effectを再生する回転です。
    /// 回転は基本Transformを使うため、Inspectorからは編集しません。
    /// </summary>
    private Quaternion? q_OverrideRotation;

    /// <summary>
    /// Effectを再生する大きさです。
    /// Scaleは基本Transformを使うため、Inspectorからは編集しません。
    /// </summary>
    private Vector3? v3_OverrideScale;

    public Vector3? v3_Position
    {
        get { return v3_OverridePosition; }
    }

    public Quaternion? q_Rotation
    {
        get { return q_OverrideRotation; }
    }

    public Vector3? v3_Scale
    {
        get { return v3_OverrideScale; }
    }

    public float? f_PlayTime
    {
        get
        {
            if (b_UsePlayTime == false)
            {
                return null;
            }

            return f_InspectorPlayTime;
        }
    }

    public float? f_EndTime
    {
        get
        {
            if (b_UseEndTime == false)
            {
                return null;
            }

            return f_InspectorEndTime;
        }
    }

    public bool? b_LoopFlag
    {
        get
        {
            if (b_UseLoopFlag == false)
            {
                return null;
            }

            return b_InspectorLoopFlag;
        }
    }

    public bool? b_HideOnEnd
    {
        get
        {
            if (b_UseHideOnEnd == false)
            {
                return null;
            }

            return b_InspectorHideOnEnd;
        }
    }

    public float? f_PlayEndTime
    {
        get
        {
            if (b_UsePlayEndTime == false)
            {
                return null;
            }

            return f_InspectorPlayEndTime;
        }
    }

    public void CSST_EffectPlayData_Init()
    {
        v3_OverridePosition = null;
        q_OverrideRotation = null;
        v3_OverrideScale = null;

        b_UsePlayTime = false;
        f_InspectorPlayTime = 0.0f;

        b_UseEndTime = false;
        f_InspectorEndTime = 0.0f;

        b_UseLoopFlag = false;
        b_InspectorLoopFlag = false;

        b_UseHideOnEnd = false;
        b_InspectorHideOnEnd = false;

        b_UsePlayEndTime = false;
        f_InspectorPlayEndTime = 0.0f;
    }

    public void SetPosition(Vector3 v_Position)
    {
        v3_OverridePosition = v_Position;
    }

    public void SetRotation(Quaternion q_Rotation)
    {
        q_OverrideRotation = q_Rotation;
    }

    public void SetScale(Vector3 sc_Scale)
    {
        v3_OverrideScale = sc_Scale;
    }

    public void SetPlayTime(float f_PlayTime)
    {
        b_UsePlayTime = true;
        f_InspectorPlayTime = f_PlayTime;
    }

    public void SetEndTime(float f_EndTime)
    {
        b_UseEndTime = true;
        f_InspectorEndTime = f_EndTime;
    }

    public void SetLoopFlag(bool b_LoopFlag)
    {
        b_UseLoopFlag = true;
        b_InspectorLoopFlag = b_LoopFlag;
    }

    public void SetHideOnEnd(bool b_HideOnEnd)
    {
        b_UseHideOnEnd = true;
        b_InspectorHideOnEnd = b_HideOnEnd;
    }

    public void SetPlayEndTime(float f_PlayEndTime)
    {
        b_UsePlayEndTime = true;
        f_InspectorPlayEndTime = f_PlayEndTime;
    }
}
