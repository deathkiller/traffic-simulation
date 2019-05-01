using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TrafficSimulation.Simulations;
using TrafficSimulation.Simulations.CarFollowing;
using TrafficSimulation.Simulations.CellBased;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Windows
{
    /// <summary>
    /// Represents main window of the application
    /// </summary>
    public partial class MainWindow : Form
    {
        private System.Windows.Forms.Timer runTimer;

        private OpenCLDispatcher dispatcher = new OpenCLDispatcher();
        private SimulationBase simulation;
        private bool isRunning;

        public MainWindow()
        {
            InitializeComponent();

            runTimer = new System.Windows.Forms.Timer();
            runTimer.Interval = 1000;
            runTimer.Tick += OnRunTimerTick;

            IReadOnlyList<OpenCLDevice> devices = dispatcher.Devices;
            for (int i = 0; i < devices.Count; i++) {
                deviceCombobox.Items.Add(devices[i]);
            }

            deviceCombobox.SelectedIndex = 0;
            deviceCombobox.Enabled = true;

            BackColor = Color.FromArgb(0xFA, 0xFA, 0xFA);
            toolStrip.BackColor = Color.White;

            BindSimulation(null);

            RefreshToolbar(false);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (runTimer != null) {
                runTimer.Dispose();
                runTimer = null;
            }

            if (simulation != null) {
                simulation.Dispose();
                simulation = null;
            }
        }

        /// <summary>
        /// Binds simulation as active
        /// </summary>
        /// <param name="sim"></param>
        private void BindSimulation(SimulationBase sim)
        {
            if (sim == null) {
                this.simulation = null;
                return;
            }

            this.simulation = sim;

            trafficView.Simulation = sim;

            runTimer.Enabled = false;
        }

        private void RefreshToolbar(bool busy)
        {
            if (busy) {
                runButton.Enabled = false;
                stepButton.Enabled = false;
                openButton.Enabled = false;
                saveButton.Enabled = false;
                newButton.Enabled = false;
                deviceCombobox.Enabled = false;
            } else {
                bool simulationIsReady = (simulation != null && simulation.IsReady);

                runButton.Enabled = simulationIsReady;

                if (runTimer.Enabled) {
                    stepButton.Enabled = false;
                    openButton.Enabled = false;
                    saveButton.Enabled = false;
                    newButton.Enabled = false;
                    deviceCombobox.Enabled = false;

                    runButton.Text = "Stop";
                } else {
                    stepButton.Enabled = simulationIsReady;
                    openButton.Enabled = true;
                    saveButton.Enabled = simulationIsReady;
                    newButton.Enabled = true;
                    deviceCombobox.Enabled = true;

                    runButton.Text = "Run";
                }
            }

        }

        /// <summary>
        /// Executes simulation step
        /// </summary>
        /// <param name="deviceIndex">Target device index</param>
        /// <returns>Returns true if successful; false, otherwise</returns>
        private bool DoSimulationStep(int deviceIndex)
        {
            try {
                if (deviceIndex == 0) {
                    simulation.DoStepReference();
                } else {
                    //simulation.DoStepOpenCL(dispatcher, dispatcher.Devices[deviceIndex]);
                    simulation.DoBatchOpenCL(dispatcher, dispatcher.Devices[deviceIndex],1);
                }

                return simulation.CheckIntegrity();
            } catch (OpenCLException ex) {
                BeginInvoke((MethodInvoker)delegate {
                    MessageBox.Show(this, ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
                return true;
            } catch (Exception ex) {
                BeginInvoke((MethodInvoker)delegate {
                    MessageBox.Show(this, ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
                return true;
            }
        }

        private void OnStepButtonClick(object sender, EventArgs e)
        {
            if (simulation == null) {
                return;
            }

            Cursor = Cursors.AppStarting;
            trafficView.Cursor = Cursors.AppStarting;

            RefreshToolbar(true);

            int deviceIndex = deviceCombobox.SelectedIndex;

            ThreadPool.UnsafeQueueUserWorkItem(_ => {
                if (isRunning) {
                    return;
                }

                isRunning = true;
                bool isCorrect = DoSimulationStep(deviceIndex);
                isRunning = false;

                BeginInvoke((MethodInvoker)delegate {
                    trafficView.Invalidate();

                    RefreshToolbar(false);

                    Cursor = Cursors.Default;
                    trafficView.Cursor = Cursors.Default;

                    if (!isCorrect) {
                        MessageBox.Show(this, "Simulation state was corrupted.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                });
            }, null);
        }

        private void OnRunButtonClick(object sender, EventArgs e)
        {
            if (simulation == null) {
                return;
            }

            runTimer.Interval = 1000;
            runTimer.Enabled ^= true;

            RefreshToolbar(false);
        }

        private void OnRunTimerTick(object sender, EventArgs e)
        {
            int deviceIndex = deviceCombobox.SelectedIndex;

            ThreadPool.UnsafeQueueUserWorkItem(_ => {
                if (isRunning) {
                    return;
                }

                isRunning = true;
                bool isCorrect = DoSimulationStep(deviceIndex);
                isRunning = false;

                BeginInvoke((MethodInvoker)delegate {
                    trafficView.Invalidate();

                    if (!isCorrect) {
                        OnRunButtonClick(null, EventArgs.Empty);
                        MessageBox.Show(this, "Simulation state was corrupted.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                });
            }, null);
        }

        private void OnNewButtonClick(object sender, EventArgs e)
        {
            using (CreateNewDialog dialog = new CreateNewDialog()) {
                DialogResult result = dialog.ShowDialog(this);
                if (result != DialogResult.OK) {
                    return;
                }

                SimulationBase sim;
                switch (dialog.SimulationType) {
                    case SimulationType.CellBased:
                        sim = new CellBasedSim();
                        break;

                    case SimulationType.CarFollowing:
                        sim = new CarFollowingSim();
                        break;

                    default:
                        throw new InvalidDataException("Unknown simulation type");
                }

                ProgressDialog progressDialog = new ProgressDialog {
                    Text = "Generating road network",
                    MainInstruction = "Generating road network…",
                    ShowInTaskbar = false,
                    MinimizeBox = false,
                    ProgressMarquee = true
                };

                ThreadPool.UnsafeQueueUserWorkItem(delegate {
                    BeginInvoke((MethodInvoker)delegate {
                        progressDialog.ShowDialog(this);
                    });

                    sim.GenerateNew(dialog.Distance, dialog.JunctionsX, dialog.JunctionsY,
                        dialog.CarCount, dialog.MaxCarCount, dialog.GeneratorProbability);

                    BindSimulation(sim);

                    BeginInvoke((MethodInvoker)delegate {
                        progressDialog.TaskCompleted();
                        RefreshToolbar(false);
                    });

                }, null);
            }
        }

        private void OnOpenButtonClick(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog()) {
                const string ext = ".trs";

                dialog.CheckFileExists = true;
                dialog.Filter = "Simulation State File (*" + ext + ")|*" + ext;
                dialog.FilterIndex = 0;
                dialog.RestoreDirectory = true;
                dialog.Title = "Load state…";

                if (dialog.ShowDialog(this) != DialogResult.OK) {
                    return;
                }

                string filename = dialog.FileName;
                using (FileStream s = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    BindSimulation(SimulationBase.LoadFromStream(s));
                    RefreshToolbar(false);
                }
            }
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            if (simulation == null) {
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog()) {
                const string ext = ".trs";

                dialog.Filter = "Simulation State File (*" + ext + ")|*" + ext;
                dialog.FilterIndex = 0;
                dialog.RestoreDirectory = true;
                dialog.Title = "Save state as…";

                dialog.FileName = "Untitled" + ext;

                if (dialog.ShowDialog(this) != DialogResult.OK) {
                    return;
                }

                string filename = dialog.FileName;

                using (FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    SimulationBase.SaveToStream(s, simulation);
                }
            }
        }

        private void OnDeviceComboboxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Nothing to do...
        }
    }
}