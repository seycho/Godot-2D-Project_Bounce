using Godot;
using System;
using System.Collections.Generic;

public partial class player_main : CharacterBody2D
{
	// player physics
	public float masMain = 5; // mass of character, kilogram
	public float accMain0 = 2000; // acceleration in moving, pixel per second^2
	public float speedDash = 2000; // dash speed
	public float speedTerminal = 500; // max speed when just moving

	// shield physics
	private float angularvel0Shield = 4 * (float)Math.PI;
	private float velShield0 = 500;
	private float posShieldDefault = 100;
	private float accShield0 = 3000;
	private float velShieldPush = 1000;

	private Dictionary<string, float> cooldown = new Dictionary<string, float>();
	private Dictionary<string, float> cooldownDefault = new Dictionary<string, float>();
	private Dictionary<string, float> duration = new Dictionary<string, float>();
	private Dictionary<string, float> durationDefault = new Dictionary<string, float>();
	private Dictionary<string, bool> condition = new Dictionary<string, bool>();
	private Vector2 foeForward = Vector2.Zero;
	private Vector2 foeBackward = Vector2.Zero;
	private Vector2 accMain = Vector2.Zero;
	private Vector2 velMain = Vector2.Zero;
	private Vector2 dirControl = Vector2.Zero;
	private Vector2 velShield = Vector2.Zero;
	public float cefFrictMain; // friction coefficient, (kilogram per second)

	private float deltaTime;

	private void AdjustCountdownDic(Dictionary<string, float> countdownDic)
	{
		foreach (string key in countdownDic.Keys)
		{
			if (countdownDic[key] > 0)
				countdownDic[key] -= deltaTime;
			else
				countdownDic[key] = 0;
		}
	}

	private void InitialProcess()
	{
		// adjust cooldown
		AdjustCountdownDic(cooldown);
		AdjustCountdownDic(duration);
		
		if (duration["player_hit"] == 0)
		{
			GetNode<Area2D>("reghit").CollisionMask += 2;
			condition["player_hit"] = false;
		}
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

	private void InputDash()
	{
		if (Input.IsActionJustPressed("player_dash"))
		{
			if (cooldown["player_dash"] == 0)
			{
				cooldown["player_dash"] = cooldownDefault["player_dash"];
				velMain += speedDash * velMain.Normalized();
			}
		}
	}

	private void InputPush()
	{
		if (Input.IsActionJustPressed("player_shieldpush"))
		{
			if ((GetNode<CharacterBody2D>("posshield/body").Position.X < posShieldDefault+1) & cooldown["shield_push"] == 0)
			{
				cooldown["shield_push"] = cooldownDefault["shield_push"];
				velShield.X += velShieldPush;
			}
		}
	}

	private void BlinkBody()
	{
		if (condition["player_hit"] == true)
		{
			if (duration["player_hit"] % 0.6 > 0.3)
				GetNode<Sprite2D>("posbody/Ball").Modulate = new Color(1, 1, 1, 0.5f);
			else
				GetNode<Sprite2D>("posbody/Ball").Modulate = new Color(1, 1, 1, 1);
		}
	}

	private void HitPlayer()
	{
		if (condition["player_hit"] == false)
		{
			foreach (var _body in GetNode<Area2D>("reghit").GetOverlappingBodies())
			{
				Vector2 _momentum = Vector2.Zero;
				float _damage =0;
				if (_body.IsInGroup("bullet"))
				{
					RigidBody2D _bulletBody = (RigidBody2D)_body;
					_momentum += (Position - _bulletBody.Position).Normalized() * _bulletBody.Mass * _bulletBody.LinearVelocity.Length();
					_damage += 1;
				}
				if (_damage > 0)
				{
					GetNode<Area2D>("reghit").CollisionMask -= 2;
					condition["player_hit"] = true;
					duration["player_hit"] = durationDefault["player_hit"];
					velMain += 2 * _momentum / masMain;
				}
			}
		}
	}

	private void HitShield()
	{
		foreach (var _body in GetNode<Area2D>("posshield/body/areahit").GetOverlappingBodies())
		{
			Vector2 _momentum = Vector2.Zero;
			if (_body.IsInGroup("bullet"))
			{
				RigidBody2D _bulletBody = (RigidBody2D)_body;
				_momentum += (Position - _bulletBody.Position).Normalized() * _bulletBody.Mass * _bulletBody.LinearVelocity.Length();
			}
			if (_momentum.Length() != 0.0f)
			{
				velMain += 2 * _momentum / masMain;
			}
		}
	}

	private void RotateShield()
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

	private void ReturnShield()
	{
		Vector2 _position = GetNode<CharacterBody2D>("posshield/body").Position;
		// adjust acc
		float _velStandardX = -velShield0 * (_position.X - posShieldDefault) / posShieldDefault;
		float _velDelta = _velStandardX - velShield.X;
		if (Math.Abs(_velDelta) > 1)
			velShield.X += accShield0 * _velDelta / Math.Abs(_velDelta) * deltaTime;

		// adjust vel
		GetNode<CharacterBody2D>("posshield/body").Position = _position + velShield * deltaTime;
		if (_position.X < 50)
		{
			_position.X = 50;
			GetNode<CharacterBody2D>("posshield/body").Position = _position;
			velShield.X = 100;
		}
	}

	private void CalCoefficientFriction()
	{
		cefFrictMain = accMain0 * masMain / speedTerminal;
	}

	private void CalMovingPhysics()
	{
		accMain = dirControl * accMain0;
		foeForward = masMain * accMain;
		foeBackward = cefFrictMain * velMain;
		velMain += foeForward / masMain * deltaTime;
		velMain -= foeBackward / masMain * deltaTime;
	}

	public override void _Ready()
	{
		cooldown.Add("shield_push", 0);
		cooldown.Add("player_dash", 0);
		cooldownDefault.Add("shield_push", 1);
		cooldownDefault.Add("player_dash", 1);
		duration.Add("player_hit", 0);
		durationDefault.Add("player_hit", 3);
		condition.Add("player_hit", false);

		CalCoefficientFriction();
		//CollisionLayer = 0;
		//CollisionMask = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		deltaTime = (float)delta;
		InitialProcess();

		InputMove();
		InputDash();
		InputPush();

		HitPlayer();
		BlinkBody();

		ReturnShield();
		RotateShield();
		HitShield();

		CalMovingPhysics();
		MoveAndCollide(velMain * deltaTime);
	}
}
