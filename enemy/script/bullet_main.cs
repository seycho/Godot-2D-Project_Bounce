using Godot;
using System;

public partial class bullet_main : RigidBody2D
{
	private float velDefault = 1000;

	public void ReflectShield()
	{
		RemoveFromGroup("enemy");
		AddToGroup("player");
		CollisionMask -= 2;
		GetNode<Sprite2D>("ballyellow").Visible = false;
	}

	public void HitPlayer()
	{
		QueueFree();
	}

	private void KeepSpeed()
	{
		bool isCollision = false;
		foreach (var _body in GetNode<Area2D>("regdet").GetOverlappingBodies())
		{
			if (_body.Name != Name)
				isCollision = true;
		}
		if (isCollision)
		{
			LinearVelocity = LinearVelocity / LinearVelocity.Length() * velDefault;
		}
	}

	private void ColliBullet()
	{
		foreach (var _body in GetNode<Area2D>("regatk").GetOverlappingBodies())
		{
			if (IsInGroup("player") & _body.IsInGroup("enemy"))
				QueueFree();
			if (IsInGroup("enemy") & _body.IsInGroup("player"))
				QueueFree();
		}
	}

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		KeepSpeed();
		ColliBullet();
	}
}
