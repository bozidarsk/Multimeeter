using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Linq;
using System.IO.Ports;
using System.IO;

using OpenGL;

public static class Program 
{
	private static LinkedList<(double, double)> points = new();
	private static Queue<double> queue = new();
	private static Stopwatch watch = new();
	private static int range = 0; // min: 0, max: 5 * 10^Range

	private static MeasurementType measurementType = MeasurementType.Voltage;
	private static readonly Dictionary<MeasurementType, MeasurementDelegate> measurements = new() 
	{
		{ MeasurementType.Voltage, Measurements.Voltage },
		{ MeasurementType.Amperage, Measurements.Amperage },
		{ MeasurementType.Resistance, Measurements.Resistance },
	};

	private static void ParseCommands() 
	{
		string? command;
		while ((command = Console.ReadLine()) != null) 
		{
			string[] tokens = command.Split(' ', 2);

			if (tokens.Length == 0 || tokens[0].Length != 1) 
			{
				Console.WriteLine("System.ArgumentException");
				continue;
			}

			switch (tokens[0][0]) 
			{
				case 'r':
					if (int.TryParse(tokens[1], out int x))
						if (x >= -2 && x <= 2)
							range = x;
						else
							Console.WriteLine("System.ArgumentOutOfRangeException");
					else
						Console.WriteLine("System.FormatException");
					continue;
				case 'm':
					if (Enum.TryParse<MeasurementType>(tokens[1], ignoreCase: true, out MeasurementType type))
						measurementType = type;
					else
						Console.WriteLine("System.FormatException");
					continue;
				case 'c':
					lock (points)
						points.Clear();
					lock (watch)
						watch.Restart();
					lock (queue)
						queue.Clear();
					continue;
				case 'p':
					try { SavePoints(tokens[1]); }
					catch (Exception e) { Console.WriteLine(e.GetType()); }
					continue;
				case 'g':
					try { SaveGraph(tokens[1]); }
					catch (Exception e) { Console.WriteLine(e.GetType()); }
					continue;
				default:
					Console.WriteLine("r INT - changes multimeeter range where min is 0 and max is 5 * 10^range");
					Console.WriteLine("m MEASUREMENTTYPE - changes 'measurementType'");
					Console.WriteLine("c - clears any data");
					Console.WriteLine("p STRING - saves the points from the graph as a csv in STRING");
					Console.WriteLine("g STRING - saves the graph as a svg in STRING");
					continue;
			}
		}

		Environment.Exit(0);
	}

	private static void ParseValues(string portName) 
	{
		SerialPort port = new SerialPort(portName, 9600);

		port.Encoding = Encoding.ASCII;
		port.NewLine = "\r\n";
		port.DataBits = 8;
		port.Parity = Parity.None;
		port.StopBits = StopBits.One;
		port.RtsEnable = true;

		port.DataReceived += (s, e) => 
		{
			if (!double.TryParse(port.ReadLine(), out double value))
				return;

			lock (queue)
				queue.Enqueue(value);
		};

		port.Open();
	}

	private static void FormatPoints() 
	{
		const double xmax = 60e+3;
		double ymax = Measurements.Max(range);
		double x, y;

		lock (queue) 
		{
			while (queue.Count > 0)
			{
				double value = queue.Dequeue();

				y = measurements[measurementType](value, range);
				y /= ymax;
				y = (-1) + y * ((1) - (-1));

				lock (watch)
					x = watch.ElapsedMilliseconds;
				x /= xmax;
				x = (-1) + x * ((1) - (-1));

				points.AddLast((x, y));
			}
		}
	}

	private static void SavePoints(string file) 
	{
		if (file == null)
			throw new ArgumentNullException();

		IEnumerable<string> generateLines() 
		{
			yield return "Time,Values";

			lock (points)
				foreach ((double x, double y) in points)
					yield return $"{x},{y}";
		};

		File.WriteAllLines(file, generateLines());
	}

	private static void SaveGraph(string file) 
	{
		if (file == null)
			throw new ArgumentNullException();

		IEnumerable<string> generateLines() 
		{
			yield return "<svg width=\"2\" height=\"2\" viewBox=\"0 0 2 2\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"100%\" height=\"100%\" fill=\"black\"/><polyline points=\"";

			lock (points)
				foreach ((double x, double y) in points)
					yield return $"{x + 1},{2 - (y + 1)}";

			yield return "\" style=\"fill:none;stroke:white;stroke-width:0.01\"/></svg>";
		};

		File.WriteAllLines(file, generateLines());
	}

	private static void RenderGraph() 
	{
		if (!GLFW.IsInitialized)
			Environment.Exit(-1);

		Window window = new Window(1280, 720, "Graph");
		window.MakeCurrentContext();

		while (!window.ShouldClose) 
		{
			glClear(0x4000);
			glBegin(3);

			FormatPoints();

			lock (points)
				foreach ((double x, double y) in points)
					glVertex2d(x, y);

			glEnd();

			window.SwapBuffers();
			Input.PollEvents();

			lock (queue)
				if (queue.Count > 1)
					Console.WriteLine($"behind with {queue.Count} values");
		}

		GLFW.Terminate();
		Environment.Exit(0);
	}

	private static void Main(string[] args) 
	{
		watch.Start();

		new Thread(RenderGraph).Start();

		ParseValues(args[0]);
		ParseCommands();
	}

	[DllImport("Opengl32")] private static extern void glVertex2d(double x, double y);
	[DllImport("Opengl32")] private static extern void glClear(int mask);
	[DllImport("Opengl32")] private static extern void glBegin(int mode);
	[DllImport("Opengl32")] private static extern void glEnd();
}
