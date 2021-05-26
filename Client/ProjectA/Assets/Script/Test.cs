using System.Collections;
using System.Collections.Generic;
using System.Text;
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
            Message.ChatMsg chat_msg = new Message.ChatMsg();
            chat_msg.msg = "Test data";
            Message.ProjectA_Msg msg =
                ProtoBuf.Serializer.ChangeType<Message.ChatMsg, Message.ProjectA_Msg>(chat_msg);
            msg.message_type = "chat_msg";
            session.SendMessage(msg, Protocol.TCP);
        }
    }


    void OnConnect(Protocol protocol_)
    {

    }


    void OnConnectFail(Protocol protocol_)
    {

    }


    void OnMessage(Message.ProjectA_Msg message_ ,Protocol protocol_)
    {

        if (message_.message_type == "chat_msg")
        {
            Message.ChatMsg chat_msg =
                ProtoBuf.Serializer.ChangeType<Message.ProjectA_Msg, Message.ChatMsg>(message_);
            Debug.Log(chat_msg.msg);
        }
        
    }

    void OnDisConnect(Protocol protocol_)
    {

    }
}
