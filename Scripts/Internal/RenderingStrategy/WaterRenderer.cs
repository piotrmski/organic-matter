using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class WaterRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0x3399ffff);

        public WaterRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((_simulationState.Parameters.WaterMoleculesStartingDistribution - cell.WaterMolecules) /
                (float)_simulationState.Parameters.WaterMoleculesStartingDistribution);
        }
    }
}
