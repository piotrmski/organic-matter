using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class RespirationRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0x2200ffff);

        public RespirationRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened(cell.TicksSinceLastRespiration / 10f);
        }
    }
}
