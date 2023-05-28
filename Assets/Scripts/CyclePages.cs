using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CyclePages : MonoBehaviour
    {
        public GameObject[] pages;
        public GameObject[] subPages;
        public int currentpage = -1;
        public bool flipMain;
        bool canTurn = true;
        // Start is called before the first frame update
        void Start()
        {
            PrepSubPages();
            Invoke("manualFlip", 1);
        }
        void manualFlip()
        {
            Inc(1);
        }

        public void GoTo(int i) {

            foreach (GameObject page in pages)
                page.SetActive(false);
            pages[i].SetActive(true);
            CloseSubPages();
        
        }
        public void Inc(int dir)
        {
            if (CloseSubPages())
                if (flipMain)
                    return;

            if (!canTurn)
                return;

            if (currentpage != -1)
                pages[currentpage].SetActive(false);


            if (dir > 0)
            {
                if (currentpage + dir <= pages.Length - 1)
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
                    currentpage = pages.Length - 1;

            }

            foreach (GameObject page in pages)
            {
                page.SetActive(false);


            }

            LeanTween.alphaCanvas(pages[currentpage].GetComponent<CanvasGroup>(), 0, 0).setOnComplete(() => pages[currentpage].SetActive(true));
            canTurn = false;


            LeanTween.alphaCanvas(pages[currentpage].GetComponent<CanvasGroup>(), 1, 0.5f).setOnComplete(() => canTurn = true);


        }
        void PrepSubPages()
        {
            foreach (GameObject page in subPages)
            {
                page.SetActive(false);
                page.GetComponent<Image>().fillAmount = 0;
                page.GetComponent<CanvasGroup>().alpha = 0;
            }
        }
        bool CloseSubPages()
        {
            bool pagesToClose = false;
            foreach (GameObject page in subPages)
                if (page.activeInHierarchy)
                {
                    page.GetComponent<ClosePanelTransition>().ClosePanel();
                    pagesToClose = true;
                }
            return pagesToClose;

        }

        // Update is called once per frame
        void Update()
        {
            //cycle pages on arrowkey down
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                Inc(1);
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                Inc(-1);
            }
        }
    }
}
