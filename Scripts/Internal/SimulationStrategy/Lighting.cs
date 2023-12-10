using Organicmatter.Scripts.Internal.Model;
using System;
using Godot;

namespace Organicmatter.Scripts.Internal.SimulationStrategy
{
    internal class Lighting : ISimulationStrategy
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private SimulationState _simulationState;

        public Lighting(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);
        }

        public void Advance()
        {
            for (int x = 0; x < _spaceWidth; ++x)
            {
                _simulationState.CellMatrix[x, _spaceHeight - 1].LightEnergy =
                    _simulationState.CellMatrix[x, _spaceHeight - 1].Type == CellType.Air
                        ? _simulationState.Parameters.DirectLightEnergy
                        : _simulationState.Parameters.DirectLightEnergy / 2;

                for (int y = _spaceHeight - 2; y > 0; --y)
                {
                    _simulationState.CellMatrix[x, y].LightEnergy =
                        _simulationState.CellMatrix[x, y].Type == CellType.Air
                            ? _simulationState.CellMatrix[x, y + 1].LightEnergy
                            : _simulationState.CellMatrix[x, y + 1].LightEnergy / 2;
                }
            }
        }
    }
}
