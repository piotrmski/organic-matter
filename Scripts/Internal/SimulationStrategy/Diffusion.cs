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

        private SimulationState _simulationState;

        private CellData[,] _lastCellMatrixState;

        public Diffusion(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _lastCellMatrixState = new CellData[_spaceWidth, _spaceHeight];
        }

        public void Advance()
        {
            Array.Copy(_simulationState.CellMatrix, _lastCellMatrixState, _spaceWidth * _spaceHeight);

            _simulationState.ForEachCell((ref CellData cell) => cell.WaterMolecules = 0);

            _simulationState.ForEachCell((ref CellData cell, int x, int y) =>
            {
                if (!cell.CanDiffuse()) { return; }

                List<Vector2I> neighborsToDiffuseTo = GetNeighborsToDiffuseTo(x, y);

                if (!neighborsToDiffuseTo.Any())
                {
                    cell.WaterMolecules += _lastCellMatrixState[x, y].WaterMolecules;
                    return;
                }

                int moistureToDiffuseOutToANeighbor = _lastCellMatrixState[x, y].WaterMolecules / 5;
                int moistureToRemain = _lastCellMatrixState[x, y].WaterMolecules - moistureToDiffuseOutToANeighbor * neighborsToDiffuseTo.Count;

                neighborsToDiffuseTo.ForEach(x => _simulationState.CellMatrix[x.X, x.Y].WaterMolecules += moistureToDiffuseOutToANeighbor);
                cell.WaterMolecules += moistureToRemain;
            });
        }

        private List<Vector2I> GetNeighborsToDiffuseTo(int x, int y)
        {
            List<Vector2I> result = new();

            CellData lastCellState = _lastCellMatrixState[x, y];
            Direction connections = _simulationState.GetCellConnections(x, y);

            if (x > 0 && lastCellState.CanDiffuseTo(_lastCellMatrixState[x - 1, y], connections.HasFlag(Direction.Left)))
            {
                result.Add(new(x - 1, y));
            }

            if (x < _simulationState.CellMatrix.GetLength(0) - 1 && lastCellState.CanDiffuseTo(_lastCellMatrixState[x + 1, y], connections.HasFlag(Direction.Right)))
            {
                result.Add(new(x + 1, y));
            }

            if (y > 0 && lastCellState.CanDiffuseTo(_lastCellMatrixState[x, y - 1], connections.HasFlag(Direction.Bottom)))
            {
                result.Add(new(x, y - 1));
            }

            if (y < _simulationState.CellMatrix.GetLength(1) - 1 && lastCellState.CanDiffuseTo(_lastCellMatrixState[x, y + 1], connections.HasFlag(Direction.Top)))
            {
                result.Add(new(x, y + 1));
            }

            return result;
        }
    }
}
