using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D.Graphics.Animation;

using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Shaders;

namespace RacingGame.Graphics
{
    public class Cyclist
    {
        XnaModel skinnedModel;
        AnimationData animData;

        RootAnimationPlayer skinnedRootPlayer;
        ModelAnimationClip skinnedRootClip;
        SkinnedAnimationPlayer skinnedPlayer;
        ModelAnimationClip skinnedClip;

        bool playingSkinned;


        XnaModel rigidModel;
        AnimationData animData2;
        RootAnimationPlayer rigidRootPlayer;
        ModelAnimationClip rigidRootClip;
        RigidAnimationPlayer rigidPlayer;
        ModelAnimationClip rigidClip;

        public float TimeScale { get; set; }

        Matrix objectMatrix;

        Effect skinned;



        public Cyclist()
        {
            //string test = typeof(AnimationDataReader).AssemblyQualifiedName;
            TimeScale = 1;
            skinnedModel = BaseGame.Content.Load<XnaModel>("Content\\Models\\bike_boy");
            objectMatrix = Matrix.CreateScale(0.5f);// Matrix.Identity;
            
            // Create animation players for the skinned model
            animData = skinnedModel.Tag as AnimationData;
            if (animData != null)
            {
                if (animData.RootAnimationClips != null && animData.RootAnimationClips.ContainsKey("Take 001"))
                {
                    skinnedRootClip = animData.RootAnimationClips["Take 001"];

                    skinnedRootPlayer = new RootAnimationPlayer();
                    skinnedRootPlayer.Completed += new EventHandler(skinnedPlayer_Completed);
                }
                if (animData.ModelAnimationClips != null && animData.ModelAnimationClips.ContainsKey("Take 001"))
                {
                    skinnedClip = animData.ModelAnimationClips["Take 001"];

                    skinnedPlayer = new SkinnedAnimationPlayer(animData.BindPose, animData.InverseBindPose, animData.SkeletonHierarchy);

                    skinnedPlayer.StartClip(skinnedClip, 1, TimeSpan.Zero);
                    skinnedPlayer.CurrentTimeValue = skinnedClip.Duration;
                    skinnedPlayer.CurrentTimeValue = TimeSpan.Zero;

                    skinnedPlayer.Completed += new EventHandler(skinnedPlayer_Completed);
                }
            }

            rigidModel = BaseGame.Content.Load<XnaModel>("Content\\Models\\bike");

            // Create animation players/clips for the rigid model
            animData2 = rigidModel.Tag as AnimationData;
            if (animData2 != null)
            {
                if (animData2.RootAnimationClips != null && animData2.RootAnimationClips.ContainsKey("Take 001"))
                {
                    rigidRootClip = animData2.RootAnimationClips["Take 001"];

                    rigidRootPlayer = new RootAnimationPlayer();
                    rigidRootPlayer.Completed += new EventHandler(skinnedPlayer_Completed);
                    rigidRootPlayer.StartClip(rigidRootClip, 1, TimeSpan.Zero);
                }
                if (animData2.ModelAnimationClips != null && animData2.ModelAnimationClips.ContainsKey("Take 001"))
                {
                    rigidClip = animData2.ModelAnimationClips["Take 001"];

                    

                    rigidPlayer = new RigidAnimationPlayer(rigidModel.Bones.Count);
                    rigidPlayer.StartClip(rigidClip, 1, TimeSpan.Zero);


                    rigidPlayer.CurrentTimeValue = rigidClip.Duration;
                    rigidPlayer.CurrentTimeValue = TimeSpan.Zero;
                  
                    rigidPlayer.Completed += new EventHandler(skinnedPlayer_Completed);
                    
                }
            }

            
            skinned = BaseGame.Content.Load<Effect>("Content\\Shaders\\skinned");
            skinned.CurrentTechnique = skinned.Techniques[0];

            Update(null);
        }

        void skinnedPlayer_Completed(object sender, EventArgs e)
        {
            playingSkinned = false;
        }

