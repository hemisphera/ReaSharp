namespace ReaSharp.Models;

public readonly struct ParameterValue
{
  public double Minimum { get; }
  public double Maximum { get; }
  public double Value { get; }
  public double Percentage => (Value - Minimum) / (Maximum - Minimum);


  public ParameterValue(double minimum, double maximum, double value)
  {
    Minimum = minimum;
    Maximum = maximum;
    Value = value;
  }


  public override string ToString()
  {
    return $"{Value} ({Minimum} - {Maximum})";
  }
}