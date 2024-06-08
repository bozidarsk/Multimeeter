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
	public double Reference { get; }
	public string Format { get; }

	public double Calculate(double value) => this.Type switch 
	{
		MeasurementType.Voltage => value * this.Reference,
		MeasurementType.Current => value * this.Reference,
		MeasurementType.Resistance => (this.Reference * value) / (1 - value),

		_ => throw new InvalidOperationException($"Invalid measurement type '{this.Type}'.")
	};

	public static IEnumerable<Measurement> GetMeasurements() 
	{
		yield return new(MeasurementType.Voltage, 50e-3, 5, "#0e-0 V");
		yield return new(MeasurementType.Voltage, 500e-3, 5, "##0e-0 V");
		yield return new(MeasurementType.Voltage, 5, 5, "0 V");
		yield return new(MeasurementType.Voltage, 50, 50, "#0 V");
		yield return new(MeasurementType.Voltage, 500, 500, "##0 V");

		yield return new(MeasurementType.Current, 5e-6, 5 / 1.1, "0e-0 A");
		yield return new(MeasurementType.Current, 50e-6, 5 / 1.1, "#0e-0 A");
		yield return new(MeasurementType.Current, 500e-6, 5 / 1.1, "##0e-0 A");
		yield return new(MeasurementType.Current, 5e-3, 50 / 1.1, "0e-0 A");
		yield return new(MeasurementType.Current, 50e-3, 500 / 1.1, "#0e-0 A");

		yield return new(MeasurementType.Resistance, 1e+3, 10e+3, "###0 Ω");
		yield return new(MeasurementType.Resistance, 10e+3, 10e+3, "#0e+0 Ω");
		yield return new(MeasurementType.Resistance, 100e+3, 10e+3, "##0e+0 Ω");
		yield return new(MeasurementType.Resistance, 1e+6, 10e+3, "#e+0 Ω");
		yield return new(MeasurementType.Resistance, 10e+6, 10e+3, "#0e+0 Ω");
	}

	public override bool Equals(object? other) 
	{
		if (other is not Measurement)
			return false;

		Measurement x = (Measurement)other;
		return this.Type == x.Type && this.Max == x.Max && this.Reference == x.Reference && this.Format == x.Format;
	}

	public Measurement(MeasurementType type, double max, double reference, string format) => 
		(this.Type, this.Max, this.Reference, this.Format) = (type, max, reference, format)
	;
}
