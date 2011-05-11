#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using RacingGame.Helpers;
using RacingGame.Properties;
using Microsoft.Xna.Framework;
#if !XBOX360
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using VbClient.Net;
using System.Threading;
using System.Collections.Generic;
#endif
#endregion

namespace RacingGame
{
    /// <summary>
    /// Program
    /// </summary>
    static class Program
    {
        

        #region Main
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments</param>
#if !XBOX360
        [STAThread]
#endif
        static void Main(string[] args)
        {
            if (args.Length == 0) 
            {
                MessageBox.Show("This program requires command line parameters to run.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StartGame(args[0]);
        }
        #endregion



        //static void netClient_RoomDetail(string teamName, string mapName, List<string> userId, List<string> userName, List<string> carId)
        //{
        //    startUpParams.TeamName = teamName;
        //    startUpParams.MapName = mapName;
        //    startUpParams.Players = new StartUpParameters.PlayerInfo[userId.Count];
        //    for (int i = 0; i < userId.Count; i++)
        //    {
        //        startUpParams.Players[i].ID = userId[i];
        //        startUpParams.Players[i].Name = userName[i];
        //        startUpParams.Players[i].CarID = carId[i];
        //    }
        //    isRoomInfoAcquired = true;
        //}


        #region StartGame
        /// <summary>
        /// Start game, is in a seperate method for 2 reasons: We want to catch
        /// any exceptions here, but not for the unit tests and we also allow
        /// the unit tests to call this method if we don't want to unit test
        /// in debug mode.
        /// </summary>
        public static void StartGame(string uid)
        {
            INetInterface netClient;


            StartUpParameters startUpParams;
            startUpParams.MapName = "Beginner";
            startUpParams.TeamName = "Test Team";
            startUpParams.Players = new StartUpParameters.PlayerInfo[1];
            startUpParams.Players[0].CarID = "0";
            startUpParams.Players[0].ID = uid;
            startUpParams.Players[0].Name = "Test Player";

            //netClient = new ClientGEvt("localhost");

            //netClient.Client.ConnectedServer += new EventHandler(Client_ConnectedServer);
            

            //while (!isRoomInfoAcquired)
            //{
                //Thread.Sleep(10);
            //}
            //netClient.RoomDetail -= netClient_RoomDetail;
#if !XBOX360
            try
            {
#endif
                using (RacingGameManager game = new RacingGameManager(netClient, uid, startUpParams))
                {
                    game.Run();
                }
#if !XBOX360
            }
            catch (NoSuitableGraphicsDeviceException)
            {

                MessageBox.Show("Pixel and vertex shaders 2.0 or greater are required.",
                    "RacingGame",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OutOfVideoMemoryException)
            {
                GameSettings.SetMinimumGraphics();

                MessageBox.Show("Insufficent video memory.\n\n" +
                    "The graphics settings have been reconfigured to the minimum. " +
                    "Please restart the application. \n\nIf you continue to receive " +
                    "this error message, your system may not meet the " +
                    "minimum requirements.",
                    "RacingGame",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }

            //static void Client_ConnectedServer(object sender, EventArgs e)
            //{
                
            //    netClient.RoomDetail += netClient_RoomDetail;
            //    netClient.ConnectToServer(Uid);
            //    netClient.RequestRoomInfo();
            //}

        #endregion
    }
}
