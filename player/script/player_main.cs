using Godot;
using System;
using System.Collections.Generic;

public partial class player_main : CharacterBody2D
{
	// public
	public float CefFrictMain; // friction coefficient, kilogram per second
	public float SpeedProcess = 1.0f;
	public float HealPoint = 0;
	public float ManaPoint = 0;
	public Dictionary<string, float[]> Cooldown = new Dictionary<string, float[]>();
	public Dictionary<string, float[]> Duration = new Dictionary<string, float[]>();
	public Dictionary<string, float[]> Trigger = new Dictionary<string, float[]>();
	public Dictionary<string, bool> Condition = new Dictionary<string, bool>();

	// player physics
	private float masMain = 5; // mass of character, kilogram
	private float accMain0 = 2000; // acceleration in moving, pixel per second^2
	private float speedDash = 2000; // dash speed
	private float speedTerminal = 500; // max speed when just moving

	// shield physics
	private float angularvel0Shield = 4 * (float)Math.PI;
	private float velShield0 = 500;
	private float posShield0 = 100;
	private float accShield0 = 3000;
	private float velShieldPush = 1000;

	// variables
	private Vector2 foeForward = Vector2.Zero;
	private Vector2 foeBackward = Vector2.Zero;
	private Vector2 accMain = Vector2.Zero;
	private Vector2 velMain = Vector2.Zero;
	private Vector2 dirControl = Vector2.Zero;
	private Vector2 velShield = Vector2.Zero;

	private float deltaTime;

	public void ColliBody(Vector2 _position, float _momentumBullet, float _damage)
	{
		if (Condition["player_hit"] == false)
		{
			Rebound((Position - _position).Normalized(), _momentumBullet);
			if (_damage > 0)
			{
				Trigger["player_hit"][0] = 10;
				ActionUnderAttack(_damage);
			}
		}
	}

	public void ColliShield(Vector2 _position, float _momentumBullet)
	{
		Rebound((GetNode<CharacterBody2D>("posshield/body").GlobalPosition - _position).Normalized(), _momentumBullet);
		if (Cooldown["mana_get"][0] == 0)
		{
			Cooldown["mana_get"][0] = Cooldown["mana_get"][1];
			GetNode<AudioStreamPlayer>("sound/reflect").Play();
			ManaPoint += 1;
		}
	}

	private void Rebound(Vector2 _direction, float _momentumBullet)
	{
		if (Cooldown["rebound"][0] == 0)
		{
			Cooldown["rebound"][0] = Cooldown["rebound"][1];
			Vector2 _momentum = _direction * _momentumBullet;
			velMain += 2 * _momentum / masMain;
		}
	}

