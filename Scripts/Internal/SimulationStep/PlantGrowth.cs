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

        private SpecialCoordinates _specialCoordinates;

        public PlantGrowth(SimulationState simulationState, SpecialCoordinates specialCoordinates)
        {
            _simulationState = simulationState;

            _specialCoordinates = specialCoordinates;

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

                SynthesizePlantCell(coordinatesToSynthesize, new(x, y), directionToSynthesize);
            });
        }

        private bool AreConditionsMetForGrowth(CellData cell)
        {
            return IsCoreStructureGrowthPossible(cell) && IsCoreStructureGrowthDesired(cell) || IsFruitGrowthPossible(cell);
        }

        private bool IsCoreStructureGrowthPossible(CellData cell)
        {
            return cell.IsPlantCoreStructure() && cell.EnergyContent >= _simulationState.Parameters.EnergyInPlantCellStructure;
        }

        private bool IsCoreStructureGrowthDesired(CellData cell)
        {
            return cell.Type == CellType.PlantGreen ? cell.EnergyContent >= _simulationState.Parameters.EnergyInPlantCellStructure * 2 : true;
        }

        private bool IsFruitGrowthPossible(CellData cell)
        {
            return cell.Type == CellType.PlantGreen &&
                cell.EnergyContent >= _simulationState.Parameters.EnergyToSynthesizeFruitCell &&
                cell.TicksSinceSynthesis >= _simulationState.Parameters.MinimumAgeToSynthesizeFruitCell &&
                cell.TicksSinceSynthesis <= _simulationState.Parameters.MaximumAgeToSynthesizeFruitCell;
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

        private void SynthesizePlantCell(Vector2I coordinatesToSynthesize, Vector2I sourceCoordinates, Direction directionOfGrowth)
        {
            ref CellData sourceCell = ref _simulationState.CellMatrix[sourceCoordinates.X, sourceCoordinates.Y];

            if (_simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y].Type == CellType.Soil)
            {
                bool soilDisplaced = TryDisplaceSoil(coordinatesToSynthesize, sourceCell.Type);

                if (!soilDisplaced) { return; }
            }

            ref CellData newCell = ref _simulationState.CellMatrix[coordinatesToSynthesize.X, coordinatesToSynthesize.Y];

            if (IsFruitGrowthPossible(sourceCell)) 
            { 
                SynthesizeFruitCell(ref newCell, ref sourceCell);

                _specialCoordinates.Fruits.Add(coordinatesToSynthesize);
            }
            else 
            {
                SynthesizePlantCoreStructureCell(ref newCell, ref sourceCell); 
            }

            newCell.GrowthOrigin = Opposite(directionOfGrowth);
            _simulationState.AddCellConnections(sourceCoordinates.X, sourceCoordinates.Y, directionOfGrowth);
        }

        private void SynthesizeFruitCell(ref CellData newCell, ref CellData sourceCell)
        {
            newCell.Type = CellType.PlantFruit;
            newCell.TicksSinceSynthesis = 0;
            newCell.EnergyContent = (sourceCell.EnergyContent - _simulationState.Parameters.EnergyInPlantCellStructure) / 2;
            sourceCell.EnergyContent -= newCell.EnergyContent + _simulationState.Parameters.EnergyInPlantCellStructure;
        }

        private void SynthesizePlantCoreStructureCell(ref CellData newCell, ref CellData sourceCell)
        {
            newCell.Type = sourceCell.Type;
            newCell.TicksSinceSynthesis = 0;
            sourceCell.EnergyContent -= _simulationState.Parameters.EnergyInPlantCellStructure;
        }

        private bool TryDisplaceSoil(Vector2I coordinatesToSynthesize, CellType type)
        {
            if (type == CellType.PlantGreen) { return false; }

            Vector2I[] displacementPath = _airInSoilSearch.FindPathToNearestAir(coordinatesToSynthesize.X, coordinatesToSynthesize.Y);

            if (displacementPath == null) { return false; }

            Vector2I airCellCoordinates = displacementPath[displacementPath.Length - 1];

            CellData airCell = _simulationState.CellMatrix[airCellCoordinates.X, airCellCoordinates.Y];

            for (int i = displacementPath.Length - 1; i > 0; --i)
            {
                _simulationState.CellMatrix[displacementPath[i].X, displacementPath[i].Y] =
                    _simulationState.CellMatrix[displacementPath[i - 1].X, displacementPath[i - 1].Y];
            }

            _simulationState.CellMatrix[displacementPath[0].X, displacementPath[0].Y] = airCell;

            return true;
        }

        private static Direction Opposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
            }

            return Direction.None;
        }
    }
}
