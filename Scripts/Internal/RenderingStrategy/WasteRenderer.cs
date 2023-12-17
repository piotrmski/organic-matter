using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class WasteRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0xffee33ff);

        public WasteRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((20 - cell.WasteContent) / 20f);
        }
    }
}
