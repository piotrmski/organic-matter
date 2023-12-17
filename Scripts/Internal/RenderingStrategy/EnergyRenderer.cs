﻿using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal class EnergyRenderer : SolidColorRenderer
    {
        private Color _baseColor = new(0x33ffeeff);

        public EnergyRenderer(SimulationState simulationState) : base(simulationState) { }

        protected override Color GetCellColor(CellData cell)
        {
            return _baseColor.Darkened((20 - cell.EnergyContent) / 20f);
        }
    }
}