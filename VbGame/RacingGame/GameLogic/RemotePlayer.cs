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

        int carStyle;
        Color carColor;


        public RemotePlayer(string id)
        {
            this.playerId = id;

        }

        public void Update(GameTime time)
        {
            
        }

    }
}
