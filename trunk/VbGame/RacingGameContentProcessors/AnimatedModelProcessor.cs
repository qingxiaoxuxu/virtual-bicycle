#region File Description
//-----------------------------------------------------------------------------
// AnimatedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Text;
using System.IO;
using Apoc3D.Graphics.Animation;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace CustomModelAnimationPipeline
{
    [ContentTypeWriter]
    public class AnimationDataWriter : ContentTypeWriter<AnimationData>
    {
        static readonly string BindPoseTag = "BindPose";
        static readonly string BindPoseCountTag = "BindPoseCount";

        static readonly string InvBindPoseTag = "InvBindPose";
        static readonly string InvBindPoseCountTag = "InvBindPoseCount";

        static readonly string ModelAnimationClipTag = "ModelAnimationClip";
        static readonly string ModelAnimationClipCountTag = "ModelAnimationClipCount";

        static readonly string RootAnimationClipTag = "RootAnimationClip";
        static readonly string RootAnimationClipCountTag = "RootAnimationClipCount";

        static readonly string BoneHierarchyTag = "BoneHierarchy";
        static readonly string BoneHierarchyCountTag = "BoneHierarchyCount";

        protected override void Write(ContentWriter output, AnimationData animData)
        {
            BinaryDataWriter data = new BinaryDataWriter();

            ContentBinaryWriter bw;
            #region BindPoseTag
            List<Matrix> bindPose = animData.BindPose;

            if (bindPose != null)
            {
                data.AddEntry(BindPoseCountTag, bindPose.Count);

                bw = data.AddEntry(BindPoseTag);
                for (int i = 0; i < bindPose.Count; i++)
                {
                    bw.Write(bindPose[i]);
                }
                bw.Close();
            }
            #endregion

            #region InvBindPoseTag
            List<Matrix> invBindPose = animData.InverseBindPose;
            if (invBindPose != null)
            {
                data.AddEntry(InvBindPoseCountTag, invBindPose.Count);

                bw = data.AddEntry(InvBindPoseTag);
                for (int i = 0; i < invBindPose.Count; i++)
                {
                    bw.Write(invBindPose[i]);
                }
                bw.Close();
            }

            #endregion

            #region AnimationClipTag

            var aclip = animData.ModelAnimationClips;

            if (aclip != null)
            {
                data.AddEntry(ModelAnimationClipCountTag, aclip.Count);

                bw = data.AddEntry(ModelAnimationClipTag);
                foreach (var e in aclip)
                {
                    bw.WriteStringUnicode(e.Key);

                    ModelAnimationClip clip = e.Value;
                    bw.Write(clip.Duration.TotalSeconds);

                    bw.Write(clip.Keyframes.Count);

                    for (int i = 0; i < clip.Keyframes.Count; i++)
                    {
                        bw.Write(clip.Keyframes[i].Bone);
                        bw.Write(clip.Keyframes[i].Time.TotalSeconds);
                        bw.Write(clip.Keyframes[i].Transform);
                    }
                }
                bw.Close();
            }


            #endregion

            #region RootAnimationClipTag
            aclip = animData.RootAnimationClips;

            if (aclip != null)
            {
                data.AddEntry(RootAnimationClipCountTag, aclip.Count);

                bw = data.AddEntry(RootAnimationClipTag);
                foreach (var e in aclip)
                {
                    bw.WriteStringUnicode(e.Key);

                    ModelAnimationClip clip = e.Value;
                    bw.Write(clip.Duration.TotalSeconds);

                    bw.Write(clip.Keyframes.Count);

                    for (int i = 0; i < clip.Keyframes.Count; i++)
                    {
                        bw.Write(clip.Keyframes[i].Bone);
                        bw.Write(clip.Keyframes[i].Time.TotalSeconds);
                        bw.Write(clip.Keyframes[i].Transform);
                    }
                }
                bw.Close();
            }

            #endregion

            #region BoneHierarchyTag

            List<int> bh = animData.SkeletonHierarchy;
            if (bh != null)
            {
                data.AddEntry(BoneHierarchyCountTag, bh.Count);

                bw = data.AddEntry(BoneHierarchyTag);
                for (int i = 0; i < bh.Count; i++)
                {
                    bw.Write(bh[i]);
                }
                bw.Close();
            }

            #endregion


            output.Write(0); //Õ¼¸öÎ»ÖÃ
            output.Flush();

            long start = output.BaseStream.Position;

            data.Save(new VirtualStream(output.BaseStream, output.BaseStream.Position));

            long end = output.BaseStream.Position;
            int size = (int)(end - start);

            output.BaseStream.Position = start - 4;
            output.Write(size);
            output.BaseStream.Position = end;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Apoc3D.Graphics.Animation.AnimationDataReader, Animation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        }
    }

    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName = "Animated Model Processor")]
    public class AnimatedModelProcessor : ModelProcessor
    {        
        const int MaxBones = 59;

         
        /// <summary> 
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelConte nt object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context) 
        { 

            ValidateMesh(input, context, null);  
                        
            List<int> boneHierarchy = new List<int>();
             
            // Chain to the base ModelProcessor class so it can convert the model data. 
            ModelContent model = base.Process(input, context);           

            // Add each of the bones
            foreach (ModelBoneContent bone in model.Bones)
            {                
                boneHierarchy.Add(model.Bones.IndexOf(bone.Parent as ModelBoneContent));
            }
            
            // Animation clips inside the object (mesh)
            Dictionary<string, ModelAnimationClip> animationClips = new Dictionary<string, ModelAnimationClip>();
            
            // Animation clips at the root of the object
            Dictionary<string, ModelAnimationClip> rootClips = new Dictionary<string, ModelAnimationClip>();

            // Process the animations
            ProcessAnimations(input, model, animationClips, rootClips);
                        
            // Store the data for the model
            model.Tag = new AnimationData(animationClips, rootClips, null, null, boneHierarchy);            
            

            return model;
        }        


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static void ProcessAnimations(
            NodeContent input,
            ModelContent model,
            Dictionary<string, ModelAnimationClip> animationClips,
            Dictionary<string, ModelAnimationClip> rootClips)
        {            
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();
            for (int i = 0; i < model.Bones.Count; i++)
            {
                string boneName = model.Bones[i].Name;
            
                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in the root of the object            
            foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
            {
                ModelAnimationClip processed = ProcessRootAnimation(animation.Value, model.Bones[0].Name);

                rootClips.Add(animation.Key, processed);
            }

            // Get the unique names of the animations on the mesh children
            List<string> animationNames = new List<string>();
            AddAnimationNodes(animationNames, input);

            // Now create those animations
            foreach (string key in animationNames)
            {
                ModelAnimationClip processed = ProcessAnimation(key, boneMap, input, model);
                
                animationClips.Add(key, processed);
            }
        }

        static void AddAnimationNodes(List<string> animationNames, NodeContent node)
        {            
            foreach (NodeContent childNode in node.Children)
            {
                // If this node doesn't have keyframes for this animation we should just skip it
                foreach (string key in childNode.Animations.Keys)
                {
                    if (!animationNames.Contains(key))
                        animationNames.Add(key);
                }

                AddAnimationNodes(animationNames, childNode);
            }
        }

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        public static ModelAnimationClip ProcessRootAnimation(AnimationContent animation, string name)
        {
            List<ModelKeyframe> keyframes = new List<ModelKeyframe>();

            // The root animation is controlling the root of the bones
            AnimationChannel channel = animation.Channels[name];
            
            // Add the transformations on the root of the model
            foreach (AnimationKeyframe keyframe in channel)
            {
                keyframes.Add(new ModelKeyframe(0, keyframe.Time, keyframe.Transform));
            }            

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new ModelAnimationClip(animation.Duration, keyframes);
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        static ModelAnimationClip ProcessAnimation(
            string animationName,
            Dictionary<string, int> boneMap,
            NodeContent input,
            ModelContent model)
        {
            List<ModelKeyframe> keyframes = new List<ModelKeyframe>();
            TimeSpan duration = TimeSpan.Zero;

            AddTransformationNodes(animationName, boneMap, input, keyframes, ref duration);

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new ModelAnimationClip(duration, keyframes);
        }

        static void AddTransformationNodes(
            string animationName, 
            Dictionary<string, int> boneMap, 
            NodeContent input, 
            List<ModelKeyframe> keyframes, 
            ref TimeSpan duration)
        {
            // Add the transformation on each of the meshes
            foreach (NodeContent childNode in input.Children)
            {
                // If this node doesn't have keyframes for this animation we should just skip it
                if (childNode.Animations.ContainsKey(animationName))
                {
                    AnimationChannel childChannel = childNode.Animations[animationName].Channels[childNode.Name];
                    if (childNode.Animations[animationName].Duration != duration)
                    {
                        if (duration < childNode.Animations[animationName].Duration)
                            duration = childNode.Animations[animationName].Duration;
                    }

                    int boneIndex;
                    if (!boneMap.TryGetValue(childNode.Name, out boneIndex))
                    {
                        throw new InvalidContentException(string.Format(
                            "Found animation for bone '{0}', which is not part of the model.", 
                            childNode.Name));
                    }

                    foreach (AnimationKeyframe keyframe in childChannel)
                    {
                        keyframes.Add(new ModelKeyframe(boneIndex, keyframe.Time, keyframe.Transform));
                    }
                }

                AddTransformationNodes(animationName, boneMap, childNode, keyframes, ref duration);
            }
        }

        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(ModelKeyframe a, ModelKeyframe b)
        {
            return a.Time.CompareTo(b.Time);
        }


        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context, string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. AnimatedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }               
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }        


        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }
    }
}
