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
                cell.NurtientContent = 0;
                cell.EnergyContent = 0;
                cell.WasteContent = 0;
            });

            _simulationState.ForEachCell((ref CellData cell, int x, int y) =>
            {
                if (!cell.CanDiffuse()) { return; }

                CellData lastCellState = _lastCellMatrixState[x, y];

                cell.NurtientContent += lastCellState.NurtientContent;
                cell.EnergyContent += lastCellState.EnergyContent;
                cell.WasteContent += lastCellState.WasteContent;

                List<DiffusionSubstanceData> neighborsToDiffuseTo = GetNeighborsToDiffuseTo(x, y);

                foreach (var diffusionData in neighborsToDiffuseTo)
                {
                    ref CellData destination = ref _simulationState.CellMatrix[diffusionData.Destination.X, diffusionData.Destination.Y];

                    destination.NurtientContent += diffusionData.Nurtients;
                    destination.EnergyContent += diffusionData.Energy;
                    destination.WasteContent += diffusionData.Waste;

                    cell.NurtientContent -= diffusionData.Nurtients;
                    cell.EnergyContent -= diffusionData.Energy;
                    cell.WasteContent -= diffusionData.Waste;
                }

                if (cell.Type == CellType.Water && cell.NurtientContent == 0)
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
                AddToListIfNotNull(result, GetSubstaceToDiffuse(lastCellState, new(x, y - 1), connections.HasFlag(Direction.Down)));
            }

            if (y < _yMax)
            {
                AddToListIfNotNull(result, GetSubstaceToDiffuse(lastCellState, new(x, y + 1), connections.HasFlag(Direction.Up)));
            }

            if (lastCellState.Type == CellType.Water)
            {
                result = FillNurtientsDiffusionDataFromWaterCell(result, lastCellState);
            }

            return result;
        }

        private DiffusionSubstanceData? GetSubstaceToDiffuse(CellData source, Vector2I destinationLocation, bool areCellsConnected)
        {
            CellData destination = _lastCellMatrixState[destinationLocation.X, destinationLocation.Y];

            if (source.Type == CellType.Soil && destination.Type == CellType.Soil)
            {
                if (source.NurtientContent <= _simulationState.Parameters.NurtientsCriticalSoilDistribution) { return null; }

                return new()
                {
                    Destination = destinationLocation,
                    Nurtients = (source.NurtientContent - _simulationState.Parameters.NurtientsCriticalSoilDistribution) / 5
                };
            }
            else if (source.Type == CellType.Soil && destination.Type == CellType.PlantRoot)
            {
                return new()
                {
                    Destination = destinationLocation,
                    Nurtients = source.NurtientContent / 5
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
                    Nurtients = source.NurtientContent / 4,
                    Energy = source.EnergyContent / 4,
                    Waste = source.WasteContent / 4
                };
            }

            return null;
        }

        private List<DiffusionSubstanceData> FillNurtientsDiffusionDataFromWaterCell(List<DiffusionSubstanceData> neighbors, CellData source)
        {
            List<DiffusionSubstanceData> result = new();

            int remainingNurtients = source.NurtientContent;
            int remainingEnergy = source.EnergyContent;
            int remainingWaste = source.WasteContent;
            int i = 0;

            neighbors.ForEach(x =>
            {
                if (i == neighbors.Count - 1)
                {
                    x.Nurtients = remainingNurtients;
                    x.Energy = remainingEnergy;
                    x.Waste = remainingWaste;
                }
                else
                {
                    x.Nurtients = source.NurtientContent / neighbors.Count;
                    x.Energy = source.EnergyContent / neighbors.Count;
                    x.Waste = source.WasteContent / neighbors.Count;
                    remainingNurtients -= x.Nurtients;
                    remainingEnergy -= x.Energy;
                    remainingWaste -= x.Waste;
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

            public int Nurtients;

            public int Energy;

            public int Waste;
        }
    }
}
