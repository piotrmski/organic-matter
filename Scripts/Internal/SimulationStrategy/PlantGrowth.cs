using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.SimulationStrategy
{
    internal class PlantGrowth : ISimulationStrategy
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private SimulationState _simulationState;

        public PlantGrowth(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);
        }

        public void Advance()
        {

        }

    }
}
