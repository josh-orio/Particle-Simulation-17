using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Particle_Simulation
{
    class Vector
    {
        private double[] position;
        private double[] velocity;
        private double[] acceleration;

        public Vector()
        {
            position = new double[3];
            velocity = new double[3];
            acceleration = new double[3];
        }

        #region mechanics
        // methods to control pva of particle.
        public void UpdatePosition(double timeElapsed)
        {
            // need to keep a copy of the old velocity values.
            double[] unupdated = new double[velocity.Length];
            for (int i = 0; i < 3; i++)
            {
                unupdated[i] = velocity[i];
            }

            // current velocity.
            UpdateVelocity(timeElapsed);

            for (int i = 0; i < 3; i++)
            {
                // area under triangle = bh/2
                // base = time
                // height = updated - unupdated
                position[i] += (timeElapsed * (velocity[i] - unupdated[i])) / 2;

                // this doesn't use any of the fancy A-Level maths integration, so mechanices will be slightly innacurate
            }
        }
        private void UpdateVelocity(double timeElapsed)
        {
            for (int i = 0; i < 3; i++)
            {
                // v = u + at
                // or
                // v += at

                velocity[i] += (acceleration[i] * timeElapsed);
            }
        }
        public void Accelerate(double mass, double[] newtons) //takes input as a precalculated resultant force.
        {
            // f = ma
            // a = f / m
            for (int i = 0; i < 3; i++)
            {
                acceleration[i] = newtons[i] / mass;
            }
        }
        #endregion

        #region getters
        // getters for particle
        public double[] GetPosition()
        {
            return position;
        }
        public double[] GetVelocity()
        {
            return velocity;
        }
        public double[] GetAcceleration()
        {
            return acceleration;
        }
        #endregion

        #region setters
        // setters
        // used when a particle has to be moved manually, i.e. by user input, rather than by the mechanics side of the program.
        public void SetPosition(double[] p)
        {
            position = p;
        }
        public void SetVelocity(double[] v)
        {
            velocity = v;
        }
        public void SetAcceleration(double[] a)
        {
            acceleration = a;
        }
        #endregion
    }
}
