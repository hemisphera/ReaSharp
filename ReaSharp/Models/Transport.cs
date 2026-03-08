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
      Reaper.SetEditCurPos2(Project.ReaperHandle, _cursorPosition.TotalSeconds, false, false);
    }
  }

  public TimeSpan PlayheadPosition { get; private set; }
  public TimeSpan PlayheadOrCursorPosition => IsPlaying ? PlayheadPosition : CursorPosition;
  public BeatPosition CursorBeatsPosition { get; private set; }
  public BeatPosition PlayheadBeatsPosition { get; private set; }
  public BeatPosition PlayheadOrCursorBeatsPosition => IsPlaying ? PlayheadBeatsPosition : CursorBeatsPosition;


  public event EventHandler? RecordingStarted;
  public event EventHandler? RecordingStopped;
  public event EventHandler? PlaybackStarted;
  public event EventHandler? PlaybackStopped;


  public Transport(Project? project = null)
  {
    Project = project ?? Project.Default;
    Update();
  }


  public void Update()
  {
    PlayheadPosition = TimeSpan.FromSeconds(Reaper.GetPlayPositionEx(Project.ReaperHandle));
    _cursorPosition = TimeSpan.FromSeconds(Reaper.GetCursorPositionEx(Project.ReaperHandle));

    var wasRecording = IsRecording;
    var wasPlaying = IsPlaying;
    _playState = Reaper.GetPlayStateEx(Project.ReaperHandle);

    PlayheadBeatsPosition = BeatPosition.FromTime(PlayheadPosition, Project);
    CursorBeatsPosition = BeatPosition.FromTime(CursorPosition, Project);

    FireEvents(wasRecording, wasPlaying);
  }

  private void FireEvents(bool wasRecording, bool wasPlaying)
  {
    if (wasRecording != IsRecording)
    {
      var handler = IsRecording ? RecordingStarted : RecordingStopped;
      handler?.Invoke(this, EventArgs.Empty);
    }

    if (wasPlaying != IsPlaying)
    {
      var handler = wasPlaying ? PlaybackStarted : PlaybackStopped;
      handler?.Invoke(this, EventArgs.Empty);
    }
  }
}