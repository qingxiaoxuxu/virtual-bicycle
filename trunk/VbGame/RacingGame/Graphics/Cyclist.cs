using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D.Graphics.Animation;

using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        


        Matrix objectMatrix;

        Effect skinned;



        public Cyclist()
        {
            //string test = typeof(AnimationDataReader).AssemblyQualifiedName;

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
                    rigidPlayer.Completed += new EventHandler(skinnedPlayer_Completed);
                    rigidPlayer.StartClip(rigidClip, 1, TimeSpan.Zero);
                }
            }


            skinned = BaseGame.Content.Load<Effect>("Content\\Shaders\\skinned");
            skinned.CurrentTechnique = skinned.Techniques[0];

        }

        void skinnedPlayer_Completed(object sender, EventArgs e)
        {
            playingSkinned = false;
        }

        public void Update(GameTime time)
        {
            GameTime gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.0167), TimeSpan.Zero, TimeSpan.FromSeconds(0.0167));

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
        public void RenderCar(int carNumber, Color carColor,
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


            //// Usually use default color values
            //Color ambientColor = Material.DefaultAmbientColor;
            //Color diffuseColor = Material.DefaultDiffuseColor;


            //EffectTechnique remCurrentTechnique = null;
            //for (int alphaPass = 0; alphaPass < 2; alphaPass++)
            //{
            //    int wheelNumber = 0;

            //    int effectParameterIndex = 0;
            //    int effectTechniqueIndex = 0;

            //    for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
            //    {
            //        ModelMesh mesh = xnaModel.Meshes[meshNum];
            //        bool dontRender = false;

            //        for (int effectNum = 0; effectNum < mesh.Effects.Count; effectNum++)
            //        {
            //            Effect effect = mesh.Effects[effectNum];
            //            if (effectNum == 0)
            //                remCurrentTechnique = effect.CurrentTechnique;

            //            // Find out if this is ReflectionSimpleGlass.fx,
            //            // NormalMapping.fx will also use reflection, but the techniques
            //            // are named in another way (SpecularWithReflection, etc.)
            //            if (cachedIsReflectionSpecularTechnique[effectTechniqueIndex++])
            //            {
            //                if (alphaPass == 0)
            //                {
            //                    dontRender = true;
            //                    effectParameterIndex += 7;
            //                    break;
            //                }

            //                // Skip the first 3 effect parameters
            //                effectParameterIndex += 3;
            //            }
            //            else
            //            {
            //                if (alphaPass == 1)
            //                {
            //                    dontRender = true;
            //                    effectParameterIndex += 7;
            //                    break;
            //                }

            //                // To improve performance we only have to set this when it
            //                // changes! Doesn't do much, because this eats only 10%
            //                // performance, 5-10% are the matrices below and most of the
            //                // performance is just rendering the car with Draw!

            //                // Overwrite car diffuse textures depending on the car number
            //                // we want to render.
            //                cachedEffectParameters[effectParameterIndex++].SetValue(
            //                    RacingGameManager.CarTexture(carNumber).XnaTexture);

            //                // Also set color
            //                cachedEffectParameters[effectParameterIndex++].SetValue(
            //                    ambientColor.ToVector4());
            //                cachedEffectParameters[effectParameterIndex++].SetValue(
            //                    diffuseColor.ToVector4());

            //                // Change shader to
            //                // VertexOutput_SpecularWithReflectionForCar20
            //                // if we changed the color.
            //                if (RacingGameManager.currentCarColor != 0 &&
            //                    effectNum == 0)
            //                {
            //                    effect.CurrentTechnique =
            //                        effect.Techniques["SpecularWithReflectionForCar20"];
            //                    // And set carHueColorChange
            //                    effect.Parameters["carHueColor"].SetValue(
            //                        carColor.ToVector3());
            //                }
            //            }

            //            Matrix meshMatrix = transforms[mesh.ParentBone.Index];

            //            // Only the wheels have 2 mesh parts (gummi and chrome)
            //            if (mesh.MeshParts.Count == 2)
            //            {
            //                wheelNumber++;
            //                meshMatrix =
            //                    Matrix.CreateRotationX(
            //                    // Rotate left 2 wheels forward, the other 2 backward!
            //                    (wheelNumber == 2 || wheelNumber == 4 ? 1 : -1) *
            //                    RacingGameManager.Player.CarWheelPos) *
            //                    meshMatrix;
            //            }

            //            // Assign world matrix
            //            BaseGame.WorldMatrix =
            //                meshMatrix *
            //                renderMatrix;

            //            // Set matrices
            //            cachedEffectParameters[effectParameterIndex++].SetValue(
            //                BaseGame.WorldMatrix);

            //            // These values should only be set once every frame (see above)!
            //            // to improve performance again, also we should access them
            //            // with EffectParameter and not via name!
            //            // But since we got only 1 car it doesn't matter so much ..
            //            cachedEffectParameters[effectParameterIndex++].SetValue(
            //                BaseGame.ViewProjectionMatrix);
            //            cachedEffectParameters[effectParameterIndex++].SetValue(
            //                BaseGame.InverseViewMatrix);
            //            // Set light direction
            //            cachedEffectParameters[effectParameterIndex++].SetValue(
            //                BaseGame.LightDirection);
            //        }

            //        // Render
            //        if (dontRender == false)
            //            mesh.Draw();

            //        // Change shader back to default render technique.
            //        // We only have to do this if the color was changed
            //        if (RacingGameManager.currentCarColor != 0 &&
            //            remCurrentTechnique != null)
            //        {
            //            mesh.Effects[0].CurrentTechnique = remCurrentTechnique;
            //        }
            //    }
            //}
        }
        #endregion

    
    }
}
