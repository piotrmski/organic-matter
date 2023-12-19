using Godot;
using Organicmatter.Scripts.Internal;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.RenderingStrategy;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;

public partial class SimViewport : TextureRect
{
	[Export]
	private int _spaceWidth = 100;

	[Export]
	private int _spaceHeight = 100;

	[Export]
	private Label _debugLabel1;

	[Export]
	private Label _debugLabel2;

	[Export]
	private ItemList _renderModeList;

	[Export]
	private ItemList _simulationSpeedList;

	private IRenderer _renderer;

	private ImageTexture _viewportTexture = new();

	private Simulation _simulation;

	private Vector2I? _hoveredCell;

	private Stopwatch _watch = new();

	public override void _Ready()
	{
		_simulation = new Simulation(_spaceWidth, _spaceHeight);

		_renderModeList.Select(0);
		
		UpdateRendererByListIndex(0);

		Texture = _viewportTexture;

		_renderModeList.ItemSelected += UpdateRendererByListIndex;

		_simulationSpeedList.Select(0);
	}

	public override void _PhysicsProcess(double delta)
	{
		AdvanceSimulationSelectedNumberOfTimes();

		_renderer.UpdateImage();

		_viewportTexture.Update(_renderer.RenderedImage);

		UpdateHoveredCellInfo();
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion mouseMotionEvent:
				if (_debugLabel1 == null) { return; }

				int x = (int)((mouseMotionEvent.Position.X - Position.X) * _spaceWidth / Size.X);
				int y = (int)((Size.Y - mouseMotionEvent.Position.Y - Position.Y) * _spaceHeight / Size.Y);

				if (x < 0 || x >= _spaceWidth || y < 0 || y >= _spaceHeight)
				{
					_hoveredCell = null;
				}
				else
				{
					_hoveredCell = new Vector2I(x, y);
				}

				UpdateHoveredCellInfo();

				return;
		}
	}

	private void AdvanceSimulationSelectedNumberOfTimes()
	{
		int multiplier = GetTickMultiplier();
		List<TimeSpan[]> executionTimes = new();

		_watch.Restart();
		for (int i = 0; i < multiplier; i++)
		{
			executionTimes.Add(_simulation.Advance());
		}
		_watch.Stop();

        TimeSpan[] totalExecutionTimes = executionTimes.Aggregate(new TimeSpan[executionTimes[0].Length], (acc, x) =>
		{
			for (int j = 0; j < x.Length; j++)
			{
				acc[j] += x[j];
			}

			return acc;
		});

		if (_debugLabel2 != null)
		{
			_debugLabel2.Text = $"Gravity: {totalExecutionTimes[0].Milliseconds} ms\n" +
				$"Diffusion: {totalExecutionTimes[1].Milliseconds} ms\n" +
				$"PlantGrowth: {totalExecutionTimes[2].Milliseconds} ms\n" +
				$"Lighting: {totalExecutionTimes[3].Milliseconds} ms\n" +
				$"PlantMetabolism: {totalExecutionTimes[4].Milliseconds} ms\n\n" +
				$"Total: {_watch.ElapsedMilliseconds} ms";
		}
	}

	private int GetTickMultiplier()
	{
		return _simulationSpeedList.GetSelectedItems().FirstOrDefault() switch
		{
			0 => 1,
			1 => 2,
			2 => 5,
			3 => 10,
			4 => 20,
			5 => 50,
			_ => 0
		};
	}

	private void UpdateHoveredCellInfo()
	{
		if (_debugLabel1 == null) { return; }

		int nutrientSum = 0;
		int energySum = 0;
		int celluloseSum = 0;
		int wasteSum = 0;

		_simulation.SimulationState.ForEachCell((ref CellData cell) =>
		{
			nutrientSum += cell.NutrientContent;
			energySum += cell.EnergyContent;
			wasteSum += cell.WasteContent;
			celluloseSum += cell.IsPlant() ? _simulation.SimulationState.Parameters.EnergyToSynthesizePlantCell : 0;
		});

		_debugLabel1.Text = $"Iteration = {_simulation.Iteration}\n\n" + 
			$"Nutrients in pure form = {nutrientSum}\n" +
			$"Nutrients as energy = {energySum}\n" +
			$"Nutrients as waste = {wasteSum}\n" +
			$"Nutrients as plant structure = {celluloseSum}\n\n" +
			$"Total nutrients = {nutrientSum + energySum + celluloseSum + wasteSum}\n\n";

		if (_hoveredCell != null)
		{
			_debugLabel1.Text += $"X = {_hoveredCell.Value.X} Y = {_hoveredCell.Value.Y}\n" +
				$"{_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y]}";
		}
	}

	private IRenderer GetRendererByListIndex(long listIndex)
	{
		return listIndex switch
		{
			0 => new DefaultRenderer(_simulation.SimulationState),
			1 => new NutrientsRenderer(_simulation.SimulationState),
			2 => new EnergyRenderer(_simulation.SimulationState),
			3 => new WasteRenderer(_simulation.SimulationState),
			4 => new PhotosynthesisRenderer(_simulation.SimulationState),
			_ => new AgeRenderer(_simulation.SimulationState),
		};
	}

	private void UpdateRendererByListIndex(long listIndex)
	{
		_renderer = GetRendererByListIndex(listIndex);

		_viewportTexture.SetImage(_renderer.RenderedImage);
	}
}
