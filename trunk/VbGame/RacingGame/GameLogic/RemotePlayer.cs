using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.GameLogic
{
    class RemotePlayer
    {
        string playerId;

        Matrix transform;

        string carStyle;
        Color carColor;

        public string ID { get { return playerId; } }
        public string CarStyle { get { return carStyle; } }
        public Color CarColor { get { return carColor; } }

        public RemotePlayer(StartUpParameters.PlayerInfo pinfo)
        {
            this.playerId = pinfo.ID;
            carStyle = pinfo.CarID;
            carColor = pinfo.CarColor;

            transform = Matrix.Identity;
        }

        public void Update(GameTime time)
        {

        }

    }
}
