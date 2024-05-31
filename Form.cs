using System;
using System.IO.Ports;
using System.Linq;

using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

public sealed class Window : Form
{
	public Window() : base()
	{
		Chart chart = new();
		chart.Dock = DockStyle.Fill;

		chart.ChartAreas.Add("area");
		chart.ChartAreas[0].AxisX.Minimum = 0;
		chart.ChartAreas[0].AxisX.Maximum = Program.MaxTime;
		chart.ChartAreas[0].AxisX.IsStartedFromZero = true;
		chart.ChartAreas[0].AxisX.Interval = 5;

		chart.ChartAreas[0].AxisY.Minimum = 0;
		chart.ChartAreas[0].AxisY.Maximum = Program.SelectedMeasurement.Max;
		chart.ChartAreas[0].AxisY.IsStartedFromZero = true;
		chart.ChartAreas[0].AxisY.LabelStyle.Format = Program.SelectedMeasurement.Format;

		chart.Series.Add(new Series());
		chart.Series[0].ChartArea = "area";
		chart.Series[0].ChartType = SeriesChartType.FastLine;
		chart.Series[0].XValueType = ChartValueType.Double;
		chart.Series[0].YValueType = ChartValueType.Double;
		chart.Series[0].YValuesPerPoint = 1;

		Program.Points = chart.Series[0].Points;

		MenuStrip menu = new();
		menu.Dock = DockStyle.Top;

		ToolStripMenuItem measurementMenu = new ToolStripMenuItem("Measurement");

		foreach (var group in Measurement.GetMeasurements().GroupBy(x => x.Type)) 
		{
			ToolStripMenuItem measurement = new(group.Key.ToString());
			measurementMenu.DropDownItems.Add(measurement);

			foreach (var m in group) 
			{
				measurement.DropDownItems.Add(new ToolStripMenuItem(
					m.Max.ToString(m.Format),
					null,
					(o, e) => 
					{
						if (Program.SelectedMeasurement.Equals(m))
							return;

						Program.SelectedMeasurement = m;
						updateMeasurements((ToolStripMenuItem)o!);
						Program.Clear();
					}
				) { Checked = Program.SelectedMeasurement.Equals(m) });
			}
		}

		void updateMeasurements(ToolStripMenuItem selected) 
		{
			foreach (ToolStripMenuItem measurement in measurementMenu.DropDownItems)
				foreach (ToolStripMenuItem item in measurement.DropDownItems)
					item.Checked = item == selected;

			chart.ChartAreas[0].AxisY.Maximum = Program.SelectedMeasurement.Max;
			chart.ChartAreas[0].AxisY.LabelStyle.Format = Program.SelectedMeasurement.Format;
		}

		ToolStripMenuItem chartMenu = new ToolStripMenuItem("Chart");
		chartMenu.DropDownItems.Add(new ToolStripMenuItem("Save As...", null, (o, e) => Program.Export()));
		chartMenu.DropDownItems.Add(new ToolStripMenuItem("Clear", null, (o, e) => Program.Clear()));

		ToolStripMenuItem plotterMenu = new ToolStripMenuItem("Plotter");
		ToolStripMenuItem portsMenu = new ToolStripMenuItem("Serial Port");
		plotterMenu.DropDownItems.Add(portsMenu);
		plotterMenu.DropDownItems.Add(new ToolStripMenuItem("Pause/Resume", null, (o, e) => Program.IsRunning = !Program.IsRunning));
		plotterMenu.DropDownOpening += (o, e) => 
		{
			portsMenu.DropDownItems.Clear();
			foreach (string port in SerialPort.GetPortNames())
				portsMenu.DropDownItems.Add(new ToolStripMenuItem(port, null, (o, e) => 
					{
						ToolStripMenuItem item = (ToolStripMenuItem)o!;
						item.Checked = true;
						Program.PortName = item.Text;
					}
				) { Checked = Program.PortName == port });
		};

		((ToolStripDropDownMenu)measurementMenu.DropDown).ShowImageMargin = false;
		((ToolStripDropDownMenu)chartMenu.DropDown).ShowImageMargin = false;
		((ToolStripDropDownMenu)plotterMenu.DropDown).ShowImageMargin = false;

		TableLayoutPanel panel = new();
		panel.ColumnCount = 1;
		panel.RowCount = 2;
		panel.Dock = DockStyle.Fill;
		panel.Controls.Add(menu);
		panel.Controls.Add(chart);

		menu.Items.Add(measurementMenu);
		menu.Items.Add(chartMenu);
		menu.Items.Add(plotterMenu);

		base.Size = new Size(1280, 720);
		base.IsMdiContainer = true;
		base.MainMenuStrip = menu;
		base.Controls.Add(panel);
		base.Show();
	}
}
