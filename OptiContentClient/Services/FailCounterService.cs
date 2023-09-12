namespace OptiContentClient.Services
{
    internal static class FailCounterService
    {
        private static string? _currentFailKey;
        private static int _currentFailCount;

        /// <summary>
        /// Reset fail-key and counter if no longer valid.
        /// </summary>
        private static void ValidateFailKey()
        {
            var failkey = DateTime.UtcNow.ToString("HHmm"); //fail-key is valid for one minute
            if (failkey != _currentFailKey)
            {
                _currentFailKey = failkey;
                _currentFailCount = 0;
            }
        }

        public static void AddFail()
        {
            ValidateFailKey();
            _currentFailCount++;
        }

        public static int FailCount
        {
            get
            {
                ValidateFailKey();
                return _currentFailCount;
            }
        }

    }
}
