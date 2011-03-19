using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using VirtualBicycle.MathLib;
using VirtualBicycle.Logic;

namespace VirtualBicycle
{
    public enum ViewMode
    {
        FirstPerson,
        ThirdPersion
    }

    public class ViewManager : ICamera
    {
        FpsCamera fpsCamera;
        ChaseCamera trdCamera;

        ViewMode mode;
        float viewSwitchLerp;

        bool isSwitching;

        Frustum frustum;
        Bicycle currentBike;

        #region Field

        public ViewManager(float aspectRatio)
        {
            fpsCamera = new FpsCamera(aspectRatio);
            trdCamera = new ChaseCamera(aspectRatio);

            trdCamera.Mass = 120;
            trdCamera.Damping = 600;


            frustum = new Frustum();

            Mode = ViewMode.FirstPerson;
        }
        #endregion

        public bool IsFreeze
        {
            get;
            set;
        }

        public Bicycle CurrentBicycle
        {
            get { return currentBike; }
            set
            {
                currentBike = value;
                trdCamera.Position = value.Position - value.Front * 10 + Vector3.UnitY * 5;
            }
        }

        public ViewMode Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    isSwitching = true;
                }
            }
        }

        #region ICamera 成员

        public Frustum Frustum
        {
            get
            {
                return frustum;
            }
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                return frustum.Projection;
            }
        }

        public Matrix ViewMatrix
        {
            get
            {
                return frustum.View;
            }
        }

        public void Update(float dt)
        {
            if (isSwitching) 
            {
                if (mode == ViewMode.FirstPerson)
                {
                    viewSwitchLerp -= 0.5f * dt;
                    if (viewSwitchLerp < 0)
                    {
                        viewSwitchLerp = 0;
                        isSwitching = false;
                    }
                }
                else
                {
                    viewSwitchLerp += 0.5f * dt;
                    if (viewSwitchLerp > 1)
                    {
                        viewSwitchLerp = 1;
                        isSwitching = false;
                    }
                }
            }

            if (isSwitching)
            {
                if (mode == ViewMode.FirstPerson)
                {
                    frustum.Projection = Matrix.Lerp(fpsCamera.ProjectionMatrix, trdCamera.ProjectionMatrix, viewSwitchLerp);
                    frustum.View = Matrix.Lerp(fpsCamera.ViewMatrix, trdCamera.ViewMatrix, viewSwitchLerp);
                }
                else
                {
                    frustum.Projection = Matrix.Lerp(fpsCamera.ProjectionMatrix, trdCamera.ProjectionMatrix, viewSwitchLerp);
                    frustum.View = Matrix.Lerp(fpsCamera.ViewMatrix, trdCamera.ViewMatrix, viewSwitchLerp);
                }
            }
            else
            {
                if (mode == ViewMode.FirstPerson)
                {
                    frustum.Projection = fpsCamera.ProjectionMatrix;
                    frustum.View = fpsCamera.ViewMatrix;
                }
                else
                {
                    frustum.Projection = trdCamera.ProjectionMatrix;
                    frustum.View = trdCamera.ViewMatrix;
                }
            }

            frustum.Update();

            if (CurrentBicycle != null && !IsFreeze)
            {
                Vector3 up = CurrentBicycle.Up;
                Vector3 front = CurrentBicycle.Front;

                Quaternion q = Quaternion.RotationAxis(front, -CurrentBicycle.DrawLeanAngle);
                Vector4 tmp = Vector3.Transform(up, q);
                up = new Vector3(tmp.X, tmp.Y, tmp.Z);


                fpsCamera.Position = CurrentBicycle.Position + up * 0.3f - front * 0.2f;
                fpsCamera.Orientation = CurrentBicycle.RigidBody.Orientation;

                trdCamera.ChasePosition = CurrentBicycle.Position;
                trdCamera.ChaseDirection = CurrentBicycle.Front;
            }

            fpsCamera.Update(dt);
            trdCamera.Update(dt);
        }

        public Vector3 Position
        {
            get
            {
                if (isSwitching)
                {
                    return Vector3.Lerp(fpsCamera.Position, trdCamera.Position, viewSwitchLerp);
                }
                else
                {
                    return (Mode == ViewMode.FirstPerson) ?
                        fpsCamera.Position : trdCamera.Position;
                }
            }
        }

        public Quaternion Orientation
        {
            get
            {
                if (isSwitching)
                {
                    return Quaternion.Slerp(fpsCamera.Orientation, trdCamera.Orientation, viewSwitchLerp);
                }
                else
                {
                    return (Mode == ViewMode.FirstPerson) ?
                        fpsCamera.Orientation : trdCamera.Orientation;
                }
            }
        }

        public float FarPlane
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.FarPlane : trdCamera.FarPlane;
            }
        }

        public void SetFarPlane(float value)
        {
            fpsCamera.FarPlane = value;
            trdCamera.FarPlane = value;
        }

        public float NearPlane
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.NearPlane : trdCamera.NearPlane;
            }
        }

        public void SetNearPlane(float value)
        {
            fpsCamera.NearPlane = value;
            trdCamera.NearPlane = value;
        }

        public float AspectRatio
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.AspectRatio : trdCamera.AspectRatio;
            }
        }

        public void SetAspectRatio(float value)
        {
            fpsCamera.AspectRatio = value;
            trdCamera.AspectRatio = value;
        }

        public float FieldOfView
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.FieldOfView : trdCamera.FieldOfView;
            }
        }

        public void SetFieldOfView(float value)
        {
            fpsCamera.FieldOfView = value;
            trdCamera.FieldOfView = value;
        }

        public Vector3 Front
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.Front : trdCamera.Front;
            }
        }

        public Vector3 Top
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.Top : trdCamera.Top;
            }
        }

        public Vector3 Right
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.Right : trdCamera.Right;
            }
        }

        public Surface RenderTarget
        {
            get
            {
                return (Mode == ViewMode.FirstPerson) ?
                    fpsCamera.RenderTarget : trdCamera.RenderTarget;
            }
        }

        public void SetRenderTarget(Surface surf)
        {
            fpsCamera.RenderTarget = surf;
            trdCamera.RenderTarget = surf;
        }

        #endregion
    }
}
