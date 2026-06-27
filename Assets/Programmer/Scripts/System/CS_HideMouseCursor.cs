using UnityEngine;

public class CS_HideMouseCursor : MonoBehaviour
{
    [Header("カーソルの非表示")][SerializeField] private bool hideCursorInEditor = true;

    void Start()
    {
#if UNITY_EDITOR
        Cursor.visible = hideCursorInEditor ? false : true;
#else
        Cursor.visible = false;
#endif
    }
}
