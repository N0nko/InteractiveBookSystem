using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenTransition : MonoBehaviour
{
    Vector3 scale, position;
    RectTransform thisRect;
    public RectTransform targetTransform;
  
    // Start is called before the first frame update
    void Start()
    {
        thisRect = gameObject.GetComponent<RectTransform>();
        scale = thisRect.sizeDelta;
        position = transform.position;
        Invoke("ScaleOut", 1f);
        Invoke("ScaleIn", 3f);
    }

    void ScaleOut() {

        LeanTween.size(thisRect, targetTransform.sizeDelta, .5f);
        LeanTween.move(gameObject, targetTransform.position, .5f);
    
    
    }
    void ScaleIn()
    {

        LeanTween.size(thisRect, scale, .5f);
        LeanTween.move(gameObject, position, .5f);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
