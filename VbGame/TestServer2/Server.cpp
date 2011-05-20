#include "Server.h"
#include "StringUtils.h"
#include "Client.h"

#define DEFAULT_PORT "2453"

String g_TeamName = L"Test Team";
String g_MapName = L"Beginner";


enum SV_STATE
{
	SV_INIT,
	SV_WAITING_JOIN,
	SV_WAITING_LOAD,
	SV_RUNNING
} m_state = SV_INIT;

int m_waitingCountDown = 333;
int m_sendDataCountDown = 33;


SOCKET m_socket;
HANDLE m_acceptThread;

vector<Client*> m_clients;



CRITICAL_SECTION m_lostConnLock;

unsigned int WINAPI _CLAcceptProc( LPVOID lpParameter );
SOCKET CreateServer();
void CloseServer();

void InitializeServer()
{
	WSADATA wsaData;
	int iResult;

	// Initialize Winsock
	iResult = WSAStartup(MAKEWORD(2,2), &wsaData);
	if (iResult != 0) {
		printf("WSAStartup failed: %d\n", iResult);
	}

	m_socket = CreateServer();

	InitializeCriticalSection(&m_lostConnLock);

}
void FinalizeServer()
{
	CloseServer();

	WSACleanup();

	DeleteCriticalSection( &m_lostConnLock );
}

SOCKET CreateServer()
{

	struct addrinfo *result = NULL, *ptr = NULL, hints;

	ZeroMemory(&hints, sizeof (hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;

	// Resolve the local address and port to be used by the server
	int iResult = getaddrinfo(NULL, DEFAULT_PORT, &hints, &result);
	if (iResult != 0) {
		printf("getaddrinfo failed: %d\n", iResult);
		WSACleanup();
		return 1;
	}
	SOCKET ListenSocket = INVALID_SOCKET;

	// Create a SOCKET for the server to listen for client connections

	ListenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);

	if (ListenSocket == INVALID_SOCKET) {
		printf("Error at socket(): %ld\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}


	// Setup the TCP listening socket
	iResult = bind( ListenSocket, result->ai_addr, (int)result->ai_addrlen);
	if (iResult == SOCKET_ERROR) {
		printf("bind failed with error: %d\n", WSAGetLastError());
		freeaddrinfo(result);
		closesocket(ListenSocket);
		WSACleanup();
		return 1;
	}
	freeaddrinfo(result);


	if ( listen( ListenSocket, SOMAXCONN ) == SOCKET_ERROR ) {
		printf( "Listen failed with error: %ld\n", WSAGetLastError() );
		closesocket(ListenSocket);
		WSACleanup();
		return 1;
	}

	m_acceptThread = ( HANDLE )_beginthreadex( NULL, 0,  _CLAcceptProc, NULL, CREATE_SUSPENDED, NULL );
	ResumeThread( m_acceptThread );


	return ListenSocket;
}
void CloseServer()
{
	closesocket(m_socket);
}

void SendStartSignal()
{
	String allState = L"START\n";

	for (size_t i=0;i<m_clients.size();i++)
	{
		m_clients[i]->Send(allState);
	}
}
void SendState()
{
	String allState = L"STATE ";
	for (size_t i=0;i<m_clients.size();i++)
	{
		allState.append(m_clients[i]->GetBikeStateString());
	}
	allState.append(L"\n");

	for (size_t i=0;i<m_clients.size();i++)
	{
		m_clients[i]->Send(allState);
	}
}
void SendPlayerInfo(Client* cl)
{
	String allInfo = L"INFO ";

	String temp = g_TeamName;
	EncodeString(temp);
	allInfo.append(temp);
	allInfo.append(L" ");

	temp = g_MapName;
	EncodeString(temp);
	allInfo.append(temp);
	allInfo.append(L" ");
	
	for (size_t i=0;i<m_clients.size();i++)
	{
		allInfo.append(m_clients[i]->GetPlayerInfoString());
	}

	allInfo.append(L"\n");
	cl->Send(allInfo);
	cl->NotifyInfoSent();
}


void UpdateServer()
{
	switch ((int)m_state)
	{
	case (int)SV_INIT:
		{
			m_state = SV_WAITING_JOIN;
			cout << "Waiting players joining in.\n";
			break;
		}
	case (int)SV_WAITING_JOIN:
		{
			if (m_clients.size())
			{
				m_waitingCountDown--;
				if (m_waitingCountDown<0)
				{
					m_state = SV_WAITING_LOAD;
					cout << "Game entering\n";
				}
			}
			break;
		}
	case (int)SV_WAITING_LOAD:
		{
			bool passed = true;
			for (size_t i=0;i<m_clients.size();i++)
			{
				if (!m_clients[i]->IsReady())
				{
					passed = false;
					break;
				}
			}
			if (passed)
			{
				SendStartSignal();
				m_state = SV_RUNNING;
				cout << "Game started\n";
			}
			break;
		}
		
	case (int)SV_RUNNING:
		{
			m_sendDataCountDown--;
			if (m_sendDataCountDown<0)
			{
				m_sendDataCountDown = 33;
				SendState();
			}
			break;
		}
	}
	for (size_t i=0;i<m_clients.size();i++)
	{
		if (m_clients[i]->getSendInfoReqested())
		{
			SendPlayerInfo(m_clients[i]);
		}
	}
}

unsigned int WINAPI _CLAcceptProc( LPVOID lpParameter )
{
	while (m_socket!=INVALID_SOCKET)
	{
		if (m_state == SV_WAITING_JOIN)
		{
			SOCKET newSock = accept(m_socket, NULL, NULL);
			if (newSock != INVALID_SOCKET)
			{
				EnterCriticalSection(&m_lostConnLock);
				Client* cl = new Client(newSock);

				LeaveCriticalSection(&m_lostConnLock);

				try{
					cl->AskID();
				}
				catch (const wchar_t* e)
				{
					cout << "Connection failed. Bad client.";
				}

				EnterCriticalSection(&m_lostConnLock);
				m_clients.push_back(cl);
				LeaveCriticalSection(&m_lostConnLock);

				cout << "Player ";
				
				char buffer[64];
				wcstombs(buffer, cl->getName().c_str(), 64);
				

				cout << buffer;

				cout << " joined. Welcome! \n";
			}

		}
		else
		{
			Sleep(50);
		}
	}

	return 0;
}


unsigned int WINAPI _CLReceiveProc( LPVOID lpParameter )
{
	Client* cl = (Client*)lpParameter;

	while (true)
	{
		try
		{
			cl->ReceivePeer();
		}
		catch (const wchar_t* e)
		{
			EnterCriticalSection(&m_lostConnLock);
			m_clients.erase(find(m_clients.begin(),m_clients.end(), cl));

			LeaveCriticalSection(&m_lostConnLock);
			cout << "Player ";

			
			char buffer[64];
			wcstombs(buffer, cl->getName().c_str(), 64);
				

			cout << buffer;
			cout << " lost connection...\n";

			delete cl;
		}		
	}

	return 0;
}


