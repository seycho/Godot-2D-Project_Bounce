using Godot;
using System;

public partial class bullet_main : RigidBody2D
{
	public float SpeedProcess = 1.0f;

	private bool isActive = true;
	private float velDefault = 1000;
	private string stateRemove = "None";

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
		GetNode<Area2D>("regdet").CollisionLayer = 0;
		GetNode<Area2D>("regdet").CollisionMask = 0;
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
	
	private void CollBodyCheck()
	{
		bool isCollision = false;
		foreach (var _body in GetNode<Area2D>("regdet").GetOverlappingBodies())
		{
			if (_body.Name != Name)
			{
				isCollision = true;
			}
		}
		if (isCollision)
		{
			GetNode<AudioStreamPlayer>("sound/hit").Play();
			
		}
		else
			LinearVelocity = LinearVelocity / LinearVelocity.Length() * velDefault * SpeedProcess;
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
				GD.Print(111);
			}
			else if (_body.IsInGroup("boss"))
			{
				if (IsInGroup("player") & _body.IsInGroup("enemy"))
				{
					isActive = false;
					_body.GetNode<bossnor1_main>(".").ColliBody();
					stateRemove = "attack";
					GetNode<Timer>("remove").Start();
				}
			}
		}
	}

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if (isActive)
		{
			CollBodyCheck();
			CollAtkCheck();
		}
		else
		{
			ActionRemove();
		}
		KinematicCollision2D _collision = MoveAndCollide(LinearVelocity * (float)delta);
		if (_collision != null)
		{
			var reflect = _collision.GetRemainder().Bounce(_collision.GetNormal());
			LinearVelocity = LinearVelocity.Bounce(_collision.GetNormal());
			MoveAndCollide(reflect);
		}
	}
}