        public void Update(GameTime time)
        {
            float ts = TimeScale * 6.0f * 0.0167f;
            if (ts > 0)
            {
                GameTime gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(ts), TimeSpan.Zero, TimeSpan.FromSeconds(ts));

                if (!playingSkinned)
                {
                    skinnedPlayer.StartClip(skinnedClip, 1, TimeSpan.Zero);

                    rigidPlayer.StartClip(rigidClip, 1, TimeSpan.Zero);

                    playingSkinned = true;

                    if (skinnedRootPlayer != null && skinnedRootClip != null)
                    {
                        skinnedRootPlayer.StartClip(skinnedRootClip, 1, TimeSpan.Zero);
                    }
                    if (rigidRootPlayer != null && rigidRootClip != null)
                    {
                        rigidRootPlayer.StartClip(rigidRootClip, 1, TimeSpan.Zero);
                    }
                }

                // If we are playing skinned animations, update the players
                if (playingSkinned)
                {
                    if (skinnedRootPlayer != null)
                        skinnedRootPlayer.Update(gameTime);
                    if (rigidRootPlayer != null)
                        rigidRootPlayer.Update(gameTime);

                    skinnedPlayer.Update(gameTime);
                    rigidPlayer.Update(gameTime);
                }
            }

        }


        #region Render car
        /// <summary>
        /// Render car model with this seperate method because we
        /// render it in 2 steps, first the solid stuff, then the alpha glass.
        /// We also rotate the wheels around :)
        /// </summary>
        /// <param name="carNumber">Car type number (0, 1 or 2) for the car
        /// texture</param>
        /// <param name="carColor">Car color we are currently using.</param>
        /// <param name="shadowCarMode">In the shadow car mode we render
        /// everything (including wheels and glass) with a special ShadowCar
        /// shader, that is very transparent. Used for the shadow car when
        /// playing that shows how we drove the last time.</param>
        /// <param name="renderMatrix">Render matrix for the car</param>
        public void RenderCar(bool renderCyclist, Color carColor,
            Matrix renderMatrix)
        {
            //renderMatrix = Matrix.CreateTranslation(renderMatrix.Translation + Vector3.UnitZ * 5);
            // Multiply object matrix by render matrix, result is used multiple
            // times here.
            renderMatrix = objectMatrix * renderMatrix;


            Matrix[] boneTransforms = null;
            if (skinnedPlayer != null)
                boneTransforms = skinnedPlayer.GetSkinTransforms();

            Matrix rootTransform = Matrix.Identity;
            if (skinnedRootPlayer != null)
                rootTransform = skinnedRootPlayer.GetCurrentTransform();


            if (renderCyclist)
            {

                foreach (ModelMesh mesh in skinnedModel.Meshes)
                {
                    //mesh.MeshParts[0].
                    //skinned.Parameters["diffuseTexture"].SetValue();


                    foreach (Effect effect in mesh.Effects)
                    {

                        //effect.Parameters["diffuseTexture"].SetValue(effect);
                        effect.Parameters["viewProj"].SetValue(BaseGame.ViewProjectionMatrix);
                        effect.Parameters["world"].SetValue(rootTransform * renderMatrix);
                        effect.Parameters["Bones"].SetValue(boneTransforms);
                        effect.CommitChanges();

                        //effect.EnableDefaultLighting();
                        //effect.Projection = BaseGame.ProjectionMatrix;
                        //effect.View = BaseGame.ViewMatrix;
                        //if (boneTransforms != null)
                        //effect.SetBoneTransforms(boneTransforms);
                        //effect.World = rootTransform * renderMatrix;
                        //effect.SpecularColor = Vector3.Zero;
                    }

                    mesh.Draw();
                }
            }
           
            boneTransforms = null;
            if (rigidPlayer != null)
                boneTransforms = rigidPlayer.GetBoneTransforms();

            rootTransform = Matrix.Identity;
            if (rigidRootPlayer != null)
                rootTransform = rigidRootPlayer.GetCurrentTransform();

            foreach (ModelMesh mesh in rigidModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = Color.Lerp(Color.White, carColor, 0.5f).ToVector3();
                    effect.Projection = BaseGame.ProjectionMatrix;
                    effect.View = BaseGame.ViewMatrix;
                    if (boneTransforms != null)
                        effect.World = boneTransforms[mesh.ParentBone.Index] * rootTransform * renderMatrix;
                    else
                        effect.World = rootTransform * renderMatrix;
                    effect.SpecularPower = 64;
                }

                mesh.Draw();
            }

        }
        #endregion


        #region Generate shadow
        /// <summary>
        /// Generate shadow for this model in the generate shadow pass
        /// of our shadow mapping shader. All objects rendered here will
        /// cast shadows to our scene (if they are in range of the light)
        /// </summary>
        /// <param name="renderMatrix">Render matrix</param>
        public void GenerateShadow(Matrix renderMatrix)
        {
            // Find out how far the object is away from the shadow,
            // we can ignore it if it is outside of the shadow generation range.
            // Everything smaller than 0.5 meter can be ignored. 
            float maxDistance =
                //nice, but not good for shadow mapping, have to use fixed value!
                5 / 2.5f + 1.015f * ShaderEffect.shadowMapping.ShadowDistance;
            if (Vector3.DistanceSquared(
                ShaderEffect.shadowMapping.ShadowLightPos, renderMatrix.Translation) >
                maxDistance * maxDistance)
                // Don't render, too far away!
                return;





            Matrix[] boneTransforms = null;
            if (rigidPlayer != null)
                boneTransforms = rigidPlayer.GetBoneTransforms();

            Matrix rootTransform = Matrix.Identity;
            if (rigidRootPlayer != null)
                rootTransform = rigidRootPlayer.GetCurrentTransform();


            // Multiply object matrix by render matrix.
            renderMatrix = objectMatrix * renderMatrix;

            for (int meshNum = 0; meshNum < rigidModel.Meshes.Count; meshNum++)
            {
                ModelMesh mesh = rigidModel.Meshes[meshNum];

                Matrix worldTrans;
                if (boneTransforms != null)
                    worldTrans = boneTransforms[mesh.ParentBone.Index] * rootTransform * renderMatrix;
                else
                    worldTrans = rootTransform * renderMatrix;

                // Use the ShadowMapShader helper method to set the world matrices
                ShaderEffect.shadowMapping.UpdateGenerateShadowWorldMatrix(
                    worldTrans);


                for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
                {
                    ModelMeshPart part = mesh.MeshParts[partNum];
                    // Render just the vertices, do not use the shaders of our model.
                    // This is the same code as ModelMeshPart.Draw() uses, but
                    // this method is internal and can't be used by us :(
                    BaseGame.Device.VertexDeclaration = part.VertexDeclaration;
                    BaseGame.Device.Vertices[0].SetSource(
                        mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                    BaseGame.Device.Indices = mesh.IndexBuffer;
                    BaseGame.Device.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        part.BaseVertex, 0,
                        part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
        }
        #endregion

        #region Use shadow
        /// <summary>
        /// Use shadow for our scene. We render all objects that should receive
        /// shadows here. Called from the ShadowMappingShader.UseShadow method.
        /// </summary>
        /// <param name="renderMatrix">Render matrix</param>
        public void UseShadow(Matrix renderMatrix)
        {
            // Find out how far the object is away from the shadow,
            // we can ignore it if it is outside of the shadow generation range.
            // Everything smaller than 0.25 meter can be ignored.
            // Note: For receiving we usually use more objects than for generating
            // shadows.
            float maxDistance =
                //nice, but not good for shadow mapping, have to use fixed value!
                1.015f * ShaderEffect.shadowMapping.ShadowDistance;
            if (Vector3.DistanceSquared(
                ShaderEffect.shadowMapping.ShadowLightPos, renderMatrix.Translation) >
                maxDistance * maxDistance)
                // Don't render, too far away!
                return;




            Matrix[] boneTransforms = null;
            if (rigidPlayer != null)
                boneTransforms = rigidPlayer.GetBoneTransforms();

            Matrix rootTransform = Matrix.Identity;
            if (rigidRootPlayer != null)
                rootTransform = rigidRootPlayer.GetCurrentTransform();




            // Multiply object matrix by render matrix.
            renderMatrix = objectMatrix * renderMatrix;

            for (int meshNum = 0; meshNum < rigidModel.Meshes.Count; meshNum++)
            {
                ModelMesh mesh = rigidModel.Meshes[meshNum];

                Matrix worldTrans;
                if (boneTransforms != null)
                    worldTrans = boneTransforms[mesh.ParentBone.Index] * rootTransform * renderMatrix;
                else
                    worldTrans = rootTransform * renderMatrix;

                
                // Use the ShadowMapShader helper method to set the world matrices
                ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(
                    worldTrans);

                for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
                {
                    ModelMeshPart part = mesh.MeshParts[partNum];
                    // Render just the vertices, do not use the shaders of our model.
                    // This is the same code as ModelMeshPart.Draw() uses, but
                    // this method is internal and can't be used by us :(
                    BaseGame.Device.VertexDeclaration = part.VertexDeclaration;
                    BaseGame.Device.Vertices[0].SetSource(
                        mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                    BaseGame.Device.Indices = mesh.IndexBuffer;
                    BaseGame.Device.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        part.BaseVertex, 0,
                        part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
        }
        #endregion
    }
}
