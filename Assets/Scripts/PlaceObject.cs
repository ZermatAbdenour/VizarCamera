using System.Collections;
using System.Collections.Generic;
using TriLibCore.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TouchPhase = UnityEngine.TouchPhase;

public class PlaceObject : MonoBehaviour
{
    public static PlaceObject instance;
    public Camera MainCamera;

    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private ModelLoader modelLoader;
    [SerializeField]
    LayerMask mask ;
    public bool rotating;


    private void Awake()
    {
        instance = this; 
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        modelLoader = GetComponent<ModelLoader>();

        Placed = false;
    }

    public bool Placed;
    [SerializeField]
    private ARRaycastManager _raycastManager;

    [SerializeField]
    private LayerMask _layerMask;
    Touch MobileTouch;
    private void Update()
    {
        
        if (modelLoader.RootGameObject == null)
            return;

        List<ARRaycastHit> Hits = new List<ARRaycastHit>();


      /*  if (!Placed && arRaycastManager.Raycast(new Vector2(Screen.width/2, Screen.height / 2), Hits, TrackableType.PlaneWithinPolygon))
        {
                modelLoader.RootGameObject.SetActive(true);
                modelLoader.RootGameObject.transform.SetPositionAndRotation(Hits[0].pose.position, Hits[0].pose.rotation);
                
            
        }*/

        

        if (Input.touchCount>0)
        {

            MobileTouch = Input.GetTouch(0);


            if (!Placed && MobileTouch.phase == TouchPhase.Began && arRaycastManager.Raycast(MobileTouch.position, Hits, TrackableType.PlaneWithinPolygon))
            {
                modelLoader.RootGameObject.SetActive(true);
                modelLoader.RootGameObject.transform.SetPositionAndRotation(Hits[0].pose.position, Hits[0].pose.rotation);
                modelLoader.RootGameObject.GetComponent<Modele3D>().Targerpos = Hits[0].pose.position - Vector3.up*0.2f;
                modelLoader.RootGameObject.GetComponent<Modele3D>().Y_initial = Hits[0].pose.rotation.y;
                modelLoader.RootGameObject.GetComponent<Modele3D>().Selected = true;


                print(MobileTouch.position);
                Placed = true;
            }

            if(Input.touchCount==1)
            if (MobileTouch.phase == TouchPhase.Began&&Placed)
            {
               
                    // Cast a ray from the touch position
                    Ray ray = MainCamera.ScreenPointToRay(Input.GetTouch(0).position);

                    // Perform the raycast and check if we hit a game object
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
                    {
                        // The raycast hit a game object - do something with it
                        GameObject selectedObject = hit.collider.gameObject;
                        print("Selected object: " + selectedObject.name);

                    
                        if (selectedObject.transform.root.GetComponent<Modele3D>())
                        {
                        if (!selectedObject.transform.root.GetComponent<Modele3D>().Selected)
                            selectedObject.transform.root.GetComponent<Modele3D>().select();
                        else 
                        {
                            StartCoroutine(IMove(selectedObject.transform.root.GetComponent<Modele3D>()));
                        }

                        }
                }
                

            }


            

             /*   MobileTouch = Input.GetTouch(0);

                if (MobileTouch.phase == TouchPhase.Began)
                {
                    Ray ray = UIElement.instance.Camera.ScreenPointToRay(Input.GetTouch(0).position);

                    // Perform the raycast and check if we hit a game object
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, UIElement.instance.LayerMask))
                    {
                        // The raycast hit a game object - do something with it
                        GameObject selectedObject = hit.collider.gameObject;
                        debuger.SetText(selectedObject.name + "  : object selected");

                        if (selectedObject.transform.root == this.gameObject)
                        {
                            debuger.SetText(selectedObject.transform.root.name + "   Moveing");
                            StartCoroutine(IMove(MobileTouch));

                        }


                    }


                }*/




            







        }

       /* if (Input.GetMouseButtonDown(0))
        {

            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);

            // Perform the raycast and check if we hit a game object
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
            {
                // The raycast hit a game object - do something with it
                GameObject selectedObject = hit.collider.gameObject;
                UIElement.instance.debugger.SetText("Selected object: " + selectedObject.name);
                print("Selected object: " + selectedObject.name);
            }
        }*/
    }
    private IEnumerator IMove(Modele3D Object)
    {

        yield return new WaitForSeconds(0.15f);
        if(!rotating)
        while (MobileTouch.phase != TouchPhase.Ended)
        {
            List<ARRaycastHit> Hits = new List<ARRaycastHit>();
            if (arRaycastManager.Raycast(MobileTouch.position, Hits, TrackableType.PlaneWithinPolygon))
            {

             //   UIElement.instance.debugger.SetText(Hits[0].pose.position.ToString());
                Object.Targerpos = Hits[0].pose.position;

            }
            yield return null;

        }
       // UIElement.instance.debugger.SetText("Done");



    }

}
