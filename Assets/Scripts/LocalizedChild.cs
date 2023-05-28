using UnityEngine;

public class LocalizedChild : MonoBehaviour
{
    public TranslationObject translations;

    private void OnEnable()
    {
        Localization.OnLanguageChanged += RefreshTranslations;
        if (Localization.instance != null)
        {
            RefreshTranslations();
        }
    }

    void OnDisable()
    {
        Localization.OnLanguageChanged -= RefreshTranslations;
    }

    public void RefreshTranslations()
    {
        if (translations.obj_LT)
            translations.obj_LT.SetActive(false);
        if (translations.obj_EN)
            translations.obj_EN.SetActive(false);

        switch (Localization.instance.currentLang)
        {
            case Localization.Language.LT:
                translations.obj_LT.SetActive(true);
                break;
            case Localization.Language.EN:
                translations.obj_EN.SetActive(true);
                break;
        }

    }
}

[System.Serializable]
public class TranslationObject
{
    public GameObject obj_LT;
    public GameObject obj_EN;
}
