using System.Collections.Generic;
using UnityEngine;
using System.Threading;
public enum EmitOption
{
	inOrbit,
	noVel,
	explosion
}
public class AtractorMulti : MonoBehaviour
{
	internal static AtractorMulti Instance;
	internal class Particle
	{
		internal Vector3 velocity;
		internal Vector3 position;
		internal float mass;
		internal float size;
		internal bool alive;
	}
	internal class Biggest
	{
		internal Vector3 pos;
		internal float mass;
		internal Vector3 lPos;
		internal float lmass;
	}
	internal class Worker
	{
		internal Worker(int i)
		{
			index = i;
			work = new Thread(new ThreadStart(Work));
			ready = false;
			work.IsBackground = true;
			work.Start();
		}
		internal Thread work;
		internal bool ready;
		internal bool velocityAdded;
		internal int index;
		void Work()
		{
			while (true)
			{
				try
				{
					if (!velocityAdded)
					{
						int start = Mathf.FloorToInt((float)Instance.count * index / Instance.worker.Length);
						int end = Mathf.FloorToInt((float)Instance.count * (index + 1) / Instance.worker.Length);
						Instance.AddVelocity(start, end);
						velocityAdded = true;
					}
					else if (!ready && Instance.VelocityAdded() && Instance.pCount != -1)
					{
						int start = Mathf.FloorToInt((float)Instance.count * index / Instance.worker.Length);
						int end = Mathf.FloorToInt((float)Instance.count * (index + 1) / Instance.worker.Length);
						Instance.Move(start, end);
						ready = true;
					}
				}
				catch (ThreadAbortException tae)
				{
					print(tae);
					return;
				}
				catch (System.Exception e)
				{
					//Debug.LogError("Error running task loop: " + e);
				}
				Thread.Sleep(0);
			}
		}
	}
	[HideInInspector]
	public bool paused = false;
	public float gravity = 0.0015f;
	public float startVel = 2f;
	public Vector2 massRange = new Vector2(2, 3);
	public float spawnRadius = 5f;
	public float density = 1f;
	public Gradient gradient;
	[HideInInspector]
	public bool useTrails = false;
	public int emitCount = 1000;
	[HideInInspector]
	public bool emit = false;
	[HideInInspector]
	public bool restart = false;
	[SerializeField]
	AnimationCurve curve;
	float scale = 0.09f;
	public int count = 1;
	internal Particle[] parti = new Particle[0];
	List<Particle> newArray = new List<Particle>();
	[SerializeField]
	ParticleSystem trailSystem;
	ParticleSystem ps;
	ParticleSystem.Particle[] part = new ParticleSystem.Particle[50000];
	//ParticleSystem.TrailModule trails;
	Vector3 one = Vector3.one;
	internal Biggest biggest = new Biggest();
	float dt = 0.016666666f;
	public EmitOption emitOption;
	int pCount = -1;
	Worker[] worker;
	private void Awake()
	{
		Instance = this;
		ps = GetComponent<ParticleSystem>();
		Application.runInBackground = true;
		Emit(1);
	}
	private void Start()
	{
		worker = new Worker[(SystemInfo.processorCount - 1) * 2];
		for (int i = 0; i < worker.Length; i++)
		{
			worker[i] = new Worker(i);
		}
	}
	private void Update()
	{
		if (paused)
			return;
		if (pCount == -1 && !Ready() && VelocityAdded())
		{
			lock (part)
			{
				pCount = ps.GetParticles(part);
			}
		}
		if (!Ready())
			return;
		parti = newArray.ToArray();
		newArray.Clear();
		count = parti.Length;
		ps.SetParticles(part, pCount);
		pCount = -1;
		if (useTrails)
		{
			var para = new ParticleSystem.EmitParams();
			for (int i = 0; i < count; i++)
			{
				para.position = parti[i].position;
				para.startSize3D = parti[i].size * one;
				para.startColor = MassToColor(parti[i].mass);
				trailSystem.Emit(para, 1);
			}
		}
		lock (biggest)
		{
			biggest.lPos = biggest.pos;
			biggest.lmass = biggest.mass;
			biggest.mass = 0.01f;
		}
		if (restart)
		{
			biggest.mass = 0.01f;
			biggest.pos = Vector3.zero;
			biggest.lPos = biggest.pos;
			biggest.lmass = biggest.mass;
			parti = new Particle[0];
			ps.Clear();
			Emit(1);
			restart = false;
		}
		if (emit)
		{
			Emit(emitCount);
			emit = false;
		}
		for (int i = 0; i < worker.Length; i++)
		{
			worker[i].velocityAdded = false;
			worker[i].ready = false;
		}
	}
	private void OnDestroy()
	{
		try
		{
			for (int i = 0; i < worker.Length; i++)
			{
				worker[i].work.Abort();
			}
		}
		catch (System.Exception ex)
		{
			print(ex);
		}
	}
	private void Emit(int c)
	{
		if (c == 1)
		{
			newArray.Add(new Particle()
			{
				position = Vector3.zero,
				velocity = Vector3.zero,
				mass = 1,
				size = MassToSize(1),
				alive = true
			});
		}
		float explosionDiameter = Mathf.Pow(c * Mathf.Pow(MassToSize(massRange.y), 3f), 0.3333333f);
		for (int i = 0; i < c; i++)
		{
			Vector3 pos = Random.insideUnitSphere;
			float mass = Random.Range(massRange.y, massRange.x);
			switch (emitOption)
			{
				case EmitOption.inOrbit:
					float y = pos.y;
					pos.y = 0f;
					pos.y = spawnRadius * curve.Evaluate(pos.magnitude) * y;
					pos.x *= spawnRadius;
					pos.z *= spawnRadius;
					Vector3 v = Vector3.Cross(pos.normalized, Vector3.up);
					v = v.normalized * Mathf.Sqrt(gravity * biggest.lmass / pos.magnitude);
					newArray.Add(new Particle()
					{
						position = pos + biggest.lPos,
						velocity = v,
						mass = mass,
						size = MassToSize(mass),
						alive = true
					});
					break;
				case EmitOption.noVel:
					newArray.Add(new Particle()
					{
						position = pos * spawnRadius,
						velocity = Vector3.zero,
						mass = mass,
						size = MassToSize(mass),
						alive = true
					});
					break;
				case EmitOption.explosion:
					pos *= explosionDiameter;
					newArray.Add(new Particle()
					{
						position = pos + biggest.lPos,
						velocity = pos * spawnRadius,
						mass = mass,
						size = MassToSize(mass),
						alive = true
					});
					break;
			}
		}
		for (int i = 0; i < parti.Length; i++)
		{
			newArray.Add(parti[i]);
		}
		parti = newArray.ToArray();
		count = parti.Length;
		newArray.Clear();

		ps.Clear();
		ps.Emit(count);
	}
	private bool VelocityAdded()
	{
		for (int i = 0; i < worker.Length; i++)
		{
			if (!worker[i].velocityAdded)
				return false;
		}
		return true;
	}
	public bool Ready()
	{
		for (int i = 0; i < worker.Length; i++)
		{
			if (!worker[i].ready)
				return false;
		}
		return true;
	}
	private void Move(int start, int end)
	{
		if (paused)
			return;
		for (int i = start; i < end; i++)
		{
			if (!parti[i].alive)
			{
				part[i].remainingLifetime = -1f;
				continue;
			}
			lock (parti[i])
			{
				parti[i].position += parti[i].velocity * dt;
			}
			part[i].position = parti[i].position;
			part[i].remainingLifetime = 40f;
			part[i].startSize3D = one * parti[i].size;
			part[i].startColor = MassToColor(parti[i].mass);
			lock (biggest)
			{
				if (parti[i].mass > biggest.mass)
				{
					biggest.mass = parti[i].mass;
					biggest.pos = parti[i].position;
				}
			}
			lock (newArray)
			{
				newArray.Add(parti[i]);
			}
		}
	}
	private void AddVelocity(int start, int end)
	{
		if (paused)
			return;
		for (int i = start; i < end; i++)
		{
			if (!parti[i].alive)
				continue;
			Vector3 awayDis = parti[i].position - biggest.lPos;
			if (awayDis.sqrMagnitude > 1000000f)
			{
				lock (parti[i])
					parti[i].alive = false;
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (j != i && parti[j].alive)
				{
					Vector3 dist = parti[i].position - parti[j].position;
					float d = dist.sqrMagnitude;
					float diameters = (parti[i].size + parti[j].size) * 0.5f;
					diameters *= diameters;
					if (d <= diameters)
					{
						if (parti[i].mass < parti[j].mass)
						{
							lock (parti[i])
							{
								parti[i].alive = false;
							}
							lock (parti[j])
							{
								//parti[j].velocity = (parti[j].velocity * (parti[j].mass - parti[i].mass) + (2f * parti[i].velocity * parti[i].mass) / (parti[i].mass + parti[j].mass));
								parti[j].velocity = parti[j].velocity * (parti[j].mass / (parti[i].mass + parti[j].mass)) + parti[i].velocity * (parti[i].mass / (parti[i].mass + parti[j].mass));
								parti[j].mass += parti[i].mass;
								parti[j].size = MassToSize(parti[j].mass);
							}
						}
						else
						{
							lock (parti[j])
							{
								parti[j].alive = false;
							}
							lock (parti[i])
							{
								//parti[i].velocity = (parti[i].velocity * (parti[i].mass - parti[j].mass) + (2f * parti[j].velocity * parti[j].mass) / (parti[i].mass + parti[j].mass));
								parti[i].velocity = parti[j].velocity * (parti[j].mass / (parti[i].mass + parti[j].mass)) + parti[i].velocity * (parti[i].mass / (parti[i].mass + parti[j].mass));
								parti[i].mass += parti[j].mass;
								parti[i].size = MassToSize(parti[i].mass);
							}
						}
					}
					else
					{
						lock (parti[i])
						{
							parti[i].velocity -= dist.normalized * (parti[j].mass * dt * gravity / d);
						}
					}
				}
			}
		}
	}
	private Color MassToColor(float mass)
	{
		return gradient.Evaluate(1f - mass / biggest.lmass);
	}
	private float MassToSize(float mass)
	{
		return Mathf.Pow(density * mass * 3f / (Mathf.PI * 4f), 0.333333f) * scale;
	}
}