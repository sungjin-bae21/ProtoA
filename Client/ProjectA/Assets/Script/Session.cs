using UnityEngine;


public delegate void ConnectHandler(Protocol protocol);
public delegate void ConnectFailHandler(Protocol protocol);
public delegate void MessageHandler(Message.ProjectA_Msg message, Protocol protocol);
public delegate void DisconnectHandler(Protocol protocol);


public class Session
{
	string ip_address;
	TcpTransport tcp_transport;

    // 하나의 delegate 에 여러 핸들러를 등록하는 등의 편의성을 위해
    // 접근 제한자를 public 으로 한다.
    public ConnectHandler connect_handler;
    public ConnectFailHandler connect_fail_handler;
    public MessageHandler message_handler;
    public DisconnectHandler disconnect_handler;


    public Session(string ip_address_)
    {
        ip_address = ip_address_;
    }


    public void Connect(int port_, Protocol protocol_)
    {
        if (protocol_ == Protocol.TCP)
        {
            ConnectHandler transport_connect_handler = OnConnect;
            ConnectFailHandler transport_fail_handler = OnConnectFail;
            MessageHandler transport_message_handler = OnMessage;
            DisconnectHandler transport_disconnect_handler = OnDisconnect;

            
            tcp_transport =
                new TcpTransport(transport_connect_handler,
                                 transport_fail_handler,
                                 transport_message_handler,
                                 transport_disconnect_handler);

            tcp_transport.ConnectServer(ip_address, port_);
        } 
    }


    void OnConnect(Protocol protocol_)
    {
        if (connect_handler == null)
        {
            return;
        }

        connect_handler(protocol_);
    }


    void OnConnectFail(Protocol protocol_)
    {
        if (protocol_ == Protocol.TCP)
        {
            tcp_transport = null;
        }

        if (connect_fail_handler == null)
        {
            return;
        }

        connect_fail_handler(protocol_);
    }


    void OnMessage(Message.ProjectA_Msg message, Protocol protocol_)
    {
        if (message_handler == null)
        {
            return;
        }

        message_handler(message, protocol_);
    }


    void OnDisconnect(Protocol protocol_)
    {
        if (disconnect_handler == null)
        {
            return;
        }

        disconnect_handler(protocol_);
    }


    public bool SendMessage(Message.ProjectA_Msg message_, Protocol protocol_)
    {
        if (protocol_ == Protocol.TCP)
        {
            if (IsConncted(protocol_))
            {
                tcp_transport.SendMessage(message_);
            }
        }

        return false;
    }


    public bool IsConncted(Protocol protocol_)
    {
        if (protocol_ == Protocol.TCP)
        {
            if (tcp_transport != null)
            {
                if (tcp_transport.IsConnected())
                {
                    return true;
                }
            }
        }

        return false;
    }


    // check session is conncted
    public bool IsConnected()
    {
        if (IsConncted(Protocol.TCP) ||
            IsConncted(Protocol.UDP))
        {
            return true;
        }

        return false;
    }
}