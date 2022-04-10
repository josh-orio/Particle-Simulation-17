using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Drawing;


namespace Particle_Simulation
{
    public class Particle
    {
        Vector pva;

        string type; // e.g. electron, proton, neutron
        int typenum;
        string group; // e.g. hadron, meson, quark
        double mass; // kilograms
        double charge; // coulombs
        int leptonnum;
        double baryonnum; // has to be double because quarks have fractions of a baryon number

        int id;

        public List<Particle> components = new List<Particle>();
        public Particle(string parType, int typeNumber, string gr, double parMass, double parCharge, int lepton, double baryon, int idNum)
        {
            pva = new Vector();

            type = parType;
            typenum = typeNumber;
            group = gr;
            mass = parMass;
            charge = parCharge;
            leptonnum = lepton;
            baryonnum = baryon;

            id = idNum;
            // components of a particle are added later, using AddComponent()
        }
        #region MechanicalMethods
        public void UpdateVector(double timeElapsed, List<Particle> particles)
        {
            double[] forces = CalculateResultantForcesOnParticle(particles);

            //double[] currentAcceleration = pva.GetAcceleration();

            pva.Accelerate(mass, forces);

            pva.UpdatePosition(timeElapsed);

        }
        public double[] CalculateResultantForcesOnParticle(List<Particle> particles)
        {
            // list of all simulation particles is going to be passed in
            // this subroutine will iterate through each of these particles and work out the forces enacted by each
            // it will store the vector force from each particle in an array, then compose them at the end
            // then it will update the acceleration of the vector class within this particle

            double[] forces = new double[3];
            
            double[] currentGrav = new double[3];
            double[] currentWeak = new double[3];
            double[] currentStrong = new double[3];
            double[] currentEM = new double[3];

            int arrayCounter = 0;

            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].GetPosition() != pva.GetPosition()) // makes sure that a particle is not calculating the force against itself or a particle that shares the same position, as this would cause an error
                {
                    // everything is going to calculated in newtons

                    currentGrav = GravitationalForce(particles[i]);
                    currentWeak = WeakForce(particles[i]);
                    currentStrong = StrongNuclearForce(particles[i]);
                    currentEM = ElectromagneticForce(particles[i]);

                    for (int j = 0; j < 3; j++)
                    {
                        forces[j] += currentGrav[j] + currentWeak[j] + currentStrong[j] + currentEM[j];
                    }

                    arrayCounter++;
                }
            }
            return forces;
        }
        private double[] GravitationalForce(Particle p) /*order of magnitude and polarity works, slight inaccuracy when tested, likely not an issue*/
        {
            // F = (G * m1 * m2) / (r^2)
            // G = gravitational constant = 0.000,000,000,066,741

            double[] force = new double[3];

            double productOfMasses; // m1 * m2
            double rSquared; // r^2
            double ratio; // (m1 * m2) / (r^2)

            for (int i = 0; i < 3; i++)
            {
                rSquared = Math.Pow(pva.GetPosition()[i] - p.GetPosition()[i], 2);

                // if statement prevents divide by zero error
                if (rSquared == 0)
                {
                    force[i] = 0;
                }
                else
                {
                    productOfMasses = mass * p.GetMass();

                    ratio = productOfMasses / rSquared;

                    force[i] = 0.000000000066741 * ratio;

                    // graviation is always attractive, so the force needs to point towards the direction of the particle
                    if (pva.GetPosition()[i] > p.GetPosition()[i])
                    {
                        force[i] = force[i] * -1;
                    }
                }
            }

            return force;
        }
        private double[] WeakForce(Particle p)
        {
            double[] force = new double[3];

            //

            return force;
        }
        private double[] StrongNuclearForce(Particle p) /*https://energywavetheory.com/forces/strong-force/*/
        {
            double[] force = new double[3];

            // nuclear force is only applied to baryons 
            if (baryonnum != 0)
            {
                //need to get an equation for this. 
            }

            // if the particle in question is not subject to the strong nuclear force, {0, 0, 0} will be returned, which makes sense as no force will be caused as a result of SNF.
            
            return force;
        }
        private double[] ElectromagneticForce(Particle p) /*works, not fully tested*/
        {
            // using Coulomb's law.
            // Coulomb's constant = 8,987,551,792.314

            double[] force = new double[3];

            double rSquared;
            double productOfCharge;
            double ratio;

            for (int i = 0; i < 3; i++)
            {
                rSquared = Math.Pow(pva.GetPosition()[i] - p.GetPosition()[i], 2);

                // have to use this if statement to remove divide by zero errors
                if (rSquared == 0)
                {
                    force[i] = 0;
                }
                else
                {
                    // to prevent polarity errors, product of charge has been made absolute and then its polarity will be determined using logic statements
                    productOfCharge = Math.Abs(charge * p.charge);

                    ratio = productOfCharge / rSquared;

                    // force can already be calculated as an absolute, then logic statements will decide whether the value needs to be *= -1
                    force[i] = 8987551792.314 * ratio;

                    // these statements determine whether the interaction taking place is a repulsive or attractive one
                    bool bothPositive = (charge > 1) & (p.charge > 1);
                    bool bothNegative = (charge < 1) & (p.charge < 1);

                    if (bothPositive || bothNegative)
                    {
                        // this branch is used in cases of repulsion

                        // polarity of force must be determined seperately for each component
                        for (int j = 0; j < 3; j++)
                        {
                            // if other particle is to the right, move left
                            if (pva.GetPosition()[j] < p.GetPosition()[j])
                            {
                                force[i] = force[i] * -1;
                            }
                        }
                    }
                    else
                    {
                        // this branch is used in cases of attraction

                        for (int j = 0; j < 3; j++)
                        {
                            // if other particle is to the left, move left
                            if (pva.GetPosition()[j] > p.GetPosition()[j])
                            {
                                force[i] = force[i] * -1;
                            }
                        }

                    }
                }
            }
            return force;
        }
        #endregion

        #region Getters
        // getters for vector of this particle.
        public double[] GetPosition()
        {
            return pva.GetPosition();
        }
        public double[] GetVelocity()
        {
            return pva.GetVelocity();
        }
        public double[] GetAcceleration()
        {
            return pva.GetAcceleration();
        }
        // getters for physical properties.
        public string GetParticleType()
        {
            return type;
        }
        public int GetTypeNumber()
        {
            return typenum;
        }
        public string GetParticleGroup()
        {
            return group;
        }
        public double GetMass()
        {
            return mass;
        }
        public double GetCharge()
        {
            return charge;
        }
        public List<Particle> GetComponents()
        {
            return components;
        }

        public int GetLeptonNumber()
        {
            return leptonnum;
        }
        public double GetBaryonNumber()
        {
            return baryonnum;
        }
        public int GetID()
        {
            return id;
        }

        #endregion

        #region Setters
        // setters for pva.
        // should not be used by mechanics computations
        public void SetPosition(double[] p)
        {
            pva.SetPosition(p);
        }
        public void SetVelocity(double[] v)
        {
            pva.SetVelocity(v);
        }
        public void SetAcceleration(double[] a)
        {
            pva.SetAcceleration(a);
        }
        public void AddComponent(Particle p)
        {
            components.Add(p);
        }
        public void SetParticleType(string t)
        {
            type = t;
        }
        public void SetID(int num)
        {
            id = num;
        }

        #endregion
    }
}
