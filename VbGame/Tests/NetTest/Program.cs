using System;
using System.Collections.Generic;
using System.Text;
using TestClient;
using RacingGame;
using System.Threading;

namespace NetTest
{
    class Program
    {

        static Random rnd = new Random();

        class FakeBikeClient : IDisposable
        {
            BikeState fakeState;

            TestNet tn = new TestNet();

            public FakeBikeClient(string uid) 
            {
                fakeState.ID = uid;
                fakeState.CompletionProgress = (float)rnd.NextDouble();
                fakeState.Transform.M44 = (float)rnd.NextDouble();


                tn.Connect(uid);

                Thread.Sleep(100);
                StartUpParameters sup = tn.DownloadStartUpParameters();

            }

            public void Ready() 
            {
                tn.TellReady();
            }

            public bool CanStart() 
            {
                return tn.CanStartGame();
            }

            public void DebugPrint() 
            {
                tn.SendBikeState(new BikeState[] { fakeState });
                BikeState[] state = tn.DownloadBikeState();
                if (state != null)
                {
                    for (int i = 0; i < state.Length; i++) 
                    {
                        Console.Write(state[i].ID);
                        Console.Write(", ");
                        Console.Write(state[i].CompletionProgress.ToString());
                        Console.Write(", ");
                        Console.WriteLine(state[i].Transform.M44.ToString());
                    }
                   
                }
                else 
                {
                    Console.WriteLine("No data.");
                }
                

            }

            #region IDisposable 成员

            public void Dispose()
            {
                tn.Disconnect();
            }

            #endregion
        }

        static void Main(string[] args)
        {
            Console.Write("Input ID:");
            string id = Console.ReadLine();

            using (FakeBikeClient cl = new FakeBikeClient(id))
            {                
                Thread.Sleep(1000);
                cl.Ready();

                while (!cl.CanStart()) 
                {
                    Thread.Sleep(1);
                }

                for (int k = 0; k < 10; k++)
                {
                    cl.DebugPrint();
                    Thread.Sleep(1000);
                }
            }

        }
    }
}
