using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapeAllLocalizedText : MonoBehaviour
{
    public string allText;
    void Start()
    {
        List<KeyValuePair<string, string>> textList = new List<KeyValuePair<string, string>>();
        LocalizedText[] localizedTexts = Resources.FindObjectsOfTypeAll<LocalizedText>();
        foreach (var text in localizedTexts)
        {
            string parentID = FindParentWithNumericName(text.gameObject.transform.parent);
            string ltText = "LT: " + text.translations.text_LT;
            string enText = "EN: " + text.translations.text_EN;
            string plText = "PL: " + text.translations.text_PL;
            string textString = parentID + "\n" + ltText + "\n" + enText + "\n" + plText + "\n";
            textList.Add(new KeyValuePair<string, string>(parentID, textString));
        }

        textList.Sort((x, y) => int.Parse(x.Key).CompareTo(int.Parse(y.Key)));

        allText = "Start\n";
        foreach (var text in textList)
        {
            allText += text.Value + "\n";
        }
        Debug.Log(allText);
    }

    private string FindParentWithNumericName(Transform transform)
    {
        string parentID = "";
        int result;
        while (transform != null)
        {
            if (int.TryParse(transform.gameObject.name, out result))
            {
                parentID = transform.gameObject.name;
                break;
            }
            transform = transform.parent;
        }
        return parentID;
    }
}