using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
{


    public UnityEvent onclickdown,onclickup;
    public void OnPointerDown(PointerEventData eventData)
    {
        onclickdown.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onclickup.Invoke(); 
    }



    public void scaledown(float i)
    {

        transform.localScale = Vector3.one * i;

    }

    
    public void scaleback()
    {

        transform.localScale = Vector3.one;
    }

    public void rotatepressed()
    {
        UIElement.instance.rotateiconpressed();
    
    }





}
   





