using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Organicmatter.Scripts.Internal.SimulationStrategy
{
    internal class Diffusion : ISimulationStrategy
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private int _xMax;

        private int _yMax;

        private SimulationState _simulationState;

        private CellData[,] _lastCellMatrixState;

        public Diffusion(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _xMax = _spaceWidth - 1;

            _yMax = _spaceHeight - 1;

            _lastCellMatrixState = new CellData[_spaceWidth, _spaceHeight];
        }

        public void Advance()
        {
            Array.Copy(_simulationState.CellMatrix, _lastCellMatrixState, _spaceWidth * _spaceHeight);

            _simulationState.ForEachCell((ref CellData cell) =>
            {
                cell.MineralContent = 0;
                cell.EnergyContent = 0;
                cell.WasteContent = 0;
            });

            _simulationState.ForEachCell((ref CellData cell, int x, int y) =>
            {
                if (!cell.CanDiffuse()) { return; }

                CellData lastCellState = _lastCellMatrixState[x, y];

                cell.MineralContent += lastCellState.MineralContent;
                cell.EnergyContent += lastCellState.EnergyContent;
                cell.WasteContent += lastCellState.WasteContent;

                List<DiffusionSubstanceData> neighborsToDiffuseTo = GetNeighborsToDiffuseTo(x, y);

                foreach (var diffusionData in neighborsToDiffuseTo)
                {
                    ref CellData destination = ref _simulationState.CellMatrix[diffusionData.Destination.X, diffusionData.Destination.Y];

                    destination.MineralContent += diffusionData.Minerals;
                    destination.EnergyContent += diffusionData.Energy;
                    destination.WasteContent += diffusionData.Waste;

                    cell.MineralContent -= diffusionData.Minerals;
                    cell.EnergyContent -= diffusionData.Energy;
                    cell.WasteContent -= diffusionData.Waste;
                }

                if (cell.Type == CellType.Water && cell.MineralContent == 0)
                {
                    cell.Type = CellType.Air;
                }
            });
        }

        private List<DiffusionSubstanceData> GetNeighborsToDiffuseTo(int x, int y)
        {
            List<DiffusionSubstanceData> result = new();

            CellData lastCellState = _lastCellMatrixState[x, y];
            Direction connections = lastCellState.IsPlant() ? _simulationState.GetCellConnections(x, y) : Direction.None;

            if (x > 0)
            {
                AddToListIfNotNull(result, GetSubstaceToDiffuse(lastCellState, new(x - 1, y), connections.HasFlag(Direction.Left)));
            }

            if (x < _xMax)
            {
                AddToListIfNotNull(result, GetSubstaceToDiffuse(lastCellState, new(x + 1, y), connections.HasFlag(Direction.Right)));
            }

            if (y > 0)
            {
                AddToListIfNotNull(result, GetSubstaceToDiffuse(lastCellState, new(x, y - 1), connections.HasFlag(Direction.Bottom)));
            }

            if (y < _yMax)
            {
                AddToListIfNotNull(result, GetSubstaceToDiffuse(lastCellState, new(x, y + 1), connections.HasFlag(Direction.Top)));
            }

            if (lastCellState.Type == CellType.Water)
            {
                result = FillMineralsDiffusionDataFromWaterCell(result, lastCellState);
            }

            return result;
        }

        private DiffusionSubstanceData? GetSubstaceToDiffuse(CellData source, Vector2I destinationLocation, bool areCellsConnected)
        {
            CellData destination = _lastCellMatrixState[destinationLocation.X, destinationLocation.Y];

            if (source.Type == CellType.Soil && destination.Type == CellType.Soil)
            {
                if (source.MineralContent <= _simulationState.Parameters.MineralsCriticalSoilDistribution) { return null; }

                return new()
                {
                    Destination = destinationLocation,
                    Minerals = (source.MineralContent - _simulationState.Parameters.MineralsCriticalSoilDistribution) / 5
                };
            }
            else if (source.Type == CellType.Soil && destination.Type == CellType.PlantRoot)
            {
                return new()
                {
                    Destination = destinationLocation,
                    Minerals = source.MineralContent / 5
                };
            }
            else if (source.Type == CellType.Water && (destination.Type == CellType.Soil || destination.Type == CellType.PlantRoot))
            {
                return new()
                {
                    Destination = destinationLocation
                };
            }
            else if (source.IsPlant() && destination.IsPlant() && areCellsConnected)
            {
                return new()
                {
                    Destination = destinationLocation,
                    Minerals = source.MineralContent / 4,
                    Energy = source.EnergyContent / 4,
                    Waste = source.WasteContent / 4
                };
            }

            return null;
        }

        private List<DiffusionSubstanceData> FillMineralsDiffusionDataFromWaterCell(List<DiffusionSubstanceData> neighbors, CellData source)
        {
            List<DiffusionSubstanceData> result = new();

            int remainingMinerals = source.MineralContent;
            int i = 0;

            neighbors.ForEach(x =>
            {
                if (i == neighbors.Count - 1)
                {
                    x.Minerals = remainingMinerals;
                }
                else
                {
                    x.Minerals = source.MineralContent / neighbors.Count;
                    remainingMinerals -= x.Minerals;
                }

                result.Add(x);

                i++;
            });

            return result;
        }

        private void AddToListIfNotNull(List<DiffusionSubstanceData> result, DiffusionSubstanceData? value)
        {
            if (value != null)
            {
                result.Add(value.Value);
            }
        }

        private struct DiffusionSubstanceData
        {
            public Vector2I Destination;

            public int Minerals;

            public int Energy;

            public int Waste;
        }
    }
}
