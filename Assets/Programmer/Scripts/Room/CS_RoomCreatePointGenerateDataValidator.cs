using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomCreatePointGenerateDataValidator.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePoint生成設定データの検証を行うクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorから生成データ検証処理を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePoint生成設定データの検証を行うクラスです。
/// </summary>
public class CS_RoomCreatePointGenerateDataValidator
{
    private const string ROOM_CREATE_POINT_TAG = "RoomCreatePoint";

    /// <summary>
    /// 生成データが有効か確認します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>有効な場合はtrue。</returns>
    public bool IsValidGenerateData(
        CS_RoomCreatePointGenerateData generateData,
        int index)
    {
        if (generateData == null)
        {
            Debug.LogWarning("[RoomCreatePointGenerateDataValidator] 生成データがnullです。Index : " + index);
            return false;
        }

        if (generateData.RoomCreatePointObject == null)
        {
            Debug.LogWarning("[RoomCreatePointGenerateDataValidator] RoomCreatePointObjectが登録されていません。Index : " + index);
            return false;
        }

        if (generateData.RoomCreatePoint == null)
        {
            Debug.LogWarning("[RoomCreatePointGenerateDataValidator] CS_RoomCreatePointが付いていません : " + generateData.RoomCreatePointObject.name);
            return false;
        }

        if (!IsRoomCreatePointTagValid(generateData.RoomCreatePointObject))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// RoomCreatePointタグが正しく設定されているか確認します。
    /// </summary>
    /// <param name="target">確認対象。</param>
    /// <returns>正しい場合はtrue。</returns>
    private bool IsRoomCreatePointTagValid(GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        try
        {
            if (!target.CompareTag(ROOM_CREATE_POINT_TAG))
            {
                Debug.LogWarning("[RoomCreatePointGenerateDataValidator] RoomCreatePointタグが付いていません : " + target.name);
                return false;
            }
        }
        catch (UnityException)
        {
            Debug.LogError("[RoomCreatePointGenerateDataValidator] Tag「" + ROOM_CREATE_POINT_TAG + "」が存在しません。UnityのTagsに追加してください。");
            return false;
        }

        return true;
    }
}
