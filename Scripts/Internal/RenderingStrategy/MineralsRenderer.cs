using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class NurtientsRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0xee33ffff);

        public NurtientsRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((_simulationState.Parameters.NurtientsCriticalSoilDistribution - cell.NurtientContent) /
                (float)_simulationState.Parameters.NurtientsCriticalSoilDistribution);
        }
    }
}
