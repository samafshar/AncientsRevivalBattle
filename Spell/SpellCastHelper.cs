using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class SpellCastHelper : MonoBehaviour
{
    public enum HelperPointType
    {
        PivotPoint,
        BeginEndPoints,
    }

    //Serialized Fields
    [SerializeField]
    private HelperPointType _type;
    [SpineBone]
    [SerializeField]
    private string _bone_SpellCastHelper01;
    [SpineBone]
    [SerializeField]
    private string _bone_SpellCastHelper02;
    [SerializeField]
    private SkeletonAnimation _skeletonAnim;
    

    //public Methods
    public SpellCastTransData GetProperCastTransData()
    {
        SpellCastTransData result = new SpellCastTransData();

        Bone b = _skeletonAnim.skeleton.FindBone(_bone_SpellCastHelper01);

        Vector3 resPos = Vector3.zero;

        if (_type == HelperPointType.PivotPoint)
            resPos = _skeletonAnim.transform.TransformPoint(b.WorldX, b.WorldY, 0);
        else
        {
            Bone b2 = _skeletonAnim.skeleton.FindBone(_bone_SpellCastHelper02);

            Vector3 startP  = _skeletonAnim.transform.TransformPoint(b.WorldX, b.WorldY, 0);
            Vector3 endP    = _skeletonAnim.transform.TransformPoint(b2.WorldX, b2.WorldY, 0);

            resPos = (startP + endP) / 2;
        }

        result.position = resPos;
        result.rotationAmount = 180 - b.WorldRotationX;

        return result;
    }
}

public class SpellCastTransData
{
    public float rotationAmount { get; set; }
    public Vector3 position { get; set; }
    public Vector3 scale { get; set; }
}
