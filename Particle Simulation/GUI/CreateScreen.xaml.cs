using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Particle_Simulation
{
    /// <summary>
    /// Interaction logic for CreateScreen.xaml
    /// </summary>
    public partial class CreateScreen : Window
    {
        Simulation sim;
        int IDofSelectedParticle = 0;
        public CreateScreen(Simulation s, double zr, double vr, double hr)
        {
            InitializeComponent();

            // simulation is passed in
            sim = s;
            // these are passed in so the grid line system can be persistent between windows
            zoomRatio = zr;
            verticalRotation = vr;
            horizontalRotation = hr;

            // gives some contrast to the elements on the side of the window
            CreateWindow.Background = Brushes.LightGray;
            CreateBox.Background = Brushes.White;

            // this is again needed for the mouse event handler to work properly inside the graying zone
            ElementsBox.Background = Brushes.White;            
        }
        #region drawing
        double zoomRatio;
        private void UpdateZoomRatio()
        {
            // called when the simulation needs to be scaled relative to the window e.g. when starting the program for the first time and no persistence data exists

            // detects the 'boundaries' of the window relative to the grid line system
            double bufferAccountedHeight = CreateWindow.ActualHeight - 120;
            double bufferAccountedWidth = CreateWindow.ActualWidth - 120;

            // the nearer window edge is taken as the 'limit'
            if (bufferAccountedHeight < bufferAccountedWidth)
            {
                zoomRatio = bufferAccountedHeight / (sim.GetRadius() * 2);
            }
            else if (bufferAccountedWidth < bufferAccountedHeight)
            {
                zoomRatio = bufferAccountedWidth / (sim.GetRadius() * 2);
            }
            else
            {
                zoomRatio = bufferAccountedHeight / (sim.GetRadius() * 2);
            }
        }
        private void ReDrawElements()
        {
            try
            {
                // has to be put in a try clause because this will crash the program at start time if not done
                ElementsBox.Children.Clear();
            }
            catch
            {

            }

            #region grayingZone
            // redraw graying zone, only used if ReDrawElements() is called from the scroll wheel event handler
            Ellipse gz = sim.DrawGrayingZone(zoomRatio);
            ElementsBox.Children.Add(gz);
            #endregion

            #region gridLines
            Ellipse[] gridLines = sim.DrawGrid(verticalRotation, horizontalRotation);
            for (int i = 0; i < gridLines.Length; i++)
            {
                // let simulation class do the 'true' drawing, then scale up in here
                gridLines[i].Width *= zoomRatio;
                gridLines[i].Height *= zoomRatio;

                // throws an error if 0 values are passed
                if (gridLines[i].Width == 0)
                {
                    gridLines[i].Width = 1;
                }
                if (gridLines[i].Height == 0)
                {
                    gridLines[i].Height = 1;
                }

                ElementsBox.Children.Add(gridLines[i]);
            }
            #endregion

            #region particles
            InstantiatedParticlesBox.Items.Clear();

            List<Line> particles = sim.DrawParticles(verticalRotation, horizontalRotation);
            for (int i = 0; i < particles.Count; i++)
            {
                #region visual
                // same as with the grid lines, scaling is applied here rather than simulation class
                particles[i].X1 *= zoomRatio;
                particles[i].X2 *= zoomRatio;
                particles[i].Y1 *= zoomRatio;
                particles[i].Y2 *= zoomRatio;

                // uses stroke width property of visual particle to ensure that the line is a square, this process has to be done here, specifically after zoomRatio is applied
                particles[i].X1 += 0.5 * particles[i].StrokeThickness;
                particles[i].X2 -= 0.5 * particles[i].StrokeThickness;

                ElementsBox.Children.Add(particles[i]);
                #endregion

                #region rightHandListBox
                // adds a button the InstantiatedParticlesBox, this is the base of functionality such as deleting particles and modifying their PVA
                TextBlock buttonBlock = new TextBlock();
                double[] pos = sim.GetParticles()[i].GetPosition();
                buttonBlock.Text = sim.GetParticles()[i].GetParticleType() + " (" + sim.GetParticles()[i].GetID() + ") { " + pos[0] + ", " + pos[1] + ", " + pos[2] + " }";
                buttonBlock.Width = 175;
                buttonBlock.TextAlignment = TextAlignment.Center;
                InstantiatedParticlesBox.Items.Add(buttonBlock);

                buttonBlock.Name = "_" + sim.GetParticles()[i].GetID();
                buttonBlock.MouseLeftButtonDown += ChangeSelectedParticle;
                #endregion
            }
            #endregion
        }
        #endregion

        #region buttonEvents
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // reads data from boxes in the top right of CreateScreen, composes it into arrays and applies it to the selected particle, identified by the variable IDofSelectedParticle
            double[] temp;

            temp = new double[] { Convert.ToDouble(posX.Text), Convert.ToDouble(posY.Text), Convert.ToDouble(posZ.Text) };
            sim.SetParticlePosition(IDofSelectedParticle, temp);

            temp = new double[] { Convert.ToDouble(velX.Text), Convert.ToDouble(velY.Text), Convert.ToDouble(velZ.Text) };
            sim.SetParticleVelocity(IDofSelectedParticle, temp);

            temp = new double[] { Convert.ToDouble(accX.Text), Convert.ToDouble(accY.Text), Convert.ToDouble(accZ.Text) };
            sim.SetParticleAcceleration(IDofSelectedParticle, temp);

            ReDrawElements();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // loops through the simulation particles list and delete the element where IDofSelectedParticle matches the particle ID
            for (int i = 0; i < sim.GetParticles().Count; i++)
            {
                if (sim.GetParticles()[i].GetID() == IDofSelectedParticle)
                {
                    sim.GetParticles().RemoveAt(i);
                }
            }
            ReDrawElements();
        }
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            // opens new spectate screen + persistence data for grid lines
            SpectateScreen ss = new SpectateScreen(sim, zoomRatio, verticalRotation, horizontalRotation);
            ss.Show();
            this.Close();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Simlation";
            sfd.Filter = "Simulation File |*.sim";

            if (sfd.ShowDialog() == true)
            {
                sim.WriteSaveFile(sfd.FileName);
            }
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Save Simulation";
            ofd.Filter = "Simulation File |*.sim";

            if (ofd.ShowDialog() == true)
            {
                sim.ReadSaveFile(ofd.FileName);
                UpdateZoomRatio();
                ReDrawElements();
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            //save current state of simulation then open mainwindow
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }
        #endregion

        #region mouseEvents
        private void ElementsBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // zoomRatio is always changed in increments of 5%, this seems to have a linear effect
            if (e.Delta > 0)
            {
                // scroll wheel moved up, zoom in
                zoomRatio += (0.05 * zoomRatio);
            }
            else if (e.Delta < 0)
            {
                // scroll wheel moved down, zoom out
                zoomRatio -= (0.05 * zoomRatio);
            }

            if (zoomRatio < 0)
            {
                // never let zoomRatio become negative as program crashes
                zoomRatio = 0;
            }
            ReDrawElements();
        }

        double verticalRotation;
        double horizontalRotation;

        bool isDrawing = false; // this variable is made true when the left mouse button is down and indicates that grids should be redrawn whenever mouse movement is detected
        Point originalMosPos; // used by the particle drawing process only. if previousMosPos is used, a particle will always be drawn
        Point previousMosPos; // a copy of the last registered mouse position needs to be recorded to determine how much rotation has occured
        Point currentMosPos;

        double sensitivityDivisor = 100;
        private void ElementsBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            originalMosPos = Mouse.GetPosition(CreateWindow);
            previousMosPos = Mouse.GetPosition(CreateWindow); // this variable changes when mouse is moved, the above variable is needed for particle placement purposes

        }
        private void ElementsBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && isDrawing)
            {
                currentMosPos = Mouse.GetPosition(CreateWindow);
                if (currentMosPos.X < (CreateWindow.Width / 2))
                {
                    // this branch is run when the mouse is on the left hand side of the screen
                    verticalRotation = -1 * (verticalRotation - ((previousMosPos.Y - currentMosPos.Y) / sensitivityDivisor)) % (2 * Math.PI);
                }
                else if (currentMosPos.X > (CreateWindow.Width / 2))
                {
                    // this branch is run on the right hand side of the screen
                    verticalRotation = 1 * (verticalRotation + ((previousMosPos.Y - currentMosPos.Y) / sensitivityDivisor)) % (2 * Math.PI);
                }

                // the handling of horizontal rotation does not changed based on the side of the screen the mouse is on
                horizontalRotation = 1 * (horizontalRotation + ((previousMosPos.X - currentMosPos.X) / sensitivityDivisor)) % (2 * Math.PI);

                // for program stability, vertical and horizontal rotation are kept within 0 and 2*pi
                if (verticalRotation < 0)
                {
                    verticalRotation = (2 * Math.PI) - verticalRotation;
                }

                if (horizontalRotation < 0)
                {
                    horizontalRotation = (2 * Math.PI) - horizontalRotation;
                }

                ReDrawElements();

                previousMosPos = currentMosPos;
            }
        }
        private void ElementsBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            currentMosPos = Mouse.GetPosition(CreateWindow);
        }
        #endregion

        #region particleHandling
        private void ParticleLibraryListBox_Loaded(object sender, RoutedEventArgs e)
        {
            string prevType = "";
            for (int i = 0; i < sim.GetTemplates().Count; i++)
            {
                if (prevType != sim.GetTemplates()[i].GetParticleGroup())
                {
                    TextBlock titleBlock = new TextBlock();
                    titleBlock.Width = 175;

                    titleBlock.Text = sim.GetTemplates()[i].GetParticleGroup();
                    titleBlock.TextAlignment = TextAlignment.Center;

                    titleBlock.Focusable = false;
                    titleBlock.IsEnabled = false;
                    titleBlock.Background = Brushes.LightGray;
                    ParticleLibraryListBox.Items.Add(titleBlock);

                    prevType = sim.GetTemplates()[i].GetParticleGroup();
                }

                // only the regular particle will be added to the list box
                // antiparticles will be accessed by right clicking the same button

                TextBlock buttonBlock = new TextBlock();
                buttonBlock.Width = 175;
                buttonBlock.Text = sim.GetTemplates()[i].GetParticleType();
                buttonBlock.TextAlignment = TextAlignment.Center;

                // add event handlers for left and right click of the buttons. left click spawns the listed particle, right click spawns the antiparticle
                buttonBlock.MouseLeftButtonDown += AddParticleEventHandler;
                buttonBlock.MouseRightButtonDown += AddAntiParticleEventHandler;
                
                // cannot pass custom arguments into an event handler, so the template index of the particle is hidden in the Name attribute of the TextBlock
                // have to add a leading underscore, WPF doesn't allow numbers to start an element name
                buttonBlock.Name = "_" + Convert.ToString(i);

                ParticleLibraryListBox.Items.Add(buttonBlock);
            }
        }
        //added event handlers here because they would look out of place in the simulation class
        private void AddParticleEventHandler(object sender, RoutedEventArgs e)
        {
            // read the name propery from the sending text block, remove the leading underscore and it gives the index of the particle template
            TextBlock tb = sender as TextBlock;
            int ti /*template index*/ = Convert.ToInt32(tb.Name.Replace("_", ""));
            Particle tp /*template particle*/ = sim.GetTemplates()[ti];

            // have to copy each value into a new particle instance, or reference problems arise
            Particle newP = new Particle(tp.GetParticleType(), tp.GetTypeNumber(), tp.GetParticleGroup(), tp.GetMass(), tp.GetCharge(), tp.GetLeptonNumber(), tp.GetBaryonNumber(), sim.NextAvailableID());
            IDofSelectedParticle = newP.GetID();
            sim.AddParticle(newP);

            ReDrawElements();
        }
        private void AddAntiParticleEventHandler(object sender, RoutedEventArgs e)
        {
            // read the name propery from the sending text block, remove the leading underscore and it gives the index of the particle template
            TextBlock tb = sender as TextBlock;
            int ti /*template index*/ = Convert.ToInt32(tb.Name.Replace("_", ""));
            Particle tp /*template particle*/ = sim.GetAntiTemplates()[ti];

            // have to copy each value into a new particle instance, or reference problems arise
            Particle newAntiP = new Particle(tp.GetParticleType(), tp.GetTypeNumber(), tp.GetParticleGroup(), tp.GetMass(), tp.GetCharge(), tp.GetLeptonNumber(), tp.GetBaryonNumber(), sim.NextAvailableID());
            IDofSelectedParticle = newAntiP.GetID();
            sim.AddParticle(newAntiP);

            ReDrawElements();
        }
        #endregion
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
        private void CreateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (zoomRatio == -1) // zoomRatio of -1 indicates a spoof value, so need to calculate a new zoomRatio
            {
                UpdateZoomRatio();
            }

            // and redraw things accordingly
            ReDrawElements();
        }
        private void ChangeSelectedParticle(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            string name = tb.Name.Replace("_", "");
            IDofSelectedParticle = Convert.ToInt32(name);

            ChangePVABoxes();
        }
        private void ChangePVABoxes()
        {
            // gets PVA from the particle whose ID matches IDofSelectedParticle
            // puts each value in the respective box
            double[] temp;

            for (int i = 0; i < sim.GetParticles().Count; i++)
            {
                if (sim.GetParticles()[i].GetID() == IDofSelectedParticle)
                {
                    temp = sim.GetParticles()[i].GetPosition();
                    posX.Text = temp[0].ToString();
                    posY.Text = temp[1].ToString();
                    posZ.Text = temp[2].ToString();

                    temp = sim.GetParticles()[i].GetVelocity();
                    velX.Text = temp[0].ToString();
                    velY.Text = temp[1].ToString();
                    velZ.Text = temp[2].ToString();

                    temp = sim.GetParticles()[i].GetAcceleration();
                    accX.Text = temp[0].ToString();
                    accY.Text = temp[1].ToString();
                    accZ.Text = temp[2].ToString();
                }
            }
        }
    }
}
