using System.Runtime.InteropServices;

namespace Hsp.ReaSharp.Models;

public sealed class FxInstanceParameter
{
  public FxInstance FxInstance { get; }

  public int Index { get; }

  public string? Name
  {
    get
    {
      const int bufferSize = 1024;
      var buffer = Marshal.AllocHGlobal(bufferSize);
      try
      {
        var ok = FxInstance.Track != null
          ? Reaper.TrackFX_GetParamName.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, buffer, bufferSize)
          : Reaper.TakeFX_GetParamName.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, buffer, bufferSize);
        return ok ? Marshal.PtrToStringAnsi(buffer) : null;
      }
      finally
      {
        Marshal.FreeHGlobal(buffer);
      }
    }
  }

  public double Minimum { get; }
  public double Maximum { get; }
  public double StepSize { get; }
  public double SmallStepSize { get; }
  public double LargeStepSize { get; }
  public bool IsToggle { get; }

  public double NormalizedValue
  {
    get => GetValueNormalized();
    set => SetValueNormalized(value);
  }


  public static FxInstanceParameter FromTrackFx(FxInstance fxInstance, int parameterIndex)
  {
    return new FxInstanceParameter(fxInstance, parameterIndex);
  }

  private FxInstanceParameter(FxInstance fxInstance, int index)
  {
    FxInstance = fxInstance;
    Index = index;
    var tempValue = GetValueInternal();
    Minimum = tempValue.min;
    Maximum = tempValue.max;
    var stepSizes = GetStepSizes();
    StepSize = stepSizes.steps;
    SmallStepSize = stepSizes.smallSteps;
    LargeStepSize = stepSizes.largeSteps;
    IsToggle = stepSizes.isToggle;
  }

  public double GetValue()
  {
    return GetValueInternal().value;
  }

  private double GetValueNormalized()
  {
    return FxInstance.Track != null
      ? Reaper.TrackFX_GetParamNormalized.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index)
      : Reaper.TakeFX_GetParamNormalized.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index);
  }

  private bool SetValueNormalized(double value)
  {
    return FxInstance.Track != null
      ? Reaper.TrackFX_SetParamNormalized.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, value)
      : Reaper.TakeFX_SetParamNormalized.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, value);
  }

  private (double min, double max, double value) GetValueInternal()
  {
    double minVal = 0, maxVal = 0;
    unsafe
    {
      var value = FxInstance.Track != null
        ? Reaper.TrackFX_GetParam.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, (nint)(&minVal), (nint)(&maxVal))
        : Reaper.TakeFX_GetParam.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, (nint)(&minVal), (nint)(&maxVal));
      return (minVal, maxVal, value);
    }
  }

  private (double steps, double smallSteps, double largeSteps, bool isToggle) GetStepSizes()
  {
    double steps = 0, smallSteps = 0, largeSteps = 0, isToggle = 0;
    unsafe
    {
      _ = FxInstance.Track != null
        ? Reaper.TrackFX_GetParameterStepSizes.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, (nint)(&steps), (nint)(&smallSteps), (nint)(&largeSteps), (nint)(&isToggle))
        : Reaper.TakeFX_GetParameterStepSizes.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, (nint)(&steps), (nint)(&smallSteps), (nint)(&largeSteps), (nint)(&isToggle));
      return (steps, smallSteps, largeSteps, isToggle != 0);
    }
  }

  public string GetFormattedValue()
  {
    const int bufferSize = 1024;
    var buffer = Marshal.AllocHGlobal(bufferSize);
    try
    {
      var ok = FxInstance.Track != null
        ? Reaper.TrackFX_GetFormattedParamValue.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, buffer, bufferSize)
        : Reaper.TakeFX_GetFormattedParamValue.Invoke(FxInstance.Owner.ReaperHandle, FxInstance.Index, Index, buffer, bufferSize);
      var strVal = ok ? Marshal.PtrToStringAnsi(buffer) : null;
      return strVal ?? string.Empty;
    }
    finally
    {
      Marshal.FreeHGlobal(buffer);
    }
  }

  public TrackFxEnvelope? GetEnvelope(TimeSpan pos, bool allowCreate = false)
  {
    if (FxInstance.Track == null) return null;
    var ptr = Reaper.GetFXEnvelope.Invoke(FxInstance.Track.ReaperHandle, FxInstance.Index, Index, allowCreate);
    return ptr == IntPtr.Zero ? null : TrackFxEnvelope.FromHandle(ptr);
  }
}