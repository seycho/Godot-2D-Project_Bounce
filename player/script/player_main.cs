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
	private float posShield0 = 100;
	private float accShield0 = 3000;
	private float velShieldPush = 1000;

	// variables
	private Dictionary<string, float[]> cooldown = new Dictionary<string, float[]>();
	private Dictionary<string, float[]> duration = new Dictionary<string, float[]>();
	private Dictionary<string, float[]> trigger = new Dictionary<string, float[]>();
	private Dictionary<string, bool> condition = new Dictionary<string, bool>();
	private Vector2 foeForward = Vector2.Zero;
	private Vector2 foeBackward = Vector2.Zero;
	private Vector2 accMain = Vector2.Zero;
	private Vector2 velMain = Vector2.Zero;
	private Vector2 dirControl = Vector2.Zero;
	private Vector2 velShield = Vector2.Zero;
	public float cefFrictMain; // friction coefficient, kilogram per second
	public float damageTotal = 0;

	private float deltaTime;
	
	private List<string> logVelocity = new List<string>();

	public void SaveLogVelocity()
	{
		string _line = velMain.ToString()+"\t"+deltaTime.ToString()+"\t"+accMain.ToString();
		logVelocity.Add(_line);

		if (Input.IsActionJustPressed("save_log"))
			System.IO.File.WriteAllLinesAsync(@"./velocity_log.txt", logVelocity);
	}

	private void CalCoefficientFriction()
	{
		cefFrictMain = accMain0 * masMain / speedTerminal;
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
		deltaTime = (float)delta;
		// adjust cooldown
		AdjustCountdownDic(cooldown);
		AdjustCountdownDic(duration);
		AdjustCountdownDic(trigger);
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
		foeBackward = cefFrictMain * velMain;
		velMain += foeForward / masMain * deltaTime;
		velMain -= foeBackward / masMain * deltaTime;
	}

	private void InputDash()
	{
		if (Input.IsActionJustPressed("player_dash"))
		{
			trigger["player_dash"][0] = trigger["player_dash"][1];
		}
	}

	private void ActionDash()
	{
		if (trigger["player_dash"][0] != 0)
		{
			if (cooldown["player_dash"][0] == 0)
			{
				trigger["player_dash"][0] = 0;
				cooldown["player_dash"][0] = cooldown["player_dash"][1];
				velMain += speedDash * velMain.Normalized();
			}
		}
	}

	private void ActionUnderAttack()
	{
		// trigger check
		if (trigger["player_hit"][0] != 0)
		{
			trigger["player_hit"][0] = 0;
			condition["player_hit"] = true;
			duration["player_hit"][0] = duration["player_hit"][1];
			GetNode<AudioStreamPlayer>("sound/hit").Play();
			GetNode<Area2D>("reghit").CollisionMask -= 2;
		}

		// blink body
		if (condition["player_hit"] == true)
		{
			if (duration["player_hit"][0] % 0.6 > 0.3)
				GetNode<Sprite2D>("posbody/Ball").Modulate = new Color(1, 1, 1, 0.5f);
			else
				GetNode<Sprite2D>("posbody/Ball").Modulate = new Color(1, 1, 1, 1);
		}

		// over
		if (duration["player_hit"][0] == 0)
		{
			condition["player_hit"] = false;
			GetNode<Area2D>("reghit").CollisionMask += 2;
		}
	}

	private void ColliPlayer()
	{
		if (condition["player_hit"] == false)
		{
			foreach (var _area in GetNode<Area2D>("reghit").GetOverlappingAreas())
			{
				Vector2 _momentum = Vector2.Zero;
				float _damage = 0;
				RigidBody2D _body = _area.GetOwner<RigidBody2D>();
				if (_body.IsInGroup("enemy"))
				{
					if (_body.IsInGroup("bullet"))
					{
						_momentum += (Position - _body.Position).Normalized() * _body.Mass * _body.LinearVelocity.Length();
						_body.GetNode<bullet_main>(".").HitPlayer();
						_damage += 1;
					}
					if (_body.IsInGroup("boss"))
					{
						_momentum += (Position - _body.Position).Normalized() * 5000;
						_damage += 5;
					}
				}
				if (_damage > 0)
				{
					damageTotal += _damage;
					trigger["player_hit"][0] = 10;
					velMain += 2 * _momentum / masMain;
				}
			}
		}
	}

	private void InputShieldPush()
	{
		if (Input.IsActionJustPressed("player_shieldpush"))
		{
			trigger["shield_push"][0] = trigger["shield_push"][1];
		}
	}

	private void ActionShieldPush()
	{
		if (trigger["shield_push"][0] != 0)
		{
			if ((GetNode<CharacterBody2D>("posshield/body").Position.X < posShield0+1) & cooldown["shield_push"][0] == 0)
			{
				trigger["shield_push"][0] = 0;
				cooldown["shield_push"][0] = cooldown["shield_push"][1];
				velShield.X += velShieldPush;
			}
		}

		if (velShield.X > 0)
		{
			float _deltaPositionX = velShield.X * deltaTime / 2;
			((CapsuleShape2D)GetNode<CollisionShape2D>("posshield/body/colshape").Shape).Radius = 8 + _deltaPositionX;
			((CapsuleShape2D)GetNode<CollisionShape2D>("posshield/body/areahit/colshape").Shape).Radius = 8 + _deltaPositionX;
			GetNode<CollisionShape2D>("posshield/body/colshape").Position = new Vector2(-_deltaPositionX, 0);
			GetNode<CollisionShape2D>("posshield/body/areahit/colshape").Position = new Vector2(-_deltaPositionX, 0);
		}
		else
		{
			((CapsuleShape2D)GetNode<CollisionShape2D>("posshield/body/colshape").Shape).Radius = 8;
			((CapsuleShape2D)GetNode<CollisionShape2D>("posshield/body/areahit/colshape").Shape).Radius = 8;
			GetNode<CollisionShape2D>("posshield/body/colshape").Position = new Vector2(0, 0);
			GetNode<CollisionShape2D>("posshield/body/areahit/colshape").Position = new Vector2(0, 0);
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
		// adjust acc
		float _velStandardX = -velShield0 * (_position.X - posShield0) / posShield0;
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

	private void ColliShield()
	{
		foreach (var _body in GetNode<Area2D>("posshield/body/areahit").GetOverlappingBodies())
		{
			Vector2 _momentum = Vector2.Zero;
			if (_body.IsInGroup("bullet"))
			{
				RigidBody2D _bulletBody = (RigidBody2D)_body;
				_momentum += (Position - _bulletBody.Position).Normalized() * _bulletBody.Mass * _bulletBody.LinearVelocity.Length();
				_bulletBody.GetNode<bullet_main>(".").ReflectShield();
			}
			if (_momentum.Length() != 0.0f)
			{
				velMain += 2 * _momentum / masMain;
				GetNode<AudioStreamPlayer>("sound/reflect").Play();
			}
		}
	}

	public override void _Ready()
	{
		cooldown.Add("shield_push", new float[] {0, 1});
		trigger.Add("shield_push", new float[] {0, 0.3f});
		cooldown.Add("player_dash", new float[] {0, 1});
		trigger.Add("player_dash", new float[] {0, 0.2f});
		condition.Add("player_hit", false);
		duration.Add("player_hit", new float[] {0, 3});
		trigger.Add("player_hit", new float[] {0, 10});

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
		ColliPlayer();
		ActionUnderAttack();

		// shield sectionaa
		InputShieldPush();
		AdjustShieldRotate();
		AdjustShieldReturn();
		ActionShieldPush();
		ColliShield();

		// skill section

		// end
		Velocity = velMain;
		MoveAndSlide();
		//SaveLogVelocity();
	}
}
