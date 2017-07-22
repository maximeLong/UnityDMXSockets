using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;
using Sirenix.OdinInspector;


public class DMXData {
	public int[] values;
};

public class SocketIOScript : MonoBehaviour {

    [GUIColor(1, .5f, .5f), Space]
    public string serverURL = "http://localhost:3000";
	protected Socket socket = null;
    
    [ListDrawerSettings(DraggableItems =false, IsReadOnly =true, HideAddButton =true)]
    public List<ChannelSubscribers> channelSubscribers = new List<ChannelSubscribers>();

    [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
    public int[] socketData = new int[512];

    public delegate void socketEventHandler(int[] socketCollection);
    public static event socketEventHandler socketUpdate;


    //start and stop network connections
    void Start()
    {
        DoOpen();
        CollectSubscribers();
        channelSubscribers.Sort();
    }
    void OnApplicationQuit() { DoClose(); }


    void DoOpen() {
	    if (socket == null) {
		    socket = IO.Socket (serverURL);

            socket.On (Socket.EVENT_CONNECT, () => {
			    Debug.Log("Socket.IO connected.");
		    });

		    socket.On ("dmx", (data) => {
			    string str = data.ToString();
			    DMXData returnData = JsonConvert.DeserializeObject<DMXData> (str);
                socketData = returnData.values;

                //send event
                updateFromSocket(socketData);

            });
	    }
	}


    public static void updateFromSocket(int[] socketCollection)
    {
        //fire event so channel Controller class can grab
        if (socketUpdate != null)
        {
            socketUpdate(socketCollection);
        }
    }

    //close connection
    void DoClose() {
		if (socket != null) {
			socket.Disconnect ();
            Debug.Log("socket disconnected");
            socket = null;
		}
	}

    //emit example
	void SendChat(string str) {
		if (socket != null) {
			socket.Emit ("chat", str);
		}
	}


    void CollectSubscribers()
    {
        DMXChannelToLight[] lights = GameObject.FindObjectsOfType<DMXChannelToLight>();
        DMXChannelToTransform[] transforms = GameObject.FindObjectsOfType<DMXChannelToTransform>();

        foreach (DMXChannelToLight light in lights)
        {
            channelSubscribers.Add( new ChannelSubscribers(light.DMXChannel, light.gameObject));
        }

        foreach (DMXChannelToTransform trans in transforms)
        {
            channelSubscribers.Add(new ChannelSubscribers(trans.DMXChannel, trans.gameObject));
        }
    }

}

[System.Serializable]
public class ChannelSubscribers : IComparable<ChannelSubscribers>
{

    [HorizontalGroup(width: 0)]
    public int channel;
    [HorizontalGroup(width: 0)]
    public GameObject subscriber;

    public ChannelSubscribers(int chan, GameObject sub)
    {
        channel = chan;
        subscriber = sub;
    }


    // Default comparer for Part type.
    public int CompareTo(ChannelSubscribers compareChannel)
    {
        // A null value means that this object is greater.
        if (compareChannel == null)
            return 1;

        else
            return this.channel.CompareTo(compareChannel.channel);
    }
}
