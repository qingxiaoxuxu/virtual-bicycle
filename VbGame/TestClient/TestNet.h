#pragma once

#include "Common.h"

using namespace RacingGame;

namespace TestClient
{
	public ref class TestNet : public INetInterface
	{
	private:
		System::String^ m_uid;
		bool m_canStartGame;
		bool m_connected;
		bool m_aborting;

		System::Threading::Thread^ m_recvThread;
		System::Object^ m_syncHelper;

		System::Collections::Generic::List<array<System::String^>^>^ m_recvQueue;


		void RecviceLoop(System::Object^ state);
	public:
		TestNet(void);

		virtual bool Connect(System::String^ uid);
		virtual void Disconnect();

		virtual StartUpParameters DownloadStartUpParameters();
		virtual void SendBikeState(array<BikeState>^ state);
		virtual array<BikeState>^ DownloadBikeState();

		virtual void TellReady();

		virtual bool CanStartGame();

	};

}
