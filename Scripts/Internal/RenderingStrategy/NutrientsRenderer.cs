using Godot;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class NutrientsRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0xee33ffff);

        public NutrientsRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((_simulationState.Parameters.NutrientsCriticalSoilDistribution - cell.NutrientContent) /
                (float)_simulationState.Parameters.NutrientsCriticalSoilDistribution);
        }
    }
}
