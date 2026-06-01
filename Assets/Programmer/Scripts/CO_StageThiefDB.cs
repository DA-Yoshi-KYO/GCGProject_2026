using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CO_StageThiefDB", menuName = "ScriptableObjects/CO_StageThiefDB", order = 1)]
public class CO_StageThiefDB : ScriptableObject
{
    [Serializable]
    public class StageThiefData
    {
        [Serializable]
        public class EnemtEntryData
        {
            [Header("部屋の識別")]
            public string roomName; // 例: "CreateRoomPoint01"など

            [Serializable]
            public class ThiefEntryDoorDirInfo
            {
                [Header("敵の入ってくる方向")]
                public CSE_RoomDoorDirection enemyDoorDir;

                [Header("敵生成位置として使うRoomMovePoint")]
                public CSS_RoomEnemyEntryData waveDataBase;
            }
            [Header("登録する敵出入口データ")]
            public List<ThiefEntryDoorDirInfo> thiefEntryDoorDirInfos;
        }

        [Header("部屋別の情報")]
        public List<EnemtEntryData> enemtEntryDatas;
    }

    [Header("ステージ別情報")]
    public List<StageThiefData> thiefData;
}
