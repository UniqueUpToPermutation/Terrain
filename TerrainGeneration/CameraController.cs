using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;

namespace TerrainGeneration
{
    /// <summary>
    /// A class which handles user IO and controls the camera
    /// </summary>
    public abstract class CameraController : IDisposable
    {
        public Camera Camera { get; set; }
        public abstract void UpdateCamera(FrameEventArgs e);
        public abstract void UpdateCameraParams();
        public abstract void Dispose();
    }


    public class OtherCameraController : CameraController
    {
        private Vector3 up = Vector3.UnitY;

        public float Phi = 0.0f;
        public float Theta = 0.0f;
        public float Radius = 64.0f;
        public const float MinRadius = 1.0f;
        public Vector3 CameraCenter = Vector3.Zero;

        public float PhiVelocity = 0.01f;
        public float ThetaVelocity = 0.01f;
        public float RadiusVelocity = 4.0f;

        protected int lastMouseX = 0;
        protected int lastMouseY = 0;
        protected int lastMouseWheel = 0;

        protected KeyboardDevice keyboard;
        protected MouseDevice mouse;

        protected bool bUpdateCamera = false;

        public OtherCameraController(Camera camera, KeyboardDevice keyboard, MouseDevice mouse)
        {
            Camera = camera;

            this.keyboard = keyboard;
            this.mouse = mouse;

            mouse.ButtonDown += OnMouseDown;
            mouse.ButtonUp += OnMouseUp;
            mouse.WheelChanged += OnMouseWheelChanged;

            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;

            UpdateCameraParams();
        }

        public Vector3 LookDirection
        {
            get
            {
              return new Vector3((float)(Math.Cos(Theta) * Math.Sin(Phi)), (float)Math.Cos(Phi), (float)(Math.Sin(Theta) * Math.Sin(Phi)));
            }
        }

        private void OnMouseWheelChanged(object sender, MouseWheelEventArgs e)
        {
            Radius += RadiusVelocity * (float)e.Delta;
            Radius = Math.Max(Radius, MinRadius);

            UpdateCameraParams();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            bUpdateCamera = false;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            bUpdateCamera = true;

            lastMouseX = e.X;
            lastMouseY = e.Y;
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            

            bUpdateCamera = false;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (keyboard[Key.W] || keyboard[Key.A] || keyboard[Key.S] || keyboard[Key.D])
                bUpdateCamera = true;
            else
                bUpdateCamera = false;
        }

        public override void UpdateCamera(FrameEventArgs e)
        {
            if (bUpdateCamera)
            {
                var SideDirection = Vector3.Cross(LookDirection, up);

                if (keyboard[Key.W])
                    Camera.Position+= LookDirection * RadiusVelocity;
                    
                if (keyboard[Key.S])
                    Camera.Position -= LookDirection * RadiusVelocity;
                if (keyboard[Key.A])
                    Camera.Position -= SideDirection * RadiusVelocity;
                if (keyboard[Key.D])
                    Camera.Position += SideDirection * RadiusVelocity;
                
                var deltaX = (float)(mouse.X - lastMouseX);
                var deltaY = (float)(mouse.Y - lastMouseY);

                lastMouseX = mouse.X;
                lastMouseY = mouse.Y;

                Phi += deltaY * PhiVelocity;
                Theta += deltaX * ThetaVelocity;

                Phi = Clamp(Phi, -(float)Math.PI, (float)Math.PI);
                Theta = Theta % (float)(2.0 * Math.PI);

                UpdateCameraParams();
            }
        }

        public override void UpdateCameraParams()
        {
            Camera.Target = Camera.Position + LookDirection;
            
        }

        public float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public override void Dispose()
        {
            mouse.ButtonDown -= OnMouseDown;
            mouse.ButtonUp -= OnMouseUp;

            keyboard.KeyDown -= OnKeyDown;
            keyboard.KeyUp -= OnKeyUp;
        }
    }

    /// <summary>
    /// A camera controller which allows for rotation around a pivot point using the mouse and mouse wheel
    /// </summary>
    public class RotationCameraController : CameraController
    {
        public float Phi = 0.0f;
        public float Theta = 0.0f;
        public float Radius = 64.0f;
        public const float MinRadius = 1.0f;
        public Vector3 CameraCenter = Vector3.Zero;

        public float PhiVelocity = 0.01f;
        public float ThetaVelocity = 0.01f;
        public float RadiusVelocity = 4.0f;

        protected int lastMouseX = 0;
        protected int lastMouseY = 0;
        protected int lastMouseWheel = 0;

        protected KeyboardDevice keyboard;
        protected MouseDevice mouse;

        protected bool bUpdateCamera = false;

        public RotationCameraController(Camera camera, KeyboardDevice keyboard, MouseDevice mouse)
        {
            Camera = camera;

            this.keyboard = keyboard;
            this.mouse = mouse;

            mouse.ButtonDown += OnMouseDown;
            mouse.ButtonUp += OnMouseUp;
            mouse.WheelChanged += OnMouseWheelChanged;

            UpdateCameraParams();
        }

        private void OnMouseWheelChanged(object sender, MouseWheelEventArgs e)
        {
            Radius += RadiusVelocity * (float)e.Delta;
            Radius = Math.Max(Radius, MinRadius);

            UpdateCameraParams();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            bUpdateCamera = false;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            bUpdateCamera = true;

            lastMouseX = e.X;
            lastMouseY = e.Y;
        }

        public override void UpdateCamera(FrameEventArgs e)
        {
            if (bUpdateCamera)
            {
                var deltaX = (float)(mouse.X - lastMouseX);
                var deltaY = (float)(mouse.Y - lastMouseY);

                lastMouseX = mouse.X;
                lastMouseY = mouse.Y;

                Phi -= deltaY * PhiVelocity;
                Theta += deltaX * ThetaVelocity;

                Phi = Clamp(Phi, 0.1f, (float)Math.PI / 2f);
                Theta = Theta % (float)(2.0 * Math.PI);

                UpdateCameraParams();
            }
        }

        public override void UpdateCameraParams()
        {
            Camera.Target = CameraCenter;
            Camera.Position = Camera.Target + Radius * new Vector3((float)(Math.Cos(Theta) * Math.Sin(Phi)), (float)Math.Cos(Phi), (float)(Math.Sin(Theta) * Math.Sin(Phi)));
        }

        public float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public override void Dispose()
        {
            mouse.ButtonDown -= OnMouseDown;
            mouse.ButtonUp -= OnMouseUp;            
        }
    }
}
