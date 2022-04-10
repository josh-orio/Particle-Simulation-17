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
using System.Timers;
using System.IO;
using Microsoft.Win32;

namespace Particle_Simulation
{
    /// <summary>
    /// Interaction logic for SpectateScreen.xaml
    /// </summary>
    public partial class SpectateScreen : Window
    {
        Simulation sim;
        Timer t = new Timer();
        public SpectateScreen(Simulation s, double zr, double vr, double hr)
        {
            InitializeComponent();

            // simulation is passed in
            sim = s;
            // these are passed in so the grid line system can be persistent between windows
            zoomRatio = zr;
            verticalRotation = vr;
            horizontalRotation = hr;

            // colouring the start and stop buttons
            StartButton.Background = Brushes.LightGreen;
            StopButton.Background = Brushes.Tomato;

            // if ElementsBox does not have a background, while inside the graying zone, the mouse focuses on something else and mouse input does not work properly
            // bit of a hacky fix but it'll do
            ElementsBox.Background = Brushes.White;

            // adding an event handler to the timer, which governs the passing of time in the simulation
            t.Elapsed += TimerTick;
        }
        private void TimerTick (object sender, ElapsedEventArgs e)
        {
            // trillionth of a second, bit arbitrary
            // this time increment needs to be large enough that it does not take ridiculously long for events to occur but also small enough that particles do not 'miss' each other
            sim.IncrementTime(1e-15);

            //ReDrawElements();
            // this does some wierd shit atm
        }
        #region drawing
        double zoomRatio;
        private void UpdateZoomRatio()
        {
            // called when the simulation needs to be scaled relative to the window e.g. when starting the program for the first time and no persistence data exists

            // detects the 'boundaries' of the window relative to the grid line system
            double bufferAccountedHeight = SpectateWindow.ActualHeight - 120;
            double bufferAccountedWidth = SpectateWindow.ActualWidth - 120;

            Console.WriteLine("fuck " + sim.GetRadius());


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
            }
            #endregion

            // updates the text blocks in the lower left of the screen which show radius and the passing of time
            RadiusBlock.Text = Convert.ToString("Radius: " + sim.GetRadius() + " m");
            TimeElapsedBlock.Text = Convert.ToString("Time Elapsed: " + sim.GetTimeElapsed() + " s");
        }
        #endregion

        #region buttonEvents
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            t.Start(); // start timer
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            t.Stop(); // stop timer
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // instantiates a new CreateScreen + maintains graphical persistence
            CreateScreen cs = new CreateScreen(sim, zoomRatio, verticalRotation, horizontalRotation);
            cs.Show();
            this.Close();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // opens save file dialog, with file type filter etc
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
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // MainWindow is the first window seen when the program is run
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
            originalMosPos = Mouse.GetPosition(SpectateWindow);
            previousMosPos = Mouse.GetPosition(SpectateWindow); // this variable changes when mouse is moved, the above variable is needed for particle placement purposes

        }
        private void ElementsBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && isDrawing)
            {
                currentMosPos = Mouse.GetPosition(SpectateWindow);
                if (currentMosPos.X < (SpectateWindow.Width / 2))
                {
                    // this branch is run when the mouse is on the left hand side of the screen
                    verticalRotation = -1 * (verticalRotation - ((previousMosPos.Y - currentMosPos.Y) / sensitivityDivisor)) % (2 * Math.PI);
                }
                else if (currentMosPos.X > (SpectateWindow.Width / 2))
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

            currentMosPos = Mouse.GetPosition(SpectateWindow);
        }
        #endregion
        private void SpectateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (zoomRatio == -1) // zoomRatio of -1 indicates a spoof value, so need to calculate a new zoomRatio
            {
                UpdateZoomRatio();
            }

            // and redraw things accordingly
            ReDrawElements();
        }

        private void IntervalBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int conversion = 0;

            // tries to convert the content of IntervalBox to an int
            try
            {
                conversion = Convert.ToInt32(IntervalBox.Text);
            }
            catch
            {

            }

            // if conversion <= 0, it means that conversion has failed or a number <= 0 was entered, so IntervalBox is set to 1000ms interval
            if (conversion <= 0)
            {
                IntervalBox.Text = "1000";
            }
            else
            {
                // this branch means a good value was entered, so the interval of the timer just needs to be modified
                t.Interval = conversion;
            }
        }
    }
}
