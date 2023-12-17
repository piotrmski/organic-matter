using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.SimulationStrategy
{
    internal class PlantMetabolism : ISimulationStrategy
    {
        private int _spaceWidth;

        private int _spaceHeight;

        private int _xMax;

        private int _yMax;

        private SimulationState _simulationState;

        public PlantMetabolism(SimulationState simulationState)
        {
            _simulationState = simulationState;

            _spaceWidth = simulationState.CellMatrix.GetLength(0);

            _spaceHeight = simulationState.CellMatrix.GetLength(1);

            _xMax = _spaceWidth - 1;

            _yMax = _spaceHeight - 1;
        }

        public void Advance()
        {
            _simulationState.ForEachCell((ref CellData cell, int x, int y) =>
            {
                IncrementCounters(ref cell);

                if (!cell.IsPlant()) { return; }

                if (cell.Type == CellType.PlantGreen)
                {
                    TakeInLightEnergy(ref cell, x, y);

                    Photosynthesize(ref cell);
                }

                ConsumeEnergyOrDie(ref cell, x, y);
            });
        }

        private void TakeInLightEnergy(ref CellData cell, int x, int y)
        {
            if (x == 0) { cell.AccumulatedLightEnergy += _simulationState.Parameters.DirectLightEnergy; }
            else if (_simulationState.CellMatrix[x - 1, y].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x - 1, y].LightEnergy; }

            if (x == _xMax) { cell.AccumulatedLightEnergy += _simulationState.Parameters.DirectLightEnergy; }
            else if (_simulationState.CellMatrix[x + 1, y].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x + 1, y].LightEnergy; }

            if (y > 0 && _simulationState.CellMatrix[x, y - 1].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x, y - 1].LightEnergy; }

            if (y == _yMax) { cell.AccumulatedLightEnergy += _simulationState.Parameters.DirectLightEnergy; }
            else if (_simulationState.CellMatrix[x, y + 1].Type == CellType.Air) { cell.AccumulatedLightEnergy += _simulationState.CellMatrix[x, y + 1].LightEnergy; }

            if (cell.AccumulatedLightEnergy > _simulationState.Parameters.LightToConvertMineralToEnergy) { cell.AccumulatedLightEnergy = _simulationState.Parameters.LightToConvertMineralToEnergy; }
        }

        private void Photosynthesize(ref CellData cell)
        {
            if (!AreConditionsMetForPhotosynthesis(cell)) { return; }

            cell.EnergyContent += 1;
            cell.AccumulatedLightEnergy -= _simulationState.Parameters.LightToConvertMineralToEnergy;
            cell.MineralContent -= 1;

            cell.TicksSinceLastPhotosynthesis = 0;
        }

        private bool AreConditionsMetForPhotosynthesis(CellData cell)
        {
            return IsPhotosynthesisPossible(cell) && IsPhotosynthesisDesired(cell);
        }

        private bool IsPhotosynthesisPossible(CellData cell)
        {
            return cell.AccumulatedLightEnergy >= _simulationState.Parameters.LightToConvertMineralToEnergy &&
                cell.MineralContent >= 1;
        }

        private bool IsPhotosynthesisDesired(CellData cell)
        {
            return true;
        }

        private void ConsumeEnergyOrDie(ref CellData cell, int x, int y)
        {
            //if (cell.AtpEnergy > 0)
            //{
            //    cell.AtpEnergy -= _simulationState.Parameters.PlantEnergyConsumptionPerTick;
            //}
            //else
            //{
            //    cell.Type = CellType.Soil;
            //    _simulationState.RemoveCellConnections(x, y);
            //}
        }

        private static void IncrementCounters(ref CellData cell)
        {
            if (cell.TicksSinceLastPhotosynthesis < int.MaxValue) cell.TicksSinceLastPhotosynthesis += 1;
            if (cell.TicksSinceSynthesis < int.MaxValue) cell.TicksSinceSynthesis += 1;
        }
    }
}
