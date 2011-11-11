using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace GravityBall
{
    class Player
    {
        private String name;
        private int level;
        private int exp;
        private Double health;
        private Double energy;
        private ArrayList skills;

        private Texture2D myTexture;
        
        public Player()
        {
            name = "Player 1";
            initializePlayer();
        }

        public Player(String aName)
        {
            name = aName;
            initializePlayer();
        }

        private Boolean initializePlayer()
        {
            level = 0;
            exp = 0;
            health = 100;
            energy = 100;
            skills = new ArrayList();

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

    }
}
