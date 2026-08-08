using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System;

namespace Utilities
{
  /// <summary>
  /// 이 로거는 'BugReport.txt' 텍스트 파일에 기록한다
  /// </summary>
  public class TextFileLogger : LoggerImplementation
  {
    /// <summary>Logs the specified error.</summary>
    /// <param name="error">The error to log.</param>
    public override void LogError(string error)
    {
      string filename = Path.GetDirectoryName(Application.ExecutablePath);
      filename += string.Format(@"\Crash\BugReport_{0}.txt", DateTime.Now.ToString("yyyyMMddHHmm"));

      List<string> data = new List<string>();

      lock (this)
      {
        if (File.Exists(filename))
        {
          using (StreamReader reader = new StreamReader(filename))
          {
            string line = null;
            do
            {
              line = reader.ReadLine();
              data.Add(line);
            }
            while (line != null);
          }
        }

        // 파일이 너무 길면 잘라낸다
        int writeStart = 0;
        if (data.Count > 500)
          writeStart = data.Count - 500;

        using (StreamWriter stream = new StreamWriter(filename, false))
        {
          for (int i = writeStart; i < data.Count; i++)
          {
            stream.WriteLine(data[i]);
          }

          stream.Write(error);
        }
      }
    }
  }
}
