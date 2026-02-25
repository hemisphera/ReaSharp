namespace ReaSharp.Models;

public class Transport
{
  private int _playState;
  private readonly Project _project;

  public bool IsPlaying => (_playState & 0x01) == 0x01;
  public bool IsPaused => (_playState & 0x02) == 0x02;
  public bool IsRecording => (_playState & 0x04) == 0x04;
  public TimeSpan CursorPositoon { get; private set; }
  public TimeSpan PlayheadPosition { get; private set; }


  public Transport(Project? project = null)
  {
    _project = project ?? Project.Default;
  }


  public void Update()
  {
    PlayheadPosition = TimeSpan.FromSeconds(Reaper.GetPlayPositionEx(_project.ReaperHandle));
    CursorPositoon = TimeSpan.FromSeconds(Reaper.GetCursorPositionEx(_project.ReaperHandle));
    _playState = Reaper.GetPlayStateEx(_project.ReaperHandle);
  }
}