using Godot;
using System;
using System.Collections.Generic;

public partial class left_scroll_random_main : Node2D
{
	private float posCurrentX = 0;
	private float posLimLefX = -1366/2;
	private float posLimRigX = 1366/2;
	private float posLimTopY = -768/2;
	private float posLimBotY = 768/2;
	private int[] randWallXRange = new int[2]{50, 200};
	private int[] randWallYRange = new int[2]{1, 100};
	private int[] randAsteroidTime = new int[2]{1, 3};

	private Random rand = new Random();
	private List<Vector2> polygonWallTop = new List<Vector2>();
	private List<Vector2> polygonWallBot = new List<Vector2>();
	private Dictionary<string, float[]> cooldown = new Dictionary<string, float[]>();

	private PackedScene originalAsteroid;
	private float posGeneWallTopX;
	private float posGeneWallBotX;
	private float deltaTime;

	private void AdjustCountdownDic(Dictionary<string, float[]> countdownDic)
	{
		foreach (string key in countdownDic.Keys)
		{
			if (countdownDic[key][0] > 0.0f)
				countdownDic[key][0] -= deltaTime;
			else
				countdownDic[key][0] = 0.0f;
		}
	}

	private void MoveLeftArea()
	{
		posCurrentX += -100f*deltaTime;
		GetNode<Node2D>("playground").Position = new Vector2(posCurrentX, 0);
	}

	private void SpawnWall()
	{
		if (posGeneWallTopX > posCurrentX+posLimLefX)
		{
			int _nextX = rand.Next(randWallXRange[0], randWallXRange[1]);
			int _nextY = rand.Next(randWallYRange[0], randWallYRange[1]);
			posGeneWallTopX -= _nextX;
			polygonWallTop.Add(new Vector2(posGeneWallTopX, posLimTopY+_nextY));

			if (polygonWallTop.Count >= 2)
			{
				Vector2[] _pointPolygon = new Vector2[polygonWallTop.Count+2];
				for (int i=0; i<polygonWallTop.Count; i++)
				{
					_pointPolygon[i] = polygonWallTop[i];
				}
				_pointPolygon[polygonWallTop.Count] = new Vector2(polygonWallTop[polygonWallTop.Count-1].X, posLimTopY);
				_pointPolygon[polygonWallTop.Count+1] = new Vector2(polygonWallTop[0].X, posLimTopY);
				GetNode<CollisionPolygon2D>("wall/colpolytop").Polygon = _pointPolygon;
				GetNode<Polygon2D>("wall/imgpolytop").Polygon = _pointPolygon;
			}
		}

		if (polygonWallTop[0].X > posCurrentX+posLimRigX+randWallXRange[1])
			polygonWallTop.RemoveAt(0);

		if (posGeneWallBotX > posCurrentX+posLimLefX)
		{
			int _nextX = rand.Next(randWallXRange[0], randWallXRange[1]);
			int _nextY = rand.Next(randWallYRange[0], randWallYRange[1]);
			posGeneWallBotX -= _nextX;
			polygonWallBot.Add(new Vector2(posGeneWallBotX, posLimBotY-_nextY));

			if (polygonWallBot.Count >= 2)
			{
				Vector2[] _pointPolygon = new Vector2[polygonWallBot.Count+2];
				for (int i=0; i<polygonWallBot.Count; i++)
				{
					_pointPolygon[i] = polygonWallBot[i];
				}
				_pointPolygon[polygonWallBot.Count] = new Vector2(polygonWallBot[polygonWallBot.Count-1].X, posLimBotY);
				_pointPolygon[polygonWallBot.Count+1] = new Vector2(polygonWallBot[0].X, posLimBotY);
				GetNode<CollisionPolygon2D>("wall/colpolybot").Polygon = _pointPolygon;
				GetNode<Polygon2D>("wall/imgpolybot").Polygon = _pointPolygon;
			}
		}

		if (polygonWallBot[0].X > posCurrentX+posLimRigX+randWallXRange[1])
			polygonWallBot.RemoveAt(0);
	}

	private void SpawnAstronoid()
	{
		if (cooldown["asteroid_spawn"][0] == 0)
		{
			cooldown["asteroid_spawn"][0] = rand.Next(randAsteroidTime[0], randAsteroidTime[1]);
			StaticBody2D _node = (StaticBody2D)(originalAsteroid.Instantiate());
			_node.Position = new Vector2(posCurrentX+posLimLefX-200, rand.Next((int)posLimTopY,(int)posLimBotY));
			GetNode<Node2D>("asteroidpak").AddChild(_node);
		}
	}

	private void RemoveAsteroid()
	{
		foreach (StaticBody2D _node in GetNode<Node2D>("asteroidpak").GetChildren())
		{
			if (GetNode<Area2D>("playground/areaend").OverlapsBody(_node))
				_node.QueueFree();
		}
	}

	private void RemoveBullet()
	{
		foreach (var _node in GetNode<Area2D>("playground/areaend").GetOverlappingBodies())
		{
			if (_node.IsInGroup("bullet"))
				_node.QueueFree();
		}
	}

	public override void _Ready()
	{
		posGeneWallTopX = posLimRigX-1;
		polygonWallTop.Add(new Vector2(posGeneWallTopX, posLimTopY+1));
		posGeneWallBotX = posLimRigX-1;
		polygonWallBot.Add(new Vector2(posGeneWallBotX, posLimBotY-1));
		cooldown.Add("asteroid_spawn", new float[] {0, 0});
		originalAsteroid = GD.Load<PackedScene>("res://terrain/asteroid.tscn");
	}

	public override void _Process(double delta)
	{
		deltaTime = (float)delta;
		AdjustCountdownDic(cooldown);
		MoveLeftArea();
		SpawnWall();
		SpawnAstronoid();
		RemoveAsteroid();
		RemoveBullet();
	}
}
