using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class DefaultRenderer : IRenderer
    {
        private SimulationState _simulationState;

        private readonly int _maxY;

        private readonly int _cellSize = 5;

        public Image RenderedImage { get; private set; }

        public DefaultRenderer(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _maxY = _simulationState.CellMatrix.GetLength(1) - 1;

            RenderedImage = Image.Create(simulationState.CellMatrix.GetLength(0) * _cellSize, simulationState.CellMatrix.GetLength(1) * _cellSize, false, Image.Format.Rgb8);
        }

        public void UpdateImage()
        {
            _simulationState.ForEachCell((ref CellData cellData, int x, int y) => DrawCell(cellData, x, y));
        }

        private void DrawCell(CellData cellData, int x, int y)
        {
            int displayedY = _maxY - y;

            Rect2I cellRect = new(x * _cellSize, displayedY * _cellSize, _cellSize, _cellSize);

            Color baseColor = GetCellBaseColor(x, y);

            RenderedImage.FillRect(cellRect, baseColor);

            if (cellData.IsPlant())
            {
                DrawBorder(x, displayedY, baseColor, _simulationState.GetCellConnections(x, y));
            }
        }

        private void DrawBorder(int x, int displayedY, Color baseColor, Direction connections)
        {
            Color borderColor = baseColor.Darkened(.1f);

            RenderedImage.SetPixel(x * _cellSize, displayedY * _cellSize, borderColor);
            RenderedImage.SetPixel((x + 1) * _cellSize - 1, displayedY * _cellSize, borderColor);
            RenderedImage.SetPixel(x * _cellSize, (displayedY + 1) * _cellSize - 1, borderColor);
            RenderedImage.SetPixel((x + 1) * _cellSize - 1, (displayedY + 1) * _cellSize - 1, borderColor);

            if (!connections.HasFlag(Direction.Left))
            {
                RenderedImage.FillRect(new(x * _cellSize, displayedY * _cellSize, 1, _cellSize), borderColor);
            }

            if (!connections.HasFlag(Direction.Right))
            {
                RenderedImage.FillRect(new((x + 1) * _cellSize - 1, displayedY * _cellSize, 1, _cellSize), borderColor);
            }

            if (!connections.HasFlag(Direction.Down))
            {
                RenderedImage.FillRect(new(x * _cellSize, (displayedY + 1) * _cellSize - 1, _cellSize, 1), borderColor);
            }

            if (!connections.HasFlag(Direction.Up))
            {
                RenderedImage.FillRect(new(x * _cellSize, displayedY * _cellSize, _cellSize, 1), borderColor);
            }
        }

        private Color GetCellBaseColor(int x, int y)
        {
            CellData cell = _simulationState.CellMatrix[x, y];

            Color baseColor;

            switch (cell.Type)
            {
                case CellType.Air:
                    baseColor = new(0xaaccffff);
                    double a = Math.Log2(cell.LightEnergy + 1);
                    double b = Math.Log2(_simulationState.Parameters.DirectLightEnergy);
                    return baseColor.Darkened((float)((b - a) / (2f * b)));
                case CellType.PlantSeed:
                    return new(0x881122ff);
                case CellType.PlantFruit:
                    return new Color(0x881122ff).Lightened((_simulationState.Parameters.EnergyInPlantSeed - cell.EnergyContent) /
                        (float)_simulationState.Parameters.EnergyInPlantSeed);
                case CellType.PlantRoot:
                    baseColor = new(0xddccaaff);
                    break;
                case CellType.PlantGreen:
                    baseColor = new(0x44aa55ff);
                    break;
                case CellType.Water:
                    baseColor = new(0x00aaffff);
                    break;
                default:
                    baseColor = new(0x605555ff);
                    break;
            }

            if (cell.CanDiffuse())
            {
                return baseColor.Darkened(cell.NutrientContent / (4f * _simulationState.Parameters.NutrientsCriticalSoilDistribution));
            }

            return baseColor;
        }
    }
}
