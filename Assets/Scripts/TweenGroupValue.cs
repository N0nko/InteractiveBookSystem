using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenGroupValue : MonoBehaviour
{
 //   public GameObject[] Items;
    public float start = -200, end = 10, time = .5f;
    // Start is called before the first frame update
    void OnEnable()
    {
        GetComponent<VerticalLayoutGroup>().spacing = start;
        LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 0, 0);
        LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 1, 1);
        LeanTween.value(gameObject, updateValueCallback, start, end, time).setEaseOutCubic().setDelay(0.7f);
        LeanTween.moveLocalX(gameObject, 200, 0);
        LeanTween.moveLocalX(gameObject, 0, 0.7f).setEaseInCubic();
    }

    void updateValueCallback(float val, float ratio)
    {
        GetComponent<VerticalLayoutGroup>().spacing = val;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
