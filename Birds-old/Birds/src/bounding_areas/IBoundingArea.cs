using Microsoft.Xna.Framework;

namespace Birds.src.bounding_areas
{
    public interface IBoundingArea
    {
        public Vector2 Position { get; set; }
        public float Radius { get; }

        bool CollidesWith(IBoundingArea boundingArea);

        //returns a tuple with the maximum X positions and maximum y position of the whole object (these two values does not necessarily belong to the same point)
        public (float, float) maxXY();

        //returns a tuple with the minimum X positions and minimum y position of the whole object (these two values does not necessarily belong to the same point)
        public (float, float) minXY();

    }
}