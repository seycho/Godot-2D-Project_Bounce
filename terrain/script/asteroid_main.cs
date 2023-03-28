using Godot;
using System;

public partial class asteroid_main : StaticBody2D
{
	private int[] randCoorNum = new int[2]{3, 12};
	private int[] randLength = new int[2]{4, 100};

	private Random rand = new Random();

	public void SpawnAsteroid()
	{
		int numPoint = rand.Next(randCoorNum[0], randCoorNum[1]);
		float deltaTheta = 2 * (float)Math.PI / numPoint;
		float[] length = new float[numPoint];
		Vector2[] pointPolygon = new Vector2[numPoint];
		for (int i=0; i<numPoint; i++)
		{
			length[i] = rand.Next(randLength[0], randLength[1]);
			pointPolygon[i] = new Vector2(length[i]*(float)Math.Cos(deltaTheta*(i+1)),length[i]*(float)Math.Sin(deltaTheta*(i+1)));
		}
		GetNode<CollisionPolygon2D>("colpoly").Polygon = pointPolygon;
		GetNode<Polygon2D>("imgpoly").Polygon = pointPolygon;
	}

	public override void _Ready()
	{
		SpawnAsteroid();
	}

	public override void _Process(double delta)
	{
	}
}
