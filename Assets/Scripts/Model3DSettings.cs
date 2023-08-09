using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Modele3D;

[CreateAssetMenu(fileName ="3DModelSettings",menuName ="3DModels")]
public class Model3DSettings : ScriptableObject
{

    public AnimationCurve animcurve;
    public ModeleTypes ModeleType;
    public float RotationSensetivite;
    public float Yintencity;

}
