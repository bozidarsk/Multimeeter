using System;

public delegate double MeasurementDelegate(double percentage, int range);

public enum MeasurementType 
{
	Voltage,
	Amperage,
	Resistance,
}

public static class Measurements 
{
	public static double Voltage(double percentage, int range) => percentage * Max(range);

	public static double Amperage(double percentage, int range) => percentage * Max(range);

	public static double Resistance(double percentage, int range) => percentage * Max(range);

	public static double Max(int range) => 5 * Math.Pow(10, range);
}
