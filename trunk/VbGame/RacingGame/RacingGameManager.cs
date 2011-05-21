#region File Description
//-----------------------------------------------------------------------------
// RacingGameManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using RacingGame.GameLogic;
using RacingGame.GameScreens;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Landscapes;
using RacingGame.Sounds;
using Model = RacingGame.Graphics.Model;
using Texture = RacingGame.Graphics.Texture;
using RacingGame.Properties;
using RacingGame.Shaders;
using System.Threading;
using System.Windows.Forms;
#endregion

namespace RacingGame
{
    /// <summary>
    /// This is the main entry class our game. Handles all game screens,
    /// which themself handle all the game logic.
    /// As you can see this class is very simple, which is really cool.
    /// </summary>
    public class RacingGameManager : BaseGame
    {
        const int SendStateCount = 6;
        static int SendStateCD;

        #region Variables
        /// <summary>
        /// Game screens stack. We can easily add and remove game screens
        /// and they follow the game logic automatically. Very cool.
        /// </summary>
        private static Stack<IGameScreen> gameScreens = new Stack<IGameScreen>();


        /// <summary>
        /// Player for the game, also allows us to control the car and contains
        /// all the required code for the car physics, chase camera and basic
        /// player values and the game time because this is the top class
        /// of many derived classes. Player, car and camera position is set
        /// when the game starts depending on the selected level.
        /// </summary>
        private static Player player;
        

        /// <summary>
        /// Car model and selection plate for the car selection screen.
        /// </summary>
        private static Cyclist carModel = null;
        static Cyclist[] remoteModels;



        /// <summary>
        /// Material for brake tracks on the road.
        /// </summary>
        private static Material brakeTrackMaterial = null;

        ///// <summary>
        ///// Car colors for the car selection screen.
        ///// </summary>
        //public static List<Color> CarColors = new List<Color>(
        //    new Color[]
        //    {
        //        Color.White,
        //        Color.Yellow,
        //        Color.Blue,
        //        Color.Purple,
        //        Color.Red,
        //        Color.Green,
        //        Color.Teal,
        //        Color.Gray,
        //        Color.Chocolate,
        //        Color.Orange,
        //        Color.SeaGreen,
        //    });

        /// <summary>
        /// Landscape we are currently using.
        /// </summary>
        private static Landscape landscape = null;

        /// <summary>
        /// Level we use for our track and landscape
        /// </summary>
        public enum Level
        {
            Beginner,
            Advanced,
            Expert,
        }

        /// <summary>
        /// Load level
        /// </summary>
        /// <param name="setNewLevel">Set new level</param>
        public static void LoadLevel(Level setNewLevel)
        {
            landscape.ReloadLevel(setNewLevel);
        }
        #endregion

        #region Properties
        /// <summary>
        /// In menu
        /// </summary>
        /// <returns>Bool</returns>
        public static bool InMenu
        {
            get
            {
                return gameScreens.Count > 0 &&
                    gameScreens.Peek().GetType() != typeof(GameScreen);
            }
        }

        /// <summary>
        /// In game?
        /// </summary>
        public static bool InGame
        {
            get
            {
                return gameScreens.Count > 0 &&
                    gameScreens.Peek().GetType() == typeof(GameScreen);
            }
        }

        /// <summary>
        /// ShowMouseCursor
        /// </summary>
        /// <returns>Bool</returns>
        public static bool ShowMouseCursor
        {
            get
            {
                // Only if not in Game, not in splash screen!
                return gameScreens.Count > 0 &&
                    gameScreens.Peek().GetType() != typeof(GameScreen);// &&
                    //gameScreens.Peek().GetType() != typeof(SplashScreen);
            }
        }

        /// <summary>
        /// In car selection screen
        /// </summary>
        /// <returns>Bool</returns>
        public static bool InCarSelectionScreen
        {
            get
            {
                return gameScreens.Count > 0;// &&
                    //gameScreens.Peek().GetType() == typeof(CarSelection);
            }
        }

