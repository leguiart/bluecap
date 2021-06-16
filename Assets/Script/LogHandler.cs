using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace Assets.Script
{
    public class LogHandler : ILogHandler
    {
        private FileStream m_FileStream;
        private StreamWriter m_StreamWriter;
        private ILogHandler m_DefaultLogHandler = Debug.unityLogger.logHandler;

        public LogHandler()
        {
            string filePath = Application.streamingAssetsPath + $"/bluecap_log-{DateTime.Now.ToString().Replace(":","_").Replace(@"/","-")}.txt";

            m_FileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            m_StreamWriter = new StreamWriter(m_FileStream);

            // Replace the default debug log handler
            Debug.unityLogger.logHandler = this;
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            m_StreamWriter.WriteLine(String.Format(format, args));
            m_StreamWriter.Flush();
            m_DefaultLogHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            m_DefaultLogHandler.LogException(exception, context);
        }
    }
}
