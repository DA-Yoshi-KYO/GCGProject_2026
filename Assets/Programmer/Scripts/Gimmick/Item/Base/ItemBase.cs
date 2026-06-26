using UnityEngine;

public class ItemBase : MonoBehaviour
{
    [SerializeField]
    private Gimmick gimmickTag;

    public Gimmick GetGimmickTag()
    {
        return gimmickTag;
    }
}
