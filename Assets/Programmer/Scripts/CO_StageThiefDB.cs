/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のA*アルゴリズムに関するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-1 | 初回作成
 * 
 */
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージ別の盗賊出現データを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "CO_StageThiefDB", menuName = "ScriptableObjects/CO_StageThiefDB", order = 1)]
public class CO_StageThiefDB : ScriptableObject
{
    /// <summary>
    /// ステージ別の盗賊出現データを管理するクラス
    /// </summary>
    [Serializable]
    public class StageThiefData
    {
        /// <summary>
        /// ウェーブ別の盗賊出現データを管理するクラス
        /// </summary>
        [Serializable]
        public class WaveThiefData
        {
            /// <summary>
            /// 部屋別の盗賊出現データを管理するクラス
            /// </summary>
            [Serializable]
            public class EnemtEntryData
            {
                [Header("部屋の識別")]
                public string roomName; // 例: "CreateRoomPoint01"など

                /// <summary>
                /// 敵の出入口方向別の盗賊出現データを管理するクラス
                /// </summary>
                [Serializable]
                public class ThiefEntryDoorDirInfo
                {
                    [Header("敵の入ってくる方向")]
                    public CSE_RoomDoorDirection enemyDoorDir;

                    [Header("出現候補の盗賊データ")]
                    public List<CO_ThiefStatusData> thiefStatusDatas;
                }
                [Header("登録する敵出入口データ")]
                public List<ThiefEntryDoorDirInfo> thiefEntryDoorDirInfos;
            }

            [Header("部屋別の情報")]
            public List<EnemtEntryData> enemtEntryDatas;
        }

        [Header("ウェーブ別情報")]
        public List<WaveThiefData> waveData;
    }

    [Header("ステージ別の敵出現データ")]
    public List<StageThiefData> stageThiefDataList;
}