        /// <summary>
        /// Player for the game, also allows us to control the car and contains
        /// all the required code for the car physics, chase camera and basic
        /// player values and the game time because this is the top class
        /// of many derived classes.
        /// Easy access here with a static property in case we need the player
        /// somewhere in the game.
        /// </summary>
        /// <returns>Player</returns>
        public static Player Player
        {
            get
            {
                return player;
            }
        }

        /// <summary>
        /// Car model
        /// </summary>
        /// <returns>Model</returns>
        public static Cyclist CarModel
        {
            get
            {
                return carModel;
            }
        }
        /// <summary>
        /// Car model
        /// </summary>
        /// <returns>Model</returns>
        public static Cyclist[] RemoteCarModel
        {
            get
            {
                return remoteModels;
            }
        }

        /// <summary>
        /// Car color
        /// </summary>
        /// <returns>Color</returns>
        public static Color LocalPlayerCarColor
        {
            get;
            private set;
        }
        public static string LocalUID
        {
            get { return localUID; }
        }
        public static string LocalPlayerName
        {
            get;
            private set;
        }


        /// <summary>
        /// Brake track material
        /// </summary>
        /// <returns>Material</returns>
        public static Material BrakeTrackMaterial
        {
            get
            {
                return brakeTrackMaterial;
            }
        }

        public static IInputInterface InputInterface
        {
            get { return inputInterface; }
        }

        /// <summary>
        /// Landscape we are currently using, used for several things (menu
        /// background, the game, some other classes outside the landscape class).
        /// </summary>
        /// <returns>Landscape</returns>
        public static Landscape Landscape
        {
            get
            {
                return landscape;
            }
        }

        public static float CarOffset
        {
            get { return carOffset; }
        }
        #endregion

        static float carOffset;
        static StartUpParameters startUpParam;
        static string localUID;
        static INetInterface netClient;
        static Dictionary<string, RemotePlayer> remotePlayers = new Dictionary<string, RemotePlayer>();
        
        static IInputInterface inputInterface;


        public static Dictionary<string, RemotePlayer> RemotePlayers
        {
            get { return remotePlayers; }
        }

        #region Constructor
        /// <summary>
        /// Create Racing game
        /// </summary>
        public RacingGameManager(INetInterface netCl, string uid, StartUpParameters sup)
            : base("Virutal Bicycle")
        {
            inputInterface = InterfaceFactory.Instance.GetInput();
            player = new Player(new Vector3(0, 0, 0));

            // Start playing the menu music
            //Sound.Play(Sound.Sounds.MenuMusic);
            localUID = uid;
            startUpParam = sup;

            netClient = netCl;
            for (int i = 0; i < sup.Players.Length; i++)
            {
                if (sup.Players[i].ID == uid)
                {
                    LocalPlayerCarColor = sup.Players[i].CarColor;
                    LocalPlayerName = sup.Players[i].Name;
                    break;
                }
            }

        }

        /// <summary>
        /// Create Racing game for unit tests, not used for anything else.
        /// </summary>
        public RacingGameManager(string unitTestName)
            : base(unitTestName)
        {
            // Don't add game screens here
        }

        /// <summary>
        /// Load car stuff
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Load models
            carModel = new Cyclist();


            Level lvl;
            try
            {
                lvl = (Level)Enum.Parse(typeof(Level), startUpParam.MapName);
            }
            catch (Exception)
            {
                MessageBox.Show("Unsupported Map: " + startUpParam.MapName, "Error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                Exit();
                return;
            }

            // Load landscape
            landscape = new Landscape(lvl);

            brakeTrackMaterial = new Material("track");

            gameScreens.Push(new GameScreen(lvl));


            netClient.TellReady();

            while (!netClient.CanStartGame())
            {
                Thread.Sleep(10);
            }
            startUpParam = netClient.DownloadStartUpParameters();
            for (int i = 0; i < startUpParam.Players.Length; i++)
            {
                if (startUpParam.Players[i].ID != localUID)
                {
                    RemotePlayer plr = new RemotePlayer(startUpParam.Players[i]);

                    remotePlayers.Add(plr.ID, plr);
                }
                else 
                {
                    carOffset = i / (float)startUpParam.Players.Length;
                    landscape.SetCarToStartPosition();
                }
            }


            remoteModels = new Cyclist[remotePlayers.Count];
            for (int i = 0; i < remotePlayers.Count; i++)
            {
                remoteModels[i] = new Cyclist();
            }
        }


