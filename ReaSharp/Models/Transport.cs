namespace ReaSharp.Models;

public class Transport
{
  private int _playState;
  private TimeSpan _cursorPosition;

  public bool IsPlaying => (_playState & 0x01) == 0x01;
  public bool IsPaused => (_playState & 0x02) == 0x02;
  public bool IsRecording => (_playState & 0x04) == 0x04;
  public Project Project { get; }

  public TimeSpan CursorPosition
  {
    get => _cursorPosition;
    set
    {
      _cursorPosition = value;
      Reaper.SetEditCurPos2.Invoke(Project.ReaperHandle, _cursorPosition.TotalSeconds, false, false);
    }
  }

  public List<ITransportChangeListener> ChangeListeners { get; } = [];

  public TimeSpan PlayheadPosition { get; private set; }
  public TimeSpan PlayheadOrCursorPosition => IsPlaying ? PlayheadPosition : CursorPosition;
  public BeatPosition CursorBeatsPosition { get; private set; }
  public BeatPosition PlayheadBeatsPosition { get; private set; }
  public BeatPosition PlayheadOrCursorBeatsPosition => IsPlaying ? PlayheadBeatsPosition : CursorBeatsPosition;


  public Transport(Project? project = null)
  {
    Project = project ?? Project.Default;
    Update();
  }


  public void Update()
  {
    PlayheadPosition = TimeSpan.FromSeconds(Reaper.GetPlayPositionEx.Invoke(Project.ReaperHandle));
    _cursorPosition = TimeSpan.FromSeconds(Reaper.GetCursorPositionEx.Invoke(Project.ReaperHandle));

    var wasRecording = IsRecording;
    var wasPlaying = IsPlaying;
    _playState = Reaper.GetPlayStateEx.Invoke(Project.ReaperHandle);

    PlayheadBeatsPosition = BeatPosition.FromTime(PlayheadPosition, Project);
    CursorBeatsPosition = BeatPosition.FromTime(CursorPosition, Project);

    FireEvents(wasRecording, wasPlaying);
  }

  public void ToggleRecord()
  {
    Reaper.Main_OnCommandEx.Invoke(1013, 0, Project.ReaperHandle);
  }

  private void FireEvents(bool wasRecording, bool wasPlaying)
  {
    foreach (var listener in ChangeListeners)
    {
      if (wasRecording != IsRecording)
      {
        Func<Task> handler = IsRecording ? listener.RecordingStarted : listener.RecordingStopped;
        handler.Invoke();
      }

      if (wasPlaying != IsPlaying)
      {
        Func<Task> handler = wasPlaying ? listener.PlaybackStarted : listener.PlaybackStopped;
        handler.Invoke();
      }
    }
  }

  public void Play()
  {
    Reaper.Main_OnCommandEx.Invoke(1007, 0, Project.ReaperHandle);
  }

  public void Stop()
  {
    Reaper.Main_OnCommandEx.Invoke(1016, 0, Project.ReaperHandle);
  }

  public void Record()
  {
    Reaper.Main_OnCommandEx.Invoke(1013, 0, Project.ReaperHandle);
  }

  public void ToggleRecordAtNextMeasure()
  {
    Reaper.Main_OnCommandEx.Invoke(40003, 0, Project.ReaperHandle);
  }

  public void ToggleRecordAtNextBeat()
  {
    Reaper.Main_OnCommandEx.Invoke(40045, 0, Project.ReaperHandle);
  }

  public void Pause(bool b)
  {
    if (IsPaused == b) return;
    Reaper.Main_OnCommandEx.Invoke(1008, 0, Project.ReaperHandle);
  }
}