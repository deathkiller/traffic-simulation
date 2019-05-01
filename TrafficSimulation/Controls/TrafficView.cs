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
        private const int CellDistance = 6;
        private const int JunctionSize = 10;
        private const float MinScaleFactor = 0.1f;
        private const float MaxScaleFactor = 10;
        private const float MouseWheelRatio = 1.4f;

        private int offsetPxX = 10;
        private int offsetPxY = 10;

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
                if (simulation == value) {
                    return;
                }

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
                if (scaleFactor == value) {
                    return;
                }

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
                    offsetPxX += (int)((e.X - mouseLast.X) / scaleFactor);
                    offsetPxY += (int)((e.Y - mouseLast.Y) / scaleFactor);
                    mouseLast = e.Location;

                    Invalidate();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (simulation != null) {
                float newScale;
                if (e.Delta < 0) {
                    newScale = scaleFactor / (-e.Delta * MouseWheelRatio / 120);
                } else {
                    newScale = scaleFactor * (e.Delta * MouseWheelRatio / 120);
                }

                if (newScale >= MinScaleFactor && newScale <= MaxScaleFactor) {
                    // Lower mouse sensitivity
                    Size clientSize = ClientSize;
                    Size clientSizeHalf = new Size(clientSize.Width / 2, clientSize.Height / 2);
                    Point point = new Point(clientSizeHalf.Width + (e.X - clientSizeHalf.Width) / 3, clientSizeHalf.Height + (e.Y - clientSizeHalf.Height) / 3);
                    ZoomToPoint(point, newScale);
                }
            }

            base.OnMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Color.White);

            if (simulation == null) {
                OnPaintNotLoadedSim(e);
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

        private void OnPaintNotLoadedSim(PaintEventArgs e)
        {
            Size size = ClientSize;
            Rectangle bounds = new Rectangle(10, 6, size.Width - 20, size.Height - 12);

            TextRenderer.DrawText(e.Graphics, "Simulation not loaded yet!\n\nGenerate a new simulation or load a saved state first.", Font, bounds,
                Color.FromArgb(unchecked((int)0xff666666)), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(e.Graphics, App.AssemblyCopyright, fontSmall, bounds,
                Color.FromArgb(unchecked((int)0xff777777)), TextFormatFlags.Left | TextFormatFlags.Bottom);

            TextRenderer.DrawText(e.Graphics, "v" + App.AssemblyVersion, fontSmall, bounds,
                Color.FromArgb(unchecked((int)0xff777777)), TextFormatFlags.Right | TextFormatFlags.Bottom);
        }

        /// <summary>
        /// Zooms to given point
        /// </summary>
        /// <param name="point">Point to zoom to</param>
        /// <param name="factor">Scale factor</param>
        public void ZoomToPoint(Point point, float factor)
        {
            if (factor > scaleFactor) {
                offsetPxX -= point.X;
                offsetPxY -= point.Y;
            } else {
                offsetPxX += point.X;
                offsetPxY += point.Y;
            }

            scaleFactor = factor;

            Invalidate();
        }
    }
}