        #endregion

        #region Add game screen
        /// <summary>
        /// Add game screen
        /// </summary>
        /// <param name="gameScreen">Game screen</param>
        public static void AddGameScreen(IGameScreen gameScreen)
        {
            // Play sound for screen click
            Sound.Play(Sound.Sounds.ScreenClick);

            // Add the game screen
            gameScreens.Push(gameScreen);
        }
        #endregion

        #region Update


        void UpdateNet(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            SendStateCD--;

            if (SendStateCD < 0)
            {
                BikeState state;
                state.ID = localUID;
                state.CompletionProgress = player.GetCompletionProgress();
                state.Transform = player.CarRenderMatrix;
                state.Velocity = player.CarDirection * player.Speed;

                SendStateCD = SendStateCount;

                netClient.SendBikeState(new BikeState[] { state });




                BikeState[] states = netClient.DownloadBikeState();
                if (states != null) 
                {
                    for (int i = 0; i < states.Length; i++)
                    {
                        RemotePlayer rp;

                        if (states[i].ID!=null && remotePlayers.TryGetValue(states[i].ID, out rp))
                        {
                            rp.NotifyNewState(states[i]);
                        }
                    }
                }


                Vector3 hoz = player.CarDirection;
                hoz.Y = 0;
                Vector3.Normalize(ref hoz, out hoz);

                float f = -Vector3.Dot(hoz, player.CarUpVector);

                f *= 5;
                if (f > 1) f = 1;
                if (f < -1) f = -1;

                inputInterface.ForceFeedBack(f);
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update game engine
            base.Update(gameTime);

            inputInterface.Update(gameTime);

            // Update player and game logic
            player.Update();
            carModel.Update(gameTime);
            for (int i = 0; i < remoteModels.Length; i++)
            {
                remoteModels[i].Update(gameTime);
            }

            foreach (var e in remotePlayers)
            {
                e.Value.Update(gameTime);
            }
            UpdateNet(gameTime);
        }
        #endregion

        #region Render
        /// <summary>
        /// Render
        /// </summary>
        protected override void Render()
        {
            // No more game screens?
            if (gameScreens.Count == 0)
            {
                // Before quiting, stop music and play crash sound :)
                Sound.PlayCrashSound(true);
                Sound.StopMusic();

                // Then quit
                Exit();
                return;
            }

            // Handle current screen
            if (gameScreens.Peek().Render())
            {
                //// If this was the options screen and the resolution has changed,
                //// apply the changes
                //if ((BaseGame.Width != GameSettings.Default.ResolutionWidth ||
                //    BaseGame.Height != GameSettings.Default.ResolutionHeight ||
                //    BaseGame.Fullscreen != GameSettings.Default.Fullscreen))
                //{
                //    BaseGame.ApplyResolutionChange();
                //}

                // Play sound for screen back
                Sound.Play(Sound.Sounds.ScreenBack);

                gameScreens.Pop();
            }
        }

        /// <summary>
        /// Post user interface rendering, in case we need it.
        /// Used for rendering the car selection 3d stuff after the UI.
        /// </summary>
        protected override void PostUIRender()
        {
            // Enable depth buffer again
            BaseGame.Device.RenderState.DepthBufferEnable = true;

            // Currently in car selection screen?
            //if (gameScreens.Count > 0) &&
               // gameScreens.Peek().GetType() == typeof(CarSelection))
               // ((CarSelection)gameScreens.Peek()).PostUIRender();

            // Do menu shader after everything
            if (RacingGameManager.InMenu &&
                PostScreenMenu.Started)
                UI.PostScreenMenuShader.Show();
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                inputInterface.Disconnect();
            }
        }
    }
}
