using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
namespace GameState
{
    

    public interface Ability
    {
        int energyRequired
        {
            get;
            set;
        }

        void upgrade();
        void shoot();
        List<Body> getBodies();
        void undo();
    }

    class GravityBallProjectile : Ability 
    {

        double damagePerformed = 5.0;

        public int energyRequired
        {
            get
            {
                return energyRequired;
            }
            set
            {
                energyRequired = value;
            }
        }

        public void upgrade()
        {
            damagePerformed = damagePerformed * 1.25;
            energyRequired = (int)((double)energyRequired * 1.25);
        }

        public void shoot()
        {
            //logic to shoot gravity ball
            //Player h = GameplayScreen._hero;

        }

        public List<Body> getBodies()
        {
            return null;
        }

        public void undo()
        {
            return;
        }
    }

    public class GravitySphere : Ability
    {
        public int energyRequired
        {
            get
            {
                return energyRequired;
            }
            set
            {
                energyRequired = value;
            }
        }

        public void upgrade()
        {
            energyRequired = (int)((double)energyRequired * 1.25);
        }

        public void shoot()
        {
            throw new NotImplementedException();
        }

        public List<Body> getBodies()
        {
            //TODO: IMPLEMENT
            return null;
        }

        public void undo()
        {
            return;
        }
    }

    public class GravityHole : Ability
    {
        public int energyRequired
        {
            get
            {
                return energyRequired;
            }
            set
            {
                energyRequired = value;
            }
        }

        //public void teleport(Player player, Vector2 pos)
        //{
          //  player.setSimPosition(pos);
        //}

        public void shoot()
        {
            throw new NotImplementedException();
        }

        public void upgrade()
        {
            throw new NotImplementedException();
        }

        public List<Body> getBodies()
        {
            return null;
        }

        public void undo()
        {
            return;
        }
    }
}
