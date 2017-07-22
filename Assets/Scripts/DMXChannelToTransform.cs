using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DMXChannelToTransform : MonoBehaviour
{

    [Title("DMX Channel Info", bold: false)]
    [Indent, Space, GUIColor(1, .5f, .5f), MinValue(1), MaxValue(512)]
    public int DMXChannel;

    [Indent, ReadOnly]
    public int DMXData = 0;

    [Indent, ShowIf("sixteenBit"), GUIColor(1, .5f, .5f), MinValue(1), MaxValue(512)]
    public int DMXChannelTwo;
    [Indent, ReadOnly, ShowIf("sixteenBit")]
    public int DMXDataTwo = 0;

    [Indent, EnumToggleButtons]
    public bitEnum ByteResolution;
    public enum bitEnum { eight, sixteen };

    [Indent, BoxGroup("Transform End Position:")]
    public Vector3 position;
    [Indent, BoxGroup("Transform End Position:")]
    public Quaternion rotation;
    [Indent, BoxGroup("Transform End Position:")]
    public Vector3 scale;

    [Button(buttonSize: 28)]
    public void syncTransforms()
    {
        position = gameObject.transform.localPosition;
        rotation = gameObject.transform.localRotation;
        scale = gameObject.transform.localScale;
    }

    //initial positions -- have to pull out the vector3 because objects cant be cloned
    private bool threading;
    private Vector3 initPosition;
    private Vector3 initScale;
    private Quaternion initRotation;

    //sixteen bit toggle
    private bool sixteenBit() { return ByteResolution == bitEnum.sixteen ? true : false; }




    void Start()
    {
        initPosition = gameObject.transform.localPosition;
        initRotation = gameObject.transform.localRotation;
        initScale = gameObject.transform.localScale;
        SocketIOScript.socketUpdate += updateLocalSocketData;
    }
    void OnApplicationQuit()
    {
        SocketIOScript.socketUpdate -= updateLocalSocketData;
    }


    void Update()
    {
        if (threading)
        {
            doTransformation();
            threading = false;
        }
    }


    public void updateLocalSocketData(int[] socketData)
    {
        DMXData = socketData[DMXChannel];
        if (sixteenBit()) { DMXDataTwo = socketData[DMXChannelTwo]; }
        threading = true;
    }

    void doTransformation()
    {
        float inputData;
        if (sixteenBit())
        {
            inputData = (DMXData + DMXDataTwo) / 2;
        }
        else
        {
            inputData = DMXData;
        }

        float Posx = lerpScerp(initPosition.x, position.x, inputData);
        float Posy = lerpScerp(initPosition.y, position.y, inputData);
        float Posz = lerpScerp(initPosition.z, position.z, inputData);

        float Scalex = lerpScerp(initScale.x, scale.x, inputData);
        float Scaley = lerpScerp(initScale.y, scale.y, inputData);
        float Scalez = lerpScerp(initScale.z, scale.z, inputData);

        float Rotx = lerpScerp(initRotation.x, rotation.x, inputData);
        float Roty = lerpScerp(initRotation.y, rotation.y, inputData);
        float Rotz = lerpScerp(initRotation.z, rotation.z, inputData);
        float Rotw = lerpScerp(initRotation.w, rotation.w, inputData);

        gameObject.transform.localPosition = new Vector3(Posx, Posy, Posz);
        gameObject.transform.localRotation = new Quaternion(Rotx, Roty, Rotz, Rotw);
        gameObject.transform.localScale = new Vector3(Scalex, Scaley, Scalez);

    }

    public float lerpScerp(float inputMin, float inputMax, float currentDMXValue)
    {
        return Mathf.Lerp(inputMin, inputMax, Mathf.InverseLerp(0, 255, currentDMXValue));
    }

}

