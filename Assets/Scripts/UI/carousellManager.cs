using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class carousellManager : MonoBehaviour
{
    Image display;

    float rotateTime = 0.4f;
    public List<Sprite> pictures;
    public List<Image> indicator;
    int lastId = -999;


    public int currentpage = -1;
    bool canTurn = true;
    // Start is called before the first frame update



    void Start()
    {
        //  FillPaths();
        // Inc();
        init();
        //StartCoroutine(LateStart());

    }

    void init()
    {
        display = GetComponent<Image>();
    }

    public void Inc(int dir)
    {


        if (!canTurn)
            return;

        if(currentpage == -1)
            currentpage = 0;


        if (dir > 0)
        {
            if (currentpage + dir <= pictures.Count - 1)
            {
                currentpage += dir;
            }
            else
                currentpage = 0;
        }
        else if (dir < 0)
        {
            if (currentpage + dir >= 0)
                currentpage += dir;
            else
                currentpage = pictures.Count - 1;

        }
        UpdateImage();
        ////foreach (GameObject page in pages)
        ////{
        ////    LeanTween.alphaCanvas(pages[currentpage].GetComponent<CanvasGroup>(), 0, 0.05f).setEaseOutCubic().setOnComplete(() => );


        ////}

        //LeanTween.alphaCanvas(pages[currentpage].GetComponent<CanvasGroup>(), 0, 0).setOnComplete(() => pages[currentpage].SetActive(true));
        //canTurn = false;


        //LeanTween.alphaCanvas(pages[currentpage].GetComponent<CanvasGroup>(), 1, 0.5f).setOnComplete(() => canTurn = true);


    }

    void UpdateImage()
    {
        if (display == null)
        {
            display = GetComponent<Image>();
        }
        if (currentpage == -1)
        {
            currentpage = 0;
        }
        if (lastId != currentpage)
        {
            display.sprite = pictures[currentpage];
            lastId = currentpage;
        }
        UpdateIndicator();
    }
    void UpdateIndicator()
    {
        for (int i = 0; i < indicator.Count; i++)
        {
            if (i == currentpage)
            {
              //  indicator[i].color = Color.white;
                indicator[i].fillAmount = 1;
            }
            else
            {
              //  indicator[i].color = Color.black;
                indicator[i].fillAmount = 0;
            }
        }
    }


    //void UpdateImage()
    //{
    //    if (display == null)
    //        init();
    //    Debug.Log(currentpage);
    //    display.sprite = pictures[currentpage];
    //    foreach (Image image in indicator)
    //        image.fillAmount = 0;
    //    if (indicator[currentpage] != null)
    //        indicator[currentpage].fillAmount = 1;
    //}

    void MovingComplete()
    {

        canTurn = true;

    }
    private IEnumerator ColorLerp(Image img, Color startColor, Color endColor, float time)
    {


        float timeElapsed = 0f;
        float totalTime = time;


        while (timeElapsed < totalTime)
        {
            timeElapsed += Time.deltaTime;
            img.color = Color.Lerp(startColor, endColor, timeElapsed / totalTime);
            yield return null;
        }


    }





    // Update is called once per frame
    void Update()
    {

    }
}
