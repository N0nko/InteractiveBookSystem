using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageButtonGroup : MonoBehaviour
{
    public ButtonInteractable[] buttonInteractables;
    public Localization localization;
    public void OnEnable()
    {
        buttonInteractables[(int)localization.currentLang].preSelected = true;
        foreach (var buttonInteractable in buttonInteractables)
        {
            buttonInteractable.gameObject.SetActive(true);
        }
    }
    public void OnDisable()
    {
        foreach (var buttonInteractable in buttonInteractables)
        {
            buttonInteractable.preSelected = false;
            buttonInteractable.gameObject.SetActive(false);
        }
    }
    public void SelectLanguage(int id) {

        for (int i = 0; i < buttonInteractables.Length; i++) {

            if (i != id)
            {
                buttonInteractables[i].Unlatch();
                buttonInteractables[i].preSelected = false;
            }
        
        }
        buttonInteractables[id].preSelected = true;
        localization.SwitchLanguage(id);


    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
