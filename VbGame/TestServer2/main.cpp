// TestServer2.cpp : 定义控制台应用程序的入口点。
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

