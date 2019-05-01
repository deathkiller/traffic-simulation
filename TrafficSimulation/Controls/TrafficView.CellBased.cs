using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TrafficSimulation.Simulations.CellBased;

namespace TrafficSimulation.Controls
{
    partial class TrafficView
    {
        private void OnPaintCellBasedSim(PaintEventArgs e, CellBasedSim simulation)
        {
            const int OverdrawSize = 60;

            if (simulation == null || simulation.Current.Cells == null) {
                return;
            }

            Matrix oldTransform = e.Graphics.Transform;
            e.Graphics.ScaleTransform(scaleFactor, scaleFactor);

            Size clientSize = ClientSize;

            ref SimulationData current = ref simulation.Current;

            for (int i = 0; i < current.Cells.Length; i++) {
                ref Cell c = ref current.Cells[i];
                ref CellToCar cToCar = ref current.CellsToCar[i];
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

                if (cToCar.CarIndex != Cell.None) {
                    ref Car car = ref current.Cars[cToCar.CarIndex];
                    ref CarUi carUi = ref current.CarsUi[cToCar.CarIndex];

                    Brush brush;
                    switch (carUi.Color) {
                        default:
                        case 0: brush = Brushes.Green; break;
                        case 1: brush = Brushes.DarkCyan; break;
                        case 2: brush = Brushes.Orange; break;
                        case 3: brush = Brushes.Pink; break;
                        case 4: brush = Brushes.Cyan; break;
                        case 5: brush = Brushes.Maroon; break;
                        case 6: brush = Brushes.Olive; break;
                        case 7: brush = Brushes.PowderBlue; break;
                        case 8: brush = Brushes.DarkOrange; break;
                        case 9: brush = Brushes.DarkGreen; break;
                    }

                    e.Graphics.FillRectangle(brush, rect);

                    if (scaleFactor > 1.3f) {
                        e.Graphics.DrawRectangle(Pens.Black, rect);
                    }
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

        private void DrawLane(PaintEventArgs e, CellBasedSim simulation, CellUi cUi, int target, bool isSelected)
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

        private void OnMouseDoubleClickCellBased(MouseEventArgs e, CellBasedSim simulation)
        {
            int mx = (int)((e.X / scaleFactor - x) / dist);
            int my = (int)((e.Y / scaleFactor - y) / dist);

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
    }
}