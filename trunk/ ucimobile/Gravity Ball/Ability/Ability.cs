using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace GameState
{
    interface Ability
    {
        int energyRequired
        {
            get;
            set;
        }

        void upgrade();
        void shoot();
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




    }
}
