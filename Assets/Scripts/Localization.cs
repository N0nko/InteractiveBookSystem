using UnityEngine;

public class Localization : MonoBehaviour
{
    public static Localization instance;
    public enum Language {LT, EN, PL}
    public Language currentLang = Language.LT;

    public delegate void LanguageChanged();
    public static event LanguageChanged OnLanguageChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
         //   DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (this != instance)
                Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        OnLanguageChanged?.Invoke();
    }

    public void SwitchLanguage(int newLang)
    {
        currentLang = (Language)newLang;
        OnLanguageChanged?.Invoke();
    }
    //Method to toggle language between LT and EN
    public void Toggle2Language()
    {
        if (currentLang == Language.LT)
        {
            currentLang = Language.EN;
        }
        else
        {
            currentLang = Language.LT;
        }
        OnLanguageChanged?.Invoke();
    }
}