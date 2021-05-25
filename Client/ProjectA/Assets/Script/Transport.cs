using System;
using System.Collections;
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


// 데이터는 다음과 같이 들어온다..
// "\n\n{message_size}{bytes data}"
public class TcpTransport
{
	public static readonly int BUFFER_SIZE = 1024;
	public static int HEADER_SIZE = 6;


	Byte[] buffer;
	int read_offset;
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

		read_offset = 0;
		buffer = new Byte[BUFFER_SIZE];
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

		transport.connect_handler(Protocol.TCP);
		socket.BeginReceive(transport.buffer,
							transport.read_offset,  // offset
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
		int read_bytes = socket.EndReceive(result_, out error);
		if (error != SocketError.Success)
        {
			transport.disconnect_handler(Protocol.TCP);
			return;
        }

		String data = String.Empty;
		if (read_bytes > 0)
		{
			transport.read_offset += read_bytes;
			// 복잡도를 줄이기 위해 데이터가 들어오는 순간 헤더와 바디를 같이 파싱하도록 한다.
			// 매번 헤더를 파싱 하지만 큰 문제가 없을거라 판단.
			ParseMessage(transport, read_bytes);
		}

		socket.BeginReceive(transport.buffer,
							0,  // offset
							BUFFER_SIZE,
							0,  // socket flag (Specifies socket send and receive behaviors.)
							new AsyncCallback(ReadCallback),
							transport);
	}


	static bool ParseMessage(TcpTransport transport_, int read_bytes_)
    {
		transport_.read_offset += read_bytes_;
		if (transport_.read_offset < HEADER_SIZE)
		{
			return false;
		}

		int body_size = 0;
		if (!ParseMessageHeader(transport_.buffer, out body_size))
        {
			return false;
        }

		// 메세지를 다 받지 못한 상태.
		if (transport_.read_offset < body_size + HEADER_SIZE)
        {
			return false;
        }

		string msg_str;
		if (!ParseMessageBody(transport_.buffer, body_size, out msg_str))
        {
			DropMessage(transport_, body_size);
			return false;
		}

		// 받은 메세지를 protobuf 로 만든다.
		DropMessage(transport_, body_size);
		return true; ;
	}


	static bool ParseMessageHeader(byte[] data_, out int body_size_)
    {
		if (data_[0] == '\n' && data_[1] == '\n')
        {
			body_size_ = BitConverter.ToInt32(data_, 3);
			return true;
        }

		body_size_ = -1; 
		return false;
    }


	static bool ParseMessageBody(byte[] data_, int body_size_, out string msg_str)
    {
		try
		{
			msg_str = Encoding.UTF8.GetString(data_, 6, body_size_);
			return true;
		}
		catch (Exception e)
        {
			// 문서 참조.
			// 1. The byte array contains invalid Unicode code points.
			// 2. A fallback occurred
			msg_str = "";
			Debug.Log(e.Message);
		}
		
		return false;
    }

	static void DropMessage(TcpTransport transport_, int body_size) 
    {
		if (transport_.read_offset == body_size + HEADER_SIZE)
        {
			transport_.buffer = new byte[BUFFER_SIZE];
			return;
		}

		// 남아있는 데이터를 유지.
		Byte[] copy = transport_.buffer;
		transport_.buffer = new byte[BUFFER_SIZE];
		Buffer.BlockCopy(copy,
						 body_size + HEADER_SIZE - 1,  //src offset
						 transport_.buffer,
						 0,  // dst offset
						 transport_.read_offset - body_size + HEADER_SIZE - 1);
	}


	public void SendMessage(String message_)
    {
		byte[] data = Encoding.UTF8.GetBytes(message_);
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
