using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warning : MonoBehaviour
{
    [SerializeField] private GameObject warningTextObject;
    [SerializeField] private float warningDuration = 2f;

    private float warningTimer = 0f;

    private void Awake()
    {
        if (warningTextObject == null)
        {
            Debug.LogError("Warning Text Object is not assigned.");
            return;
        }
        warningTextObject.SetActive(false);
    }

    private void Update()
    {
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f)
            {
                HideWarning();
            }
        }
    }

    public void ShowWarning()
    {
        warningTextObject.SetActive(true);
        warningTimer = warningDuration;
    }

    private void HideWarning()
    {
        warningTextObject.SetActive(false);
    }
}
