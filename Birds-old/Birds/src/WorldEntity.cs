using Birds.src.bounding_areas;
using Birds.src.controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Birds.src
{
    public class WorldEntity : Movable, IEntity
    {
        #region Properties
        protected Sprite sprite = null;
        public bool IsVisible { get { return sprite.isVisible; } set { sprite.isVisible = value; } }
        public OrientedBoundingBox OBB;
        public IBoundingArea BoundingArea {get{return OBB;}}
        public BoundingCircle BoundingCircle { get; set; }
        public override Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                sprite.Position = value;
                OBB.Position = value;
                BoundingCircle.Position = value;
            }
        }
        public override float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                sprite.Rotation = value;
                OBB.Rotation = value;
            }
        }
        public Vector2 Origin
        {
            get { return origin; }
            set
            {
                origin = value;
                sprite.Origin = value;
            }
        }
        protected Vector2 origin;
        public float Width { get { return sprite.Width; } }
        public float Height { get { return sprite.Height; } }
        public float Radius { get { return BoundingCircle.Radius; } }
        public bool IsCollidable { get; set; }
        public Vector2 MassCenter { get { return position; } }

        public IDs Team { get;set; }
        public Controller Manager { get;set; }
        public Color Color { get;set;}

        

        public static float REPULSIONDISTANCE = 100;
        #endregion
        public WorldEntity(Texture2D texture, Vector2 position, float rotation = 0, float mass = 1, float thrust = 1, float friction = 0.1f, bool isVisible = true, bool isCollidable = true) : base(position, rotation, mass, thrust, friction)
        {
            this.sprite = new Sprite(texture);
            OBB = new OrientedBoundingBox(position, rotation, sprite.Width, sprite.Height);
            BoundingCircle = new BoundingCircle(position, OBB.Radius);
            Position = position;
            Rotation = rotation;
            IsVisible = isVisible;
            IsCollidable = isCollidable;
            Origin = new Vector2(Width / 2, Height / 2);
        }
        #region Methods
        public void Draw(SpriteBatch sb)
        {
            sprite.Draw(sb);
        }
        public void Collide(IEntity e)
        {
            //collission repulsion
            Vector2 vectorFromOther = e.Position - position;
            float distance = vectorFromOther.Length();
            vectorFromOther.Normalize();
            Vector2 collissionRepulsion = 0.5f * Vector2.Normalize(-vectorFromOther) * (Vector2.Dot(velocity, vectorFromOther) * Mass + Vector2.Dot(e.Velocity, -vectorFromOther) * e.Mass); //make velocity depend on position
            TotalExteriorForce += collissionRepulsion;

            //overlap repulsion
            float distance2 = (position - e.Position).Length();
            if (distance2 < 5)
                distance2 = 5;
            float radius = Radius * (e.Mass + Mass) / 2;
            Vector2 overlapRepulsion = 500f * Vector2.Normalize(position - e.Position) / distance2;
            TotalExteriorForce += overlapRepulsion;
        }
        public void GenerateAxes()
        {
            OBB.GenerateAxes();
        }

        public bool CollidesWith(IEntity e)
        {
            return IsCollidable && e.IsCollidable && BoundingArea.CollidesWith(e.BoundingArea);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);/*
            if (Health < 0)
                Die();*/
        }
        #endregion
    }
}
