// TestServer2.cpp : �������̨Ӧ�ó������ڵ㡣
//

#include "Server.h"

using namespace std;

int _tmain(int argc, _TCHAR* argv[])
{
	InitializeServer();

	while (true)
	{
		Sleep(1);
		UpdateServer();
	}

	FinalizeServer();

	return 0;
}

