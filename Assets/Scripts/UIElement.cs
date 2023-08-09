using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIElement : MonoBehaviour

    
{
    public static UIElement instance;
    public TextMeshProUGUI debugger;
    public List<Modele3D> Object =new List<Modele3D>();
    public  Modele3D SelectedObject;
    public GameObject SelectedUI;
    public Camera Camera;
    public PlaceObject placer;
    public LayerMask LayerMask;
    public Button rotate;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
    private void Update()
    {
        if (SelectedObject != null)
        {
            SelectedUI.SetActive(true);
        }
        else
        {
            SelectedUI.SetActive(false);
        }

    }
    public void dselect()
    {
        SelectedObject.Diselect();
        SelectedObject = null;
    }
    public void remove()
    {
        placer.Placed = false;
        SelectedObject.remove();
        SelectedObject = null;
    
    }
    public void rotateiconpressed()
    {
        SelectedObject.rotateiconpressed = true;
       // SelectedObject.touchpos = Input.GetTouch(0).position;
       // StartCoroutine(SelectedObject.Irotate());

    }

}
