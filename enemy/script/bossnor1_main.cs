using Godot;
using System;
using System.Collections.Generic;

public partial class bossnor1_main : RigidBody2D
{
	// public
	public float SpeedProcess = 1.0f;
	public float HealPoint = 0;
	public float ManaPoint = 0;
	public Vector2 Velocity = new Vector2(0, 0);
	public Vector2 posMoveCen = new Vector2(0, 0);
	public Dictionary<string, float[]> Cooldown = new Dictionary<string, float[]>();
	public Dictionary<string, float[]> Duration = new Dictionary<string, float[]>();
	public Dictionary<string, float[]> Trigger = new Dictionary<string, float[]>();
	public Dictionary<string, bool> Condition = new Dictionary<string, bool>();

	private Random rand = new Random();
	private Vector2 posLimRandom = new Vector2(768/2, 768/2);
	private Vector2 posMoveAim = new Vector2(0, 0);
	private Vector2 posMoveDes = new Vector2(0, 0);

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

	private void InitialProcess(double delta)
	{
		deltaTime = (float)delta * SpeedProcess;
		AdjustCountdownDic(Cooldown);
		AdjustCountdownDic(Duration);
		ManaPoint += 1 * deltaTime;
	}

	private double ModelDiffusion(double x,double sigma)
	{
		double _result = 0;
		if (Math.Abs(x) <= sigma)
		{
			double _fun = Math.Exp(Math.Pow(Math.Pow(sigma,2)-Math.Pow((double)x,2),0.5)/sigma)-1;
			double _nor = Math.Exp(Math.Pow(Math.Pow(sigma,2),0.5)/sigma)-1;
			_result = _fun/_nor;
		}
		return _result;
	}

	private float GetNextPos(double posSta, double sigma)
	{
		double _x = posSta;
		double _xNext;
		do {
			_xNext = _x + (rand.NextDouble()-0.5)*2*sigma*2;
		} while (rand.NextDouble() > ModelDiffusion(_xNext, sigma));
		return (float)_xNext;
	}

	private void ActionMoveRandom()
	{
		if (Cooldown["random_aim"][0] == 0)
		{
			float _xAim = GetNextPos(posMoveAim.X, posLimRandom.X);
			float _yAim = GetNextPos(posMoveAim.Y, posLimRandom.Y);
			posMoveAim = new Vector2(_xAim, _yAim);
			Cooldown["random_aim"][0] = 0.5f+(float)rand.NextDouble()*2.0f;
			posMoveDes = posMoveCen + posMoveAim;
		}
	}

	private void ActionMoveDestination()
	{
		Vector2 _forward = posMoveDes - Position;
		Vector2 _directionMove = _forward.Normalized();
		Velocity = _directionMove * _forward.DistanceTo(Vector2.Zero);
		Position = Position + Velocity * deltaTime;
	}

	private void ActionUnderAttack()
	{
		if (Trigger["enemy_hit"][0] != 0)
		{
			Trigger["enemy_hit"][0] = 0;
			Condition["enemy_hit"] = true;
			Duration["enemy_hit"][0] = Duration["enemy_hit"][1];
			GetNode<AudioStreamPlayer>("sound/hit").Play();
			HealPoint -= 1;
		}

		if (Condition["enemy_hit"] == true)
		{
			float _DurationTimeRatio = Duration["enemy_hit"][0] / Duration["enemy_hit"][1];
			float _alpha = 0.5f * _DurationTimeRatio;
			GetNode<Sprite2D>("posimg/BallCyan").Modulate = new Color(1, 1, 1, _alpha);
			float shakeX = (float)rand.NextDouble() * _DurationTimeRatio * 10;
			float shakeY = (float)rand.NextDouble() * _DurationTimeRatio * 10;
			GetNode<Marker2D>("posimg").Position = new Vector2(shakeX, shakeY);
		}

		if (Duration["enemy_hit"][0] == 0)
		{
			Condition["enemy_hit"] = false;
		}
	}

	private void ColliEnemy()
	{
		foreach (var _area in GetNode<Area2D>("areahit").GetOverlappingAreas())
		{
			float _damage = 0;
			RigidBody2D _bulletBody = _area.GetOwner<RigidBody2D>();
			if (_bulletBody.IsInGroup("player"))
			{
				_bulletBody.GetNode<bullet_main>(".").HitEnemy();
				_damage += 1;
			}
			if (_damage > 0)
			{
				Trigger["enemy_hit"][0] = 10;
			}
		}
	}

	public override void _Ready()
	{
		Cooldown.Add("random_aim", new float[] {0, 0});
		Condition.Add("enemy_hit", false);
		Duration.Add("enemy_hit", new float[] {0, 1});
		Trigger.Add("enemy_hit", new float[] {0, 10});
	}

	public override void _Process(double delta)
	{
		InitialProcess(delta);
		ActionMoveRandom();
		ActionMoveDestination();

		ColliEnemy();
		ActionUnderAttack();
	}
}
