using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            //TODO
            throw new NotImplementedException();
        }
    }
}
