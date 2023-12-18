using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class MineralsRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0xee33ffff);

        public MineralsRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((_simulationState.Parameters.MineralsCriticalSoilDistribution - cell.MineralContent) /
                (float)_simulationState.Parameters.MineralsCriticalSoilDistribution);
        }
    }
}
