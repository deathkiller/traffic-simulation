using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TrafficSimulation.Simulations.CarFollowing;

namespace TrafficSimulation.Controls
{
    partial class TrafficView
    {
        private Pen[] carPens;

        private void OnPaintCarFollowingSim(PaintEventArgs e, CarFollowingSim simulation)
        {
            const int OverdrawSize = 60;

            if (simulation == null || simulation.Current.Cells == null) {
                return;
            }

            InitializePensCarFollowingSim();

            Matrix oldTransform = e.Graphics.Transform;
            e.Graphics.ScaleTransform(scaleFactor, scaleFactor);

            Size clientSize = ClientSize;

            ref SimulationData current = ref simulation.Current;

            for (int i = 0; i < current.Cells.Length; i++) {
                ref Cell c = ref current.Cells[i];
                ref CellUi cUi = ref current.CellsUi[i];

                Rectangle rect = new Rectangle(
                            x + (cUi.X * dist) - (junctionSize / 4), y + (cUi.Y * dist) - (junctionSize / 4),
                            junctionSize / 2 + 1, junctionSize / 2 + 1);

                if (rect.X < -OverdrawSize / scaleFactor ||
                    rect.Y < -OverdrawSize / scaleFactor ||
                    rect.Right > (clientSize.Width + OverdrawSize) / scaleFactor ||
                    rect.Bottom > (clientSize.Height + OverdrawSize) / scaleFactor) {
                    continue;
                }

                bool isSelected = (selectedJunction != Cell.None && c.NearestJunctionIndex == selectedJunction);

                DrawLane(e, simulation, cUi, c.T1, isSelected);
                DrawLane(e, simulation, cUi, c.T2, isSelected);
                DrawLane(e, simulation, cUi, c.T3, isSelected);
                DrawLane(e, simulation, cUi, c.T4, isSelected);
                DrawLane(e, simulation, cUi, c.T5, isSelected);

                int nextIdx = Cell.None;
                if (c.T1 != Cell.None) nextIdx = c.T1;
                else if (c.T2 != Cell.None) nextIdx = c.T2;
                else if(c.T3 != Cell.None) nextIdx = c.T3;
                else if(c.T4 != Cell.None) nextIdx = c.T4;
                else if(c.T5 != Cell.None) nextIdx = c.T5;

                if (nextIdx == Cell.None) {
                    continue;
                }

                ref Cell next = ref current.Cells[nextIdx];
                ref CellUi nextUi = ref current.CellsUi[nextIdx];

                if (c.Length <= 0) {
                    continue;
                }

                for (int j = 0; j < current.CarsPerCell; j++) {
                    int idx = j + i * current.CarsPerCell;
                    int carIdx = current.CellsToCar[idx];
                    if (carIdx == Cell.None) {
                        continue;
                    }

                    ref Car car = ref current.Cars[carIdx];
                    ref CarUi carUi = ref current.CarsUi[carIdx];
                    float from = (car.PositionInCell - car.Size) / c.Length;
                    float to = (car.PositionInCell) / c.Length;

                    e.Graphics.DrawLine(carPens[carUi.Color % carPens.Length],
                        x + (cUi.X + from * (nextUi.X - cUi.X)) * dist,
                        y + (cUi.Y + from * (nextUi.Y - cUi.Y)) * dist,
                        x + (cUi.X + to * (nextUi.X - cUi.X)) * dist,
                        y + (cUi.Y + to * (nextUi.Y - cUi.Y)) * dist);
                }
            }

            for (int i = 0; i < current.Junctions.Length; i++) {
                ref Junction j = ref current.Junctions[i];
                ref Cell c = ref current.Cells[j.CellIndex];
                ref CellUi cUi = ref current.CellsUi[j.CellIndex];

                Rectangle rect = new Rectangle(
                        x + (cUi.X * dist) - (junctionSize / 2), y + (cUi.Y * dist) - (junctionSize / 2),
                        junctionSize, junctionSize);

                if (rect.X < -OverdrawSize / scaleFactor ||
                    rect.Y < -OverdrawSize / scaleFactor ||
                    rect.Right > (clientSize.Width + OverdrawSize) / scaleFactor ||
                    rect.Bottom > (clientSize.Height + OverdrawSize) / scaleFactor) {
                    continue;
                }

                e.Graphics.DrawRectangle(Pens.Black, rect);

            }

            e.Graphics.Transform = oldTransform;

            // Draw text
            string text = simulation.ToString();

            if (selectedJunction != Cell.None) {
                ref Junction junction = ref current.Junctions[selectedJunction];
                text += "\n\nWaiting on selected: " + junction.WaitingCount;
            }

            const int TextX = 6;
            const int TextY = 6;

            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX - 2, TextY), Color.White);
            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX + 2, TextY), Color.White);
            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX, TextY - 2), Color.White);
            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX, TextY + 2), Color.White);

            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX - 1, TextY - 1), Color.White);
            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX + 1, TextY - 1), Color.White);
            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX - 1, TextY + 1), Color.White);
            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX + 1, TextY + 1), Color.White);

            TextRenderer.DrawText(e.Graphics, text, Font, new Point(TextX, TextY), Color.FromArgb(unchecked((int)0xff444444)));
        }

        private void DrawLane(PaintEventArgs e, CarFollowingSim simulation, CellUi cUi, int target, bool isSelected)
        {
            if (target == Cell.None) {
                return;
            }

            Pen pen = (isSelected ? Pens.Red : Pens.Gray);

            ref CellUi cUi2 = ref simulation.Current.CellsUi[target];

            e.Graphics.DrawLine(pen,
                x + (cUi.X * dist), y + (cUi.Y * dist),
                x + (cUi2.X * dist), y + (cUi2.Y * dist));
        }

        private void OnMouseDoubleClickCarFollowing(MouseEventArgs e, CarFollowingSim simulation)
        {
            int mx = (int)((e.X / scaleFactor - x) / dist);
            int my = (int)((e.Y / scaleFactor - y) / dist);

            float nearestDistance = float.MaxValue;
            int nearestIndex = 0;

            ref SimulationData current = ref simulation.Current;
            for (int i = 0; i < current.Junctions.Length; i++)
            {
                ref Junction junction = ref current.Junctions[i];
                ref CellUi c = ref current.CellsUi[junction.CellIndex];

                float dx = (c.X) - mx;
                float dy = (c.Y) - my;
                float distance = dx * dx + dy * dy;
                if (nearestDistance > distance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            selectedJunction = nearestIndex;

            Invalidate();
        }

        private void InitializePensCarFollowingSim()
        {
            const int PenSize = 5;

            if (carPens != null) {
                return;
            }

            carPens = new Pen[10];
            carPens[0] = new Pen(Color.Green, PenSize);
            carPens[1] = new Pen(Color.DarkCyan, PenSize);
            carPens[2] = new Pen(Color.Orange, PenSize);
            carPens[3] = new Pen(Color.Pink, PenSize);
            carPens[4] = new Pen(Color.Cyan, PenSize);
            carPens[5] = new Pen(Color.Maroon, PenSize);
            carPens[6] = new Pen(Color.Olive, PenSize);
            carPens[7] = new Pen(Color.PowderBlue, PenSize);
            carPens[8] = new Pen(Color.DarkOrange, PenSize);
            carPens[9] = new Pen(Color.DarkGreen, PenSize);
        }
    }
}
