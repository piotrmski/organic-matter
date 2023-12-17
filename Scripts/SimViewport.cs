using Godot;
using Organicmatter.Scripts.Internal;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.RenderingStrategy;
using System.Linq;

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

	private System.Diagnostics.Stopwatch watch = new();

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
		watch.Restart();
		AdvanceSimulationSelectedNumberOfTimes();
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
		}
	}

	private void AdvanceSimulationSelectedNumberOfTimes()
	{
		int multiplier = _simulationSpeedList.GetSelectedItems().FirstOrDefault() switch
		{
			0 => 1,
			1 => 2,
			2 => 5,
			3 => 10,
			4 => 20,
			5 => 50,
			_ => 0
		};

		for (int i = 0; i < multiplier; i++) _simulation.Advance();
	}

	private void UpdateHoveredCellInfo()
	{
		if (_debugLabel1 == null) { return; }

		int mineralSum = 0;
		int energySum = 0;
        int celluloseSum = 0;
        int wasteSum = 0;

		_simulation.SimulationState.ForEachCell((ref CellData cell) =>
		{
            mineralSum += cell.MineralContent;
            energySum += cell.EnergyContent;
            wasteSum += cell.WasteContent;
			celluloseSum += cell.IsPlant() || cell.Type == CellType.Soil ? _simulation.SimulationState.Parameters.EnergyToSynthesizePlantCell : 0;
		});

		_debugLabel1.Text = $"Minerals in pure form = {mineralSum}\n" +
			$"Minerals as energy = {energySum}\n" +
			$"Minerals as waste = {wasteSum}\n" +
			$"Minerals as plant structure = {celluloseSum}\n\n" +
			$"Total minerals = {mineralSum + energySum + celluloseSum + wasteSum}\n\n";

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
			1 => new MineralsRenderer(_simulation.SimulationState),
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
