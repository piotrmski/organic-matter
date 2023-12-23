using Godot;
using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;

namespace Organicmatter.Scripts.Internal.SimulationStep
{
    internal class PlantGrowth : ISimulationStep
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private int _xMax;

        private int _yMax;

        private SimulationState _simulationState;

        private AirInSoilSearch _airInSoilSearch;

        private RandomNumberGenerator _rng = new();

        public PlantGrowth(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _airInSoilSearch = new(simulationState);

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _xMax = _spaceWidth - 1;

            _yMax = _spaceHeight - 1;

            _rng.Randomize();
        }

        public void Advance()
        {
            _simulationState.ForEachCell((ref CellData cell, int x, int y) =>
            {
                if (!AreConditionsMetForGrowth(cell)) { return; }

                Direction connections = _simulationState.GetCellConnections(x, y);

                int numberOfConnections = GetNumberOfConnections(connections);

                if (numberOfConnections >= 3) { return; }

                Direction directionToSynthesize = GetRandomDirectionToSynthesize(cell, connections);

                if (directionToSynthesize == Direction.None) { return; }

                Vector2I coordinatesToSynthesize = GetNeighborCoordinates(x, y, directionToSynthesize);

                if (!AreCoordinatesLegal(coordinatesToSynthesize) ||
                    !_simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].CanPlantSynthesizeHere()) { return; }

                int numberOfPlantsInNeighborhood = GetNumberOfPlantsInNeighborhood(coordinatesToSynthesize.X, coordinatesToSynthesize.Y);

                if (numberOfPlantsInNeighborhood > 1) { return; }

                SynthesizePlant(coordinatesToSynthesize, cell.Type, x, y, directionToSynthesize);
            });
        }

        private bool AreConditionsMetForGrowth(CellData cell)
        {
            return IsGrowthPossible(cell) && IsGrowthDesired(cell);
        }

        private bool IsGrowthPossible(CellData cell)
        {
            return cell.IsPlantCoreStructure() && cell.EnergyContent >= _simulationState.Parameters.EnergyInPlantCellStructure;
        }

        private bool IsGrowthDesired(CellData cell)
        {
            return cell.NutrientContent >= _simulationState.Parameters.EnergyInPlantCellStructure / 2;
        }

        private int GetNumberOfConnections(Direction connections)
        {
            int result = 0;

            if (connections.HasFlag(Direction.Left)) ++result;
            if (connections.HasFlag(Direction.Right)) ++result;
            if (connections.HasFlag(Direction.Down)) ++result;
            if (connections.HasFlag(Direction.Up)) ++result;

            return result;
        }

        private int GetNumberOfPlantsInNeighborhood(int x, int y)
        {
            int result = 0;

            if (x > 0 && _simulationState.CellMatrix[x - 1, y].IsPlant()) ++result;
            if (x < _xMax && _simulationState.CellMatrix[x + 1, y].IsPlant()) ++result;
            if (y > 0 && _simulationState.CellMatrix[x, y - 1].IsPlant()) ++result;
            if (y < _yMax && _simulationState.CellMatrix[x, y + 1].IsPlant()) ++result;

            return result;
        }

        private Direction GetRandomDirectionToSynthesize(CellData cell, Direction connections)
        {
            if (cell.Type == CellType.PlantGreen) return GetRandomDirectionToSynthesizeForGreen(connections);

            if (cell.Type == CellType.PlantRoot) return GetRandomDirectionToSynthesizeForRoot(connections);

            return Direction.None;
        }

        private Direction GetRandomDirectionToSynthesizeForGreen(Direction connections)
        {
            switch (connections)
            {
                case Direction.Down:
                    switch (_rng.Randi() % 3)
                    {
                        case 0:  return Direction.Left;
                        case 1:  return Direction.Right;
                        default: return Direction.Up;
                    }
                case Direction.Left:
                    switch (_rng.Randi() % 2)
                    {
                        case 0: return Direction.Right;
                        default: return Direction.Up;
                    }
                case Direction.Right:
                    switch (_rng.Randi() % 2)
                    {
                        case 0: return Direction.Left;
                        default: return Direction.Up;
                    }
                case Direction.Down | Direction.Left: return Direction.Up;
                case Direction.Down | Direction.Right: return Direction.Up;
                case Direction.Up | Direction.Left: return Direction.Right;
                case Direction.Up | Direction.Right: return Direction.Left;

                default: return Direction.None;
            }
        }

        private Direction GetRandomDirectionToSynthesizeForRoot(Direction connections)
        {
            switch (connections)
            {
                case Direction.Up:
                    switch (_rng.Randi() % 3)
                    {
                        case 0: return Direction.Left;
                        case 1: return Direction.Right;
                        default: return Direction.Down;
                    }
                case Direction.Left:
                case Direction.Up | Direction.Left:
                    switch (_rng.Randi() % 2)
                    {
                        case 0: return Direction.Right;
                        default: return Direction.Down;
                    }
                case Direction.Right:
                case Direction.Up | Direction.Right:
                    switch (_rng.Randi() % 2)
                    {
                        case 0: return Direction.Left;
                        default: return Direction.Down;
                    }
                case Direction.Down | Direction.Left: return Direction.Right;
                case Direction.Down | Direction.Right: return Direction.Left;
                case Direction.Left | Direction.Right: return Direction.Down;

                default: return Direction.None;
            }
        }

        private Vector2I GetNeighborCoordinates(int x, int y, Direction directionToSynthesize)
        {
            switch (directionToSynthesize)
            {
                case Direction.Left: return new(x - 1, y);
                case Direction.Right: return new(x + 1, y);
                case Direction.Down: return new(x, y - 1);
                default: return new(x, y + 1);
            }
        }

        private bool AreCoordinatesLegal(Vector2I coordinates)
        {
            return coordinates.X >= 0 && coordinates.X < _spaceWidth && coordinates.Y >= 0 && coordinates.Y < _spaceHeight;
        }

        private void SynthesizePlant(Vector2I coordinatesToSynthesize, CellType type, int sourceX, int sourceY, Direction directionOfGrowth)
        {
            if (_simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].Type == CellType.Soil)
            {
                if (type == CellType.PlantGreen) { return; }

                Vector2I[] displacementPath = _airInSoilSearch.FindPathToNearestAir(coordinatesToSynthesize.X, coordinatesToSynthesize.Y);

                if (displacementPath == null) { return; }

                for (int i = displacementPath.Length - 1; i > 0; --i)
                {
                    SwapCellsByCoordinates(displacementPath[i], displacementPath[i - 1]);
                }
            }

            _simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].Type = type;
            _simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].TicksSinceSynthesis = 0;
            _simulationState.CellMatrix[sourceX, sourceY].EnergyContent -= _simulationState.Parameters.EnergyInPlantCellStructure;
            _simulationState.AddCellConnections(sourceX, sourceY, directionOfGrowth);
        }

        private void SwapCellsByCoordinates(Vector2I a, Vector2I b)
        {
            (_simulationState.CellMatrix[a.X, a.Y], _simulationState.CellMatrix[b.X, b.Y]) =
                (_simulationState.CellMatrix[b.X, b.Y], _simulationState.CellMatrix[a.X, a.Y]);
        }
    }
}
