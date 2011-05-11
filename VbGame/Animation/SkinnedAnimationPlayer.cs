#region File Description
//-----------------------------------------------------------------------------
// SkinnedAnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Apoc3D.Graphics.Animation
{
    /// <summary>
    /// The animation player manipulates a skinned model
    /// </summary>
    public class SkinnedAnimationPlayer : ModelAnimationPlayerBase
    {
        bool isFirstPlay = true;
        // Current animation transform matrices.
        Matrix[] boneTransforms;
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;

        List<Matrix> bindPose;
        List<Matrix> inverseBindPose;
        List<int> skeletonHierarchy;
        
        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public SkinnedAnimationPlayer(List<Matrix> bindPose, List<Matrix> inverseBindPose, List<int> skeletonHierarchy)
        {
            if (bindPose == null || bindPose.Count == 0)
                throw new Exception("Bad arguments to model animation player");
            
            boneTransforms = new Matrix[bindPose.Count];
            worldTransforms = new Matrix[bindPose.Count];
            skinTransforms = new Matrix[bindPose.Count];

            this.bindPose = bindPose;
            this.inverseBindPose = inverseBindPose;
            this.skeletonHierarchy = skeletonHierarchy;                
        }

        /// <summary>
        /// Initializes the animation clip
        /// </summary>
        protected override void InitClip()
        {
            if (isFirstPlay)
            {
                bindPose.CopyTo(boneTransforms);
                isFirstPlay = false;
            }
            InitSetTime(TimeSpan.FromSeconds(0.07));
            //List<ModelKeyframe> keyframes = CurrentClip.Keyframes;

            //for (int i = 0; i < keyframes.Count; i++)
            //{
            //    ModelKeyframe keyframe = keyframes[i];

            //    if (keyframe.Time.TotalSeconds > 0.1)
            //    {
            //        break;
            //    }

            //    // Use this keyframe
            //    SetKeyframe(keyframe);
            //}
        }

        /// <summary>
        /// Sets the key frame for the passed in frame
        /// </summary>
        /// <param name="keyframe"></param>
        protected override void SetKeyframe(ModelKeyframe keyframe)
        {
            boneTransforms[keyframe.Bone] = keyframe.Transform;
        }

        /// <summary>
        /// Updates the transformations ultimately needed for rendering
        /// </summary>
        protected override void OnUpdate()
        {
            if (CurrentClip != null)
            {
                // Root bone.
                worldTransforms[0] = boneTransforms[0];
                skinTransforms[0] = inverseBindPose[0] * worldTransforms[0];

                // Child bones.
                for (int bone = 1; bone < worldTransforms.Length; bone++)
                {
                    int parentBone = skeletonHierarchy[bone];

                    worldTransforms[bone] = boneTransforms[bone] * worldTransforms[parentBone];
                    skinTransforms[bone] = inverseBindPose[bone] * worldTransforms[bone];
                }                
            }
        }                       

        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        private Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        }

        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        private Matrix[] GetWorldTransforms()
        {
            return worldTransforms;
        }

        /// <summary>
        /// Gets the current bone transform matrices, relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }

        public override Matrix GetTransform(int boneId)
        {
            return Matrix.Identity;
        }
    }
}
