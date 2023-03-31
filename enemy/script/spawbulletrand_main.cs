using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class spawbulletrand_main : Node2D
{
	[Export(PropertyHint.Range, "0,6.283184")]
	public float centerTheta = 3.141592f;
	[Export(PropertyHint.Range, "0,6.283184")]
	public float deltaTheta = 1.570796f;
	
	private Dictionary<string, float[]> cooldown = new Dictionary<string, float[]>();
	private PackedScene originalBullet;
	private Random rand = new Random();

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
/*
	private async Task SpawnBulletNode()
	{
		await Task.Run( ()=>
		{
			float radian = centerTheta-0.5f*deltaTheta + rand.Next()%deltaTheta;
			RigidBody2D _node = (RigidBody2D)(originalBullet.Instantiate());
			_node.Position = GlobalPosition;
			_node.LinearVelocity = new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian)) * 1000;
			GetOwner<Node2D>().GetNode<Node2D>("bulletpak").AddChild(_node);
		});
	}
*/
	private void SpawnBulletNode()
	{
		float radian = centerTheta-0.5f*deltaTheta + rand.Next()%deltaTheta;
		RigidBody2D _node = (RigidBody2D)(originalBullet.Instantiate());
		_node.Position = GlobalPosition;
		_node.LinearVelocity = new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian)) * 1000;
		GetOwner<Node2D>().GetNode<Node2D>("bulletpak").AddChild(_node);
	}

	public override void _Ready()
	{
		cooldown.Add("spawn", new float[] {0, 1.0f});
		originalBullet = GD.Load<PackedScene>("res://enemy/bullet.tscn");
	}

	public override void _Process(double delta)
	{
		deltaTime = (float)delta;
		AdjustCountdownDic(cooldown);
		
		if (cooldown["spawn"][0] == 0)
		{
			cooldown["spawn"][0] = cooldown["spawn"][1];
			
			SpawnBulletNode();
		}
	}
}
