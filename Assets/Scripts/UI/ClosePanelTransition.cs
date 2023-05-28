using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosePanelTransition : MonoBehaviour
{

    CanvasGroup canvasGroup;
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ClosePanel() {

        LeanTween.alphaCanvas(canvasGroup, 0f, .5f).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
    
}
