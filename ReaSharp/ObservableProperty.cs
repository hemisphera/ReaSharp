namespace ReaSharp;

public class ObservableProperty<T>
{
  private T? _value;


  public Func<T?, T?, Task>? ValueChangedCallback { get; set; }


  public T? Get()
  {
    return _value;
  }

  public bool HasValue()
  {
    return _value != null;
  }

  public async Task Set(T? newValue, bool doNotNotify = false)
  {
    var oldValue = _value;
    _value = newValue;
    if (!doNotNotify && !EqualityComparer<T?>.Default.Equals(oldValue, newValue))
      if (ValueChangedCallback != null)
        await ValueChangedCallback.Invoke(oldValue, newValue);
  }
}