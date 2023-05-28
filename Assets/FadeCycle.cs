using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeCycle : MonoBehaviour
{
    Image image;
    Color minColor = new Color(1, 1, 1, 0.2f);
    bool run = true;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        StartCoroutine(FadeLoop());
    }

    IEnumerator FadeLoop()
    {
        float timer = 0;
        while (run)
        {
            while (timer < 1)
            {
                timer += Time.deltaTime;
                image.color = Color.Lerp(Color.white, minColor, Mathf.SmoothStep(0, 1, timer));
                yield return null;
            }

            while (timer >= 0)
            {
                timer -= Time.deltaTime;
                image.color = Color.Lerp(Color.white, minColor, Mathf.SmoothStep(0, 1, timer));
                yield return null;
            }
        }

    }
}
