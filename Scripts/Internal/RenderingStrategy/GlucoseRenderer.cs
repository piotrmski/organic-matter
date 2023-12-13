using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class GlucoseRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0xffeeffff);

        public GlucoseRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((_simulationState.Parameters.GlucoseInCellulose - cell.GlucoseMolecules) /
                (float)_simulationState.Parameters.GlucoseInCellulose);
        }
    }
}
