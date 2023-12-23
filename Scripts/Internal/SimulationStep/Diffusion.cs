using Godot;
using Organicmatter.Scripts.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Organicmatter.Scripts.Internal.SimulationStep
{
    internal class Diffusion : ISimulationStep
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private int _xMax;

        private int _yMax;

        private SimulationState _simulationState;

        private CellData[,] _lastCellMatrixState;

        private Vector2I[][] _cellSetsToProcessInParallel;

        public Diffusion(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _xMax = _spaceWidth - 1;

            _yMax = _spaceHeight - 1;

            _lastCellMatrixState = new CellData[_spaceWidth, _spaceHeight];

            _cellSetsToProcessInParallel = GetCellSetsToProcessInParallel();
        }

        private Vector2I[][] GetCellSetsToProcessInParallel()
        {
            List<Vector2I>[] cellSets = new List<Vector2I>[9];
            for (int i = 0; i < 9; i++) { cellSets[i] = new List<Vector2I>(); }

            _simulationState.ForEachCell((x, y) =>
            {
                int setIndex = (x % 3) * 3 + (y % 3);
                cellSets[setIndex].Add(new Vector2I(x, y));
            });

            return cellSets.Select(x => x.ToArray()).ToArray();
        }

        public void Advance()
        {
            Array.Copy(_simulationState.CellMatrix, _lastCellMatrixState, _spaceWidth * _spaceHeight);

            foreach(Vector2I[] cells in _cellSetsToProcessInParallel)
            {
                Parallel.ForEach(cells, cellCoordinates =>
                {
                    ref CellData cell = ref _simulationState.CellMatrix[cellCoordinates.X, cellCoordinates.Y];

                    if (!cell.CanDiffuse()) { return; }

                    DiffusionSubstanceData[] neighborsToDiffuseTo = GetNeighborsToDiffuseTo(cellCoordinates.X, cellCoordinates.Y);

                    foreach (var diffusionData in neighborsToDiffuseTo)
                    {
                        if (!diffusionData.ShouldDiffuse) { continue; }

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
        }

        private DiffusionSubstanceData[] GetNeighborsToDiffuseTo(int x, int y)
        {
            DiffusionSubstanceData[] result = new DiffusionSubstanceData[4];

            CellData lastCellState = _lastCellMatrixState[x, y];
            Direction connections = lastCellState.IsPlant() ? _simulationState.GetCellConnections(x, y) : Direction.None;

            if (x > 0)
            {
                result[0] = GetSubstaceToDiffuse(lastCellState, new(x - 1, y), connections.HasFlag(Direction.Left));
            }

            if (x < _xMax)
            {
                result[1] = GetSubstaceToDiffuse(lastCellState, new(x + 1, y), connections.HasFlag(Direction.Right));
            }

            if (y > 0)
            {
                result[2] = GetSubstaceToDiffuse(lastCellState, new(x, y - 1), connections.HasFlag(Direction.Down));
            }

            if (y < _yMax)
            {
                result[3] = GetSubstaceToDiffuse(lastCellState, new(x, y + 1), connections.HasFlag(Direction.Up));
            }

            if (lastCellState.Type == CellType.Water)
            {
                FillNutrientsDiffusionDataFromWaterCell(result, lastCellState);
            }

            return result;
        }

        private DiffusionSubstanceData GetSubstaceToDiffuse(CellData source, Vector2I destinationLocation, bool areCellsConnected)
        {
            CellData destination = _lastCellMatrixState[destinationLocation.X, destinationLocation.Y];

            if (source.Type == CellType.Soil && destination.Type == CellType.Soil)
            {
                return new()
                {
                    ShouldDiffuse = true,
                    Destination = destinationLocation,
                    Nutrients = source.NutrientContent > _simulationState.Parameters.NutrientsCriticalSoilDistribution ?
                        (source.NutrientContent - _simulationState.Parameters.NutrientsCriticalSoilDistribution) / 5 :
                        0,
                    Energy = source.EnergyContent / 5,
                    Waste = source.WasteContent / 5
                };
            }
            else if (source.Type == CellType.Soil && destination.Type == CellType.PlantRoot)
            {
                return new()
                {
                    ShouldDiffuse = true,
                    Destination = destinationLocation,
                    Nutrients = source.NutrientContent / 5
                };
            }
            else if (source.Type == CellType.Water && (destination.Type == CellType.Soil || destination.Type == CellType.PlantRoot))
            {
                return new()
                {
                    ShouldDiffuse = true,
                    Destination = destinationLocation
                };
            }
            else if (source.IsPlantCoreStructure() && destination.IsPlantCoreStructure() && areCellsConnected)
            {
                return new()
                {
                    ShouldDiffuse = true,
                    Destination = destinationLocation,
                    Nutrients = source.NutrientContent / 4,
                    Energy = source.EnergyContent / 4,
                    Waste = source.WasteContent / 4
                };
            }

            return new();
        }

        private void FillNutrientsDiffusionDataFromWaterCell(DiffusionSubstanceData[] neighbors, CellData source)
        {
            int remainingNutrients = source.NutrientContent;
            int remainingEnergy = source.EnergyContent;
            int remainingWaste = source.WasteContent;

            int neighborsToDiffuseToCount = neighbors.Count(x => x.ShouldDiffuse);
            int diffusedToNeighborNumber = 0;

            for (int i = 0; i < 4; ++i)
            {
                ref DiffusionSubstanceData x = ref neighbors[i];

                if (!x.ShouldDiffuse) { continue; }

                ++diffusedToNeighborNumber;

                if (diffusedToNeighborNumber == neighborsToDiffuseToCount)
                {
                    x.Nutrients = remainingNutrients;
                    x.Energy = remainingEnergy;
                    x.Waste = remainingWaste;
                }
                else
                {
                    x.Nutrients = source.NutrientContent / neighborsToDiffuseToCount;
                    x.Energy = source.EnergyContent / neighborsToDiffuseToCount;
                    x.Waste = source.WasteContent / neighborsToDiffuseToCount;

                    remainingNutrients -= x.Nutrients;
                    remainingEnergy -= x.Energy;
                    remainingWaste -= x.Waste;
                }
            }
        }

        private struct DiffusionSubstanceData
        {
            public bool ShouldDiffuse;

            public Vector2I Destination;

            public int Nutrients;

            public int Energy;

            public int Waste;
        }
    }
}
