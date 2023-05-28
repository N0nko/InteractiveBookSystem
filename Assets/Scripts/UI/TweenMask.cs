using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenMask : MonoBehaviour
{
    public float start = 0, end = 1, time = .5f;
    Image fillObject;
    CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void OnEnable()
    {
        fillObject = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        LeanTween.value(gameObject, updateValueCallback, start, end, time).setEaseOutCubic();
        canvasGroup.alpha = 1;
        //LeanTween.alphaCanvas(canvasGroup, end, time-0.1f);
    }
    void updateValueCallback(float val, float ratio)
    {
        fillObject.fillAmount = val;
       // canvasGroup.alpha = val;


    }
    private void OnDisable()
    {
        fillObject.fillAmount = 0;
      //  canvasGroup.alpha = 0;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
