using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    /// <summary>
    /// Camera type enumeration
    /// </summary>
    public enum CameraType
    {
        Perspective,
        Orthographic
    }

    /// <summary>
    /// A camera entity class, used for rendering
    /// </summary>
    public class Camera
    {
        public const float DefaultFieldOfView = (float)Math.PI / 3f;
        public const float DefaultNearPlane = 1.0f;
        public const float DefaultFarPlane = 1000.0f;

        protected float fieldOfView = DefaultFieldOfView;
        protected float nearPlane = DefaultNearPlane;
        protected float farPlane = DefaultFarPlane;

        protected Vector3 target;
        protected Vector3 position;
        protected Vector3 up = Vector3.UnitY;
        protected CameraType type = CameraType.Perspective;

        protected int renderWidth;
        protected int renderHeight;

        public int RenderWidth { get { return renderWidth; } set { renderWidth = value; } }
        public int RenderHeight { get { return renderHeight; } set { renderHeight = value; } }

        public Camera(int renderWidth, int renderHeight)
        {
            this.renderWidth = renderWidth;
            this.renderHeight = renderHeight;
        }

        public float FieldOfView 
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                fieldOfView = value;
            }
        }

        public float NearPlane
        {
            get
            {
                return nearPlane;
            }
            set
            {
                nearPlane = value;
            }
        }

        public float FarPlane
        {
            get
            {
                return farPlane;
            }
            set
            {
                farPlane = value;
            }
        }

        public void CopyTo(Camera other)
        {
            other.fieldOfView = fieldOfView;
            other.nearPlane = nearPlane;
            other.farPlane = farPlane;
            other.target = target;
            other.up = up;
            other.position = position;
            other.type = type;
        }

        /// <summary>
        /// Get the view transformation of this camera
        /// </summary>
        /// <param name="matrix">Matrix output</param>
        public void GetView(out OpenTK.Matrix4 matrix)
        {
            matrix = Matrix4.LookAt(position, target, up);
        }

        /// <summary>
        /// Get the projection transformation of this camera
        /// </summary>
        /// <param name="projection">The projection output</param>
        /// <param name="renderWidth">The render width</param>
        /// <param name="renderHeight">The render height</param>
        public void GetProjection(out OpenTK.Matrix4 projection, int renderWidth, int renderHeight)
        {
            if (type == CameraType.Perspective)
            {
                float aspectRatio = (float)renderWidth / (float)renderHeight;

                Matrix4.CreatePerspectiveFieldOfView(fieldOfView,
                    aspectRatio, nearPlane, farPlane, out projection);
            }
            else
            {
                Matrix4.CreateOrthographic((float)renderWidth, (float)renderHeight, 
                    nearPlane, farPlane, out projection);
            }
        }

        /// <summary>
        /// The object the camera is looking at
        /// </summary>
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// The up direction of the camera
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        /// <summary>
        /// The position of the camera
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// The type of camera this is
        /// </summary>
        public CameraType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
    }
}
