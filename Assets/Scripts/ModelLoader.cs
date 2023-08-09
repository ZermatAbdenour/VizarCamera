using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriLibCore;
using UnityEngine.Networking;
using TMPro;

public class ModelLoader : MonoBehaviour
{
    
    /// <summary>
    /// The Model URL.
    /// </summary>
    public string BaseURL = "http://abdenourzermat-001-site1.htempurl.com/";
    public string ProductID = "";
    public ProductDto Product;
    public GameObject loadingscreen;
    public TextMeshProUGUI textprogress;

    [HideInInspector]public GameObject RootGameObject;
    /// <summary>
    /// Creates the AssetLoaderOptions instance, configures the Web Request, and downloads the Model.
    /// </summary>
    /// <remarks>
    /// You can create the AssetLoaderOptions by right clicking on the Assets Explorer and selecting "TriLib->Create->AssetLoaderOptions->Pre-Built AssetLoaderOptions".
    /// </remarks>
    private void Start()
    {
        loadingscreen.SetActive(true);
    }
   
    /// <summary>
    /// Called when any error occurs.
    /// </summary>
    /// <param name="obj">The contextualized error, containing the original exception and the context passed to the method where the error was thrown.</param>
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
        textprogress.SetText("error");

    }

    /// <summary>
    /// Called when the Model loading progress changes.
    /// </summary>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    /// <param name="progress">The loading progress.</param>
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
        textprogress.SetText(((int)(progress*100))+"%");
        
    }

    /// <summary>
    /// Called when the Model (including Textures and Materials) has been fully loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Materials loaded. Model fully loaded.");
    }

    /// <summary>
    /// Called when the Model Meshes and hierarchy are loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model loaded. Loading materials.");
        RootGameObject = assetLoaderContext.RootGameObject;
        RootGameObject.SetActive(false);
        RootGameObject.AddComponent<Modele3D>();
     
        // RootGameObject.AddComponent<BoxCollider>();

        MeshFilter[] meshFilters = RootGameObject.GetComponentsInChildren<MeshFilter>();
        print(meshFilters);
        RootGameObject.layer = 6;
        foreach (MeshFilter meshFilter in meshFilters)
        {
            meshFilter.gameObject.AddComponent<BoxCollider>();
            meshFilter.gameObject.layer = 6;
        }
        loadingscreen.SetActive(false);
    }

    public void GetProduct()
    {
        StartCoroutine(FetchProduct());
    }


    private IEnumerator FetchProduct()

    {
        print(BaseURL + "products/" + ProductID );
        using (UnityWebRequest request = UnityWebRequest.Get(BaseURL + "products/" + ProductID))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                textprogress.SetText("error");

            }
            else
            {

                Product = JsonUtility.FromJson<ProductDto>(request.downloadHandler.text);
                GetModel(Product.modelID,Product.modelExtension);



            }
        }
    }

    private void GetModel(string ModelID,string ModelExtension)
    {
        //Getting the Model
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        var webRequest = AssetDownloader.CreateWebRequest(BaseURL+"models/"+ModelID+ ModelExtension);
        AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions,null,ModelExtension);
    }
}
