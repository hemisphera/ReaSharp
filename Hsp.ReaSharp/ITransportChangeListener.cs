namespace Hsp.ReaSharp;

public interface ITransportChangeListener
{
  Task RecordingStarted();
  Task RecordingStopped();
  Task PlaybackStarted();
  Task PlaybackStopped();
}