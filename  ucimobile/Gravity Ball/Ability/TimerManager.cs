using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace GameState
{
    class TimerManager
    {
        private List<Timer> _expiringTimers;
        private World _world;
        private double elapsedTime;

        public TimerManager(ref World physicsEngine)
        {
            this._world = physicsEngine;
            this._expiringTimers = new List<Timer>();
        }

        public void addTimer(Timer newTimer)
        {
            _expiringTimers.Add(newTimer);
        }

        public void removeTimer(Timer expiredTimer)
        {
            //Remove body/objects from physics engine and remove from TimerManager
            if (_expiringTimers.Contains(expiredTimer))
            {
                // undo each Ability's change if applicable
                if (expiredTimer.getBodiesList() != null)
                {
                    foreach (Body b in expiredTimer.getBodiesList())
                    {
                        b.Dispose();
                    }
                }
                // call method undo if applicable
                if (expiredTimer.getUndoMethod() != null)
                {
                    expiredTimer.getUndoMethod().DynamicInvoke();
                }
                _expiringTimers.Remove(expiredTimer);
                expiredTimer.undoChanges();
            }
        }

        public void update(GameTime gameTime)
        {
            this.elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            List<Timer> toRemove = new List<Timer>();
            // add expired timers to a list, then delete later so we don't confuse the iterator
            foreach (Timer t in this._expiringTimers)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds >= t.getDeadline())
                {
                    toRemove.Add(t);
                }
            }
            foreach (Timer t in toRemove)
            {
                removeTimer(t);
            }
        }

    }

    class Timer
    {
        double deadline;
        List<Body> bodies;
        Ability ability;
        Delegate undoMethod;
        private GameTime gameTime;

        public Timer(Ability activeAbility, double expiryTime)
        {
            this.ability = activeAbility;
            bodies = new List<Body>();
            if (activeAbility.getBodies() != null)
            {
                foreach (Body b in activeAbility.getBodies())
                {
                    bodies.Add(b);
                }
            }
            deadline = expiryTime;
        }

        public Timer(Ability activeAbility, GameTime gameTime, double deltaInMilliseconds)
        {
            this.ability = activeAbility;
            bodies = new List<Body>();
            if (activeAbility.getBodies() != null)
            {
                foreach (Body b in activeAbility.getBodies())
                {
                    bodies.Add(b);
                }
            }
            deadline = gameTime.TotalGameTime.TotalMilliseconds + deltaInMilliseconds;
        }

        public Timer(Delegate methodToCall, GameTime gameTime, double delta)
        {
            this.undoMethod = methodToCall;
            this.gameTime = gameTime;
            this.deadline = gameTime.TotalGameTime.TotalMilliseconds + delta;
        }

        public List<Body> getBodiesList()
        {
            return bodies;
        }

        public double getDeadline()
        {
            return deadline;
        }

        public void undoChanges()
        {
            if (this.ability != null)
                this.ability.undo();
        }

        public Delegate getUndoMethod()
        {
            return this.undoMethod;
        }
    }
}
