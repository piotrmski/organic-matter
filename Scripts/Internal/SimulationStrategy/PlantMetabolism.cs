﻿using Organicmatter.Scripts.Internal.Model;

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
                if (!cell.IsPlant()) { return; }

                if (cell.Type == CellType.PlantGreen)
                {
                    TakeInLightEnergy(ref cell, x, y);

                    Photosynthesize(ref cell);
                }

                if (IsAtpLow(cell)) { Respire(ref cell); }

                ConsumeAtpOrDie(ref cell, x, y);
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

            if (cell.AccumulatedLightEnergy > _simulationState.Parameters.EnergyInGlucose) { cell.AccumulatedLightEnergy = _simulationState.Parameters.EnergyInGlucose; }
        }

        private void Photosynthesize(ref CellData cell)
        {
            if (cell.AccumulatedLightEnergy < _simulationState.Parameters.EnergyInGlucose ||
                cell.WaterMolecules < 6 ||
                _simulationState.CarbonDioxydeMolecules < 6) { return; }

            cell.GlucoseMolecules += 1;
            _simulationState.OxygenMolecules += 6;
            cell.AccumulatedLightEnergy -= _simulationState.Parameters.EnergyInGlucose;
            cell.WaterMolecules -= 6;
            _simulationState.CarbonDioxydeMolecules -= 6;
        }

        private void Respire(ref CellData cell)
        {
            if (cell.GlucoseMolecules < 1 ||
                _simulationState.OxygenMolecules < 6) { return; }

            cell.GlucoseMolecules -= 1;
            _simulationState.OxygenMolecules -= 6;
            cell.AtpEnergy += _simulationState.Parameters.EnergyInGlucose;
            cell.WaterMolecules += 6;
            _simulationState.CarbonDioxydeMolecules += 6;
        }

        private bool IsAtpLow(CellData cell)
        {
            int energyInGlucose = cell.GlucoseMolecules * _simulationState.Parameters.EnergyInGlucose;

            return energyInGlucose > cell.AtpEnergy;
        }

        private void ConsumeAtpOrDie(ref CellData cell, int x, int y)
        {
            if (cell.AtpEnergy > 0)
            {
                --cell.AtpEnergy;
            }
            else
            {
                cell.Type = CellType.Soil;
                _simulationState.RemoveCellConnections(x, y);
            }
        }
    }
}
