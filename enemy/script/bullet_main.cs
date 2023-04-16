using Godot;
using System;

public partial class bullet_main : RigidBody2D
{
	public float SpeedProcess = 1.0f;
	public Vector2 Velocity = new Vector2(0, 0);

	private bool isActive = true;
	private float velDefault = 1000;
	private string stateRemove = "None";
	
	private float deltaTime;

	public void ReflectShield()
	{
		RemoveFromGroup("enemy");
		AddToGroup("player");
		CollisionMask -= 2;
		GetNode<Sprite2D>("ballyellow").Visible = false;
	}

	private void RemoveCrash()
	{
		isActive = false;
		stateRemove = "crash";
		GetNode<Timer>("remove").Start();
	}
	
	private void ActionRemove()
	{
		Visible = false;
		LinearVelocity = Vector2.Zero;
		CollisionLayer = 0;
		CollisionMask = 0;
		if (stateRemove == "crash")
		{
			if (GetNode<AudioStreamPlayer>("sound/crash").Playing == false)
				GetNode<AudioStreamPlayer>("sound/crash").Play();
		}
		if (GetNode<Timer>("remove").TimeLeft <= 0)
		{
			QueueFree();
		}
	}

	private void InitialProcess(double delta)
	{
		deltaTime = (float)delta * SpeedProcess;
		LinearVelocity = Velocity.Normalized() * velDefault * SpeedProcess;
	}

	private void CollBodyCheck()
	{
		KinematicCollision2D _collision = MoveAndCollide(Velocity * deltaTime);
		if (_collision != null)
		{
			GetNode<AudioStreamPlayer>("sound/hit").Play();
			Velocity = Velocity.Bounce(_collision.GetNormal());
		}
	}

	private void CollAtkCheck()
	{
		foreach (var _body in GetNode<Area2D>("regatk").GetOverlappingBodies())
		{
			if (_body.IsInGroup("bullet"))
			{
				if (IsInGroup("player") & _body.IsInGroup("enemy"))
				{
					RemoveCrash();
				}
				else if (IsInGroup("enemy") & _body.IsInGroup("player"))
				{
					RemoveCrash();
				}
			}
			else if (_body.IsInGroup("body"))
			{
				if (_body.IsInGroup("player"))
				{
					float _damage = 0;
					if (IsInGroup("enemy"))
					{
						isActive = false;
						stateRemove = "attack";
						GetNode<Timer>("remove").Start();
						_damage = 1;
					}
					_body.GetNode<player_main>(".").ColliBody(Position, Mass * LinearVelocity.Length(), _damage);
				}
			}
			else if (_body.IsInGroup("shield"))
			{
				_body.GetOwner<CharacterBody2D>().GetNode<player_main>(".").ColliShield(Position, Mass * LinearVelocity.Length());
				ReflectShield();
			}
			else if (_body.IsInGroup("boss"))
			{
				if (IsInGroup("player") & _body.IsInGroup("enemy"))
				{
					isActive = false;
					stateRemove = "attack";
					GetNode<Timer>("remove").Start();
					_body.GetNode<bossnor1_main>(".").ColliBody();
				}
			}
		}
	}

	public override void _Ready()
	{
		Velocity = LinearVelocity;
	}

	public override void _Process(double delta)
	{
		InitialProcess(delta);

		if (isActive)
		{
			CollBodyCheck();
			CollAtkCheck();
		}
		else
		{
			ActionRemove();
		}
	}
}
