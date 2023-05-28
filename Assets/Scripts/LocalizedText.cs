using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public Translation translations;
    private TextMeshProUGUI label;

    [Space(20)]
    public bool changeSizeLT;
    public float newSizeLT;
    public bool changeSizeEN;
    public float newSizeEN;
    public bool changeSizePL;
    public float newSizePL;

    private void Start()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Localization.OnLanguageChanged += RefreshTranslations;


        if (Localization.instance != null)
        {
            if(label == null) label = GetComponent<TextMeshProUGUI>();
            RefreshTranslations();
        }
    }

    void OnDisable()
    {
        Localization.OnLanguageChanged -= RefreshTranslations;
    }

    public void RefreshTranslations()
    {
        if (label != null)
            switch (Localization.instance.currentLang)
        {
            case Localization.Language.LT:
                
                label.text = translations.text_LT;
                if(changeSizeLT)
                {
                    label.fontSize = newSizeLT;
                }
                break;
            case Localization.Language.EN:
                label.text = translations.text_EN;
                if (changeSizeEN)
                {
                    label.fontSize = newSizeEN;
                }
                break;
                case Localization.Language.PL:
                    label.text = translations.text_PL;
                    if (changeSizePL)
                    {
                        label.fontSize = newSizePL;
                    }
                    break;
              
            }      
    }
}

[System.Serializable]
public class Translation
{
    [Multiline]
    public string text_LT;
    [Multiline]
    public string text_EN;
    [Multiline]
    public string text_PL;
}