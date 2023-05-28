using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalisedSpriteSwap : MonoBehaviour
{
    public Sprite[] sprites;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnEnable()
    {
        Localization.OnLanguageChanged += RefreshTranslations;


        if (Localization.instance != null)
        {
            RefreshTranslations();
        }
    }
    public void RefreshTranslations()
    {

        GetComponent<Image>().sprite = sprites[(int)Localization.instance.currentLang];

    }
    // Update is called once per frame
    void Update()
    {

    }
}
