using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class AgeRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0x2200ffff);

        public AgeRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened(cell.IsPlant () ? (1000 - cell.TicksSinceSynthesis) / 1000f : 1);
        }
    }
}
