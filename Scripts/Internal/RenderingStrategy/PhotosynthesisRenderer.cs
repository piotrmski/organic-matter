using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class PhotosynthesisRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0x66ff00ff);

        public PhotosynthesisRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened(cell.TicksSinceLastPhotosynthesis / 10f);
        }
    }
}
