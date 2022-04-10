using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace Particle_Simulation
{
    public class Simulation
    {
        double elapsedTime; // seconds
        double radius; // metres
        double expansionCoefficient;
        double expansionExponent;

        List<Particle> particles = new List<Particle>();
        List<Particle> templates = new List<Particle>();
        List<Particle> antiTemplates = new List<Particle>();

        int nextAvailID;
        string projectDirectory;
        public int NextAvailableID()
        {
            nextAvailID++;
            return nextAvailID - 1;
            // this is a wierd piece of code to look at, but nextAvailID++ cannot be run after the return statement
        }
        public Simulation(double eC, double eE)
        {
            expansionCoefficient = eC;
            expansionExponent = eE;

            nextAvailID = 0;

            // when simulation is instantiated, it needs to have a non zero radius
            // with equation governed expansion, at t = 0, radius == 0
            // so elapse time by small value, maybe 6.63e-34, so simulation has non-zero size and particle can be placed
            IncrementTime(6.63e-34);

            #region loadingParticleLibrary

            // loading the particle library is done whenever a simulation is instantiated
            // this is useful in scenarios involving particle placement, particle composition, particle decomposition and save/load file handling

            // set poject directory
            projectDirectory = Directory.GetCurrentDirectory();

            // loop removes information after backslahses to travel 3 subfolders back up the file system, this is where 'particle library.txt' is located
            for (int i = 0; i < 3; i++)
            {
                projectDirectory = projectDirectory.Remove(projectDirectory.LastIndexOf("\\"));
            }

            StreamReader sr = new StreamReader(projectDirectory + "\\Particle Library.txt");
            sr.ReadLine(); // skip first line which is a header, the header is used to show what each column in the library file represents

            string line; // current line
            string[] splitLine; // current line, with white space removed and seperated by comma
            string[] splitComponents; // e.g. splitLine[7] == "(1/1/2)" THEN splitComponents == {"1", "1", "2"}
            List<int[]> arrayOfComponents = new List<int[]>(); // e.g. splitComponents == {"1", "1", "2"} THEN arrayOfComponents[n] == {particle id int, 1, 1, 2} 
            int[] temp;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                splitLine = line.Split(',');
                for (int i = 1; i < splitLine.Length; i++)
                {
                    splitLine[i] = splitLine[i].Replace(" ", "");
                }

                #region addTemplate
                templates.Add(new Particle(
                    splitLine[0],
                    Convert.ToInt32(splitLine[1]),
                    splitLine[2],
                    Convert.ToDouble(splitLine[3]),
                    Convert.ToDouble(splitLine[4]),
                    Convert.ToInt32(splitLine[5]),
                    Convert.ToDouble(splitLine[6]),
                    0 // templates don't need proper identifiers, the id will be reassigned before a particle template is added to the main particle list
                    ));
                #endregion

                #region addAntiParticleTemplate
                antiTemplates.Add(new Particle(
                    "Anti-" + splitLine[0],
                    -1 * Convert.ToInt32(splitLine[1]),
                    splitLine[2],
                    Convert.ToDouble(splitLine[3]),
                    -1 * Convert.ToDouble(splitLine[4]),
                    -1 * Convert.ToInt32(splitLine[5]),
                    -1 * Convert.ToDouble(splitLine[6]),
                    0
                    ));
                #endregion

                splitLine[7] = splitLine[7].Replace("(", "").Replace(")", "");

                if (splitLine[7] == "")
                {
                    // particle is not a composite
                    arrayOfComponents.Add(new int[]{ Convert.ToInt32(splitLine[1]) });
                }
                else
                {
                    // particle is a composite
                    splitComponents = splitLine[7].Split('/');
                    temp = new int[splitComponents.Length + 1];
                    temp[0] = Convert.ToInt32(splitLine[1]);

                    for (int i = 0; i < splitComponents.Length; i++)
                    {
                        temp[i + 1] = Convert.ToInt32(splitComponents[i]);
                    }

                    arrayOfComponents.Add(temp);
                }
            }

            for (int i = 0; i < arrayOfComponents.Count; i++)
            {
                // in order for this to work properly, the components of each particle need to have a lower type number than the composite
                // this is fine since no particle in reality is made of a heavier one, 'particle library.txt' always needs to be kept in ascending mass order though

                int targetComposite = arrayOfComponents[i][0];
                int targetComponentNumber;
                Particle targetComponent;
                Particle targetAntiComponent;
                if (arrayOfComponents.Count > 1)
                {
                    for (int j = 1; j < arrayOfComponents[i].Length; j++)
                    {
                        targetComponentNumber = arrayOfComponents[i][j];

                        // SearchTemplates() function is used to search for a component particle
                        // the returned data is then added as a component

                        targetComponent = SearchTemplates(targetComponentNumber);
                        templates[targetComposite - 1].AddComponent(targetComponent);

                        targetAntiComponent = SearchTemplates(-1 * targetComponentNumber);
                        antiTemplates[targetComposite - 1].AddComponent(targetAntiComponent);

                    }
                }
            }
            #endregion
        }

        #region Mechanics
        public void IncrementTime(double tInc)
        {
            //going to assume that tInc is a sensible value, will test and sanitise as needed

            elapsedTime += tInc;

            //first thing to do is to update the bounds of the container
            radius = expansionCoefficient * (Math.Pow(elapsedTime, expansionExponent));
            Console.WriteLine("radius: " + radius);

            //need to loop through every particle and tell them to each update themselves
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].UpdateVector(tInc, particles);

                //limit radius
                if (GetHypotenuse(particles[i].GetPosition()) > radius)
                {
                    double[] rotation = GetParticleRotation(particles[i].GetPosition());
                    particles[i].SetPosition(GetCoordinate(radius, rotation[0], rotation[1]));
                }
            }
        }
        public double GetTimeElapsed()
        {
            return elapsedTime;
        }
        
        public void AddParticle(Particle p)
        {
            particles.Add(p);
        }
        public void RemoveParticle(int ID)
        {
            // uses the unique ID of the particle that should be removed, loops through all particles and removes if ID matches
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].GetID() == ID)
                {
                    particles.RemoveAt(i);
                }
            }
        }
        public void SplitParticle()
        {

        }
        public void CombineParticles()
        {

        }
        #endregion

        #region Graphical
        public void ShowCreateScreen()
        {
            CreateScreen cs = new CreateScreen(this, -1, 0, 0);
            cs.Show();
        }
        public void ShowSpectateScreen()
        {
            SpectateScreen spec = new SpectateScreen(this, -1, 0, 0);
            spec.Show();
        }

        string[] types = { "Quarks", "Leptons", "Mesons", "Baryons" };
        int[] sizes = { 2, 3, 5, 7 };
        public List<Line> DrawParticles(double verticalRotation, double horizontalRotation)
        {
            List<Line> visualParticles = new List<Line>();
            Line currentParticle;
            string currentType;

            double[] currentPosition;
            double[] particleRotation;
            double[] visualPosition;

            Particle currentP;

            for (int i = 0; i < particles.Count; i++)
            {
                #region mathematics
                currentP = particles[i];
                Console.WriteLine(particles[i].GetParticleType());
                currentPosition = currentP.GetPosition();
                Console.WriteLine("pos: " + currentPosition[0] + " " + currentPosition[1] + " " + currentPosition[2]);
                particleRotation = GetParticleRotation(currentPosition);

                particleRotation[0] += verticalRotation;
                particleRotation[1] += horizontalRotation;

                for (int j = 0; j < particleRotation.Length; j++)
                {
                    particleRotation[j] = particleRotation[j] % (2 * Math.PI);
                }

                visualPosition = GetCoordinate(GetHypotenuse(currentPosition), particleRotation[0], particleRotation[1]);

                // WPF has the y axis flipped in its coordinate system, this accounts for that but does not affect any physics calculations
                visualPosition[1] *= -1;
                #endregion

                #region graphics
                currentParticle = new Line();
                currentParticle.Stroke = Brushes.Black;

                currentParticle.VerticalAlignment = VerticalAlignment.Center;
                currentParticle.HorizontalAlignment = HorizontalAlignment.Center;

                currentType = particles[i].GetParticleGroup();
                for (int j = 0; j < types.Length; j++)
                {
                    if (currentType == types[j])
                    {
                        currentParticle.StrokeThickness = sizes[j];
                    }
                }

                if (visualPosition[0] >= 0)
                {
                    visualPosition[0] *= 2;
                }
                if (visualPosition[1] >= 0)
                {
                    visualPosition[1] *= 2;
                }

                currentParticle.X1 = visualPosition[0];
                currentParticle.X2 = visualPosition[0];
                currentParticle.Y1 = visualPosition[1];
                currentParticle.Y2 = visualPosition[1];

                currentParticle.Visibility = Visibility.Visible;

                visualParticles.Add(currentParticle);
                #endregion
            }
            return visualParticles;
        }
        private double[] GetCoordinate(double radius, double verticalAngle, double horizontalAngle)
        {
            double[] coord = new double[3];

            coord[0] = radius * Math.Cos(verticalAngle) * Math.Cos(horizontalAngle);

            coord[1] = radius * Math.Sin(verticalAngle);

            if (horizontalAngle > Math.PI / 2 && horizontalAngle < 3 * Math.PI / 2)
            {
                coord[1] *= -1;
            }

            return coord;
        }
        private double[] GetParticleRotation(double[] pos)
        {
            #region verticalRotation
            double x = pos[0];
            double y = pos[1];

            double verticalRotation = Math.Atan(y / x);

            if (y < 0 && x >= 0)
            {
                verticalRotation = (2 * Math.PI) + verticalRotation;
            }

            if (x < 0)
            {
                verticalRotation = Math.PI + verticalRotation;
            }

            if (double.IsNaN(verticalRotation))
            {
                verticalRotation = 0;
            }
            #endregion

            #region horizontalRotation
            double z = pos[2];
            double xy = GetHypotenuse(new double[] { x, y });

            double horizontalRotation = Math.Atan(z / xy);

            if (z < 0)
            {
                horizontalRotation = (2 * Math.PI) + Math.Atan(z / xy);
            }

            if (double.IsNaN(horizontalRotation))
            {
                horizontalRotation = 0;
            }
            #endregion

            return new double[] { verticalRotation, horizontalRotation };
        }
        public Ellipse[] DrawGrid(double verticalRotation, double horizontalRotation)
        {
            RotateTransform rot = new RotateTransform((-1 * verticalRotation) * (180 / Math.PI));

            double Xwidth = Math.Abs(Math.Cos(horizontalRotation)) * radius * 2;
            double Xheight = radius * 2;
            Ellipse X = DrawEllipse(Xwidth, Xheight, rot, Brushes.Blue);

            double Ywidth = Math.Abs(Math.Sin(horizontalRotation)) * radius * 2;
            double Yheight = radius * 2;
            Ellipse Y = DrawEllipse(Ywidth, Yheight, rot, Brushes.Red);

            double Zwidth = GetHypotenuse(new double[] { Xwidth, Ywidth });
            double Zheight = Math.Abs(Math.Sin(verticalRotation) * Math.Sin(horizontalRotation) * Math.Cos(horizontalRotation)) * radius * 2;
            Ellipse Z = DrawEllipse(Zwidth, Zheight, rot, Brushes.Green);

            return new Ellipse[] { X, Y, Z };
        }
        
        public Ellipse DrawGrayingZone(double zoomRatio)
        {
            Ellipse grayingZone = new Ellipse();
            // an arbitrarily high value is used to define the size of this ellipse
            // this is because it is supposed to be able to cover the entire part of the screen that is not part of the simulation 
            grayingZone.Width = (radius * zoomRatio * 2) + 10000;
            grayingZone.Height = (radius * zoomRatio * 2) + 10000;

            grayingZone.Stroke = Brushes.Lavender; // nice colour
            grayingZone.StrokeThickness = 5000; // this value being half of the value added to width and height allows it the graying to just touch the edge of the simulation

            grayingZone.VerticalAlignment = VerticalAlignment.Center;
            grayingZone.HorizontalAlignment = HorizontalAlignment.Center;

            return grayingZone;
        }
        private double GetHypotenuse(double[] lengths)
        {
            // takes an array of lengths, squares them, sums them, then roots the result
            double sum = 0;

            for (int i = 0; i < lengths.Length; i++)
            {
                sum += Math.Pow(lengths[i], 2);
            }

            return Math.Sqrt(sum);
        }

        private Ellipse DrawEllipse(double width, double height, RotateTransform rot, Brush b)
        {
            Ellipse e = new Ellipse();
            e.Visibility = Visibility.Visible;
            e.Stroke = b;
            e.Width = width;
            e.Height = height;

            // the RotateTransform argument is calculated by the vertical and horizontal rotation of the simulation gridlines
            e.RenderTransform = rot;
            e.RenderTransformOrigin = new Point(0.5, 0.5); // this is used to set the origin of the Ellipse to the center of the screen, default is top left

            return e;
        }
        #endregion

        #region FileHandling
        public void ReadSaveFile(string path)
        {
            //example of a save file structure

            // 6.63e-34(elapsed time)
            // 1(radius)
            // 0.5(expansion coefficient)
            // 0.25(expansion exponent)
            // 4, { posx / posY / posZ}, { velX / velY / velZ}, { accX / accY / accZ}
            // 17, { posx / posY / posZ}, { velX / velY / velZ}, { accX / accY / accZ}s

            // particle type 4 correlates to an electron
            // particle type 17 correlates to a proton

            // when a particle type is read, a new particle is instantiated from the template list, then the position, velocity and acceleration data is set
            // to indicate an antiparticle in the save file, the type number will be preface by a '-', so a positron would be denoted by -4

            StreamReader sr = new StreamReader(path);

            // the first four lines of any save file will be these simulation properties, in this order, this same standard is upheld in WriteSaveFile()
            elapsedTime = Convert.ToDouble(sr.ReadLine());
            radius = Convert.ToDouble(sr.ReadLine());
            expansionCoefficient = Convert.ToDouble(sr.ReadLine());
            expansionExponent = Convert.ToDouble(sr.ReadLine());

            Console.WriteLine(elapsedTime);
            Console.WriteLine(radius);
            Console.WriteLine(expansionCoefficient);
            Console.WriteLine(expansionExponent);

            string line;
            string[] splitLine;
            int type;
            string[] temp;
            double[] pos;
            double[] vel;
            double[] acc;

            List<double[]> positions = new List<double[]>();
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                line = line.Replace(" ", "");
                splitLine = line.Split(',');

                type = Convert.ToInt32(splitLine[0]);

                for (int i = 1; i < 4; i++)
                {
                    splitLine[i] = splitLine[i].Replace("{", "");
                    splitLine[i] = splitLine[i].Replace("}", "");
                }

                pos = new double[3];
                vel = new double[3];
                acc = new double[3];

                #region parseParticlePosition
                temp = splitLine[1].Split('/');
                for (int i = 0; i < 3; i++)
                {
                    pos[i] = Convert.ToDouble(temp[i]);
                }
                #endregion

                #region parseParticleVelocity
                temp = splitLine[2].Split('/');
                for (int i = 0; i < 3; i++)
                {
                    vel[i] = Convert.ToDouble(temp[i]);
                }
                #endregion

                #region parseParticleAccleration
                temp = splitLine[3].Split('/');
                for (int i = 0; i < 3; i++)
                {
                    acc[i] = Convert.ToDouble(temp[i]);
                }
                #endregion

                if (!splitLine[0].StartsWith("-"))
                {
                    // particle has been read
                    Particle t /*template*/ = templates[type - 1];
                    particles.Add(new Particle(t.GetParticleType(), t.GetTypeNumber(), t.GetParticleGroup(), t.GetMass(), t.GetCharge(), t.GetLeptonNumber(), t.GetBaryonNumber(), this.NextAvailableID()));
                }
                else
                {
                    // antiparticle has been read
                    Particle t /*template*/ = antiTemplates[type - 1];
                    particles.Add(new Particle(t.GetParticleType(), t.GetTypeNumber(), t.GetParticleGroup(), t.GetMass(), t.GetCharge(), t.GetLeptonNumber(), t.GetBaryonNumber(), this.NextAvailableID()));
                }

                particles[particles.Count - 1].SetPosition(pos);
                particles[particles.Count - 1].SetVelocity(vel);
                particles[particles.Count - 1].SetAcceleration(acc);
            }
            sr.Close();
        }
        public void WriteSaveFile(string path)
        {
            StreamWriter sw = new StreamWriter(path);

            // always have to write these four lines first
            sw.WriteLine(Convert.ToString(elapsedTime));
            sw.WriteLine(Convert.ToString(radius));
            sw.WriteLine(Convert.ToString(expansionCoefficient));
            sw.WriteLine(Convert.ToString(expansionExponent));

            string thisLine;
            double[] temp;

            for (int i = 0; i < particles.Count; i++)
            {
                // loops through each of the particles
                // the way save files are constructed, the only data that needs to be written is the particle type number and its PVA, the rest of the data is all stored by the template variables

                // each line is structured as:
                // particle type number, { positionX, positionY, positionZ }, { velocityX, velocityY, velocityZ }, { accelerationX, accelerationY, accelerationZ }

                thisLine = particles[i].GetTypeNumber() + ", ";
                Console.WriteLine(particles[i].GetTypeNumber());

                temp = particles[i].GetPosition();
                thisLine += "{ " + temp[0] + " / " + temp[1] + " / " + temp[2] + " }, ";

                temp = particles[i].GetVelocity();
                thisLine += "{ " + temp[0] + " / " + temp[1] + " / " + temp[2] + " }, ";

                temp = particles[i].GetAcceleration();
                thisLine += "{ " + temp[0] + " / " + temp[1] + " / " + temp[2] + " }";

                sw.WriteLine(thisLine);

            }
            sw.Close();
        }
        public Particle SearchTemplates(double targetParticleNumber)
        {
            Particle targetTemplate = templates[0]; // this assignement is just done to spoof the compiler, in actuality a particle determined by the for loop will always be returned

            for (int i = 0; i < templates.Count; i++)
            {
                if (templates[i].GetTypeNumber() == targetParticleNumber)
                {
                    targetTemplate = templates[i];
                }
                else if (antiTemplates[i].GetTypeNumber() == targetParticleNumber)
                {
                    targetTemplate = antiTemplates[i];
                }
            }

            return targetTemplate;
        }
        #endregion

        #region getters
        public double GetRadius()
        {
            return radius;
        }
        public List<Particle> GetTemplates()
        {
            return templates;
        }
        public List<Particle> GetAntiTemplates()
        {
            return antiTemplates;
        }
        public string GetProjectDirectory()
        {
            return projectDirectory;
        }
        public List<Particle> GetParticles()
        {
            return particles;
        }
        public List<Particle> GetParticleTemplates()
        {
            return templates;
        }
        public List<Particle> GetAntiParticleTemplates()
        {
            return antiTemplates;
        }
        #endregion

        #region setters
        public void SetParticlePosition(int id, double[] pos)
        {
            foreach (Particle p in particles)
            {
                if (p.GetID() == id)
                {
                    p.SetPosition(pos);
                }
            }
        }
        public void SetParticleVelocity(int id, double[] vel)
        {
            foreach (Particle p in particles)
            {
                if (p.GetID() == id)
                {
                    p.SetVelocity(vel);
                }
            }
        }
        public void SetParticleAcceleration(int id, double[] acc)
        {
            foreach (Particle p in particles)
            {
                if (p.GetID() == id)
                {
                    p.SetAcceleration(acc);
                }
            }
        }
        #endregion
    }
}