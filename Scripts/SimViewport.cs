using Godot;
using Organicmatter.Scripts.Internal;
using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;

public partial class SimViewport : TextureRect
{
	[Export]
	private int _spaceWidth = 100;

	[Export]
	private int _spaceHeight = 100;

	[Export]
	private double _simulationStepLength = .05;

	[Export]
	private Label _debugLabel1;

	[Export]
	private Label _debugLabel2;

	private Renderer _renderer;

	private ImageTexture _viewportTexture = new();

	private Simulation _simulation;

	private double _timeSinceLastSimulationStep = 0;

	private Vector2I? _hoveredCell;

	private System.Diagnostics.Stopwatch watch = new();

	private AirInSoilSearch _airInSoilSearch; // todo temporary

	public override void _Ready()
	{
		_simulation = new Simulation(_spaceWidth, _spaceHeight);

        _renderer = new Renderer(_simulation.SimulationState);

        _viewportTexture.SetImage(_renderer.RenderedImage);

		Texture = _viewportTexture;

		_airInSoilSearch = new AirInSoilSearch(_simulation.SimulationState); // todo temporary

	}

	public override void _PhysicsProcess(double delta)
	{
		_timeSinceLastSimulationStep += delta;

		if (_timeSinceLastSimulationStep < _simulationStepLength) { return; }

		_timeSinceLastSimulationStep -= _simulationStepLength;

		watch.Restart();
		_simulation.Advance();
		watch.Stop();

		if (_debugLabel2 != null) _debugLabel2.Text = $"{watch.ElapsedMilliseconds} ms";

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

			case InputEventMouseButton mouseButtonEvent: // Debug interaction
				if (_hoveredCell == null || !mouseButtonEvent.IsPressed()) { return; }

				CellData newCell = new()
				{
					Type = mouseButtonEvent.ButtonIndex == MouseButton.Left ? CellType.PlantGreen : CellType.PlantRoot
				};

				if (_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y].Type == CellType.Air)
				{
					_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y] = newCell;
					return;
				}

				if (_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y].Type == CellType.Soil)
				{
					Vector2I? airLocation = _airInSoilSearch.FindNearestAir(_hoveredCell.Value.X, _hoveredCell.Value.Y);

					if (airLocation == null) { return; }

					(_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y], _simulation.SimulationState.CellMatrix[airLocation.Value.X, airLocation.Value.Y]) =
						(_simulation.SimulationState.CellMatrix[airLocation.Value.X, airLocation.Value.Y], _simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y]);

					_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y] = newCell;
				}

				return;
		}
	}

	private void UpdateHoveredCellInfo()
	{
		if (_debugLabel1 == null) { return; }

		if (_hoveredCell == null)
		{
			int sum = 0;
			_simulation.SimulationState.ForEachCell((ref CellData cell) => sum += cell.WaterMolecules);

			_debugLabel1.Text = $"Total moisture content = {sum}";

			return;
		}

		_debugLabel1.Text = $"X = {_hoveredCell.Value.X} Y = {_hoveredCell.Value.Y}\n" +
			$"{_simulation.SimulationState.CellMatrix[_hoveredCell.Value.X, _hoveredCell.Value.Y]}";
	}
}
