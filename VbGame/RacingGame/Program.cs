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
using System.Threading;
using System.Collections.Generic;
using VbClient;
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
            InterfacePlugin.Load();

            StartGame(args[0]);
        }
        #endregion

        #region StartGame
        /// <summary>
        /// Start game, is in a seperate method for 2 reasons: We want to catch
        /// any exceptions here, but not for the unit tests and we also allow
        /// the unit tests to call this method if we don't want to unit test
        /// in debug mode.
        /// </summary>
        public static void StartGame(string uid)
        {
            INetInterface netClient = InterfaceFactory.Instance.GetNetwork();

            bool result = netClient.Connect(uid);

            if (!result)
            {
                MessageBox.Show("Cannot connect to server.",
                         "RacingGame",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Thread.Sleep(100);
            StartUpParameters sup = netClient.DownloadStartUpParameters();
            
#if !XBOX360
            try
            {
#endif
                using (RacingGameManager game = new RacingGameManager(netClient, uid, sup))
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
            netClient.Disconnect();
        }

        #endregion
    }
}
