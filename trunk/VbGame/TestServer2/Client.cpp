#include "Client.h"

#include "StringUtils.h"

struct PlayerData
{
	String ID;
	String Name;
	String CarID;
	uint CarColor;
} const static playerDatabase[4] =
{
	{ L"1",		L"Will Wright",		L"A",	0xffff0000 },
	{ L"2",		L"H.G Hells",		L"A",	0xff00ff00 },
	{ L"3",		L"John Romero",		L"A",	0xff0000ff },
	{ L"4",		L"Sid Meier",		L"A",	0xffffff00 },
};

unsigned int WINAPI _CLReceiveProc( LPVOID lpParameter );

void Client::RetriveInfoFromDB()
{
	for (int i=0;i<4;i++)
	{
		if (!_wcsicmp(m_id.c_str(), playerDatabase[i].ID.c_str()))
		{
			m_name = playerDatabase[i].Name;
			m_carId = playerDatabase[i].CarID;
			m_carColor = playerDatabase[i].CarColor;
			return;
		}
	}
}

Client::Client(SOCKET sck)
	: m_clientSocket(sck), m_progress(0), m_carColor(0), m_ready(false), m_sendInfoReqested(false), m_answeredID(false)
{
	m_name = L"Unkown";
	m_id = L"Unk";

	ZeroMemory(&m_velocity, sizeof(m_velocity));
	ZeroMemory(&m_transform, sizeof(m_transform));

	InitializeCriticalSection(&m_dataLock);

	m_recvThread = ( HANDLE )_beginthreadex( NULL, 0,  _CLReceiveProc, (LPVOID)this, CREATE_SUSPENDED, NULL );
	ResumeThread( m_recvThread );
}
Client::~Client()
{		
	SuspendThread(m_recvThread);

	DeleteCriticalSection( &m_dataLock );		
	closesocket(m_clientSocket);
}
String Client::GetBikeStateString()
{
	EnterCriticalSection(&m_dataLock);

	String temp = m_id;
	EncodeString(temp);
	String result = temp;
	result.append(L" ");

	temp = StringUtils::ToString(m_progress); EncodeString(temp);
	result.append(temp);
	result.append(L" ");

	for (int i=0;i<3;i++)
	{
		temp = StringUtils::ToString(m_velocity[i]); EncodeString(temp);
		result.append(temp);
		result.append(L" ");
	}
	for (int i=0;i<16;i++)
	{
		temp = StringUtils::ToString(m_transform[i]); EncodeString(temp);
		result.append(temp);
		result.append(L" ");
	}
	LeaveCriticalSection(&m_dataLock);

	return result;
}
String Client::GetPlayerInfoString()
{
	EnterCriticalSection(&m_dataLock);

	String temp = m_id; EncodeString(temp);
	String result = temp;
	result.append(L" ");

	temp = m_name; EncodeString(temp);
	result.append(temp);
	result.append(L" ");


	temp = m_carId; EncodeString(temp);
	result.append(temp);
	result.append(L" ");


	temp = StringUtils::ToString(m_carColor); EncodeString(temp);
	result.append(temp);
	result.append(L" ");

	LeaveCriticalSection(&m_dataLock);
	return result;
}

void Client::AskID()
{
	Send(L"TELLID\n");

	while (!m_answeredID)
		Sleep(1);

	RetriveInfoFromDB();		
}

void Client::ReceivePeer()
{
	String str = ReceiveLine();

	vector<String> arg = StringUtils::Split(str);

	if (arg.size()>0)
	{
		if (!_wcsicmp(arg[0].c_str(), L"STATE"))
		{
			if (arg.size() == (16+3+1+1 +1))
			{
				EnterCriticalSection(&m_dataLock);

				String tmp = arg[0+1];
				DecodeString(tmp);
				m_id = tmp;

				tmp = arg[1+1];
				DecodeString(tmp);
				float tmp2 = StringUtils::ParseSingle(tmp);
				m_progress = tmp2;

				for (int i=0;i<3;i++)
				{
					tmp = arg[i+2+1];
					DecodeString(tmp);
					tmp2 = StringUtils::ParseSingle(tmp);
					m_velocity[i] = tmp2;
				}
				for (int i=0;i<16;i++)
				{
					tmp = arg[i+5+1];
					DecodeString(tmp);
					tmp2 = StringUtils::ParseSingle(tmp);
					m_transform[i] = tmp2;
				}
				LeaveCriticalSection(&m_dataLock);
			}
			else
			{
				cout << "Invalid bike state.";
			}
		}
		else if (!_wcsicmp(arg[0].c_str(), L"READY"))
		{
			m_ready = true;
		}
		else if (!_wcsicmp(arg[0].c_str(), L"TELLINFO"))
		{
			m_sendInfoReqested = true;
		}
		else if (!_wcsicmp(arg[0].c_str(), L"ID"))
		{
			if (arg.size() == 2)
			{
				m_id = arg[1];
				DecodeString(m_id);
				m_answeredID = true;
			}
			else
			{
				cout << "Invalid ID.";
				closesocket(m_clientSocket);
				
			}
		}
	}

}


bool Client::Send(const String& str)
{
	int iResult;

	memset(SendBuffer,0, sizeof(SendBuffer));
	int actl = static_cast<int>( str.length());
	while (actl >= 1 && (str[actl-1] == ' ' || str[actl-1] == '\n' || str[actl-1] == '\r'))
	{
		actl--;
	}
	
	String str2 = str.substr(0, actl);
	str2.append(L"\n");
	

	BOOL flag;
	int size = WideCharToMultiByte(CP_ACP, WC_NO_BEST_FIT_CHARS, str2.c_str(), (int)str2.length(), SendBuffer, SendBufferSize, NULL, &flag);

	//memcpy(SendBuffer, str2.c_str(), str2.length() * sizeof(wchar_t));

	// Send an initial buffer
	iResult = send(m_clientSocket,
		SendBuffer, 
		(int)size, 0);

	if (iResult == SOCKET_ERROR) {
		printf("send failed: %d\n", WSAGetLastError());
		return false;
	}
	return true;
	//int iResult;
	//
	//memset(SendBuffer,0, sizeof(SendBuffer));
	////size_t size = wcstombs(SendBuffer, str.c_str(), SendBufferSize);

	//BOOL flag;
	//int size = WideCharToMultiByte(CP_ACP, WC_NO_BEST_FIT_CHARS, str.c_str(), (int)str.length(), SendBuffer, SendBufferSize, NULL, &flag);
	//// Send an initial buffer
	//iResult = send(m_clientSocket,
	//	SendBuffer, 
	//	(int)size, 0);

	//if (iResult == SOCKET_ERROR) {
	//	printf("send failed: %d\n", WSAGetLastError());
	//	return false;
	//}
	//return true;
}
String Client::ReceiveLine()
{
	String str(L"");

	bool finished = false;
	while (!finished)
	{
		memset(RecvBuffer, 0, RecvBufferSize);

		int rbytes = recv(m_clientSocket, RecvBuffer,RecvBufferSize, MSG_PEEK);

		if (rbytes<0)
		{
			throw L"Discononected";
		}
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

				rbytes = recv(m_clientSocket, RecvBuffer, len, 0);

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




