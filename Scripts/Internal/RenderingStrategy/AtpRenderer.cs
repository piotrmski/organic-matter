using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class AtpRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0xffee33ff);

        public AtpRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((_simulationState.Parameters.EnergyToSynthesizeCellulose - cell.AtpEnergy) /
                (float)_simulationState.Parameters.EnergyToSynthesizeCellulose);
        }
    }
}
