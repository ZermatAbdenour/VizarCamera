using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class msgreciver : MonoBehaviour
{

    public ModelLoader loader;

    public bool Recived;

    public void setproductid(string id)
    {
        print(id+" aktham  dddddddddddddddddddddddddd");
        loader.ProductID = id;
        Recived = true;
        loader.GetProduct();
    }
}
