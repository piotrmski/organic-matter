using Godot;
using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Organicmatter.Scripts.Internal.SimulationStrategy
{
    internal class PlantGrowth : ISimulationStrategy
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private int _xMax;

        private int _yMax;

        private SimulationState _simulationState;

        private AirInSoilSearch _airInSoilSearch;

        private RandomNumberGenerator _rng = new();

        private uint[,] _pregeneratedRandomInts;

        private Vector2I[][] _cellSetsToProcessInParallel;

        private Direction[,] _growthDirection;

        public PlantGrowth(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _airInSoilSearch = new(simulationState);

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _xMax = _spaceWidth - 1;

            _yMax = _spaceHeight - 1;

            _rng.Randomize();

            _pregeneratedRandomInts = new uint[_spaceWidth, _spaceHeight];

            _growthDirection = new Direction[_spaceWidth, _spaceHeight];

            _cellSetsToProcessInParallel = GetCellSetsToProcessInParallel();
        }

        private Vector2I[][] GetCellSetsToProcessInParallel()
        {
            List<Vector2I>[] cellSets = new List<Vector2I>[16];
            for (int i = 0; i < 16; i++) { cellSets[i] = new List<Vector2I>(); }

            _simulationState.ForEachCell((x, y) =>
            {
                cellSets[CoordinatesToSetIndex(x, y)].Add(new Vector2I(x, y));
            });

            return cellSets.Select(x => x.ToArray()).ToArray();
        }

        public void Advance()
        {
            PregenerateRandomInts();

            for (int i = 0; i < 16; i++)
            {
                Parallel.ForEach(_cellSetsToProcessInParallel[i], cellCoordinates =>
                {
                    ref CellData cell = ref _simulationState.CellMatrix[cellCoordinates.X, cellCoordinates.Y];

                    if (!AreConditionsMetForGrowth(cell)) { return; }

                    Direction connections = _simulationState.GetCellConnections(cellCoordinates.X, cellCoordinates.Y);

                    int numberOfConnections = GetNumberOfConnections(connections);

                    if (numberOfConnections >= 3) { return; }

                    Direction directionToSynthesize = GetRandomDirectionToSynthesize(cell, connections, _pregeneratedRandomInts[cellCoordinates.X, cellCoordinates.Y]);

                    if (directionToSynthesize == Direction.None) { return; }

                    Vector2I coordinatesToSynthesize = GetNeighborCoordinates(cellCoordinates, directionToSynthesize);

                    if (!AreCoordinatesLegal(coordinatesToSynthesize) ||
                        !_simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].CanPlantSynthesizeHere()) { return; }

                    int numberOfPlantsInNeighborhood = GetNumberOfPlantsInNeighborhood(coordinatesToSynthesize.X, coordinatesToSynthesize.Y);

                    if (numberOfPlantsInNeighborhood > 1) { return; }

                    _growthDirection[cellCoordinates.X, cellCoordinates.Y] = directionToSynthesize;
                });

                Vector2I setOffset = SetIndexToOffset(i);

                for (int x = setOffset.X; x < _spaceWidth; x += 4)
                {
                    for (int y = setOffset.Y; y < _spaceHeight; y += 4)
                    {
                        if (_growthDirection[x, y] != Direction.None)
                        {
                            SynthesizePlant(new(x, y), _growthDirection[x, y]);

                            _growthDirection[x, y] = Direction.None;
                        }
                    }
                }
            }
        }

        private bool AreConditionsMetForGrowth(CellData cell)
        {
            return IsGrowthPossible(cell) && IsGrowthDesired(cell);
        }

        private bool IsGrowthPossible(CellData cell)
        {
            return cell.IsPlant() && cell.EnergyContent >= _simulationState.Parameters.EnergyToSynthesizePlantCell;
        }

        private bool IsGrowthDesired(CellData cell)
        {
            return cell.NutrientContent >= _simulationState.Parameters.EnergyToSynthesizePlantCell / 2;
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

        private Direction GetRandomDirectionToSynthesize(CellData cell, Direction connections, uint randomInt)
        {
            if (cell.Type == CellType.PlantGreen) return GetRandomDirectionToSynthesizeForGreen(connections, randomInt);

            if (cell.Type == CellType.PlantRoot) return GetRandomDirectionToSynthesizeForRoot(connections, randomInt);

            return Direction.None;
        }

        private Direction GetRandomDirectionToSynthesizeForGreen(Direction connections, uint randomInt)
        {
            switch (connections)
            {
                case Direction.Down:
                    switch (randomInt % 3)
                    {
                        case 0:  return Direction.Left;
                        case 1:  return Direction.Right;
                        default: return Direction.Up;
                    }
                case Direction.Left:
                    switch (randomInt % 2)
                    {
                        case 0: return Direction.Right;
                        default: return Direction.Up;
                    }
                case Direction.Right:
                    switch (randomInt % 2)
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

        private Direction GetRandomDirectionToSynthesizeForRoot(Direction connections, uint randomInt)
        {
            switch (connections)
            {
                case Direction.Up:
                    switch (randomInt % 3)
                    {
                        case 0: return Direction.Left;
                        case 1: return Direction.Right;
                        default: return Direction.Down;
                    }
                case Direction.Left:
                case Direction.Up | Direction.Left:
                    switch (randomInt % 2)
                    {
                        case 0: return Direction.Right;
                        default: return Direction.Down;
                    }
                case Direction.Right:
                case Direction.Up | Direction.Right:
                    switch (randomInt % 2)
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

        private Vector2I GetNeighborCoordinates(Vector2I coordinates, Direction directionToSynthesize)
        {
            switch (directionToSynthesize)
            {
                case Direction.Left: return coordinates + new Vector2I(-1, 0);
                case Direction.Right: return coordinates + new Vector2I(1, 0);
                case Direction.Down: return coordinates + new Vector2I(0, -1);
                default: return coordinates + new Vector2I(0, 1);
            }
        }

        private bool AreCoordinatesLegal(Vector2I coordinates)
        {
            return coordinates.X >= 0 && coordinates.X < _spaceWidth && coordinates.Y >= 0 && coordinates.Y < _spaceHeight;
        }

        private void SynthesizePlant(Vector2I sourceCoordinates, Direction directionOfGrowth)
        {
            CellType type = _simulationState.CellMatrix[sourceCoordinates.X, sourceCoordinates.Y].Type;

            Vector2I coordinatesToSynthesize = GetNeighborCoordinates(sourceCoordinates, directionOfGrowth);

            if (_simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].Type == CellType.Soil)
            {
                if (type == CellType.PlantGreen && directionOfGrowth != Direction.Up) { return; }

                Vector2I[] displacementPath = _airInSoilSearch.FindPathToNearestAir(coordinatesToSynthesize.X, coordinatesToSynthesize.Y);

                if (displacementPath == null) { return; }

                for (int i = displacementPath.Length - 1; i > 0; --i)
                {
                    SwapCellsByCoordinates(displacementPath[i], displacementPath[i - 1]);
                }
            }

            _simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].Type = type;
            _simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].TicksSinceSynthesis = 0;
            _simulationState.CellMatrix[sourceCoordinates.X, sourceCoordinates.Y].EnergyContent -= _simulationState.Parameters.EnergyToSynthesizePlantCell;
            _simulationState.AddCellConnections(sourceCoordinates.X, sourceCoordinates.Y, directionOfGrowth);
        }

        private void SwapCellsByCoordinates(Vector2I a, Vector2I b)
        {
            (_simulationState.CellMatrix[a.X, a.Y], _simulationState.CellMatrix[b.X, b.Y]) =
                (_simulationState.CellMatrix[b.X, b.Y], _simulationState.CellMatrix[a.X, a.Y]);
        }

        private void PregenerateRandomInts()
        {
            _simulationState.ForEachCell((x, y) =>
            {
                _pregeneratedRandomInts[x, y] = _rng.Randi();
            });
        }

        private int CoordinatesToSetIndex(int x, int y)
        {
            return (x % 4) * 4 + (y % 4);
        }

        private Vector2I SetIndexToOffset(int index)
        {
            return new(index / 4, index % 4);
        }
    }
}
