using Godot;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Organicmatter.Scripts.Internal.SimulationStep
{
    internal class Gravity : ISimulationStep
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private List<int>[] _columnsToProcessIndependently;

        private SimulationState _simulationState;

        private RandomNumberGenerator _rng = new();

        private SeedCoordinates _seedCoordinates;

        public Gravity(SimulationState simulationState, SeedCoordinates seedCoordinates)
        {
            _simulationState = simulationState;

            _seedCoordinates = seedCoordinates;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _columnsToProcessIndependently = new List<int>[3];

            _columnsToProcessIndependently[0] = new();
            _columnsToProcessIndependently[1] = new();
            _columnsToProcessIndependently[2] = new();

            for (int x = 0; x < _spaceWidth; ++x)
            {
                _columnsToProcessIndependently[x % 3].Add(x);
            }

            _rng.Randomize();
        }

        public void Advance()
        {
            foreach (List<int> columns in _columnsToProcessIndependently)
            {
                Parallel.ForEach(columns, x =>
                {
                    for (int y = 1; y < _spaceHeight; ++y)
                    {
                        if (!_simulationState.CellMatrix[x, y].CanFall()) { continue; }

                        FallSoilOrWater(x, y);
                    }
                });
            }

            _seedCoordinates.Update(seedCoordinates =>
            {
                Vector2I newCoordinates = GetNewSeedCoordinates(seedCoordinates);

                Swap(seedCoordinates.X, seedCoordinates.Y, newCoordinates.X, newCoordinates.Y);

                return newCoordinates;
            });
        }

        private void FallSoilOrWater(int x, int y)
        {
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
            (_simulationState.CellMatrix[x1, y1], _simulationState.CellMatrix[x2, y2]) =
                (_simulationState.CellMatrix[x2, y2], _simulationState.CellMatrix[x1, y1]);
        }

        private Vector2I GetNewSeedCoordinates(Vector2I coordinates)
        {
            Direction directionToFall = (_rng.Randi() % 11) switch
            {
                0 => Direction.Down,
                1 => Direction.Down,
                2 => Direction.Down | Direction.Left,
                3 => Direction.Down | Direction.Left,
                4 => Direction.Down | Direction.Right,
                5 => Direction.Down | Direction.Right,
                6 => Direction.Left,
                7 => Direction.Left | Direction.Up,
                8 => Direction.Up,
                9 => Direction.Up | Direction.Right,
                _ => Direction.Right
            };

            Vector2I neighborCoordinates = GetNeighborCoordinates(coordinates, directionToFall);

            if (AreCoordinatesValid(neighborCoordinates) &&
                !_simulationState.CellMatrix[neighborCoordinates.X, neighborCoordinates.Y].IsSolid())
            {
                return neighborCoordinates;
            }

            return coordinates;
        }

        private Vector2I GetNeighborCoordinates(Vector2I coordinates, Direction direction)
        {
            if (direction.HasFlag(Direction.Left)) { --coordinates.X; }
            if (direction.HasFlag(Direction.Right)) { ++coordinates.X; }
            if (direction.HasFlag(Direction.Down)) { --coordinates.Y; }
            if (direction.HasFlag(Direction.Up)) { ++coordinates.Y; }

            return coordinates;
        }

        private bool AreCoordinatesValid(Vector2I coordinates)
        {
            return coordinates.X >= 0 && coordinates.Y >= 0 &&
                coordinates.X < _spaceWidth && coordinates.Y < _spaceHeight;
        }
    }
}