	private void CalCoefficientFriction()
	{
		CefFrictMain = accMain0 * masMain / speedTerminal;
	}

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
		// adjust Cooldown
		AdjustCountdownDic(Cooldown);
		AdjustCountdownDic(Duration);
		AdjustCountdownDic(Trigger);
	}

	private void InputMove()
	{
		dirControl = Vector2.Zero;
		if (Input.IsActionPressed("player_U"))
			dirControl.Y -= 1;
		if (Input.IsActionPressed("player_D"))
			dirControl.Y += 1;
		if (Input.IsActionPressed("player_L"))
			dirControl.X -= 1;
		if (Input.IsActionPressed("player_R"))
			dirControl.X += 1;
		dirControl = dirControl.Normalized();
	}

	private void AdjustMovingPhysics()
	{
		accMain = dirControl * accMain0;
		foeForward = masMain * accMain;
		foeBackward = CefFrictMain * velMain;
		velMain += foeForward / masMain * deltaTime;
		velMain -= foeBackward / masMain * deltaTime;
	}

	private void InputDash()
	{
		if (Input.IsActionJustPressed("player_dash"))
		{
			Trigger["player_dash"][0] = Trigger["player_dash"][1];
		}
	}

	private void ActionDash()
	{
		if (Trigger["player_dash"][0] != 0)
		{
			if (Cooldown["player_dash"][0] == 0)
			{
				Trigger["player_dash"][0] = 0;
				Cooldown["player_dash"][0] = Cooldown["player_dash"][1];
				velMain += speedDash * velMain.Normalized();
			}
		}
	}

	private void ActionUnderAttack(float _damage)
	{
		// Trigger check
		if (Trigger["player_hit"][0] != 0)
		{
			Trigger["player_hit"][0] = 0;
			Condition["player_hit"] = true;
			Duration["player_hit"][0] = Duration["player_hit"][1];
			GetNode<AudioStreamPlayer>("sound/hit").Play();
			CollisionLayer = 0;
			HealPoint -= _damage;
		}

		// blink body
		if (Condition["player_hit"] == true)
		{
			if (Duration["player_hit"][0] % 0.6 > 0.3)
				GetNode<Sprite2D>("posbody/Ball").Modulate = new Color(1, 1, 1, 0.5f);
			else
				GetNode<Sprite2D>("posbody/Ball").Modulate = new Color(1, 1, 1, 1);
		}

		// over
		if (Duration["player_hit"][0] == 0)
		{
			Condition["player_hit"] = false;
			CollisionLayer = 2;
		}
	}

	private void InputShieldPush()
	{
		if (Input.IsActionJustPressed("player_shieldpush"))
		{
			Trigger["shield_push"][0] = Trigger["shield_push"][1];
		}
	}

	private void ActionShieldPush()
	{
		if (Trigger["shield_push"][0] > 0)
		{
			if (Cooldown["shield_push"][0] == 0)
			{
				Trigger["shield_push"][0] = 0;
				Cooldown["shield_push"][0] = Cooldown["shield_push"][1];
				velShield.X = velShieldPush;
			}
		}

		if (velShield.X > 0)
		{
			float _deltaPositionX = velShield.X * deltaTime / 2;
			((CapsuleShape2D)GetNode<CollisionShape2D>("posshield/body/colshape").Shape).Radius = 8 + _deltaPositionX;
			GetNode<CollisionShape2D>("posshield/body/colshape").Position = new Vector2(-_deltaPositionX, 0);
		}
		else
		{
			((CapsuleShape2D)GetNode<CollisionShape2D>("posshield/body/colshape").Shape).Radius = 8;
			GetNode<CollisionShape2D>("posshield/body/colshape").Position = new Vector2(0, 0);
		}
	}

	private void AdjustShieldRotate()
	{
		float _angleNode = GetNode<Marker2D>("posshield").Rotation;
		Vector2 _dirNode = new Vector2((float)Math.Cos(_angleNode), (float)Math.Sin(_angleNode));
		Vector2 _dirMouse = (GetGlobalMousePosition() - Position).Normalized();
		float _outerProduction = _dirNode.X * _dirMouse.Y - _dirNode.Y * _dirMouse.X;
		float _dirAngular = _outerProduction / Math.Abs(_outerProduction);
		float _thetaDelta = (float)Math.Acos(_dirNode.Dot(_dirMouse));

		float _angularvel = _dirAngular * _thetaDelta / (float)Math.PI * angularvel0Shield;
		if (float.IsNaN(_angularvel))
			_angularvel = 0;
		GetNode<Marker2D>("posshield").Rotation = _angleNode + _angularvel * deltaTime;
		GetNode<CharacterBody2D>("posshield/body").Rotation = _angularvel / 20;
	}

	private void AdjustShieldReturn()
	{
		Vector2 _position = GetNode<CharacterBody2D>("posshield/body").Position;
		if (_position.X >= posShield0)
		{
			// adjust acc
			float _velStandardX = -velShield0 * (_position.X - posShield0) / posShield0;
			float _velDelta = _velStandardX - velShield.X;
			if (Math.Abs(_velDelta) > 1)
				velShield.X += accShield0 * _velDelta / Math.Abs(_velDelta) * deltaTime;

			// adjust vel
			GetNode<CharacterBody2D>("posshield/body").Position = _position + velShield * deltaTime;
		}
		else
		{
			_position.X = 100;
			GetNode<CharacterBody2D>("posshield/body").Position = _position;
		}
	}

	public override void _Ready()
	{
		Cooldown.Add("shield_push", new float[] {0, 1});
		Trigger.Add("shield_push", new float[] {0, 0.3f});
		Cooldown.Add("player_dash", new float[] {0, 1});
		Trigger.Add("player_dash", new float[] {0, 0.2f});
		Condition.Add("player_hit", false);
		Duration.Add("player_hit", new float[] {0, 3});
		Trigger.Add("player_hit", new float[] {0, 10});
		Cooldown.Add("rebound", new float[] {0, 0.2f});
		Cooldown.Add("mana_get", new float[] {0, 0.1f});

		CalCoefficientFriction();
	}

	public override void _PhysicsProcess(double delta)
	{
		InitialProcess(delta);

		// player section
		InputMove();
		AdjustMovingPhysics();
		InputDash();
		ActionDash();
		ActionUnderAttack(0);

		// shield sectionaa
		InputShieldPush();
		AdjustShieldRotate();
		AdjustShieldReturn();
		ActionShieldPush();

		// skill section

		// end
		Velocity = velMain * SpeedProcess;
		MoveAndSlide();
	}
}
