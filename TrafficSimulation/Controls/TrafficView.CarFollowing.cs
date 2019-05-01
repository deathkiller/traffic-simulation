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

            if (simulation == null || simulation.Current.Cells == null)
            {
                return;
            }

            InitializePensCarFollowingSim();

            Matrix oldTransform = e.Graphics.Transform;
            e.Graphics.ScaleTransform(scaleFactor, scaleFactor);

            Size clientSize = ClientSize;

            ref SimulationData current = ref simulation.Current;

            PaintCells(e, simulation, OverdrawSize, clientSize, current);

            PaintJunctions(e, OverdrawSize, clientSize, current);

            e.Graphics.Transform = oldTransform;

            DrawStateText(e, simulation, current);
        }

        private void PaintCells(PaintEventArgs e, CarFollowingSim simulation, int OverdrawSize, Size clientSize, SimulationData current)
        {
            for (int i = 0; i < current.Cells.Length; i++) {
                ref Cell c = ref current.Cells[i];
                ref CellUi cUi = ref current.CellsUi[i];

                Rectangle rect = new Rectangle(
                            offsetPxX + (cUi.X * CellDistance) - (JunctionSize / 4), offsetPxY + (cUi.Y * CellDistance) - (JunctionSize / 4),
                            JunctionSize / 2 + 1, JunctionSize / 2 + 1);

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
                else if (c.T3 != Cell.None) nextIdx = c.T3;
                else if (c.T4 != Cell.None) nextIdx = c.T4;
                else if (c.T5 != Cell.None) nextIdx = c.T5;

                if (nextIdx == Cell.None) {
                    continue;
                }

                ref CellUi nextUi = ref current.CellsUi[nextIdx];
                PaintCarsInCell(e, current, i, c, cUi, nextUi);
            }
        }

        private void PaintCarsInCell(PaintEventArgs e, SimulationData current, int i, Cell c, CellUi cUi, CellUi nextUi)
        {
            if (c.Length <= 0f || scaleFactor < 0.2f) {
                return;
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
                    offsetPxX + (cUi.X + from * (nextUi.X - cUi.X)) * CellDistance,
                    offsetPxY + (cUi.Y + from * (nextUi.Y - cUi.Y)) * CellDistance,
                    offsetPxX + (cUi.X + to * (nextUi.X - cUi.X)) * CellDistance,
                    offsetPxY + (cUi.Y + to * (nextUi.Y - cUi.Y)) * CellDistance);
            }
        }

        private void PaintJunctions(PaintEventArgs e, int OverdrawSize, Size clientSize, SimulationData current)
        {
            if (scaleFactor < 0.2f) {
                return;
            }

            for (int i = 0; i < current.Junctions.Length; i++) {
                ref Junction j = ref current.Junctions[i];
                ref Cell c = ref current.Cells[j.CellIndex];
                ref CellUi cUi = ref current.CellsUi[j.CellIndex];

                Rectangle rect = new Rectangle(
                        offsetPxX + (cUi.X * CellDistance) - (JunctionSize / 2), offsetPxY + (cUi.Y * CellDistance) - (JunctionSize / 2),
                        JunctionSize, JunctionSize);

                if (rect.X < -OverdrawSize / scaleFactor ||
                    rect.Y < -OverdrawSize / scaleFactor ||
                    rect.Right > (clientSize.Width + OverdrawSize) / scaleFactor ||
                    rect.Bottom > (clientSize.Height + OverdrawSize) / scaleFactor) {
                    continue;
                }

                e.Graphics.DrawRectangle(Pens.Black, rect);

            }
        }

        private void DrawLane(PaintEventArgs e, CarFollowingSim simulation, CellUi cUi, int target, bool isSelected)
        {
            if (target == Cell.None) {
                return;
            }

            Pen pen = (isSelected ? Pens.Red : Pens.Gray);

            ref CellUi cUi2 = ref simulation.Current.CellsUi[target];

            e.Graphics.DrawLine(pen,
                offsetPxX + (cUi.X * CellDistance), offsetPxY + (cUi.Y * CellDistance),
                offsetPxX + (cUi2.X * CellDistance), offsetPxY + (cUi2.Y * CellDistance));
        }


        private void DrawStateText(PaintEventArgs e, CarFollowingSim simulation, SimulationData current)
        {
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

        private void OnMouseDoubleClickCarFollowing(MouseEventArgs e, CarFollowingSim simulation)
        {
            int mx = (int)((e.X / scaleFactor - offsetPxX) / CellDistance);
            int my = (int)((e.Y / scaleFactor - offsetPxY) / CellDistance);

            float nearestDistance = float.MaxValue;
            int nearestIndex = 0;

            ref SimulationData current = ref simulation.Current;
            for (int i = 0; i < current.Junctions.Length; i++) {
                ref Junction junction = ref current.Junctions[i];
                ref CellUi c = ref current.CellsUi[junction.CellIndex];

                float dx = (c.X) - mx;
                float dy = (c.Y) - my;
                float distance = dx * dx + dy * dy;
                if (nearestDistance > distance) {
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

            carPens = new Pen[] {
                new Pen(Color.Green, PenSize),
                new Pen(Color.DarkCyan, PenSize),
                new Pen(Color.Orange, PenSize),
                new Pen(Color.Pink, PenSize),
                new Pen(Color.Cyan, PenSize),
                new Pen(Color.Maroon, PenSize),
                new Pen(Color.Olive, PenSize),
                new Pen(Color.PowderBlue, PenSize),
                new Pen(Color.DarkOrange, PenSize),
                new Pen(Color.DarkGreen, PenSize)
            };
        }
    }
}
