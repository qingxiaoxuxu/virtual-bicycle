
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace Apoc3D.Graphics.Animation
{
    /// <summary>
    /// The animation player contains a single transformation that is used to move/position/scale
    /// something.
    /// </summary>
    public class RootAnimationPlayer : ModelAnimationPlayerBase
    {
        Matrix currentTransform;        
        
        /// <summary>
        /// Initializes the transformation to the identity
        /// </summary>
        protected override void InitClip()
        {
            this.currentTransform = CurrentClip.Keyframes.Count > 0 ? CurrentClip.Keyframes[0].Transform : Matrix.Identity;
        }

        /// <summary>
        /// Sets the key frame by storing the current transform
        /// </summary>
        /// <param name="keyframe"></param>
        protected override void SetKeyframe(ModelKeyframe keyframe)
        {
            this.currentTransform = keyframe.Transform;
        }

        /// <summary>
        /// Gets the current transformation being applied
        /// </summary>
        /// <returns>Transformation matrix</returns>
        public Matrix GetCurrentTransform()
        {
            return this.currentTransform;
        }

        public override Matrix GetTransform(int boneId)
        {
            return currentTransform;
        }
    }
}
