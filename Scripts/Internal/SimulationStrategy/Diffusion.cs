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
                cell.NutrientContent = 0;
                cell.EnergyContent = 0;
                cell.WasteContent = 0;
            });

            _simulationState.ForEachCell((ref CellData cell, int x, int y) =>
            {
                if (!cell.CanDiffuse()) { return; }

                CellData lastCellState = _lastCellMatrixState[x, y];

                cell.NutrientContent += lastCellState.NutrientContent;
                cell.EnergyContent += lastCellState.EnergyContent;
                cell.WasteContent += lastCellState.WasteContent;

                List<DiffusionSubstanceData> neighborsToDiffuseTo = GetNeighborsToDiffuseTo(x, y);

                foreach (var diffusionData in neighborsToDiffuseTo)
                {
                    ref CellData destination = ref _simulationState.CellMatrix[diffusionData.Destination.X, diffusionData.Destination.Y];

                    destination.NutrientContent += diffusionData.Nutrients;
                    destination.EnergyContent += diffusionData.Energy;
                    destination.WasteContent += diffusionData.Waste;

                    cell.NutrientContent -= diffusionData.Nutrients;
                    cell.EnergyContent -= diffusionData.Energy;
                    cell.WasteContent -= diffusionData.Waste;
                }

                if (cell.Type == CellType.Water && cell.NutrientContent == 0)
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
                result = FillNutrientsDiffusionDataFromWaterCell(result, lastCellState);
            }

            return result;
        }

        private DiffusionSubstanceData? GetSubstaceToDiffuse(CellData source, Vector2I destinationLocation, bool areCellsConnected)
        {
            CellData destination = _lastCellMatrixState[destinationLocation.X, destinationLocation.Y];

            if (source.Type == CellType.Soil && destination.Type == CellType.Soil)
            {
                if (source.NutrientContent <= _simulationState.Parameters.NutrientsCriticalSoilDistribution) { return null; }

                return new()
                {
                    Destination = destinationLocation,
                    Nutrients = (source.NutrientContent - _simulationState.Parameters.NutrientsCriticalSoilDistribution) / 5
                };
            }
            else if (source.Type == CellType.Soil && destination.Type == CellType.PlantRoot)
            {
                return new()
                {
                    Destination = destinationLocation,
                    Nutrients = source.NutrientContent / 5
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
                    Nutrients = source.NutrientContent / 4,
                    Energy = source.EnergyContent / 4,
                    Waste = source.WasteContent / 4
                };
            }

            return null;
        }

        private List<DiffusionSubstanceData> FillNutrientsDiffusionDataFromWaterCell(List<DiffusionSubstanceData> neighbors, CellData source)
        {
            List<DiffusionSubstanceData> result = new();

            int remainingNutrients = source.NutrientContent;
            int remainingEnergy = source.EnergyContent;
            int remainingWaste = source.WasteContent;
            int i = 0;

            neighbors.ForEach(x =>
            {
                if (i == neighbors.Count - 1)
                {
                    x.Nutrients = remainingNutrients;
                    x.Energy = remainingEnergy;
                    x.Waste = remainingWaste;
                }
                else
                {
                    x.Nutrients = source.NutrientContent / neighbors.Count;
                    x.Energy = source.EnergyContent / neighbors.Count;
                    x.Waste = source.WasteContent / neighbors.Count;
                    remainingNutrients -= x.Nutrients;
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

            public int Nutrients;

            public int Energy;

            public int Waste;
        }
    }
}
