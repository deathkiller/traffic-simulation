using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Windows
{
    /// <summary>
    /// Represents generic progress dialog
    /// </summary>
    public partial class ProgressDialog : Form
    {
        private Font mainInstructionFont;
        private Timer animationTimer;
        private int animationTime, animationValue;

        private string mainInstruction;
        private bool isCancelled, canClose;

        public string MainInstruction
        {
            get { return mainInstruction; }
            set
            {
                if (mainInstruction == value)
                    return;

                mainInstruction = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Progress, value must be between 0 and 100
        /// </summary>
        public int Progress
        {
            get { return progressBar.Value; }
            set { progressBar.Value = value; }
        }

        public bool ProgressMarquee
        {
            get { return (progressBar.Style == ProgressBarStyle.Marquee); }
            set { progressBar.Style = (value ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks); }
        }

        public bool IsCancelled
        {
            get { return isCancelled; }
        }

        public ProgressDialog()
        {
            InitializeComponent();

            DoubleBuffered = true;

            mainInstructionFont = UI.GetMainInstructionFont();

            animationTimer = new Timer();
            animationTimer.Interval = 100;
            animationTimer.Tick += OnAnimationTimer;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            animationTimer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!canClose) {
                e.Cancel = true;
                CancelTask();
            }

            animationTimer.Stop();

            base.OnClosing(e);
        }

        private void OnAnimationTimer(object sender, EventArgs e)
        {
            animationTime = unchecked(animationTime + 1);
            animationValue = 280 + (int)(Math.Sin(animationTime * 0.06f) * 150);

            Invalidate(false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Size clientSize = ClientSize;

            e.Graphics.FillRectangle(SystemBrushes.Window, 0, 0, clientSize.Width, cancelButton.Top - 10);
            using (Pen pen = new Pen(Color.FromArgb(unchecked((int)0xffdfdfdf)))) {
                e.Graphics.DrawLine(pen, 0, cancelButton.Top - 10, clientSize.Width, cancelButton.Top - 10);
            }

            UI.PaintDirtGradient(e.Graphics, 1, 1, clientSize.Width - 1, cancelButton.Top - 10 - 2, animationValue + 60, animationValue);

            TextRenderer.DrawText(e.Graphics, mainInstruction, mainInstructionFont, new Point(18, 9), Color.Black);
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            CancelTask();
        }

        /// <summary>
        /// Cancel running background task
        /// </summary>
        private void CancelTask()
        {
            isCancelled = true;
            cancelButton.Enabled = false;

            progressBar.Style = ProgressBarStyle.Marquee;
            UI.EnableCloseBox(this, false);
        }

        /// <summary>
        /// Background task is completed, allow dialog to close
        /// </summary>
        public void TaskCompleted()
        {
            canClose = true;

            SetVisibleCore(false);
            Close();
        }
    }
}