#pragma once

namespace TestClient
{
 	public ref class InterfacePlugin
	{
	public:
		static void Load() { RacingGame::InterfaceFactory::Instance::RegisterNewNetwork(gcnew TestNet()); }
	};
}