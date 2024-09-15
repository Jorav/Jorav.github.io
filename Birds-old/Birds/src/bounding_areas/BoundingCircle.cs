

using System;
using Microsoft.Xna.Framework;

namespace Birds.src.bounding_areas
{
    public class BoundingCircle : IBoundingArea
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Area {get{return Radius*Radius;}}

        public BoundingCircle(Vector2 position, float radius) 
        {
            Position = position;
            Radius = radius;
        }

        public BoundingCircle CombinedBoundingCircle(BoundingCircle cOther)
        {
            BoundingCircle largestCircle;
            BoundingCircle smallestCircle;
            if(Radius > cOther.Radius)
            {
                largestCircle = this;
                smallestCircle = cOther;
            }
            else
            {
                largestCircle = cOther;
                smallestCircle = this;
            }
            Vector2 smallestToLargest = largestCircle.Position-smallestCircle.Position;
            float distance = smallestToLargest.Length();
            smallestToLargest.Normalize();

            if(largestCircle.Radius > distance+smallestCircle.Radius) //if smallest circle completely inside large circle
                return BoundingAreaFactory.CreateCircle(largestCircle.Position, largestCircle.Radius);
            else if(distance>=largestCircle.Radius) //if smallest circle center outside large circle
            {
                Vector2 position = (smallestCircle.Position+largestCircle.Position+smallestToLargest*largestCircle.Radius-smallestToLargest*smallestCircle.Radius)/2;
                float radius = (largestCircle.Position-position).Length() + largestCircle.Radius;
                return BoundingAreaFactory.CreateCircle(position, radius);
            }
            else //if smallest circle center inside large circle but not fully
            {
                Vector2 position = (smallestCircle.Position+largestCircle.Position+smallestToLargest*largestCircle.Radius-smallestToLargest*smallestCircle.Radius)/2;
                float radius = (largestCircle.Position-position).Length()+largestCircle.Radius;
                return BoundingAreaFactory.CreateCircle(position, radius);
            }
        }

        //returns a tuple with the maximum X positions and maximum y position of the whole object (these two values does not necessarily belong to the same point)
        public (float, float) maxXY(){
            return (Position.X+Radius, Position.Y+Radius);
        }
        //returns a tuple with the minimum X positions and minimum y position of the whole object (these two values does not necessarily belong to the same point)
        public (float, float) minXY(){
            return (Position.X-Radius, Position.Y-Radius);
        }

        public bool CollidesWith(BoundingCircle c){
            return Math.Sqrt(Math.Pow((double)(Position.X) - (double)(c.Position.X), 2) + Math.Pow((double)(Position.Y) - (double)(c.Position.Y), 2)) <= (Radius + c.Radius);
        }

        public bool CollidesWith(IBoundingArea boundingArea)
        {
            if(boundingArea is BoundingCircle boundingCircle){
                return CollidesWith(boundingCircle);
            }
            else
                throw new Exception("comparing different boundingarea types (that are not currently supported)");
        }
    }
}
