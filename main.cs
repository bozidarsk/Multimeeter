using System;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Linq;

using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

public static class Program 
{
	public static DataPointCollection? Points;

	public static Measurement SelectedMeasurement = Measurement.GetMeasurements().Where(x => x.Type == MeasurementType.Voltage && x.Max == 5).Single();
	public static string? PortName = SerialPort.GetPortNames().FirstOrDefault();
	public static bool IsRunning = true;
	public const double MaxTime = 60;

	private static Stopwatch watch = new();

	public static void Export() 
	{
		IEnumerable<string> generateSVG() 
		{
			yield return "<svg width=\"2\" height=\"2\" viewBox=\"0 0 2 2\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"100%\" height=\"100%\" fill=\"black\"/><polyline points=\"";

			foreach (DataPoint p in Program.Points!)
				yield return $"{p.XValue + 1},{2 - (p.YValues.Single() + 1)}";

			yield return "\" style=\"fill:none;stroke:white;stroke-width:0.01\"/></svg>";
		};

		IEnumerable<string> generateCSV() 
		{
			yield return "Time,Value";

			foreach (DataPoint p in Program.Points!)
				yield return $"{p.XValue},{p.YValues.Single()}";
		};

		SaveFileDialog dialog = new();
		dialog.Title = "Save Chart As";
		dialog.Filter = "Scalable Vector Graphics|*.svg|Comma Separated Values|*.csv";

		dialog.ShowDialog();

		if (string.IsNullOrWhiteSpace(dialog.FileName))
			return;

		File.WriteAllLines(dialog.FileName, dialog.FilterIndex switch 
			{
				1 => generateSVG(),
				2 => generateCSV(),

				_ => throw new InvalidOperationException()
			}
		);
	}

	public static void Clear() 
	{
		Program.Points!.Clear();
		watch.Restart();
	}

	private static void OpenPort(SerialPort port) 
	{
		if (port.IsOpen)
			port.Close();

		port.PortName = PortName;
		port.BaudRate = 9600;
		port.Encoding = Encoding.ASCII;
		port.NewLine = "\r\n";
		port.DataBits = 8;
		port.Parity = Parity.None;
		port.StopBits = StopBits.One;
		port.RtsEnable = true;

		port.DataReceived += static (s, e) => 
		{
			if (!double.TryParse(((SerialPort)s).ReadLine(), out double value) || !IsRunning)
				return;

			double time = (double)watch.ElapsedMilliseconds * 1e-3;
			if (time > MaxTime) 
			{
				watch.Restart();
				time = 0;
			}

			Console.Write(value);
			Console.Write('\t');
			value = Program.SelectedMeasurement.Calculate(value);
			Console.WriteLine(value);

			Program.Points!.AddXY(time, value);
		};

		port.Open();
	}

	[STAThread]
	private static void Main() 
	{
		Window window = new();
		SerialPort port = new();

		watch.Start();

		while (true) 
		{
			window.Text = 
				$"[{Program.Points!.LastOrDefault()?.YValues.Single().ToString(Program.SelectedMeasurement.Format) ?? "0"}] "
				+ "Chart"
				+ ((Program.PortName != null) ? $" on {Program.PortName}" : "")
				+ (!IsRunning ? " - Paused" : "")
			;

			if (Program.PortName != null && (port.PortName != PortName || (!port.IsOpen && port.PortName != Program.PortName)))
				OpenPort(port);

			if (IsRunning && !watch.IsRunning) watch.Start();
			if (!IsRunning && watch.IsRunning) watch.Stop();

			Application.DoEvents();

			if (!window.Visible) return;
		}
	}
}
