using Birds.src.BVH;
using Birds.src.controllers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Birds.src
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public float Rotation { get; set; }
        public float Zoom { get; set; }
        public float Width { get { return Game1.ScreenWidth / Zoom; } }
        public float Height { get { return Game1.ScreenHeight / Zoom; } }
        public bool AutoAdjustZoom { get; set; }
        public float GameZoom { get { if (Controller != null) return 1.2f*Game1.ScreenHeight / (Game1.ScreenHeight + 1 * Controller.Radius); else return 1; } }
        public Controller Controller { get; set; }
        private float zoomSpeed;

        public Camera([OptionalAttribute] Controller controller, float zoomSpeed = 0.001f)
        {
            if (controller != null)
            {
                Position = controller.Position;
                this.Controller = controller;
            }
            else
                Position = Vector2.Zero;
            PreviousPosition = Position;
            Rotation = 0;
            Zoom = GameZoom;
            this.zoomSpeed = zoomSpeed;
            AutoAdjustZoom = true;
            UpdateTransformMatrix();
        }

        public void Update()
        {
            PreviousPosition = Position;
            if (Controller != null)
                AdjustPosition();
            if (AutoAdjustZoom)
            {
                AdjustZoom(GameZoom);
            }

            Rotation = 0;
            UpdateTransformMatrix();
        }

        private void AdjustPosition()
        {
            PreviousPosition = Position;
            Position = PreviousPosition + 0.001f*(Controller.Position-PreviousPosition);
        }

        private void AdjustZoom(float optimalZoom)
        {
            if (optimalZoom > Zoom)
            {
                if (optimalZoom / Zoom > 1 + zoomSpeed)
                    Zoom *= 1 + zoomSpeed;
                else
                    Zoom = optimalZoom;
            }
            else if (optimalZoom < Zoom)
            {
                if (Zoom / optimalZoom > 1 + zoomSpeed)
                    Zoom /= 1 + zoomSpeed;
                else
                    Zoom = optimalZoom;
            }
        }

        public void UpdateTransformMatrix()
        {
            Matrix position = Matrix.CreateTranslation(
                -Position.X,
                -Position.Y,
                0);
            Matrix rotation = Matrix.CreateRotationZ(Rotation);
            Matrix origin = Matrix.CreateTranslation(
                Game1.ScreenWidth / 2,
                Game1.ScreenHeight / 2,
                0);
            Matrix zoom = Matrix.CreateScale(Zoom, Zoom, 0);
            Transform = position * rotation * zoom * origin;
        }
    }
}
