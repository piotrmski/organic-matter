using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal abstract class SolidColorRenderer : IRenderer
    {
        protected SimulationState _simulationState;

        private readonly int _maxY;

        public Image RenderedImage { get; private set; }

        public SolidColorRenderer(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _maxY = simulationState.CellMatrix.GetLength(1) - 1;

            RenderedImage = Image.Create(simulationState.CellMatrix.GetLength(0), simulationState.CellMatrix.GetLength(1), false, Image.Format.Rgb8);
        }

        public void UpdateImage()
        {
            _simulationState.ForEachCell((ref CellData cell, int x, int y) => RenderedImage.SetPixel(x, _maxY - y, GetCellColor(cell)));
        }

        protected abstract Color GetCellColor(CellData cell);
    }
}
