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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Controllers;
using FarseerPhysics;

namespace GameState
{
    enum AbilityType
    {
        GRAVITY_BALL,
        GRAVITY_SPHERE,
        GRAVITY_HOLE,
        GRAVITY_FLIP
    };

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
        private bool isJumping = false;
        private bool performingGravityFlip = false;
        private bool performingGravitySphere = false;
        private Texture2D _myTexture;
        private Vector2 simPosition;
        private AbilityType currentAbility = 0;
        Body planet;
        CircleShape planetShape;
        GravityController gravity;
        public delegate void undoGravityFlipDelegate();
        public delegate void undoGravitySphereDelegate();
        
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
            _body = BodyFactory.CreateCircle(gameWorld, ConvertUnits.ToSimUnits(radius), 1f, ConvertUnits.ToSimUnits(new Vector2(300f,300f)), this);
            _body.BodyType = BodyType.Dynamic;
            _body.Friction = 5f;
            _body.Mass = 0f;
            _body.Inertia = 0f;
            _body.Restitution = .01f;
            _body.CollisionCategories = Category.Cat4;
            //_body.CollisionGroup = 2;

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
                    this._body.ApplyForce(new Vector2(10f, 0f));
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
                    this._body.ApplyForce(new Vector2(-10f, 0f));
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
                _body.Rotation = 0f;
                this.isJumping = true;
                if (GameplayScreen.getWorld().Gravity.Y > 0)
                    this._body.ApplyLinearImpulse(new Vector2(0f, -10f)); //change to 10f for normal gameplay
                else this._body.ApplyLinearImpulse(new Vector2(0f, 10f));
                _body.OnCollision += new OnCollisionEventHandler(this.onCollision);
            }    
        }

        bool onCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // since this event handler is called everytime player collides with object,
            // we check if it's a wall or dynamic object (Category 2).  Category 1 are waypoints
            // so we want it to return false and allow it into the region.
            if (fixtureA.CollisionCategories == Category.Cat2 
                || fixtureA.CollisionCategories == Category.Cat3
                || fixtureB.CollisionCategories == Category.Cat2
                || fixtureB.CollisionCategories == Category.Cat3)
            {
                Vector2 norm;
                FixedArray2<Vector2> pts;
                contact.GetWorldManifold(out norm, out pts);

                // if normal is facing up and vertical velocity is downward we can jump
                if ((GameplayScreen.getWorld().Gravity.Y >= 0 && norm.Y < 0) || 
                    GameplayScreen.getWorld().Gravity.Y <=0 && norm.Y > 0) // && this._body.LinearVelocity.Y > 0)
                {
                    isJumping = false;
                    this._body.AngularVelocity = 0f;
                    this._body.Rotation = 0f;
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
            if (elapsedTime > 2000)
            {
                elapsedTime -= 1500;
                if (energy < MAX_ENERGY)
                    energy += .5;
                if (health < MAX_HEALTH)
                    health += 1;
            }

            if (_body.LinearVelocity.X >= -1f && _body.LinearVelocity.X <= 1f)
            {
                _body.Rotation = 0f;
            }
        }

        private void showReplayScreen()
        {
            GameplayScreen.getInstance().showRetryScreen();
        }
        
        public void draw(SpriteBatch sb)
        {
            float rotation;
            if (isJumping)
                rotation = 0f;
            else rotation = (float)Math.Sin(_body.Rotation)/4;
                     
            Vector2 origin = new Vector2((_myTexture.Width / 2), (_myTexture.Height / 2));
            Vector2 poss = ConvertUnits.ToDisplayUnits(_body.Position);
            sb.Draw(_myTexture, getWorldPosition(), null, Color.WhiteSmoke, _body.Rotation, origin, 1f, SpriteEffects.None, 0);

            if (performingGravitySphere)
            {
                Console.WriteLine(gravity.IsActiveOn(planet));
            }
        
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
            switch (currentAbility)
            {
                case AbilityType.GRAVITY_BALL:
                    {
                        break;
                    }
                case AbilityType.GRAVITY_SPHERE:
                    {
                        performGravitySphere();
                        break;
                    }
                case AbilityType.GRAVITY_HOLE:
                    {
                        //performGravityHole();
                        break;
                    }
                case AbilityType.GRAVITY_FLIP:
                    {
                        performGravityFlip();
                        break;
                    }
                default: return;
            }
            
        }

        public void performGravityHole(Vector2 transport)
        {
            if (energy > 15)
            {
                this.setSimPosition(transport);
                energy -= 15;
            }
        }

        internal void selectAbility(AbilityType p)
        {
            currentAbility = p;
        }

        public bool die(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureA.Body.Equals(this._body) || fixtureB.Body.Equals(this._body)
                && contact.IsTouching())
            {
                health -= 10;
                if (this.health <= 0)
                {
                    float x = _body.LinearVelocity.X;
                    float y = _body.LinearVelocity.Y * 1.7f;
                    _body.Inertia = 0f;
                    _body.AngularVelocity = 0f;
                    _body.ApplyLinearImpulse(new Vector2(-x, -y));
                    _body.CollisionCategories = Category.None;
                    this.health = 0;
                    GameplayScreen.getInstance().showRetryScreen();
                }
                return false;
            }
            else return false;
        }

        public double getHealth()
        {
            return health;
        }

        public double getEnergy()
        {
            return energy;
        }

        public bool take10Damage(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (contact.IsTouching() && (fixtureA.Body == _body || fixtureB.Body == _body))
            {
                //only take away 10 hitpoints for every 1 second fixtures are touching.
                health -= 5;
                _body.Inertia = 0f;
                _body.Rotation = 0f;
                _body.AngularVelocity = 0f;
                _body.LinearVelocity = new Vector2(0f, _body.LinearVelocity.Y);
                _body.ApplyLinearImpulse(new Vector2(-5f, 0f));

            }
            // Always touch
            return true;
        }

        public void undoGravityFlip()
        {
            performingGravityFlip = false;
            Vector2 oldGravity = GameplayScreen._world.Gravity;
            GameplayScreen._world.Gravity = new Vector2(0f, -oldGravity.Y);
            
        }

        public void performGravityFlip()
        {

             if (!performingGravityFlip && energy > 40)
            {
                performingGravityFlip = true;
                energy -= 40;
                Vector2 oldGravity = GameplayScreen._world.Gravity;
                GameplayScreen._world.Gravity = new Vector2(0f, -oldGravity.Y);
                //Delegate d = new Delegate(this, "undoGravityFlip");
                Timer newTimer = new Timer(new undoGravityFlipDelegate(this.undoGravityFlip), GameplayScreen.getInstance().getCurrentGameTime(), 2000);
                GameplayScreen.getInstance().gameTimers.addTimer(newTimer);
            }
            //else flash screen/make error sound.
        }

        public void undoGravitySphere()
        {
            performingGravitySphere = false;
            planet.Dispose();
            gravity.DisabledOnCategories = Category.All;
            gravity.DisabledOnGroup = 2;
        }

        public void performGravitySphere()
        {
            performingGravitySphere = true;
            //create sphere first
            gravity = new GravityController(5f, 10f, 2f);
            //gravity.DisabledOnGroup = 3;
            //gravity.EnabledOnGroup = 2;
            gravity.DisabledOnCategories = Category.Cat4;
            gravity.EnabledOnCategories = Category.Cat3;
           

            GameplayScreen._world.AddController(gravity);
            
            // fixture for planet (the one that has gravity)
            planet = BodyFactory.CreateBody(GameplayScreen._world);
            planet.Position = GameplayScreen.clickedPosition;
            CircleShape planetShape = new CircleShape(2, 1);
            planet.CreateFixture(planetShape);
            planet.CollidesWith = Category.None;
            planet.IgnoreCCD = false;
            gravity.AddBody(planet);

           

            
            for (int i = 0; i < GameplayScreen._world.BodyList.Count; i++)
            {
                Body p  = GameplayScreen._world.BodyList[i];
                //p.CollisionCategories = Category.Cat2;
                //p.CollisionGroup = 3;
                GameplayScreen._world.BodyList[i] = p;
                //gravity.AddBody(p);
            }

           


            Timer newTimer = new Timer(new undoGravitySphereDelegate(this.undoGravitySphere), GameplayScreen.getInstance().getCurrentGameTime(), 2000);
            GameplayScreen.getInstance().gameTimers.addTimer(newTimer);
        }


        public AbilityType getSelectedAbility()
        {
            return currentAbility;
        }

    }
}
