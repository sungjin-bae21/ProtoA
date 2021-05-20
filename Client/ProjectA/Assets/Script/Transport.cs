using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;


public enum Protocol
{
	TCP,
	UDP
}


public class TcpTransport
{
	public static readonly int BUFFER_SIZE = 1024;

	byte[] buffer;
	StringBuilder string_builder;
	Socket socket;

	ConnectHandler connect_handler;
	ConnectFailHandler connect_fail_handler;
	MessageHandler message_handler;
	DisconnectHandler disconnect_handler;

	// 재시도를 위해서 TcpTransport 객체를 새로 생성하는 방식을 채택.
	// 버퍼가 다시 할당되지만 큰 문제는 없을것이라 판단.
	public TcpTransport(ConnectHandler connect_handler_,
						ConnectFailHandler connect_fail_handler_,
						MessageHandler message_handler_,
						DisconnectHandler disconnect_handler_)
    {
		Assert.IsTrue(connect_handler_ != null);
		Assert.IsTrue(connect_fail_handler_ != null);
		Assert.IsTrue(message_handler_ != null);
		Assert.IsTrue(disconnect_handler_ != null);

		connect_handler = connect_handler_;
		connect_fail_handler = connect_fail_handler_;
		message_handler = message_handler_;
		disconnect_handler = disconnect_handler_;

		buffer = new byte[BUFFER_SIZE];
		string_builder = new StringBuilder();
		socket = new Socket(AddressFamily.InterNetwork,
					        SocketType.Stream,
					        ProtocolType.Tcp);
	}


	~TcpTransport()
    {
		connect_handler = null;
		connect_fail_handler = null;
		message_handler = null;
		disconnect_handler = null;

		socket.Close();
	}


	public bool IsConnected()
    {
		return socket.Connected;
    }


	public void ConnectServer(string ip_address_, int port_)
	{
		socket.BeginConnect(ip_address_,
							port_,
							new AsyncCallback(ConnectCallback),
							this);
	}


	static void ConnectCallback(IAsyncResult result_)
    {
		TcpTransport transport = (TcpTransport)result_.AsyncState;
		Socket socket = transport.socket;
		socket.EndConnect(result_);

		if (!socket.Connected)
		{
			transport.connect_fail_handler(Protocol.TCP);
			return;
		}

		socket.BeginReceive(transport.buffer,
							0,  // offset
							BUFFER_SIZE,
							0,  // socket flag (Specifies socket send and receive behaviors.)
							new AsyncCallback(ReadCallback),
							transport);
	}


	static void ReadCallback(IAsyncResult result_)
	{
	
		TcpTransport transport = (TcpTransport)result_.AsyncState;
		Socket socket = transport.socket;
		SocketError error;
		int lenght = socket.EndReceive(result_, out error);
		if (error != SocketError.Success)
        {
			transport.disconnect_handler(Protocol.TCP);
			return;
        }

		String data = String.Empty;
		if (lenght > 0)
		{
			transport.string_builder.Append(
				Encoding.UTF8.GetString(transport.buffer, 0, lenght));

			Debug.Log("recv data ");
			// 추후 개선필요.
			data = transport.string_builder.ToString();
			Debug.Log(data);
			if (data.IndexOf("<EOF>") > -1)
			{
				transport.message_handler(data, Protocol.TCP);
			}
		}

		socket.BeginReceive(transport.buffer,
							0,  // offset
							BUFFER_SIZE,
							0,  // socket flag (Specifies socket send and receive behaviors.)
							new AsyncCallback(ReadCallback),
							transport);
	}


	public void SendMessage(String message_)
    {
		byte[] data = Encoding.UTF8.GetBytes(message_);

		// Begin sending the data to the remote device.  
		socket.BeginSend(data,
			             0,  // offset
                         data.Length,
						 0,  // socket flag (Specifies socket send and receive behaviors.)
						 new AsyncCallback(SendCallback),
						 this);
	}

	
	static void SendCallback(IAsyncResult result_)
    { 
		TcpTransport transport = (TcpTransport)result_.AsyncState;
		Socket socket = transport.socket;
		SocketError error;
		int lenght = socket.EndSend(result_, out error);
		if (error != SocketError.Success)
		{
			transport.disconnect_handler(Protocol.TCP);
			return;
		}

		Assert.IsTrue(lenght > 0);
	}
}
