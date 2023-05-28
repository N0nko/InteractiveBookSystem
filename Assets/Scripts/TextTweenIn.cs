using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTweenIn : MonoBehaviour
{
    // Start is called before the first frame update
    public int tweenDir = -1;
    public float delay;
    public float duration = 1;
    float startPos = 9999;
    void OnEnable()
    {
        if (startPos == 9999) startPos = transform.localPosition.x;
        LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 0, 0);
        LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 1, duration).setEaseInCubic().setDelay(delay);

        LeanTween.moveLocalX(gameObject, startPos + 200 * tweenDir, 0);
        LeanTween.moveLocalX(gameObject, startPos, duration - 0.3f).setEaseOutCubic().setDelay(delay);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
