using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DMXChannelToLight : MonoBehaviour
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

    [BoxGroup("LightSource End Values:")]
    public Color color;
    [BoxGroup("LightSource End Values:")]
    public float range;
    [BoxGroup("LightSource End Values:")]
    public float intensity;

    [Button(buttonSize: 28)]
    public void syncLightSource()
    {
        color = gameObject.GetComponent<Light>().color;
        range = gameObject.GetComponent<Light>().range;
        intensity = gameObject.GetComponent<Light>().intensity;
    }

    //initial positions -- have to pull out the vector3 because objects cant be cloned
    private bool threading;
    private Color initColor;
    private float initRange;
    private float initIntensity;

    //sixteen bit toggle
    private bool sixteenBit() { return ByteResolution == bitEnum.sixteen ? true : false; }




    void Start()
    {
        initColor = gameObject.GetComponent<Light>().color;
        initRange = gameObject.GetComponent<Light>().range;
        initIntensity = gameObject.GetComponent<Light>().intensity;
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

        float Colr = lerpScerp(initColor.r, color.r, inputData);
        float Colg = lerpScerp(initColor.g, color.g, inputData);
        float Colb = lerpScerp(initColor.b, color.b, inputData);
        float Cola = lerpScerp(initColor.a, color.a, inputData);

        float intense = lerpScerp(initIntensity, intensity, inputData);
        float rang = lerpScerp(initRange, range, inputData);

        gameObject.GetComponent<Light>().color = new Vector4(Colr, Colg, Colb, Cola);
        gameObject.GetComponent<Light>().intensity = intense;
        gameObject.GetComponent<Light>().range = rang;

    }

    public float lerpScerp(float inputMin, float inputMax, float currentDMXValue)
    {
        return Mathf.Lerp(inputMin, inputMax, Mathf.InverseLerp(0, 255, currentDMXValue));
    }

}

