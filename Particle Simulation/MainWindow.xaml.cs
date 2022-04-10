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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace Particle_Simulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> recentSimPaths = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            
        }
        #region buttonEvents
        private void CreateSimButton_Click(object sender, RoutedEventArgs e)
        {
            // a pre-creation window needs to be opened for the user to define the 'simulation bounds' equation
            ContainerExpansionControl ce = new ContainerExpansionControl();
            ce.ShowDialog();

            if (ce.DialogResult == true)
            {
                Simulation main = new Simulation(ce.Coefficient(), ce.Exponent());
                main.ShowCreateScreen();
                this.Close();
            }
        }

        private void LoadPreSetButton_Click(object sender, RoutedEventArgs e)
        {
            // gets directory of parent folder of project and adds the appropriate string to push it to the preset folder
            string direcOfPreset = GoToMainParticleSimulationDirectory() + "\\Preset Simulations";
            //MessageBox.Show(direcOfPreset); debugging line

            OpenFileDialog of = new OpenFileDialog();
            of.Title = "Open Preset Simulation";
            of.InitialDirectory = direcOfPreset;
            of.Filter = "Simulation File |*.sim";
            
            if (of.ShowDialog() == true)
            {
                Simulation main = new Simulation(-1, -1); // these spoof arguments are immediately overwritten by the ReadSaveFile() method
                main.ReadSaveFile(of.FileName);
                main.ShowSpectateScreen();
                this.Close();
            }
        }
        #endregion
        private void RecentSimListBox_Loaded(object sender, RoutedEventArgs e)
        {
            // gets directory of txt file holding recent simulations
            string pathOfRecSims = GoToMainParticleSimulationDirectory() + "\\Recent Simulations.txt";

            // this is going to instantiate the buttons in the list box of recent simulations 
            StreamReader sr = new StreamReader(pathOfRecSims);

            List<string> simDirectories = new List<string>();
            
            while (!sr.EndOfStream)
            {
                simDirectories.Add(sr.ReadLine());
            }

            string fileName;
            string lastAccessDate;
            TextBlock tb;

            for (int i = 0; i < simDirectories.Count; i++)
            {
                // remove file extension from name
                fileName = System.IO.Path.GetFileName(simDirectories[i]);
                fileName = fileName.Remove(fileName.LastIndexOf("."));

                // adds date to button in form dd mm yyyy
                lastAccessDate = Convert.ToString(File.GetLastAccessTime(simDirectories[i]));
                lastAccessDate = lastAccessDate.Remove(lastAccessDate.LastIndexOf(" "));

                // adds the button to menu box
                tb = new TextBlock();
                tb.Text = fileName + " - " + lastAccessDate;
                tb.TextAlignment = TextAlignment.Center;
                tb.Width = 300;
                tb.Background = Brushes.LightGray;
                tb.MouseLeftButtonDown += OpenSimFromMenu;
                
                tb.Name = "_" + Convert.ToString(i);
                recentSimPaths.Add(simDirectories[i]);

                RecentSimListBox.Items.Add(tb);
            }
        }
        private void OpenSimFromMenu(object sender, EventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            int pathIndex = Convert.ToInt32(tb.Name.Replace("_", ""));

            Simulation sim = new Simulation(1,1);

            try
            {
                sim.ReadSaveFile(recentSimPaths[pathIndex]);
            }
            catch
            {

            }

            sim.ShowSpectateScreen();
            this.Close();
        }

        private string GoToMainParticleSimulationDirectory()
        {
            // starts by getting directory of program execution
            string direc = Directory.GetCurrentDirectory();

            // loop removes information after backslahses to travel 3 subfolders back up the file system
            for (int i = 0; i < 3; i++)
            {
                direc = direc.Remove(direc.LastIndexOf("\\"));
            }
            //MessageBox.Show(direc); debugging line

            return direc;
        }
    }
}
