using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.GameLogic
{
    public class RemotePlayer
    {
        string playerId;

        Matrix transform;

        string carStyle;
        Color carColor;
        BikeState state;
        
        public string Name
        {
            get;
            private set;
        }
        public float CompletionProgress
        {
            get;
            private set;
        }
        public string ID { get { return playerId; } }
        public string CarStyle { get { return carStyle; } }
        public Color CarColor { get { return carColor; } }

        public Matrix Transform { get { return transform; } }

        public RemotePlayer(StartUpParameters.PlayerInfo pinfo)
        {
            Name = pinfo.Name;
            
            this.playerId = pinfo.ID;
            carStyle = pinfo.CarID;
            carColor = pinfo.CarColor;

            transform = Matrix.Identity;
        }

        public void NotifyNewState(BikeState state) 
        {
            this.state = state;
            CompletionProgress = state.CompletionProgress;
            transform = state.Transform;

        }
        public void Update(GameTime time)
        {
            transform.Translation += state.Velocity * (float)time.ElapsedGameTime.TotalSeconds;            
        }

    }
}
