using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Modele3D : MonoBehaviour
{
    public enum ModeleTypes
    {
        OnGround,OnWall
    }
    public ModeleTypes ModeleType;
    public bool Selected;
    public Vector3 touchpos;

    public AnimationCurve movementCurve;
    public float Yintencity;

    [Range(0,1)]
    public float RotationSensetivite;
    public float Y_initial,X_initial;
    private float Yanime;


    public Vector3 SavedPosition,Targerpos;
    private Touch FirstTouch,SecondTouch;

    public TextMeshProUGUI debuger;

    private Model3DSettings modelsettings;
    private Touch MobileTouch;
    private Vector2 lastTouchPosition;
    public bool rotateiconpressed;

    private void Awake()
    {
        modelsettings = (Model3DSettings)Resources.Load("3DModelSettings");
        //set settings
        ModeleType = modelsettings.ModeleType;
        movementCurve = modelsettings.animcurve;
        Yintencity = modelsettings.Yintencity;
        RotationSensetivite = modelsettings.RotationSensetivite;
        debuger = UIElement.instance.debugger;


        Y_initial = transform.rotation.y;
        X_initial = transform.rotation.x;
        SavedPosition = transform.position-Vector3.up;

    }


    private void Update()
    {
        
       if (Selected)
        {

            
            UIElement.instance.SelectedObject = this;
            Yanime += Time.deltaTime;


            if (Yanime > 1)
                Yanime = 0;







            if (Input.touchCount > 0)
            {
                FirstTouch = Input.GetTouch(0);

                if (rotateiconpressed && FirstTouch.phase == TouchPhase.Began)
                {


                    PlaceObject.instance.rotating = true;
                    rotateiconpressed = false;
                    touchpos = Input.GetTouch(0).position;
                    StartCoroutine(Irotate());
                }

            }








            transform.position = Vector3.Lerp(transform.position, Targerpos + new Vector3(0, movementCurve.Evaluate(Yanime) * Yintencity, 0), Time.deltaTime * 10);
        }
        else
        {
            Yanime = 0;
            transform.position = Targerpos;
        }


        
    }
 

    public void select()
    {
        if (!Selected)
        { 
        
        Selected = true;
        SavedPosition = transform.position;
        
        }


    }
    public void remove()
    {
        Diselect();
        gameObject.SetActive(false);
        
    
    
    }
    

    Vector3 DragVector => touchpos - new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
    private IEnumerator Irotate()
    {
        while (FirstTouch.phase != TouchPhase.Ended)
        {
            if (ModeleType == ModeleTypes.OnGround)
            {
                
                
                //transform.rotation =  Quaternion.Euler(transform.rotation.x, Y_initial + Mathf.Abs(DragVector.x) * Mathf.Sign(DragVector.x) * RotationSensetivite, transform.rotation.z);
                transform.eulerAngles += Vector3.up * - FirstTouch.deltaPosition.x; 
            }
                
            if (ModeleType == ModeleTypes.OnWall)
                transform.eulerAngles += Vector3.up * -(FirstTouch.deltaPosition.magnitude > SecondTouch.deltaPosition.magnitude ? FirstTouch.deltaPosition.magnitude : SecondTouch.deltaPosition.magnitude);



            yield return null;
        }
        if (ModeleType == ModeleTypes.OnGround)
            Y_initial = Y_initial + DragVector.x  * RotationSensetivite;
        if (ModeleType == ModeleTypes.OnWall)
            X_initial = X_initial + DragVector.x * RotationSensetivite;


        PlaceObject.instance.rotating = false;
    }

    
    public void Diselect()
    {

        Selected = false;
        StopAllCoroutines();
    }

}
