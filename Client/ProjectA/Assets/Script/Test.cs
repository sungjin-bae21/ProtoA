using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Session session;


    void Start()
    {
        session = new Session("127.0.0.1");
        session.connect_handler += OnConnect;
        session.connect_fail_handler += OnConnectFail;
        session.message_handler += OnMessage;
        session.disconnect_handler += OnDisConnect;

        session.Connect(8000, Protocol.TCP);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            session.SendMessage("Test data", Protocol.TCP);
        }
    }


    void OnConnect(Protocol protocol_)
    {

    }


    void OnConnectFail(Protocol protocol_)
    {

    }


    void OnMessage(string message ,Protocol protocol_)
    {
        Debug.Log(message);
    }

    void OnDisConnect(Protocol protocol_)
    {

    }
}
