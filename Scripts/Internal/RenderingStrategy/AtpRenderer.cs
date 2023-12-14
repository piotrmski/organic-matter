using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class AtpRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0xffee33ff);

        private int _maxValue;

        public AtpRenderer(SimulationState simulationState) : base(simulationState) 
        { 
            _maxValue = 2 * simulationState.Parameters.EnergyInGlucose + simulationState.Parameters.EnergyRequiredToSynthesizeRoot;
        }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((_maxValue - cell.AtpEnergy) / (float)_maxValue);
        }
    }
}
