namespace GameBattle.EcsSystem
{
    public static class BLogger
    {
        private static ILogger m_logger = new DefaultEcsLoggerHelper();
        
        public static ILogger Logger
        {
            get => m_logger;
            set => m_logger = value ?? throw new System.ArgumentNullException(nameof(value));
        }
        
        public static void SetLogLevel(bool debug, bool info, bool warning, bool error)
        {
            if (m_logger == null) return;
            {
                m_logger.SetLogLevel(debug, info, warning, error);
            }
        }
        
        public static void Log(object message)
        {
            if (m_logger == null) return;
            {
                m_logger.Log(message);
            }
        }
        
        public static void Info(object message)
        {
            if (m_logger == null) return;
            {
                m_logger.Info(message);
            }
        }
        
        public static void Warning(object message)
        {
            if (m_logger == null) return;
            {
                m_logger.Warning(message);
            }
        }
        
        public static void Error(object message)
        {
            if (m_logger == null) return;
            {
                m_logger.Error(message);
            }
        }
        
        public static void Exception(System.Exception exception)
        {
            if (m_logger == null) return;
            {
                m_logger.Exception(exception);
            }
        }
    }
}