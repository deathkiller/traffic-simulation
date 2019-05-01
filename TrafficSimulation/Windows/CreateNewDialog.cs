using System.Windows.Forms;
using TrafficSimulation.Simulations;

namespace TrafficSimulation.Windows
{
    /// <summary>
    /// Represents "Create new simulation" dialog
    /// </summary>
    public partial class CreateNewDialog : Form
    {
        public SimulationType SimulationType
        {
            get
            {
                if (cellBasedRadio.Checked) {
                    return SimulationType.CellBased;
                } else if (carFollowingRadio.Checked) {
                    return SimulationType.CarFollowing;
                } else {
                    return SimulationType.Unknown;
                }
            }
        }

        public int Distance
        {
            get
            {
                return (int)distanceBox.Value;
            }
        }

        public int JunctionsX
        {
            get
            {
                return (int)junctionsXBox.Value;
            }
        }

        public int JunctionsY
        {
            get
            {
                return (int)junctionsYBox.Value;
            }
        }

        public int CarCount
        {
            get
            {
                return (int)carsBox.Value;
            }
        }

        public int MaxCarCount
        {
            get
            {
                return (int)maxCarsBox.Value;
            }
        }

        public float GeneratorProbability
        {
            get
            {
                return (float)generatorProbabilityBox.Value;
            }
        }

        public CreateNewDialog()
        {
            InitializeComponent();
        }
    }
}