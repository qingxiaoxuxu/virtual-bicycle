using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Graphics;

namespace RacingGame.GameLogic
{
    public class RemotePlayer
    {
        string playerId;




        Matrix oldTransform;
        Matrix newTransform;

        Matrix transform;

        float lerpPrg;

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
        public float Speed { get { return state.Velocity.Length(); } }
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

            oldTransform = transform;
            newTransform = state.Transform;

            lerpPrg = 0;

            //if (firstSet)
            //{
            //    oldPos[3] = oldPos[2] = oldPos[1] = oldPos[0] = state.Transform.Translation;
            //    oldOri[0] = Quaternion.CreateFromRotationMatrix(state.Transform);
            //    oldOri[3] = oldOri[2] = oldOri[1] = oldOri[0];
            //}
            //else
            //{
            //    for (int i = 0; i < oldPos.Length - 1; i++)
            //    {
            //        oldPos[i] = oldPos[i + 1];
            //        oldOri[i] = oldOri[i + 1];
            //    }
            //    oldPos[3] = state.Transform.Translation;
            //    oldOri[3] = Quaternion.CreateFromRotationMatrix(state.Transform);

            //}

            //Vector3 pos = Vector3.Zero;


            //for (int i = 0; i < oldPos.Length; i++)
            //{
            //    pos += state.Transform.Translation * 0.25f;
            //}
            //Quaternion ori1 = Quaternion.Slerp(oldOri[0], oldOri[1], 0.5f);
            //Quaternion ori2 = Quaternion.Slerp(oldOri[2], oldOri[3], 0.5f);
            //Quaternion ori = Quaternion.Slerp(ori1, ori2, 0.5f);

            //transform = Matrix.CreateFromQuaternion(ori);
            //transform.Translation = pos;
        }
        public void Update(GameTime time)
        {
            lerpPrg += BaseGame.MoveFactorPerSecond * 5;
            if (lerpPrg > 1)
                lerpPrg = 1;

            transform = Matrix.Lerp(oldTransform, newTransform, lerpPrg);
            //Vector3 np =  transform.Translation + state.Velocity * BaseGame.MoveFactorPerSecond * 0.1f;
            //for (int i = 0; i < oldPos.Length - 1; i++)
            //{
            //    oldPos[i] = oldPos[i + 1];
            //}
            //oldPos[3] = np;


            //Vector3 pos = Vector3.Zero;


            //for (int i = 0; i < oldPos.Length; i++)
            //{
            //    pos += state.Transform.Translation * 0.25f;
            //}
            //Quaternion ori1 = Quaternion.Slerp(oldOri[0], oldOri[1], 0.5f);
            //Quaternion ori2 = Quaternion.Slerp(oldOri[2], oldOri[3], 0.5f);
            //Quaternion ori = Quaternion.Slerp(ori1, ori2, 0.5f);

            //transform = Matrix.CreateFromQuaternion(ori);
            //transform.Translation = pos;
        }

    }
}
