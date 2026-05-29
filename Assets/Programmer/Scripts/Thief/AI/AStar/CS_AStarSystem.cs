/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のA*アルゴリズムに関するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*アルゴリズムに関するシステム
/// </summary>
public class CS_AStarSystem
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [Tooltip("移動ルート")]
    private List<Transform> moveRoute;
    public IReadOnlyList<Transform> read_MoveRoute => moveRoute;

    [Tooltip("ルートが構築されているかどうか")]
    public bool HasRoute => moveRoute != null && moveRoute.Count > 0;

    [Tooltip("ルートを更新したかどうか")]
    private bool isRouteUpdated = true;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="thiefAI">ThiefAIスクリプトへの参照</param>
    public CS_AStarSystem(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
    }

    /// <summary>
    /// A* 探索で扱う「部屋ノード」情報
    /// </summary>
    private sealed class RouteNode
    {
        /// <summary>この探索ノードが表す部屋</summary>
        public CS_RoomNode Room;

        /// <summary>ひとつ前のノード（経路復元に使用）</summary>
        public RouteNode Parent;

        /// <summary>
        /// Parent.Room -> Room に行く時に通過する「Parent側」のドア。
        /// moveRouteにはこのドアTransformを並べる。
        /// </summary>
        public Transform ViaDoor;

        /// <summary>開始から現在部屋までの実コスト</summary>
        public float G;

        /// <summary>現在部屋から終点部屋までの推定コスト</summary>
        public float H;

        /// <summary>評価値（小さいほど優先度が高い）</summary>
        public float F => G + H;

        public RouteNode(CS_RoomNode room, RouteNode parent, Transform viaDoor, float g, float h)
        {
            Room = room;
            Parent = parent;
            ViaDoor = viaDoor;
            G = g;
            H = h;
        }
    }

    /// <summary>
    /// 「ある部屋から隣接部屋へ移動する」ための辺情報。
    /// </summary>
    private struct NeighborEdge
    {
        public CS_RoomNode NextRoom;
        public Transform Door;
        public float Cost;

        public NeighborEdge(CS_RoomNode nextRoom, Transform door, float cost)
        {
            NextRoom = nextRoom;
            Door = door;
            Cost = cost;
        }
    }

    /// <summary>
    /// ルートを構築する処理
    /// </summary>
    /// <param name="end">ルートの終点</param>
    public void ConstructionRoute(Transform end)
    {
        // -------- 前提チェック --------
        // currentRoom が取れていない場合はルート構築できない
        if (thiefAI.read_MemorySystem.read_CurrentRoom == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute: currentRoom が nullです。");
            return;
        }
        // 終点が無い場合もルート構築できない
        if (end == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute: end が nullです。");
            return;
        }

        // --------ルートリストの初期化 --------
        //既存のルートが残っていると誤動作するので毎回クリアする
        if (moveRoute == null) moveRoute = new List<Transform>();
        moveRoute.Clear();

        // -------- 終点が属する部屋(endRoom)を特定 --------
        // end は Transformなので、その Transform が属する RoomCreatePoint を Raycast 等で取得し、
        //そこから RoomNode を引き当てる
        CS_RoomNode endRoom = null;
        try
        {
            GameObject endRoomObj = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(end.gameObject);
            if (endRoomObj != null)
            {
                endRoom = endRoomObj.GetComponentInChildren<CS_RoomNode>();
            }
        }
        catch
        {
            // Raycast実装やシーン状態によって例外が起きる可能性があるため、ここでは握りつぶす
            // （取得できない場合はフォールバックを行う）
        }

        //取得できなかった場合のフォールバック：firstRoom を終点部屋とみなす
        // （通常は firstEntryPoint が firstRoom にある想定のため、ここで破綻しにくくする）
        if (endRoom == null)
        {
            endRoom = thiefAI.read_MemorySystem.read_FirstRoom;
        }

        //それでも取れない場合は中断
        if (endRoom == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute: 終点部屋(endRoom)を特定できません。");
            return;
        }

        // -------- A*探索の準備 --------
        // open : 探索候補ノード（まだ確定していない）
        // closed : 確定済みノード（これ以上更新しない）
        // bestG : 各部屋(RoomNode)に到達する最小コスト(G)の記録
        var open = new List<RouteNode>();
        var closed = new HashSet<CS_RoomNode>();
        var bestG = new Dictionary<CS_RoomNode, float>();

        // 開始ノード：現在部屋
        // - G:開始なので0
        // - H:ヒューリスティック（ここでは部屋座標間距離）
        RouteNode start = new RouteNode(thiefAI.read_MemorySystem.read_CurrentRoom, null, null, 0f, Heuristic(thiefAI.read_MemorySystem.read_CurrentRoom, endRoom));
        open.Add(start);
        bestG[thiefAI.read_MemorySystem.read_CurrentRoom] = 0f;

        // 終点到達時にここへ入る
        RouteNode goal = null;

        // -------- A*探索 --------
        // open が空になるまで探索（=到達不可）
        while (open.Count > 0)
        {
            // open の中から、評価値 F = G + H が最小のノードを選ぶ
            // ※優先度付きキューではなく線形探索（部屋数が少ない前提）
            int bestIndex = 0;
            float bestF = open[0].F;
            for (int i = 1 ; i < open.Count ; i++)
            {
                float f = open[i].F;
                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }

            // 最小Fのノードを current として取り出す
            RouteNode current = open[bestIndex];
            open.RemoveAt(bestIndex);

            // 終点部屋に到達したら探索終了
            if (current.Room == endRoom)
            {
                goal = current;
                break;
            }

            // 確定済みに追加
            closed.Add(current.Room);

            // 現在部屋から行ける隣接部屋を列挙
            foreach (var edge in GetNeighbors(current.Room))
            {
                // 隣接が取得できない場合はスキップ
                if (edge.NextRoom == null) continue;

                // 「行ったことのある部屋のみ」を通る（記憶に無い部屋は通らない）
                if (!thiefAI.read_MemorySystem.read_RoomMemorys.ContainsKey(edge.NextRoom)) continue;

                // closed に入っている部屋は再評価しない
                if (closed.Contains(edge.NextRoom)) continue;

                // Gコスト更新：現在までのコスト + 隣接へ行くコスト
                float tentativeG = current.G + edge.Cost;

                //既に「より良い経路(Gが小さい)」が記録されているなら更新しない
                float recordedG;
                if (bestG.TryGetValue(edge.NextRoom, out recordedG) && tentativeG >= recordedG)
                {
                    continue;
                }

                // この部屋への最良Gを更新
                bestG[edge.NextRoom] = tentativeG;

                // open 内に同じ部屋が存在するか確認（あれば親などを更新、なければ追加）
                RouteNode exist = null;
                for (int i = 0 ; i < open.Count ; i++)
                {
                    if (open[i].Room == edge.NextRoom)
                    {
                        exist = open[i];
                        break;
                    }
                }

                // ヒューリスティック（終点までの推定距離）
                float h = Heuristic(edge.NextRoom, endRoom);

                if (exist != null)
                {
                    //既存ノード更新
                    // Parent :どの部屋から来たか
                    // ViaDoor: Parent -> exist.Room に行くために通るドア(Parent側)
                    exist.Parent = current;
                    exist.ViaDoor = edge.Door;
                    exist.G = tentativeG;
                    exist.H = h;
                }
                else
                {
                    // 新規ノード追加
                    open.Add(new RouteNode(edge.NextRoom, current, edge.Door, tentativeG, h));
                }
            }
        }

        // -------- 探索結果の判定 --------
        // goal が null のままなら到達できなかった（もしくは訪問済み制限で経路が作れなかった）
        if (goal == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute:ルート構築に失敗（到達不可 or 訪問済み部屋が不足） 現在:" + thiefAI.read_MemorySystem.read_CurrentRoom.name + " 終点:" + endRoom.name);
            return;
        }

        // --------ルート復元（goalから startへ親を辿る） --------
        // goalから Parent を辿る形で復元すると「逆順」になるため、いったんreversedDoors に積んでから反転します。
        // moveRoute に入れるのは「各部屋から次の部屋へ行くために通るドア(Transform)」です。
        var reversedDoors = new List<Transform>();
        RouteNode node = goal;
        while (node != null && node.Parent != null)
        {
            // node.ViaDoor は「node.Parent.Roomから node.Room に行くためのドア(Parent側)」
            if (node.ViaDoor != null)
            {
                reversedDoors.Add(node.ViaDoor);
            }
            node = node.Parent;
        }

        // start -> goal の順になるように reverseして moveRouteへ設定
        for (int i = reversedDoors.Count - 1 ; i >= 0 ; i--)
        {
            moveRoute.Add(reversedDoors[i]);
        }

        // -------- 最終目的地(end)をルート末尾に追加 --------
        if (end != null)
        {
            // 二重追加を避ける（既に末尾がendなら追加しない）
            if (moveRoute.Count == 0 || moveRoute[moveRoute.Count - 1] != end)
            {
                moveRoute.Add(end);
            }
        }

        isRouteUpdated = true;
    }

    /// <summary>
    /// 指定した部屋から、接続している隣接部屋の一覧（辺）を列挙する。
    /// </summary>
    private IEnumerable<NeighborEdge> GetNeighbors(CS_RoomNode from)
    {
        if (from == null) yield break;

        // RoomNode は RoomCreatePoint の子として配置されている前提
        var createPoint = from.transform.parent != null ? from.transform.parent.GetComponent<CS_RoomCreatePoint>() : null;
        if (createPoint == null) yield break;

        // 接続しているドア方向（Right/Left/Front/Back）
        List<CSE_RoomDoorDirection> dirs = createPoint.GetConnectDirections();
        if (dirs == null) yield break;

        foreach (var dir in dirs)
        {
            // from側のドアTransform（後で moveRoute に積むのはこの Transform）
            Transform door = from.GetDirectionWallToDoor(dir);

            // 接続先 RoomNode
            CS_RoomMoveConnection connection;
            if (!createPoint.TryGetConnection(dir, out connection) || connection == null || connection.TargetCreatePoint == null) continue;

            CS_RoomNode nextRoom = connection.TargetCreatePoint.GetComponentInChildren<CS_RoomNode>();
            if (nextRoom == null) continue;

            // コスト：部屋中心間の距離（取得できなければ1 とする）
            float cost = 1f;
            try
            {
                cost = Vector3.Distance(from.transform.position, nextRoom.transform.position);
            }
            catch
            {
                cost = 1f;
            }

            yield return new NeighborEdge(nextRoom, door, cost);
        }
    }

    /// <summary>
    /// A* のヒューリスティック関数。
    /// </summary>
    private float Heuristic(CS_RoomNode a, CS_RoomNode b)
    {
        if (a == null || b == null) return 0f;
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    /// <summary>
    /// ルートをクリアする処理
    /// </summary>
    public void ClearRoute()
    {
        if (moveRoute != null)
        {
            moveRoute.Clear();
        }
    }

    /// <summary>
    /// ルートの更新処理
    /// </summary>
    /// <param name="exploredDistanceThreshold"></param>
    public void UpdateRoute(float exploredDistanceThreshold)
    {
        if (moveRoute == null) return;

        // ルートの先頭の要素を取得
        Transform targetPoint = moveRoute[0];

        // ルートの先頭が null の場合
        if (targetPoint == null)
        {
            // 先頭の要素をルートから削除
            moveRoute.RemoveAt(0);
            isRouteUpdated = true;
        }

        // 移動システムを使って移動
        if (isRouteUpdated)
        {
            thiefAI.read_MoveSystem.MoveTo(targetPoint.position);
            isRouteUpdated = false;
        }
    }

    /// <summary>
    /// ワープ処理（ルートの先頭を飛ばす）
    /// </summary>
    public void WarpAction()
    {
        if (moveRoute == null || moveRoute.Count == 0) return;

        // 先頭の要素をルートから削除
        moveRoute.RemoveAt(0);
        isRouteUpdated = true;

        // ルートの先頭が無くなった場合は、移動ルートを削除
        if (moveRoute.Count == 0)
        {
            moveRoute = null;
        }
    }

    /// <summary>
    /// ルートの最終目的地の取得
    /// </summary>
    /// <returns>ルートの最終目的地の座標</returns>
    public Vector3 GetTargetPoint()
    {
        if (moveRoute == null || moveRoute.Count == 0) return Vector3.zero;
        Transform targetPoint = moveRoute[moveRoute.Count - 1];
        if (targetPoint != null)
        {
            return targetPoint.position;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
