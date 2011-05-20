
#include "Client.h"

#define DEFAULT_PORT0 "2453"

namespace TestClient
{
	static const int SendBufferSize = 1024;
	static char SendBuffer[SendBufferSize];
	static const int RecvBufferSize = 512;
	static char RecvBuffer[RecvBufferSize];

	static wchar_t RecvBufferW[RecvBufferSize];

	char Client::s_serverAdd[20];
	SOCKET Client::m_socket;

	void Client::Initialize()
	{
		WSADATA wsaData;
		int iResult;

		// Initialize Winsock
		iResult = WSAStartup(MAKEWORD(2,2), &wsaData);
		if (iResult != 0) {
			printf("WSAStartup failed: %d\n", iResult);
		}

		m_socket = CreateConnection();
	}
	void Client::Finalize()
	{
		CloseConnection(m_socket);

		WSACleanup();
	}
	SOCKET Client::CreateConnection()
	{
		//return INVALID_SOCKET;
		struct addrinfo *result = NULL,
			*ptr = NULL,
			hints;

		ZeroMemory( &hints, sizeof(hints) );
		hints.ai_family = AF_UNSPEC;
		hints.ai_socktype = SOCK_STREAM;
		hints.ai_protocol = IPPROTO_TCP;


		// Resolve the server address and port
		int iResult = getaddrinfo(s_serverAdd, DEFAULT_PORT0, &hints, &result);//s_serverAdd
		if (iResult != 0) {
			printf("getaddrinfo failed: %d\n", iResult);
			//WSACleanup();
			return INVALID_SOCKET;
		}

		SOCKET ConnectSocket = INVALID_SOCKET;

		// Attempt to connect to the first address returned by
		// the call to getaddrinfo
		ptr = result;

		float success = false;
		do 
		{
			success = true;

			// Create a SOCKET for connecting to server
			ConnectSocket = socket(ptr->ai_family, ptr->ai_socktype, 
				ptr->ai_protocol);


			if (ConnectSocket == INVALID_SOCKET) {
				printf("Error at socket(): %ld\n", WSAGetLastError());
				freeaddrinfo(result);
				//WSACleanup();
				success = false;
				//return INVALID_SOCKET;
			}


			// Connect to server.
			iResult = connect( ConnectSocket, ptr->ai_addr, (int)ptr->ai_addrlen);
			if (iResult == SOCKET_ERROR) {                  
				closesocket(ConnectSocket);
				ConnectSocket = INVALID_SOCKET;
				success = false;
				//return INVALID_SOCKET;
			}

			ptr = ptr->ai_next;
		} while (ptr && !success);


		// Should really try the next address returned by getaddrinfo
		// if the connect call failed
		// But for this simple example we just free the resources
		// returned by getaddrinfo and print an error message

		freeaddrinfo(result);

		if (ConnectSocket == INVALID_SOCKET) {
			printf("Unable to connect to server!\n");
			//WSACleanup();
			return INVALID_SOCKET;
		}
		return ConnectSocket;
	}
	void Client::CloseConnection(SOCKET sck)
	{
		//return;
		closesocket(sck);
	}
	bool Client::Send(const String& str)
	{
		int iResult;

		memset(SendBuffer,0, sizeof(SendBuffer));
		//size_t size = wcstombs(SendBuffer, str.c_str(), SendBufferSize);

		
		int actl = static_cast<int>( str.length());
		while (actl >= 1 && (str[actl-1] == ' ' || str[actl-1] == '\n' || str[actl-1] == '\r'))
		{
			actl--;
		}
		
		String str2 = str.substr(0, actl);
		str2.append(L"\n");
		

		BOOL flag;
		int size = WideCharToMultiByte(CP_ACP, WC_NO_BEST_FIT_CHARS, str2.c_str(), (int)str2.length(), SendBuffer, SendBufferSize, NULL, &flag);
		// Send an initial buffer
		iResult = send(m_socket,
			SendBuffer, 
			(int)size, 0);

		if (iResult == SOCKET_ERROR) {
			printf("send failed: %d\n", WSAGetLastError());
			return false;
		}
		return true;
	}
	String Client::ReceiveLine()
	{
		String str(L"");

		bool finished = false;
		while (!finished)
		{
			memset(RecvBuffer, 0, RecvBufferSize);

			int rbytes = recv(m_socket, RecvBuffer,RecvBufferSize, MSG_PEEK);

			if (rbytes>0)
			{
				const wchar_t LineEnd = '\n';

				memset(RecvBufferW, 0, RecvBufferSize*sizeof(wchar_t));
				int wslen = MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, RecvBuffer, 
					rbytes, RecvBufferW, RecvBufferSize);

				const wchar_t* wData = RecvBufferW;

				int len = 0;
				for (int i=0;i<wslen;i++)
				{
					if (wData[i]==LineEnd)
					{
						len++;
						finished = true;
						break;
					}
					else if (!wData[i])
					{
						break;
					}
					else
					{
						len += wData[i]>0xff ? 2 : 1;
					}
				}

				if (len >0)
				{
					memset(RecvBuffer, 0, RecvBufferSize);

					rbytes = recv(m_socket, RecvBuffer, len, 0);

					memset(RecvBufferW, 0, RecvBufferSize*sizeof(wchar_t));
					wslen = MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, RecvBuffer, 
						rbytes, RecvBufferW, RecvBufferSize);//mbstowcs(RecvBufferW, RecvBuffer , RecvBufferSize);

					str.append(RecvBufferW);
				}
			}
			Sleep(1);
		}

		int actl = static_cast<int>( str.length());
		while (actl >= 1 && (str[actl-1] == ' ' || str[actl-1] == '\n' || str[actl-1] == '\r'))
		{
			actl--;
		}
		
		String str2 = str.substr(0, actl);
		return str2;
	}


}