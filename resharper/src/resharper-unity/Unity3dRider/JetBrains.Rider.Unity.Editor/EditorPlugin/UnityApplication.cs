using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Platform.RdFramework;
using JetBrains.Util.Logging;
using UnityEditor;
using UnityEngine;

namespace JetBrains.Rider.Unity.Editor
{
  public class UnityApplication
  {
    private readonly RiderProtocolController myProtocolController;
    private readonly MainThreadDispatcher myMainThreadScheduler;
    public UnityApplication(RiderProtocolController controller, MainThreadDispatcher mainThreadScheduler)
    {
      myProtocolController = controller;
      myMainThreadScheduler = mainThreadScheduler;
    }
    
    private static readonly ILog Logger = Log.GetLog("UnityApplication");
    
    public static string GetExternalScriptEditor()
    {
      return EditorPrefs.GetString("kScriptsDefaultApp");
    }

    public static void SetExternalScriptEditor(string path)
    {
      EditorPrefs.SetString("kScriptsDefaultApp", path);
    }

    public void UnityLogRegisterCallBack()
    {
      var eventInfo = typeof(Application).GetEvent("logMessageReceived", BindingFlags.Static | BindingFlags.Public);
      if (eventInfo != null)
      {
        eventInfo.AddEventHandler(null, new Application.LogCallback(ApplicationOnLogMessageReceived));
        AppDomain.CurrentDomain.DomainUnload += (EventHandler) ((_, __) =>
        {
          eventInfo.RemoveEventHandler(null, new Application.LogCallback(ApplicationOnLogMessageReceived));
        });
      }
      else
      {
#pragma warning disable 612, 618
        Application.RegisterLogCallback(ApplicationOnLogMessageReceived);
#pragma warning restore 612, 618
      }
    }

    private void ApplicationOnLogMessageReceived(string message, string stackTrace, LogType type)
    {
      if (Settings.SendConsoleToRider)
      {
        if (myProtocolController == null)
          return;
        // use Protocol to pass log entries to Rider
        myMainThreadScheduler.InvokeOrQueue(() =>
        {
          if (myProtocolController.Model != null)
          {
            switch (type)
            {
              case LogType.Error:
              case LogType.Exception:
                SentLogEvent(message, stackTrace, RdLogEventType.Error);
                break;
              case LogType.Warning:
                SentLogEvent(message, stackTrace, RdLogEventType.Warning);
                break;
              default:
                SentLogEvent(message, stackTrace, RdLogEventType.Message);
                break;
            }
          }
        });
      }
    }
    
    private void SentLogEvent(string message, string stackTrace, RdLogEventType type)
    {
      if (!message.StartsWith("[Rider][TRACE]")) // avoid sending because in Trace mode log about sending log event to Rider, will also appear in unity log
        myProtocolController.Model.LogModelInitialized.Value.Log.Fire(new RdLogEvent(type, message, stackTrace));
    }

    /// <summary>
    /// Force Unity To Write Project File
    /// </summary>
    public static void SyncSolution()
    {
      var T = Type.GetType("UnityEditor.SyncVS,UnityEditor");
      var syncSolution = T.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.Static);
      syncSolution.Invoke(null, null);
    }

    internal static Version UnityVersion
    {
      get
      {
        var ver = Application.unityVersion.Split(".".ToCharArray()).Take(2).Aggregate((a, b) => a + "." + b);
        Logger.Verbose("Unity version: " + ver);
        return new Version(ver);
      }
    }
    
    public static void AddRiderToRecentlyUsedScriptApp(string userAppPath, string recentAppsKey)
    {
      for (int index = 0; index < 10; ++index)
      {
        string path = EditorPrefs.GetString(recentAppsKey + (object) index);
        if (File.Exists(path) && Path.GetFileName(path).ToLower().Contains("rider"))
          return;
      }
      EditorPrefs.SetString(recentAppsKey + 9, userAppPath);
    }
  }
}