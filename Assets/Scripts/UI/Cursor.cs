using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    public enum ControlType
    {
        Object, Cursor
    }

    [SerializeField]
    public ControlType cursorType;

    private RectTransform rectTransform;
    private int uiLayer;
    private Camera mainCamera;

    private ButtonInteractable currentButtonInteractable;
    private ButtonInteractable previousButtonInteractable;

    private void Start()
    {
        uiLayer = LayerMask.NameToLayer("Buttons");
        rectTransform = gameObject.GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        IsPointerOverUIElement(RectTransformToScreenSpace(rectTransform).position);
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            GetComponent<Image>().enabled = !GetComponent<Image>().isActiveAndEnabled;
        }
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

    public bool IsPointerOverUIElement(Vector2 screenPosition)
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults(screenPosition));
    }

    private bool IsPointerOverUIElement(List<RaycastResult> raycastResults)
    {

        foreach (RaycastResult raycastResult in raycastResults)
        {
            if (raycastResult.gameObject.layer == uiLayer)
            {
            
                if (raycastResult.gameObject.CompareTag("Block"))
                    break;

                if (raycastResult.gameObject.CompareTag("Interactable"))
                {
                    currentButtonInteractable = raycastResult.gameObject.GetComponent<ButtonInteractable>();

                    if (currentButtonInteractable)
                    {
                        if (currentButtonInteractable != previousButtonInteractable)
                        {
                            if (previousButtonInteractable != null)
                                previousButtonInteractable.selected = false;
                        }

                        currentButtonInteractable.selected = true;
                        previousButtonInteractable = currentButtonInteractable;
                        return true;
                    }
                }
            }
        }

        if (previousButtonInteractable != null)
            previousButtonInteractable.selected = false;

        previousButtonInteractable = null;
        return false;
    }

    private List<RaycastResult> GetEventSystemRaycastResults(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        if (cursorType == ControlType.Cursor)
        {
            eventData.position = Input.mousePosition;
        }

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    
    }
    void OnDisable()
    {
        if (previousButtonInteractable != null)
        {
            previousButtonInteractable.selected = false;
            previousButtonInteractable = null;
        }
    }

}
