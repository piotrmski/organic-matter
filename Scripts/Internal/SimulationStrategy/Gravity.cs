using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.SimulationStrategy
{
    internal class Gravity : ISimulationStrategy
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private SimulationState _simulationState;

        public Gravity(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);
        }

        public void Advance()
        {
            for (int x = 0; x < _spaceWidth; ++x)
            {
                for (int y = 1; y < _spaceHeight; ++y)
                {
                    if (!_simulationState.CellMatrix[x, y].CanFall()) { continue; }

                    if (!IsSolidOnBottom(x, y))
                    {
                        Swap(x, y, x, y - 1);
                    }
                    else if (!IsSolidOnBottomLeft(x, y))
                    {
                        Swap(x, y, x - 1, y - 1);
                    }
                    else if (!IsSolidOnBottomRight(x, y))
                    {
                        Swap(x, y, x + 1, y - 1);
                    }
                }
            }
        }

        private bool IsSolidOnBottom(int x, int y)
        {
            return _simulationState.CellMatrix[x, y - 1].IsSolid();
        }

        private bool IsSolidOnBottomLeft(int x, int y)
        {
            return x == 0 || _simulationState.CellMatrix[x - 1, y - 1].IsSolid();
        }

        private bool IsSolidOnBottomRight(int x, int y)
        {
            return x == _spaceWidth - 1 || _simulationState.CellMatrix[x + 1, y - 1].IsSolid();
        }

        private void Swap(int x1, int y1, int x2, int y2)
        {
            (_simulationState.CellMatrix[x1, y1], _simulationState.CellMatrix[x2, y2]) = (_simulationState.CellMatrix[x2, y2], _simulationState.CellMatrix[x1, y1]);
        }
    }
}
