using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Birds.src.bounding_areas;
using Birds.src.controllers;

namespace Birds.src.bounding_areas
{
    public class AxisAlignedBoundingBox
    {
        private Vector2 UL { get; set; }
        private Vector2 DL { get; set; }
        private Vector2 DR { get; set; }
        private Vector2 UR { get; set; }
        public float Area { get { return Width * Height; } }
        public float Width { get { return (int)(Math.Round((UR - UL).Length())); } }
        public float Height { get { return (int)(Math.Round((UR - DR).Length())); } }
        //public Vector2 AbsolutePosition { get { return (UL + DR) / 2; } }
        private Vector2 position;
        public Vector2 Position
        {
            set
            {
                Vector2 change = value - position;
                UL += change;
                DL += change;
                DR += change;
                UR += change;
                position = value;
            }
            get
            {
                return position;
            }
        }
        public float Radius;

        public AxisAlignedBoundingBox(Vector2 upperLeftCorner, int width, int height)
        {
            SetBox(upperLeftCorner,width,height);
        }

        public AxisAlignedBoundingBox(IBoundingArea boundingArea)
        {
            (float, float) minXY = boundingArea.minXY();
            (float, float) maxXY = boundingArea.maxXY();
            float width = maxXY.Item1-minXY.Item1;
            float height = maxXY.Item2-minXY.Item2;
            SetBox(new Vector2(minXY.Item1, minXY.Item2),width,height);
        }

        public void SetBox(Vector2 upperLeftCorner, float width, float height)
        {
            UL = new Vector2(upperLeftCorner.X, upperLeftCorner.Y);
            DL = new Vector2(upperLeftCorner.X, upperLeftCorner.Y + height);
            DR = new Vector2(upperLeftCorner.X + width, upperLeftCorner.Y + height);
            UR = new Vector2(upperLeftCorner.X + width, upperLeftCorner.Y);
            this.position = upperLeftCorner + new Vector2(width/2, height/2);
            Radius = (float)Math.Sqrt(Math.Pow(Width / 2, 2) + Math.Pow(Height / 2, 2));
        }

        public AxisAlignedBoundingBox CombinedAABB(AxisAlignedBoundingBox AABBOther)
        {
            float xMin = Math.Min(UL.X, AABBOther.UL.X);
            float xMax = Math.Max(UR.X, AABBOther.UR.X);
            float yMin = Math.Min(UL.Y, AABBOther.UL.Y);
            float yMax = Math.Max(DL.Y, AABBOther.DL.Y);
            return BoundingAreaFactory.CreateAABB(new Vector2(xMin, yMin), (int)Math.Round(xMax - xMin), (int)Math.Round(yMax - yMin));
        }

        public static AxisAlignedBoundingBox CombinedAABB(IEntity[] entities){
            IBoundingArea[] OBBs = new IBoundingArea[entities.Length]; //TODO: SORT LIST ON AXIS
            for (int i = 0; i < entities.Length; i++)
                OBBs[i] = entities[i].BoundingArea;
            return CombinedAABB(OBBs);
        }

        public static AxisAlignedBoundingBox CombinedAABB(IBoundingArea[] boundingAreas)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach(IBoundingArea BC in boundingAreas){
                (float, float) maxXY = BC.maxXY();
                (float, float) minXY = BC.minXY();
                if (maxXY.Item1 > maxX)
                    maxX = maxXY.Item1;
                else if (minXY.Item1 < minX)
                    minX = minXY.Item1;
                if (maxXY.Item2 > maxY)
                    maxY = maxXY.Item2;
                else if (minXY.Item2 < minY)
                    minY = minXY.Item2;
            }
            return BoundingAreaFactory.CreateAABB(new Vector2(minX, minY), (int)Math.Round(maxX - minX), (int)Math.Round(maxY - minY));
        }

        public bool CollidesWith(AxisAlignedBoundingBox AABB)
        {
            return UL.X < AABB.UL.X + AABB.Width &&
                    UL.X + Width > AABB.UL.X &&
                    UL.Y < AABB.UL.Y + AABB.Height &&
                    UL.Y + Height > AABB.UL.Y;
        }
    }
}