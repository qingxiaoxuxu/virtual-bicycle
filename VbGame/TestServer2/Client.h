#ifndef CLIENT_H
#define CLIENT_H
#pragma once

#include "Common.h"

using namespace std;

class Client
{
private:

	String m_id;
	String m_name;
	String m_carId;
	uint m_carColor;

	bool m_sendInfoReqested;
	bool m_ready;
	float m_progress;
	float m_velocity[3];
	float m_transform[16];

	bool m_answeredID;
	void RetriveInfoFromDB();

public:
	static const int SendBufferSize = 1024;
	char SendBuffer[SendBufferSize];
	static const int RecvBufferSize = 512;
	char RecvBuffer[RecvBufferSize];
	wchar_t RecvBufferW[RecvBufferSize];

	SOCKET m_clientSocket;


	HANDLE m_recvThread;


	CRITICAL_SECTION m_dataLock;


	const String &getID() const { return m_id; }
	const String &getName() const { return m_name; }
	const String &getCarID() const { return m_carId; }
	uint getCarColor() const { return m_carColor; }

	bool IsReady() const { return m_ready; }
	bool getSendInfoReqested() const { return m_sendInfoReqested; }
	void NotifyInfoSent() { m_sendInfoReqested = false; }

	Client(SOCKET sck);	
	~Client();

	String GetBikeStateString();	
	String GetPlayerInfoString();
	void AskID();
	
	void ReceivePeer();
	

	bool Send(const String& str);
	String ReceiveLine();
	
};
#endif