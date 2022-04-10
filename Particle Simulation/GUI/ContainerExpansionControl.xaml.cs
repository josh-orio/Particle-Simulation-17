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

namespace Particle_Simulation
{
    /// <summary>
    /// Interaction logic for ContainerExpansionControl.xaml
    /// </summary>
    public partial class ContainerExpansionControl : Window
    {
        double[] expansionBounds;
        
        public ContainerExpansionControl()
        {
            InitializeComponent();
        }
        #region buttonEvents
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(CoefficientField.Text == "") && !(ExponentField.Text == ""))
            {
                // neither field is allowed to be left empty or program crashes
                // this has the effect of the button not doing anything if either field is blank
                expansionBounds = new double[] { Convert.ToDouble(CoefficientField.Text), Convert.ToDouble(ExponentField.Text) };
                this.DialogResult = true;
                this.Close();
            }
         }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        #endregion

        #region getters
        public double Coefficient()
        {
            return expansionBounds[0];
        }
        public double Exponent()
        {
            return expansionBounds[1];
        }
        #endregion
        private void CoefficientField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CoefficientField.Text != "")
            {
                double text = Convert.ToDouble(CoefficientField.Text);
                if (!(text > 0))
                {
                    CoefficientField.Text = "1";
                }
            }
            
        }
    }
}
