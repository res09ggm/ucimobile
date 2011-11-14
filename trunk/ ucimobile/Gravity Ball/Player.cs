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
        private ArrayList skills;
        public Vector2 position; //should change to private or protected?
        public Body _body;
        private Boolean isJumping = false;

        private Texture2D myTexture;
        
        public Player(ref World gameWorld)
        {
            name = "Player 1";
            initializePlayer(ref gameWorld);
        }

        public Player(ref World gameWorld, String aName)
        {
            name = aName;
            initializePlayer(ref gameWorld);
        }

        private Boolean initializePlayer(ref World gameWorld)
        {
            level = 0;
            exp = 0;
            health = 100;
            energy = 100;
            skills = new ArrayList();

            //Console.WriteLine("Creating Hero Body");

           /* this.setTexture(GameplayScreen.content.Load<Texture2D>("stick"));
            float rad = this.getTexture().Width / 2;
            //Body b = BodyFactory.CreateCircle(GameplayScreen._world, ConvertUnits.ToSimUnits(rad), 1f, ConvertUnits.ToSimUnits(item.Position), this);
            _body = BodyFactory.CreateCircle(GameplayScreen._world, rad, 10f, new Vector2(500, 500));

            _body.BodyType = BodyType.Dynamic;
            
            Vector2 location = new Vector2(275,275);
            Fixture f = FixtureFactory.AttachCircle(rad, 1f, _body);
            _body.CreateFixture(f.Shape);
            position = new Vector2(500,500);
            */
            return true;
        }

        public void addAbility(Ability aAbility)
        {
            skills.Add(aAbility);
        }

        public Texture2D getTexture()
        {
            return myTexture;
        }

        public void setTexture(Texture2D aTexture)
        {
            myTexture = aTexture;
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
            //throw new NotImplementedException();
            if (!isJumping)
                this._body.ApplyTorque(20);
            //this._body.ApplyLinearImpulse(new Vector2(1f, 0f));
            Console.WriteLine("Player::moveRight()::applyTorque(-10f)");
            this.position = _body.Position;
        }

        internal void moveLeft()
        {
            if (!isJumping)
                this._body.ApplyTorque(-20);
            Console.WriteLine("Player::moveRight()::applyTorque(10f)");
            this.position = _body.Position;
            
        }

        internal void jump()
        {
            //throw new NotImplementedException();
            
             if (!isJumping)
            {
                this.isJumping = true;
                this._body.ApplyLinearImpulse(new Vector2(0f, -20f));
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
            

            /*position = ConvertUnits.ToDisplayUnits(_body.Position);
            Console.WriteLine("Player:: _body.position=" + _body.Position.X + "," + _body.Position.Y);
            Console.WriteLine("Player:: position=" + position.X + "," + position.Y);
            */
        }
        

        
        public void draw(SpriteBatch sb)
        {
            position = _body.Position;
            Vector2 wpos = ConvertUnits.ToDisplayUnits(_body.Position);
            wpos.X = wpos.X - (myTexture.Width / 2);
            wpos.Y = wpos.Y - (myTexture.Height / 2);
            sb.Draw(myTexture, wpos, Color.Wheat);
        }
    }
}
