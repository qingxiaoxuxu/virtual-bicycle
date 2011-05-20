#pragma once
#pragma unmanaged

#include "Common.h"


namespace TestClient
{
	class Client
	{
	private:
		static char s_serverAdd[];

		static SOCKET m_socket;

		Client(void) {}
		~Client(void) {}

		static SOCKET CreateConnection();
		static void CloseConnection(SOCKET sck);


	public:
		static void SetServerAddress(const String &str)
		{
			BOOL flag;
			WideCharToMultiByte(CP_ACP, WC_NO_BEST_FIT_CHARS, str.c_str(), (int)str.length(), s_serverAdd, 20, NULL, &flag);
		}
		static void Initialize();
		static void Finalize();


		static bool Send(const String& str);
		static String ReceiveLine();



	};

}
