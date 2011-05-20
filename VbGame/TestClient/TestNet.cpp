#include "TestNet.h"
#include "Client.h"
#include "StringUtils.h"

// server
//TELLID; ask the client to tell PlayerID
//INFO; tell the client all players' info and room info
//STATE; tell the client all players' game state
//START; tell the client to start game now
//
// client
//ID; tell the server PlayerID when TELLID is asked
//TELLINFO; request all players' info
//STATE; send the client's game state
//READY; tell the server the client is ready

#pragma managed

using namespace std;

void MarshalString ( System::String ^ s, String& os ) {
	using namespace System::Runtime::InteropServices;
	const wchar_t* chars = 
		(const wchar_t*)(Marshal::StringToHGlobalUni(s)).ToPointer();
	os = chars;
	Marshal::FreeHGlobal(System::IntPtr((void*)chars));
}
bool CompareString(System::String ^ a, System::String ^ b)
{
	int al = a->Length;
	int bl = b->Length;

	System::Char c1 = a[al-1];
	System::Char c2 = b[bl-1];

	//int len = System::Math::Min(a->Length, b->Length);
	if (al!=bl)
		return false;

	for (int i=0;i<a->Length;i++)
		if (a[i]!=b[i])
			return false;
	return true;
}
namespace TestClient
{

	TestNet::TestNet(void)
	{
		m_connected = false;
		m_canStartGame = false;

		m_recvQueue = gcnew System::Collections::Generic::List<array<System::String^>^>();
		m_syncHelper = gcnew System::Object();


		System::Threading::ParameterizedThreadStart^ ts = gcnew System::Threading::ParameterizedThreadStart(this,&TestNet::RecviceLoop);

		m_recvThread = gcnew System::Threading::Thread(ts);
		
	}


	void TestNet::RecviceLoop(System::Object^ state)
	{
		while(true)
		{
			String str = Client::ReceiveLine();

			vector<String> args = StringUtils::Split(str);

			int len = (int)args.size();
			array<System::String^>^ margs = gcnew array<System::String^>(len);

			for (int i=0;i<len;i++)
			{
				DecodeString(args[i]);
				margs[i] = gcnew System::String(args[i].c_str());
			}
			
			if (CompareString(margs[0], "TELLID"))
			{
				String toSend = L"ID ";
											
				String id;
				MarshalString(m_uid, id);
				EncodeString(id);

				toSend.append(id);
				toSend.append(L" \n");

				Client::Send(toSend);
				m_connected = true;
			}
			else if (CompareString(margs[0], "START"))
			{
				m_canStartGame = true;
			}
			else
			{
				System::Threading::Monitor::Enter(m_syncHelper);
				try
				{
					m_recvQueue->Add(margs);
				}
				finally
				{
					System::Threading::Monitor::Exit(m_syncHelper);
				}
			}


		}
	}



	void TestNet::Connect(System::String^ uid)
	{
		m_uid = uid;

		Client::SetServerAddress(L"localhost");
		Client::Initialize();

		m_recvThread->Start();


		while (!m_connected)
		{
			System::Threading::Thread::Sleep(10);
		}

	}


	StartUpParameters TestNet::DownloadStartUpParameters()
	{
		Client::Send(L"TELLINFO\n");

		array<System::String^>^ marg ;
		bool passed = false;
		while (!passed)
		{

			System::Threading::Monitor::Enter(m_syncHelper);
			try
			{				
				for (int i=0;i<m_recvQueue->Count;i++)
				{
					if (CompareString(m_recvQueue[i][0], "INFO"))
					{
						marg = m_recvQueue[i];
						passed = true;
						m_recvQueue->RemoveAt(i);
						break;
					}
				}
			}
			finally
			{
				System::Threading::Monitor::Exit(m_syncHelper);
			}
			
			System::Threading::Thread::Sleep(10);
		}

		StartUpParameters result;
		result.TeamName = marg[1];
		result.MapName = marg[2];

		int pc = (marg->Length-3) / 4;
		
		result.Players = gcnew array<RacingGame::StartUpParameters::PlayerInfo>(pc);
		for (int i=0;i<pc;i++)
		{
			result.Players[i].ID = marg[3+i*4];
			result.Players[i].Name = marg[3+i*4+1];
			result.Players[i].CarID = marg[3+i*4+2];

			System::UInt32 c = System::UInt32::Parse(marg[3+i*4+3]);

			System::Byte a = (System::Byte)(c >> 24);
			System::Byte r = (System::Byte)((c >> 16) & 0xff);
			System::Byte g = (System::Byte)((c >> 8) & 0xff);
			System::Byte b = (System::Byte)(c & 0xff);

			result.Players[i].CarColor = Microsoft::Xna::Framework::Graphics::Color(r,g,b,a);
		}
		return result;
	}
	void TestNet::SendBikeState(array<BikeState>^ state)
	{
		String result = L"STATE ";

		for (int i=0;i<state->Length;i++)
		{			
			String temp;
			MarshalString(state[i].ID, temp);
			EncodeString(temp);
			result.append(temp);
			result.append(L" ");

			temp = StringUtils::ToString(state[i].CompletionProgress); EncodeString(temp);
			result.append(temp);
			result.append(L" ");
			

			temp = StringUtils::ToString(state[i].Velocity.X); EncodeString(temp);
			result.append(temp);
			result.append(L" ");

			temp = StringUtils::ToString(state[i].Velocity.Y); EncodeString(temp);
			result.append(temp);
			result.append(L" ");

			temp = StringUtils::ToString(state[i].Velocity.Z); EncodeString(temp);
			result.append(temp);
			result.append(L" ");

			pin_ptr<float> m = &state[i].Transform.M11;

			for (int j=0;j<16;j++)
			{
				temp = StringUtils::ToString(m[j]); EncodeString(temp);
				result.append(temp);
				result.append(L" ");
			}
		}
		result.append(L"\n");
		Client::Send(result);
	}
	array<BikeState>^ TestNet::DownloadBikeState()
	{
		array<System::String^>^ marg = nullptr;
	
		System::Threading::Monitor::Enter(m_syncHelper);
		try
		{				
			for (int i=m_recvQueue->Count-1;
				i>=0;i--)
			{
				if (CompareString(m_recvQueue[i][0], "STATE"))
				{
					marg = m_recvQueue[i];

					m_recvQueue->RemoveAt(i);
					
				}
			}
		}
		finally
		{
			System::Threading::Monitor::Exit(m_syncHelper);
		}

		const int ItemSize = 16+3+2;
		if (marg!=nullptr)
		{			
			int sc = (marg->Length-1) / ItemSize;
			array<BikeState>^ result = gcnew array<BikeState>(sc);

			for (int i=0;i<sc;i++)
			{
				int ofs = i*ItemSize+1;
				result[i].ID = marg[ofs++];
				result[i].CompletionProgress = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Velocity.X = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Velocity.Y = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Velocity.Z = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);

				result[i].Transform.M11 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M12 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M13 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M14 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M21 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M22 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M23 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M24 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M31 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M32 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M33 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M34 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M41 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M42 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M43 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);
				result[i].Transform.M44 = System::Single::Parse(marg[ofs++], System::Globalization::CultureInfo::InvariantCulture);

			}
			return result;
		}
		
		return nullptr;
	}

	void TestNet::TellReady()
	{
		Client::Send(L"READY\n");
	}
	bool TestNet::CanStartGame()
	{
		return m_canStartGame;
	}

	void TestNet::Disconnect()
	{
		m_recvThread->Abort();

		Client::Finalize();
	}


}