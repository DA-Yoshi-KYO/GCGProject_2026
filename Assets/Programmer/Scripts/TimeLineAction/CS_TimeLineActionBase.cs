using UnityEngine;
using System.Collections;

public abstract class CS_TimeLineActionBase : MonoBehaviour
{
    [SerializeField] protected float duration = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public abstract void PlayAction();
    public abstract IEnumerator Action();
}
