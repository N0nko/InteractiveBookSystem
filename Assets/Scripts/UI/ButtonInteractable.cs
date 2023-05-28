using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractable : MonoBehaviour
{
    [HideInInspector]
    public bool selected;
    Image image;
    Button button;

    bool latched, filled;
    public bool preSelected;

    public ButtonType buttonType;
    public FillType fillType;
    public Color FilledColor = Color.white;
    Color defaultColor;
    public Image remoteFill;
    //public bool preFilled;

    public enum ButtonType
    {
        Momentary,
        Latching
    }

    public enum FillType
    {
        Radial,
        Fade,
        None
    }


    void OnEnable()
    {
        if (remoteFill != null)
            image = remoteFill;
        else
            image = GetComponent<Image>();
        defaultColor = image.color;
        button = GetComponent<Button>();
        gameObject.layer = LayerMask.NameToLayer("Buttons");

        switch (fillType)
        {
            case FillType.Radial:

                image.type = Image.Type.Filled;
                image.fillAmount = 0;
                if (preSelected)
                {
                    image.fillAmount = 1;
                    image.color = FilledColor;
                }
                break;
            case FillType.Fade:
                image.type = Image.Type.Simple;
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                if (preSelected)
                {
                    latched = true;
                    image.color = FilledColor;
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 1);

                }
                break;
            default:
                break;
        }

        // gameObject.tag = "Interactable";
    }

    public void Unlatch()
    {

        latched = false;
    }

    void Update()
    {
      
        if (selected)
        {
            switch (fillType)
            {
                case FillType.Radial:
                    if (image.fillAmount < 1)
                        image.fillAmount += Time.deltaTime * 1.5f;
                    else if (!filled)
                    {
                        filled = true;
                        button.onClick.Invoke();
                        if (buttonType == ButtonType.Latching)
                        {
                            latched = true;
                            LeanTween.value(gameObject, setColorCallback, defaultColor, FilledColor, .25f);

                        }
                        else
                            try
                            {
                                if (image.gameObject.activeInHierarchy)
                                    StartCoroutine(WaitForUnselected());
                            }
                            catch (System.Exception)
                            {

                                // throw;
                            }



                    }
                    break;
                case FillType.Fade:
                    if (image.color.a < 1)
                        image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a + Time.deltaTime * 1.5f);
                    else if (!filled)
                    {
                        filled = true;
                        button.onClick.Invoke();
                        if (buttonType == ButtonType.Latching)
                        {
                            latched = true;
                            LeanTween.value(gameObject, setColorCallback, defaultColor, FilledColor, .25f);

                        }
                        else
                            if (image.gameObject.activeInHierarchy)
                            StartCoroutine(WaitForUnselected());
                    }
                    break;
                case FillType.None:
                    button.onClick.Invoke();
                    break;
                default:
                    break;
            }



        }
        else if (!latched)
        {
            switch (fillType)
            {
                case FillType.Radial:
                    if (image.fillAmount >= 0)
                        image.fillAmount -= Time.deltaTime * 1.5f;
                    //if (image.color == FilledColor)
                    //    LeanTween.value(gameObject, setColorCallback, FilledColor, defaultColor, .25f);
                    break;
                case FillType.Fade:
                    if (image.color.a >= 0)
                        image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - Time.deltaTime * 1.5f);
                    if (buttonType == ButtonType.Latching)
                        if (image.color == FilledColor)
                            LeanTween.value(gameObject, setColorCallback, FilledColor, defaultColor, .25f);
                    break;
                default:
                    break;
            }

            filled = false;
        }
    }
    IEnumerator WaitForUnselected()
    {

        yield return new WaitUntil(() => !selected);
        switch (fillType)
        {
            case FillType.Radial:
                if (image.fillAmount > 0)
                    image.fillAmount = 0;

                break;
            case FillType.Fade:
                if (image.color.a > 0)
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

                break;
        }

    }

    private void setColorCallback(Color c)
    {
        image.color = c;

        var tempColor = image.color;
        tempColor.a = 1f;
        image.color = tempColor;
    }

    private void OnDisable()
    {
        image.fillAmount = 0;
    }
}
