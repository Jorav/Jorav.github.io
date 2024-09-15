using Birds.src.bounding_areas;
using Birds.src.controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Birds.src
{
    public interface IEntity
    {
        public Vector2 Position { get; set; }
        public float Radius { get; }
        public IDs Team {get; set;}
        public float Mass {get; set;}
        public Controller Manager {get; set;}
        public Color Color {get;set;}
        public IBoundingArea BoundingArea {get;}
        public bool IsCollidable{get;}
        public Vector2 Velocity { get; }
        public void RotateTo(Vector2 position);
        public void Update(GameTime gameTime);
        public void Draw(SpriteBatch sb);
        public void Accelerate(Vector2 directionalVector, float thrust);
        public void Accelerate(Vector2 directionalVector);
        public bool CollidesWith(IEntity otherEntity);
        public void Collide(IEntity otherEntity);
        public void AccelerateTo(Vector2 position, float thrust);
        //public bool CollidesWith(IEntity c);
    }
}