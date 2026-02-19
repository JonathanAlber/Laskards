namespace Systems.Tweening.Core
{
    /// <summary>
    /// Interface for tween-like objects.
    /// </summary>
    public interface ITween
    {
        bool IsRunning { get; }
        bool IsCompleted { get; }

        void Start();
        void Stop();
        void Tick(float deltaTime);
    }
}
