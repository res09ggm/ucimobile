using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        private String name;
        private int level;
        private int exp;
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
            level = 0;
            exp = 0;
            health = 100;
            energy = 100;
            skills = new Ability[4];

            Console.WriteLine("Creating Hero Body");

            _myTexture = GameplayScreen.content.Load<Texture2D>("textures/gravityball");
            float radius = _myTexture.Width / 2;
            _body = BodyFactory.CreateCircle(gameWorld, ConvertUnits.ToSimUnits(radius), 1f, ConvertUnits.ToSimUnits(new Vector2(30f,30f)), this);
            _body.BodyType = BodyType.Dynamic;
            _body.Friction = 5f;
            _body.Mass = 0f;
            _body.Inertia = 0f;
            _body.Restitution = .01f;

            Fixture f = FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(radius), 0f, _body);
            //GameplayScreen._camera.TrackingBody = _body;
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
                //throw new NotImplementedException();
                if (!isJumping)
                {
                    //this._body.ApplyTorque(20);
                    this._body.ApplyForce(new Vector2(20f, 0f));

                }
                else
                    this._body.ApplyForce(new Vector2(10f, 0f));
                //this._body.ApplyLinearImpulse(new Vector2(1f, 0f));
                Console.WriteLine("Player::moveRight()::applyTorque(-10f)");
            }
            this.position = _body.Position;
        }

        internal void moveLeft()
        {
            if (_body.LinearVelocity.X >= -10f) // Change to -7f for normal gameplay
            {
                if (!isJumping)
                {
                    //this._body.ApplyTorque(-20);
                    this._body.ApplyForce(new Vector2(-20f, 0f));
                }
                else
                    this._body.ApplyForce(new Vector2(-10f, 0f));
                Console.WriteLine("Player::moveRight()::applyTorque(10f)");
            }
            this.position = _body.Position;
            
        }

        internal void jump()
        {
            //throw new NotImplementedException();
            
             if (!isJumping)
            {
                this.isJumping = true;
                this._body.ApplyLinearImpulse(new Vector2(0f, -15f)); //change to 10f for normal gameplay
                _body.OnCollision += new OnCollisionEventHandler(this.onCollision);
                //_body.OnCollision += this.OnCollision;
            }    
        }

        bool onCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            
                Vector2 norm;
                FixedArray2<Vector2> pts;
                contact.GetWorldManifold(out norm, out pts);

                // if normal is facing up and vertical velocity is downward we can jump
                if (norm.Y < 0) //&& this._body.LinearVelocity.Y < 0)
                {
                    isJumping = false;
                    this._body.AngularVelocity = 0f;
                    return true;
                }

                // determine a wall collision to drop horizontal velocity 
                // if I can't jump and the normal is not the same direction as the
                // direction player is moving towards then player cant move (IE: wall collision)
                // this.canMove = !(this.direction != norm.X);  // (not working need better solution)

                // allow jumping through a platform from the bottom


                return true;

        }

        
        public void update()
        {
        }
        
        public void draw(SpriteBatch sb)
        {
            Vector2 wpos = getWorldPosition();
            wpos.X = wpos.X - (_myTexture.Width / 2);
            wpos.Y = wpos.Y - (_myTexture.Height / 2);
            sb.Draw(_myTexture, wpos, Color.Wheat);
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

        internal bool die(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            this.health = 0;
            GameplayScreen.retry();
            return true;
        }
    }
}
