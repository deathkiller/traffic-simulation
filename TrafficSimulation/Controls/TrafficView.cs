using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TrafficSimulation.Simulations;

namespace TrafficSimulation.Controls
{
    /// <summary>
    /// Control that shows visualization of specified traffic simulation
    /// </summary>
    public partial class TrafficView : Control
    {
        private const int dist = 6;
        private const int junctionSize = 10;

        private int x = 10;
        private int y = 10;

        private SimulationBase simulation;
        private bool mouseDown;
        private Point mouseLast;

        private float scaleFactor = 1f;

        private int selectedJunction = -1;

        private Font fontSmall;

        /// <summary>
        /// Gets of sets bound traffic simulation
        /// </summary>
        public SimulationBase Simulation
        {
            get
            {
                return simulation;
            }
            set
            {
                if (simulation == value) return;

                selectedJunction = -1;
                simulation = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Current scale factor
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ScaleFactor
        {
            get
            {
                return scaleFactor;
            }
            set
            {
                if (scaleFactor == value) return;

                scaleFactor = value;
                Invalidate();
            }
        }

        public TrafficView()
        {
            SetStyle(
                ControlStyles.UserPaint | ControlStyles.StandardClick | ControlStyles.Selectable |
                ControlStyles.StandardDoubleClick | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, false);

            fontSmall = new Font(Font.FontFamily, Font.Size * 0.8f, Font.Unit);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();

            if (simulation != null) {
                mouseDown = true;
                mouseLast = e.Location;

                Cursor = Cursors.SizeAll;
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (simulation != null) {
                mouseDown = false;

                Cursor = Parent.Cursor;
                Invalidate();
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (simulation != null && e.Button == MouseButtons.Left) {
                switch (simulation) {
                    case Simulations.CellBased.CellBasedSim cellBased:
                        OnMouseDoubleClickCellBased(e, cellBased);
                        break;

                    case Simulations.CarFollowing.CarFollowingSim carFollowing:
                        OnMouseDoubleClickCarFollowing(e, carFollowing);
                        break;

                }
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (simulation != null) {
                if (mouseDown) {
                    x += (int)((e.X - mouseLast.X) / scaleFactor);
                    y += (int)((e.Y - mouseLast.Y) / scaleFactor);
                    mouseLast = e.Location;

                    Invalidate();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (simulation != null) {
                if (e.Delta > 0) {
                    ScaleFactor *= e.Delta * 1.4f / 120;
                } else {
                    ScaleFactor /= -e.Delta * 1.4f / 120;
                }
            }

            base.OnMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Color.White);

            if (simulation == null) {
                Size size = ClientSize;
                Rectangle bounds = new Rectangle(10, 6, size.Width - 20, size.Height - 12);

                TextRenderer.DrawText(e.Graphics, "Simulation not loaded yet!\n\nGenerate a new simulation or load a saved state first.", Font, bounds,
                    Color.FromArgb(unchecked((int)0xff666666)), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(e.Graphics, App.AssemblyCopyright, fontSmall, bounds,
                    Color.FromArgb(unchecked((int)0xff777777)), TextFormatFlags.Left | TextFormatFlags.Bottom);

                TextRenderer.DrawText(e.Graphics, "v" + App.AssemblyVersion, fontSmall, bounds,
                    Color.FromArgb(unchecked((int)0xff777777)), TextFormatFlags.Right | TextFormatFlags.Bottom);
                return;
            }

            switch (simulation) {
                case Simulations.CellBased.CellBasedSim cellBased:
                    OnPaintCellBasedSim(e, cellBased);
                    break;

                case Simulations.CarFollowing.CarFollowingSim carFollowing:
                    OnPaintCarFollowingSim(e, carFollowing);
                    break;
            }
        }
    }
}