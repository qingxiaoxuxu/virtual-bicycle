#include "InterfacePlugin.h"
#include "TestNet.h"

using namespace TestClient;

namespace RacingGame
{
	void InterfacePlugin::Load()
	{		
		InterfaceFactory::Instance->RegisterNewNetwork(gcnew TestNet());
	}
}
