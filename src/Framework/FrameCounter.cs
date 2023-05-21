namespace VoxelGame.Framework
{
    /// <summary>
    /// Simple class for determining the frame rate.
    /// </summary>
    public class FrameCounter
    {
        public int FrameRate { get; private set; }

        private double _ticks;
        private int _frames;
        private float _updateInterval;
        private float _normalizeFactor;

        /// <param name="updateInterval">
        /// Duration to record the framerate for before updating <see cref="FrameRate"/> in ms.
        /// </param>
        public FrameCounter(float updateInterval)
        {
            _updateInterval = updateInterval;
            _normalizeFactor = 1f / updateInterval;
        }

        public void Sample(double frameTime)
        {
            _frames++;
            _ticks += frameTime;
            if (_ticks >= _updateInterval)
            {
                _ticks -= _updateInterval;
                FrameRate = (int)MathF.Round(_frames * _normalizeFactor);
                _frames = 0;
            }
        }
    }
}
