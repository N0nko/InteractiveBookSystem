using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public GraphicRaycaster graphicRaycaster;
    public EventSystem eventSystem;

    private Button currentButton;
    private Image currentImage;

    void Update()
    {
        if (eventSystem == null || graphicRaycaster == null)
        {
            Debug.LogError("Missing variables");
            return;
        }

        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        pointerEventData.position = screenPoint;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        RaycastResult closestResult = new RaycastResult();
        float closestDistance = float.MaxValue;

        foreach (RaycastResult result in results)
        {
            Canvas canvas = result.gameObject.GetComponentInParent<Canvas>();
            if (canvas == graphicRaycaster.GetComponent<Canvas>())
            {
                float distance = Vector3.Distance(transform.position, result.worldPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestResult = result;
                }
            }
        }

        Button button = closestResult.gameObject?.GetComponent<Button>();
        Image image = button?.gameObject.transform.Find("Fill")?.GetComponent<Image>();

        if (button != currentButton || image != currentImage)
        {
            if (currentImage != null && currentImage.type != Image.Type.Filled)
            {
                Color tempColor = currentImage.color;
                tempColor.a = 0;
                currentImage.color = tempColor;
            }

            currentButton = button;
            currentImage = image;
        }

        if (currentButton != null && currentImage != null)
        {
            if (currentImage.type == Image.Type.Filled)
            {
                currentImage.fillAmount += Time.deltaTime;
                if (currentImage.fillAmount >= 1)
                {
                    currentButton.onClick.Invoke();
                    currentImage.fillAmount = 0;
                }
            }
            else
            {
                Color tempColor = currentImage.color;
                tempColor.a = Mathf.Lerp(tempColor.a, 1f, Time.deltaTime);
                currentImage.color = tempColor;
            }
        }
    }
}
