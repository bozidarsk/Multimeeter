using System;

#pragma warning disable CS0659 // 'Measurement' overrides Object.Equals(object o) but does not override Object.GetHashCode()

public enum MeasurementType 
{
	Voltage,
	Current,
	Resistance,
}

public sealed class Measurement 
{
	public MeasurementType Type { get; }
	public double Max { get; }
	public string Format { get; }

	private const double r = 10e+3;
	private const double R1 = 90e+3;
	private const double R2 = 990e+3;

	private double AsVoltage(double percentage) => percentage * this.Max;

	private double AsCurrent(double percentage) 
	{
		return percentage * (this.Max);


		// double maxVoltage = r * this.Max;
		// double voltage = percentage * maxVoltage;

		// return voltage / r;
	}

	private double AsResistance(double percentage) => percentage * this.Max;

	public double Calculate(double value) => this.Type switch 
	{
		MeasurementType.Voltage => AsVoltage(value),
		MeasurementType.Current => AsCurrent(value),
		MeasurementType.Resistance => AsResistance(value),

		_ => throw new InvalidOperationException($"Invalid measurement type '{this.Type}'.")
	};

	public static IEnumerable<Measurement> GetMeasurements() 
	{
		yield return new(MeasurementType.Voltage, 50e-3, "00e-0 V");
		yield return new(MeasurementType.Voltage, 500e-3, "000e-0 V");
		yield return new(MeasurementType.Voltage, 5, "0 V");
		yield return new(MeasurementType.Voltage, 50, "00 V");
		yield return new(MeasurementType.Voltage, 500, "000 V");

		yield return new(MeasurementType.Current, 5e-6, "0e-0 A");
		yield return new(MeasurementType.Current, 50e-6, "00e-0 A");
		yield return new(MeasurementType.Current, 500e-6, "000e-0 A");
		yield return new(MeasurementType.Current, 5e-3, "0e-0 A");
		yield return new(MeasurementType.Current, 50e-3, "00e-0 A");

		yield return new(MeasurementType.Resistance, 10e+6, "00e+0 Ω");
	}

	public override bool Equals(object? other) 
	{
		if (other is not Measurement)
			return false;

		Measurement x = (Measurement)other;
		return this.Type == x.Type && this.Max == x.Max && this.Format == x.Format;
	}

	public Measurement(MeasurementType type, double max, string format) => (this.Type, this.Max, this.Format) = (type, max, format);
}
