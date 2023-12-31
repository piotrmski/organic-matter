﻿using Godot;
using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;
using System;

namespace Organicmatter.Scripts.Internal.SimulationStep
{
    internal class FruitDevelopment : ISimulationStep
    {
        private SimulationState _simulationState;

        private SpecialCoordinates _specialCoordinates;

        public FruitDevelopment(SimulationState simulationState, SpecialCoordinates specialCoordinates)
        {
            _simulationState = simulationState;

            _specialCoordinates = specialCoordinates;
        }

        public void Advance()
        {
            _specialCoordinates.Fruits.Delete(coordinates =>
            {
                if (IsFruitDetatched(coordinates) || IsFruitDeveloped(coordinates))
                {
                    ConvertToSeed(coordinates);

                    return true;
                }

                return false;
            });
        }

        private void ConvertToSeed(Vector2I coordinates)
        {
            _simulationState.CellMatrix[coordinates.X, coordinates.Y].Type = CellType.PlantSeed;
            _simulationState.CellMatrix[coordinates.X, coordinates.Y].TicksSinceSynthesis = 0;
            _simulationState.CellMatrix[coordinates.X, coordinates.Y].GrowthOrigin = Direction.None;
            _simulationState.RemoveCellConnections(coordinates.X, coordinates.Y);
            _specialCoordinates.Seeds.Add(coordinates);
        }

        private bool IsFruitDeveloped(Vector2I coordinates)
        {
            return _simulationState.CellMatrix[coordinates.X, coordinates.Y].EnergyContent >= _simulationState.Parameters.EnergyInPlantSeed;
        }

        private bool IsFruitDetatched(Vector2I coordinates)
        {
            return _simulationState.GetCellConnections(coordinates.X, coordinates.Y) == Direction.None;
        }
    }
}
