using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Utilities
{
  /// <summary>Enumerated type that defines how users will be notified of exceptions</summary>
  public enum NotificationType
  {
    /// <summary>Users will not be notified, exceptions will be automatically logged to the registered loggers</summary>
    Silent,
    /// <summary>Users will be notified an exception has occurred, exceptions will be automatically logged to the registered loggers</summary>
    Inform,
    /// <summary>Users will be notified an exception has occurred and will be asked if they want the exception logged</summary>
    Ask
  }

  /// <summary>
  /// 오류를 여러 출력 대상에 기록하기 위한 추상 클래스. 주로 Windows Forms 응용 프로그램에서 사용한다.
  /// </summary>
  public abstract class LoggerImplementation
  {
    /// <summary>Logs the specified error.</summary>
    /// <param name="error">The error to log.</param>
    public abstract void LogError(string error);
  }

  /// <summary>
  /// 처리되지 않은 예외를 기록하는 클래스
  /// </summary>
  public class ExceptionLogger
  {
    /// <summary>
    /// ExceptionLogger 인스턴스를 새로 만든다
    /// </summary>
    public ExceptionLogger()
    {
      Application.ThreadException +=
        new System.Threading.ThreadExceptionEventHandler(OnThreadException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
      loggers = new List<LoggerImplementation>();
    }

    private List<LoggerImplementation> loggers;
    /// <summary>
    /// 사용할 로거 구현을 목록에 추가한다.
    /// </summary>
    /// <param name="logger">The logger to add.</param>
    public void AddLogger(LoggerImplementation logger)
    {
      loggers.Add(logger);
    }

    private NotificationType notificationType = NotificationType.Ask;
    /// <summary>
    /// 사용자에게 보여줄 알림 종류를 가져오거나 설정한다.
    /// </summary>
    public NotificationType NotificationType
    {
      get { return notificationType; }
      set { notificationType = value; }
    }

    delegate void LogExceptionDelegate(Exception e);
    private void HandleException(Exception e)
    {
        /*
      switch (notificationType)
      {
        case NotificationType.Ask :
          if (MessageBox.Show("An unexpected error occurred - " + e.Message +
          ". Do you wish to log the error?", "Error", MessageBoxButtons.YesNo) == DialogResult.No)
            return;
          break;
        case NotificationType.Inform :
          MessageBox.Show("An unexpected error occurred - " + e.Message);
          break;
        case NotificationType.Silent :
          break;
      }
        */
      LogExceptionDelegate logDelegate = new LogExceptionDelegate(LogException);
      logDelegate.BeginInvoke(e, new AsyncCallback(LogCallBack), null);
    }

    // 처리되지 않은 예외가 발생했을 때
    // 호출될 이벤트 핸들러
    private void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
      // 예외를 파일에 기록한다
      HandleException(e.Exception);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      HandleException((Exception)e.ExceptionObject);
    }

    private void LogCallBack(IAsyncResult result)
    {
      AsyncResult asyncResult = (AsyncResult)result;
      LogExceptionDelegate logDelegate = (LogExceptionDelegate)asyncResult.AsyncDelegate;
      if (!asyncResult.EndInvokeCalled)
      {
        logDelegate.EndInvoke(result);
      }
    }

    private string GetExceptionTypeStack(Exception e)
    {
      if (e.InnerException != null)
      {
        StringBuilder message = new StringBuilder();
        message.AppendLine(GetExceptionTypeStack(e.InnerException));
        message.AppendLine("   " + e.GetType().ToString());
        return (message.ToString());
      }
      else
      {
        return "   " + e.GetType().ToString();
      }
    }

    private string GetExceptionMessageStack(Exception e)
    {
      if (e.InnerException != null)
      {
        StringBuilder message = new StringBuilder();
        message.AppendLine(GetExceptionMessageStack(e.InnerException));
        message.AppendLine("   " + e.Message);
        return (message.ToString());
      }
      else
      {
        return "   " + e.Message;
      }
    }

    private string GetExceptionCallStack(Exception e)
    {
      if (e.InnerException != null)
      {
        StringBuilder message = new StringBuilder();
        message.AppendLine(GetExceptionCallStack(e.InnerException));
        message.AppendLine("--- Next Call Stack:");
        message.AppendLine(e.StackTrace);
        return (message.ToString());
      }
      else
      {
        return e.StackTrace;
      }
    }

    private static TimeSpan GetSystemUpTime()
    {
      PerformanceCounter upTime = new PerformanceCounter("System", "System Up Time");
      upTime.NextValue();
      return TimeSpan.FromSeconds(upTime.NextValue());
    }

    // 사용 가능한 메모리 조회용
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX 
    { 
      public uint dwLength; 
      public uint dwMemoryLoad; 
      public ulong ullTotalPhys; 
      public ulong ullAvailPhys; 
      public ulong ullTotalPageFile; 
      public ulong ullAvailPageFile; 
      public ulong ullTotalVirtual; 
      public ulong ullAvailVirtual; 
      public ulong ullAvailExtendedVirtual; 
      
      public MEMORYSTATUSEX() 
      { 
        this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); 
      } 
    }
    
    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    /// <summary>writes exception details to the registered loggers</summary>
    /// <param name="exception">The exception to log.</param>
    public void LogException(Exception exception)
    {
      StringBuilder error = new StringBuilder();

      error.AppendLine("Application:       " + Application.ProductName);
      error.AppendLine("Version:           " + Application.ProductVersion);
      error.AppendLine("Date:              " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
      error.AppendLine("Computer name:     " + SystemInformation.ComputerName);
      error.AppendLine("User name:         " + SystemInformation.UserName);
      error.AppendLine("OS:                " + Environment.OSVersion.ToString());
      error.AppendLine("Culture:           " + CultureInfo.CurrentCulture.Name);
      error.AppendLine("Resolution:        " + SystemInformation.PrimaryMonitorSize.ToString());
      error.AppendLine("System up time:    " + GetSystemUpTime());
      error.AppendLine("App up time:       " +
        (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString());

      MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX(); 
      if (GlobalMemoryStatusEx(memStatus)) 
      {
        error.AppendLine("Total memory:      " + memStatus.ullTotalPhys / (1024 * 1024) + "Mb");
        error.AppendLine("Available memory:  " + memStatus.ullAvailPhys / (1024 * 1024) + "Mb");
      }

      error.AppendLine("");

      error.AppendLine("Exception classes:   ");
      error.Append(GetExceptionTypeStack(exception));
      error.AppendLine("");
      error.AppendLine("Exception messages: ");
      error.Append(GetExceptionMessageStack(exception));

      error.AppendLine("");
      error.AppendLine("Stack Traces:");
      error.Append(GetExceptionCallStack(exception));
      error.AppendLine("");
      error.AppendLine("Loaded Modules:");
      Process thisProcess = Process.GetCurrentProcess();
      foreach (ProcessModule module in thisProcess.Modules)
      {
        error.AppendLine(module.FileName + " " + module.FileVersionInfo.FileVersion);
      }

      for (int i = 0; i < loggers.Count; i++)
      {
        loggers[i].LogError(error.ToString());
      }
    }
  }
}


