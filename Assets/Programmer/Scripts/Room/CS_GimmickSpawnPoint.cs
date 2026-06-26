using UnityEngine;

public class CS_GimmickSpawnPoint : MonoBehaviour
{
    public void GimmickSpawn(GameObject gimmick)
    {
        if (!gimmick.CompareTag("Item"))
        {
            Debug.LogWarning("ポップさせるオブジェクトにItemタグが付いていません。");
            return;
        }

        Transform gimmickTransform = Instantiate(gimmick, transform.position, Quaternion.identity).transform;
        gimmickTransform.SetParent(GameObject.Find("DropItems").transform);
    }
}
