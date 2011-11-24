using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using GameStateManagement;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics;

namespace GameState
{
    class Player
    {
        private double MAX_HEALTH;
        private double MAX_ENERGY;
        private String name;
        private int level;
        private int exp;
        private double elapsedTime = 0f;
        private Double health;
        private Double energy;
        private Ability[] skills;
        private Vector2 position; //should change to private or protected?
        public Body _body;
        private Boolean isJumping = false;
        private Texture2D _myTexture;
        private Vector2 simPosition;
        private int abilityIndex = 0;
        
        public Player(World gameWorld)
        {
            name = "Player 1";
            initializePlayer(gameWorld);
        }

        public Player(World gameWorld, String aName)
        {
            name = aName;
            initializePlayer(gameWorld);
        }

        private Boolean initializePlayer(World gameWorld)
        {
            MAX_ENERGY = 100;
            MAX_HEALTH = 100;
            level = 0;
            exp = 0;
            health = MAX_HEALTH;
            energy = MAX_ENERGY;
            skills = new Ability[4];

            _myTexture = GameplayScreen.content.Load<Texture2D>("textures/gravityball_128");
            float radius = _myTexture.Width / 2;
            _body = BodyFactory.CreateCircle(gameWorld, ConvertUnits.ToSimUnits(radius), 1f, ConvertUnits.ToSimUnits(new Vector2(30f,30f)), this);
            _body.BodyType = BodyType.Dynamic;
            _body.Friction = 5f;
            _body.Mass = 0f;
            _body.Inertia = 0f;
            _body.Restitution = .01f;

            Fixture f = FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(radius), 0f, _body);
            return true;
        }

        public Texture2D getTexture()
        {
            return _myTexture;
        }

        public void setTexture(Texture2D aTexture)
        {
            _myTexture = aTexture;
        }

        public String getName()
        {
            return name;
        }

        public void setName(String aName)
        {
            name = aName;
        }


        internal void moveRight()
        {
            if (_body.LinearVelocity.X <= 10f) // change to 7f for normal gameplay
            {
                if (!isJumping)
                {
                    this._body.ApplyForce(new Vector2(20f, 0f));
                }
                else
                    this._body.ApplyForce(new Vector2(10f, 0f));
            }
            this.position = _body.Position;
        }

        internal void moveLeft()
        {
            if (_body.LinearVelocity.X >= -10f) // Change to -7f for normal gameplay
            {
                if (!isJumping)
                {
                    this._body.ApplyForce(new Vector2(-20f, 0f));
                }
                else
                    this._body.ApplyForce(new Vector2(-10f, 0f));
            }
            this.position = _body.Position;
            
        }

        internal void jump()
        {
             if (!isJumping)
            {
                this.isJumping = true;
                this._body.ApplyLinearImpulse(new Vector2(0f, -15f)); //change to 10f for normal gameplay
                _body.OnCollision += new OnCollisionEventHandler(this.onCollision);
            }    
        }

        bool onCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // since this event handler is called everytime player collides with object,
            // we check if it's a wall or dynamic object (Category 2).  Category 1 are waypoints
            // so we want it to return false and allow it into the region.
            if (fixtureA.CollisionCategories == Category.Cat2 
                || fixtureB.CollisionCategories == Category.Cat2)
            {
                Vector2 norm;
                FixedArray2<Vector2> pts;
                contact.GetWorldManifold(out norm, out pts);

                // if normal is facing up and vertical velocity is downward we can jump
                if (norm.Y < 0) // && this._body.LinearVelocity.Y > 0)
                {
                    isJumping = false;
                    this._body.AngularVelocity = 0f;
                }

                // determine a wall collision to drop horizontal velocity 
                // if I can't jump and the normal is not the same direction as the
                // direction player is moving towards then player cant move (IE: wall collision)
                // this.canMove = !(this.direction != norm.X);  // (not working need better solution)

                // allow jumping through a platform from the bottom

                return true;
            }
            else return false;
        }

        
        public void update(GameTime gameTime)
        {
            double milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            if (health < 0)
                showReplayScreen();
            
            elapsedTime += milliseconds;
            if (elapsedTime > 1000)
            {
                elapsedTime -= 1000;
                if (energy < MAX_ENERGY)
                    energy += 1;
                if (health < MAX_HEALTH)
                    health += 5;
            }
        }

        private void showReplayScreen()
        {
            GameplayScreen.getInstance().showRetryScreen();
        }
        
        public void draw(SpriteBatch sb)
        {
            Vector2 wpos = getWorldPosition();
            wpos.X = wpos.X - (_myTexture.Width / 2);
            wpos.Y = wpos.Y - (_myTexture.Height / 2);
            sb.Draw(_myTexture, wpos, Color.WhiteSmoke);
        }

        public void setWorldPosition(Vector2 worldPos)
        {
            position = worldPos;
            //make sure texture and body/fixture are aligned
            _body.Position = ConvertUnits.ToSimUnits(worldPos);

        }

        public Vector2 getWorldPosition()
        {
            return ConvertUnits.ToDisplayUnits(_body.Position);
        }

        public void setSimPosition(Vector2 simPos)
        {
            position = ConvertUnits.ToDisplayUnits(simPos);
            _body.Position = simPos;
        }

        public Vector2 getSimPosition()
        {
            return _body.Position;
        }

        internal void action()
        {
           // if (skills[abilityIndex]
            //skills[abilityIndex].shoot();
        }

        internal void selectAbility(int p)
        {
            abilityIndex = p;
        }

        public bool die(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (contact.IsTouching() && this.health > 0)
            {
                float x = _body.LinearVelocity.X;
                float y = _body.LinearVelocity.Y * 1.7f;
                _body.Inertia = 0f;
                _body.AngularVelocity = 0f;
                _body.ApplyLinearImpulse(new Vector2(-x, -y));
                _body.CollisionCategories = Category.None;
                this.health = 0;
                GameplayScreen.getInstance().showRetryScreen();
                return false;
            }
            else return false;
        }
    }
}
