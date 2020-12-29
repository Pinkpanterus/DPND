using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseOverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isOverButton;
    public Action<bool> onMouseChangePosition;


    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        isOverButton = true;
        onMouseChangePosition?.Invoke(isOverButton);
    }

  
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        isOverButton = false;
        onMouseChangePosition?.Invoke(isOverButton);
    }

    

    private void Update()
    {
        
    }
  
}
