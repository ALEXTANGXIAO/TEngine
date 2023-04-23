// ----------------------------------------
//
//  BuglyAgent.cs
//
//  Author:
//       Yeelik, <bugly@tencent.com>
//
//  Copyright (c) 2015 Bugly, Tencent. All rights reserved.
//
// ----------------------------------------
//
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using System.Runtime.InteropServices;

// We dont use the LogType enum in Unity as the numerical order doesnt suit our purposes
/// <summary>
/// Log severity. 
/// { Log, LogDebug, LogInfo, LogWarning, LogAssert, LogError, LogException }
/// </summary>
public enum LogSeverity
{
    Log,
    LogDebug,
    LogInfo,
    LogWarning,
    LogAssert,
    LogError,
    LogException
}

/// <summary>
/// Bugly agent.
/// </summary>
public sealed class BuglyAgent
{
    
    // Define delegate support multicasting to replace the 'Application.LogCallback'
    public delegate void LogCallbackDelegate (string condition,string stackTrace,LogType type);

    /// <summary>
    /// Configs the type of the crash reporter and customized log level to upload
    /// </summary>
    /// <param name="type">Type. Default=0, 1=Bugly v2.x MSDK=2</param>
    /// <param name="logLevel">Log level. Off=0,Error=1,Warn=2,Info=3,Debug=4</param>
    public static void ConfigCrashReporter(int type, int logLevel){
        _SetCrashReporterType (type);
        _SetCrashReporterLogLevel (logLevel);
    }

    /// <summary>
    /// Init sdk with the specified appId. 
    /// <para>This will initialize sdk to report native exception such as obj-c, c/c++, java exceptions, and also enable c# exception handler to report c# exception logs</para>
    /// </summary>
    /// <param name="appId">App identifier.</param>
    public static void InitWithAppId (string appId)
    {
        if (IsInitialized) {
            DebugLog (null, "BuglyAgent has already been initialized.");
            
            return;
        }
        
        if (string.IsNullOrEmpty (appId)) {
            return;
        }
        
        // init the sdk with app id
        InitBuglyAgent (appId);
        DebugLog (null, "Initialized with app id: {0}", appId);
        
        // Register the LogCallbackHandler by Application.RegisterLogCallback(Application.LogCallback)
        _RegisterExceptionHandler ();
    }
    
    /// <summary>
    /// Only Enable the C# exception handler. 
    /// 
    /// <para>
    /// You can call it when you do not call the 'InitWithAppId(string)', but you must make sure initialized the sdk in elsewhere, 
    /// such as the native code in associated Android or iOS project.
    /// </para>
    /// 
    /// <para>
    /// Default Level is <c>LogError</c>, so the LogError, LogException will auto report.
    /// </para>
    /// 
    /// <para>
    /// You can call the method <code>BuglyAgent.ConfigAutoReportLogLevel(LogSeverity)</code>
    /// to change the level to auto report if you known what are you doing.
    /// </para>
    /// 
    /// </summary>
    public static void EnableExceptionHandler ()
    {
        if (IsInitialized) {
            DebugLog (null, "BuglyAgent has already been initialized.");
            return;
        }
        
        DebugLog (null, "Only enable the exception handler, please make sure you has initialized the sdk in the native code in associated Android or iOS project.");
        
        // Register the LogCallbackHandler by Application.RegisterLogCallback(Application.LogCallback)
        _RegisterExceptionHandler ();
    }
    
    /// <summary>
    /// Registers the log callback handler. 
    /// 
    /// If you need register logcallback using Application.RegisterLogCallback(LogCallback),
    /// you can call this method to replace it.
    /// 
    /// <para></para>
    /// </summary>
    /// <param name="handler">Handler.</param>
    public static void RegisterLogCallback (LogCallbackDelegate handler)
    {
        if (handler != null) {
            DebugLog (null, "Add log callback handler: {0}", handler);
            
            _LogCallbackEventHandler += handler;
        }
    }
    
    /// <summary>
    /// Sets the log callback extras handler.
    /// </summary>
    /// <param name="handler">Handler.</param>
    public static void SetLogCallbackExtrasHandler(Func<Dictionary<string, string>> handler){
        if (handler != null) {
            _LogCallbackExtrasHandler = handler;
            
            DebugLog(null, "Add log callback extra data handler : {0}", handler);
        }
    }
    
    /// <summary>
    /// Reports the exception.
    /// </summary>
    /// <param name="e">E.</param>
    /// <param name="message">Message.</param>
    public static void ReportException (System.Exception e, string message)
    {
        if (!IsInitialized) {
            return;
        }
        
        DebugLog (null, "Report exception: {0}\n------------\n{1}\n------------", message, e);
        
        _HandleException (e, message, false);
    }
    
    /// <summary>
    /// Reports the exception.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="message">Message.</param>
    /// <param name="stackTrace">Stack trace.</param>
    public static void ReportException (string name, string message, string stackTrace)
    {
        if (!IsInitialized) {
            return;
        }
        
        DebugLog (null, "Report exception: {0} {1} \n{2}", name, message, stackTrace);
        
        _HandleException (LogSeverity.LogException, name, message, stackTrace, false);
    }
    
    /// <summary>
    /// Unregisters the log callback.
    /// </summary>
    /// <param name="handler">Handler.</param>
    public static void UnregisterLogCallback (LogCallbackDelegate handler)
    {
        if (handler != null) {
            DebugLog (null, "Remove log callback handler");
            
            _LogCallbackEventHandler -= handler;
        }
    }
    
    /// <summary>
    /// Sets the user identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    public static void SetUserId (string userId)
    {
        if (!IsInitialized) {
            return;
        }
        DebugLog (null, "Set user id: {0}", userId);
        
        SetUserInfo (userId);
    }
    
    /// <summary>
    /// Sets the scene.
    /// </summary>
    /// <param name="sceneId">Scene identifier.</param>
    public static void SetScene (int sceneId)
    {
        if (!IsInitialized) {
            return;
        }
        DebugLog (null, "Set scene: {0}", sceneId);
        
        SetCurrentScene (sceneId);
    }
    
    /// <summary>
    /// Adds the scene data.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public static void AddSceneData (string key, string value)
    {
        if (!IsInitialized) {
            return;
        }
        
        DebugLog (null, "Add scene data: [{0}, {1}]", key, value);
        
        AddKeyAndValueInScene (key, value);
    }
    
    /// <summary>
    /// Configs the debug mode.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> debug mode.</param>
    public static void ConfigDebugMode (bool enable)
    {
        EnableDebugMode (enable);
        DebugLog (null, "{0} the log message print to console", enable ? "Enable" : "Disable");
    }
    
    /// <summary>
    /// Configs the auto quit application.
    /// </summary>
    /// <param name="autoQuit">If set to <c>true</c> auto quit.</param>
    public static void ConfigAutoQuitApplication (bool autoQuit)
    {
        _autoQuitApplicationAfterReport = autoQuit;
    }
    
    /// <summary>
    /// Configs the auto report log level. Default is LogSeverity.LogError.
    /// <example>
    /// LogSeverity { Log, LogDebug, LogInfo, LogWarning, LogAssert, LogError, LogException }
    /// </example>
    /// </summary>
    /// 
    /// <param name="level">Level.</param> 
    public static void ConfigAutoReportLogLevel (LogSeverity level)
    {
        _autoReportLogLevel = level;
    }
    
    /// <summary>
    /// Configs the default.
    /// </summary>
    /// <param name="channel">Channel.</param>
    /// <param name="version">Version.</param>
    /// <param name="user">User.</param>
    /// <param name="delay">Delay.</param>
    public static void ConfigDefault (string channel, string version, string user, long delay)
    {
        DebugLog (null, "Config default channel:{0}, version:{1}, user:{2}, delay:{3}", channel, version, user, delay);
        ConfigDefaultBeforeInit (channel, version, user, delay);
    }
    
    /// <summary>
    /// Logs the debug.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void DebugLog (string tag, string format, params object[] args)
    {
        if(!_debugMode) {
            return;
        }

        if (string.IsNullOrEmpty (format)) {
            return;
        }
        
        Console.WriteLine ("[BuglyAgent] <Debug> - {0} : {1}", tag, string.Format (format, args));
    }
    
    /// <summary>
    /// Prints the log.
    /// </summary>
    /// <param name="level">Level.</param>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void PrintLog (LogSeverity level, string format, params object[] args)
    {
        if (string.IsNullOrEmpty (format)) {
            return;
        }
        
        LogRecord (level, string.Format (format, args));
    }
    
    #if UNITY_EDITOR || UNITY_STANDALONE
    
    #region Interface(Empty) in Editor 
    private static void InitBuglyAgent (string appId)
    {
    }
    
    private static void ConfigDefaultBeforeInit(string channel, string version, string user, long delay){
    }
    
    private static void EnableDebugMode(bool enable){
    }
    
    private static void SetUserInfo(string userInfo){
    }
    
    private static void ReportException (int type,string name, string message, string stackTrace, bool quitProgram)
    {
    }
    
    private static void SetCurrentScene(int sceneId) {
    }
    
    private static void AddKeyAndValueInScene(string key, string value){
    }
    
    private static void AddExtraDataWithException(string key, string value) {
        // only impl for iOS
    }
    
    private static void LogRecord(LogSeverity level, string message){
    }
    
    private static void SetUnityVersion(){
        
    }
    #endregion
    
    #elif UNITY_ANDROID
    //  #if UNITY_ANDROID
    
    #region Interface for Android
    private static readonly string GAME_AGENT_CLASS = "com.tencent.bugly.agent.GameAgent";
    private static readonly int TYPE_U3D_CRASH = 4;
    private static readonly int GAME_TYPE_UNITY = 2;
    private static bool hasSetGameType = false;
    private static AndroidJavaClass _gameAgentClass = null;
    
    public static AndroidJavaClass GameAgent {
        get {
            if (_gameAgentClass == null) {
                _gameAgentClass = new AndroidJavaClass(GAME_AGENT_CLASS);
//                using (AndroidJavaClass clazz = new AndroidJavaClass(CLASS_UNITYAGENT)) {
//                    _gameAgentClass = clazz.CallStatic<AndroidJavaObject> ("getInstance");
//                }
            }
            if (!hasSetGameType) {
                // set game type: unity(2).
                _gameAgentClass.CallStatic ("setGameType", GAME_TYPE_UNITY);
                hasSetGameType = true;
            }
            return _gameAgentClass;
        }
    }
    
    private static string _configChannel;
    private static string _configVersion;
    private static string _configUser;
    private static long _configDelayTime;
    
    private static void ConfigDefaultBeforeInit(string channel, string version, string user, long delay){
        _configChannel = channel;
        _configVersion = version;
        _configUser = user;
        _configDelayTime = delay;
    }

    private static bool _configCrashReporterPackage = false;
    
    private static void ConfigCrashReporterPackage(){
        
        if (!_configCrashReporterPackage) {
            try {
                GameAgent.CallStatic("setSdkPackageName", _crashReporterPackage);
                _configCrashReporterPackage = true;
            } catch {
                
            }
        }
        
    }

    private static void InitBuglyAgent(string appId)
    {
        if (IsInitialized) {
            return;
        }
        
        ConfigCrashReporterPackage();

        try {
            GameAgent.CallStatic("initCrashReport", appId, _configChannel, _configVersion, _configUser, _configDelayTime);
            _isInitialized = true;
        } catch {
            
        }
    }
    
    private static void EnableDebugMode(bool enable){
        _debugMode = enable;

        ConfigCrashReporterPackage();

        try {
            GameAgent.CallStatic("setLogEnable", enable);
        } catch {
            
        }
    }
    
    private static void SetUserInfo(string userInfo){
        ConfigCrashReporterPackage();

        try {
            GameAgent.CallStatic("setUserId", userInfo);
        } catch {
        }
    }
    
    private static void ReportException (int type, string name, string reason, string stackTrace, bool quitProgram)
    {
        ConfigCrashReporterPackage();

        try {
            GameAgent.CallStatic("postException", TYPE_U3D_CRASH, name, reason, stackTrace, quitProgram);
        } catch {
            
        }
    }
    
    private static void SetCurrentScene(int sceneId) {
        ConfigCrashReporterPackage();

        try {
            GameAgent.CallStatic("setUserSceneTag", sceneId);
        } catch {
            
        }
    }
    
    private static void SetUnityVersion(){
        ConfigCrashReporterPackage();

        try {
            GameAgent.CallStatic("setSdkConfig", "UnityVersion", Application.unityVersion);
        } catch {
            
        }
    }
    
    private static void AddKeyAndValueInScene(string key, string value){
        ConfigCrashReporterPackage();

        try {
            GameAgent.CallStatic("putUserData", key, value);
        } catch {
            
        }
    }
    
    private static void AddExtraDataWithException(string key, string value) {
        // no impl
    }
    
    private static void LogRecord(LogSeverity level, string message){
        if (level < LogSeverity.LogWarning) {
            DebugLog (level.ToString (), message);
        }

        ConfigCrashReporterPackage();
        
        try {
            GameAgent.CallStatic("printLog", string.Format ("<{0}> - {1}", level.ToString (), message));
        } catch {
            
        }
    }
    
    #endregion
    
    #elif UNITY_IPHONE || UNITY_IOS
    
    #region Interface for iOS
    
    private static bool _crashReporterTypeConfiged = false;

    private static void ConfigCrashReporterType(){
        if (!_crashReporterTypeConfiged) {
            try {
                _BuglyConfigCrashReporterType(_crashReporterType);
                _crashReporterTypeConfiged = true;
             } catch {
                
            }
        }
    }

    private static void ConfigDefaultBeforeInit(string channel, string version, string user, long delay){
        ConfigCrashReporterType();
        
        try {
            _BuglyDefaultConfig(channel, version, user, null);
        } catch {
            
        }
    }
    
    private static void EnableDebugMode(bool enable){
        _debugMode = enable;
    }
    
    private static void InitBuglyAgent (string appId)
    {
        ConfigCrashReporterType();
        
        if(!string.IsNullOrEmpty(appId)) {
            
            _BuglyInit(appId, _debugMode, _crashReproterCustomizedLogLevel); // Log level 
        }
    }
    
    private static void SetUnityVersion(){
        ConfigCrashReporterType();
        
        _BuglySetExtraConfig("UnityVersion", Application.unityVersion);
    }
    
    private static void SetUserInfo(string userInfo){
        if(!string.IsNullOrEmpty(userInfo)) {
            ConfigCrashReporterType();
            
            _BuglySetUserId(userInfo);
        }
    }
    
    private static void ReportException (int type, string name, string reason, string stackTrace, bool quitProgram)
    {
        ConfigCrashReporterType();
        
        string extraInfo = "";
        Dictionary<string, string> extras = null;
        if (_LogCallbackExtrasHandler != null) {
            extras = _LogCallbackExtrasHandler();
        }
        if (extras == null || extras.Count == 0) {
            extras = new Dictionary<string, string> ();
            extras.Add ("UnityVersion", Application.unityVersion);
        }
        
        if (extras != null && extras.Count > 0) {
            if (!extras.ContainsKey("UnityVersion")) {
                extras.Add ("UnityVersion", Application.unityVersion);
            }
            
            StringBuilder builder = new StringBuilder();
            foreach(KeyValuePair<string,string> kvp in extras){
                builder.Append(string.Format("\"{0}\" : \"{1}\"", kvp.Key, kvp.Value)).Append(" , ");
            }
            extraInfo = string.Format("{{ {0} }}", builder.ToString().TrimEnd(" , ".ToCharArray()));
        }
        
        // 4 is C# exception
        _BuglyReportException(4, name, reason, stackTrace, extraInfo, quitProgram);
    }
    
    private static void SetCurrentScene(int sceneId) {
        ConfigCrashReporterType();
        
        _BuglySetTag(sceneId);
    }
    
    private static void AddKeyAndValueInScene(string key, string value){
        ConfigCrashReporterType();
        
        _BuglySetKeyValue(key, value);
    }
    
    private static void AddExtraDataWithException(string key, string value) {
        
    }
    
    private static void LogRecord(LogSeverity level, string message){
        if (level < LogSeverity.LogWarning) {
            DebugLog (level.ToString (), message);
        }
        
        ConfigCrashReporterType();
        
        _BuglyLogMessage(LogSeverityToInt(level), null, message);
    }
    
    private static int LogSeverityToInt(LogSeverity logLevel){
        int level = 5;
        switch(logLevel) {
        case LogSeverity.Log:
            level = 5;
            break;
        case LogSeverity.LogDebug:
            level = 4;
            break;
        case LogSeverity.LogInfo:
            level = 3;
            break;
        case LogSeverity.LogWarning:
        case LogSeverity.LogAssert:
            level = 2;
            break;
        case LogSeverity.LogError:
        case LogSeverity.LogException:
            level = 1;
            break;
        default:
            level = 0;
            break;
        }
        return level;
    }
    
    // --- dllimport start ---
    [DllImport("__Internal")]
    private static extern void _BuglyInit(string appId, bool debug, int level);
    
    [DllImport("__Internal")]
    private static extern void _BuglySetUserId(string userId);
    
    [DllImport("__Internal")]
    private static extern void _BuglySetTag(int tag);
    
    [DllImport("__Internal")]
    private static extern void _BuglySetKeyValue(string key, string value);
    
    [DllImport("__Internal")]
    private static extern void _BuglyReportException(int type, string name, string reason, string stackTrace, string extras, bool quit);
    
    [DllImport("__Internal")]
    private static extern void _BuglyDefaultConfig(string channel, string version, string user, string deviceId);
    
    [DllImport("__Internal")]
    private static extern void _BuglyLogMessage(int level, string tag, string log);
    
    [DllImport("__Internal")]
    private static extern void _BuglyConfigCrashReporterType(int type);
    
    [DllImport("__Internal")]
    private static extern void _BuglySetExtraConfig(string key, string value);
    
    // dllimport end
    #endregion
    
    #endif
    
    #region Privated Fields and Methods 
    private static event LogCallbackDelegate _LogCallbackEventHandler;
    
    private static bool _isInitialized = false;
    private static LogSeverity _autoReportLogLevel = LogSeverity.LogError;

    private static int _crashReporterType = 1;  // Default=0,1=Bugly-V2,MSDKBugly=2, IMSDKBugly=3

#if UNITY_ANDROID
    // The crash reporter package name, default is 'com.tencent.bugly'
#pragma warning disable CS0414
    private static string _crashReporterPackage = "com.tencent.bugly";
#pragma warning restore CS0414
#endif
#if UNITY_IPHONE || UNITY_IOS
    private static int _crashReproterCustomizedLogLevel = 2; // Off=0,Error=1,Warn=2,Info=3,Debug=4
#endif

    #pragma warning disable 414
    private static bool _debugMode = false;
    private static bool _autoQuitApplicationAfterReport = false;
    
    private static readonly int EXCEPTION_TYPE_UNCAUGHT = 1;
    private static readonly int EXCEPTION_TYPE_CAUGHT = 2;
    private static readonly string _pluginVersion = "1.5.1";
    
    private static Func<Dictionary<string, string>> _LogCallbackExtrasHandler;
    
    public static string PluginVersion {
        get { return _pluginVersion; }
    }
    
    public static bool IsInitialized {
        get { return _isInitialized; }
    }
    
    public static bool AutoQuitApplicationAfterReport {
        get { return _autoQuitApplicationAfterReport; }
    }

    private static void _SetCrashReporterType(int type){
        _crashReporterType = type;

        if (_crashReporterType == 2) {
#if UNITY_ANDROID
            _crashReporterPackage = "com.tencent.bugly.msdk";
#endif
        }

    }

    private static void _SetCrashReporterLogLevel(int logLevel){
#if UNITY_IPHONE || UNITY_IOS
        _crashReproterCustomizedLogLevel = logLevel;
#endif
    }

    private static void _RegisterExceptionHandler ()
    {
        try {
            // hold only one instance 
            
            #if UNITY_5
            Application.logMessageReceived += _OnLogCallbackHandler;
            #else
#pragma warning disable CS0618
            Application.RegisterLogCallback (_OnLogCallbackHandler);
#pragma warning restore CS0618
#endif
            AppDomain.CurrentDomain.UnhandledException += _OnUncaughtExceptionHandler;
            
            _isInitialized = true;
            
            DebugLog (null, "Register the log callback in Unity {0}", Application.unityVersion);
        } catch {
            
        }
        
        SetUnityVersion ();
    }
    
    private static void _UnregisterExceptionHandler ()
    {
        try {
            #if UNITY_5
            Application.logMessageReceived -= _OnLogCallbackHandler;
            #else
#pragma warning disable CS0618
            Application.RegisterLogCallback (null);
#pragma warning restore CS0618
#endif
            System.AppDomain.CurrentDomain.UnhandledException -= _OnUncaughtExceptionHandler;
            DebugLog (null, "Unregister the log callback in unity {0}", Application.unityVersion);
        } catch {
            
        }
    }
    
    private static void _OnLogCallbackHandler (string condition, string stackTrace, LogType type)
    {
        if (_LogCallbackEventHandler != null) {
            _LogCallbackEventHandler (condition, stackTrace, type);
        }
        
        if (!IsInitialized) {
            return;
        }
        
        if (!string.IsNullOrEmpty (condition) && condition.Contains ("[BuglyAgent] <Log>")) {
            return;
        }
        
        if (_uncaughtAutoReportOnce) {
            return;
        }
        
        // convert the log level
        LogSeverity logLevel = LogSeverity.Log;
        switch (type) {
        case LogType.Exception:
            logLevel = LogSeverity.LogException;
            break;
        case LogType.Error:
            logLevel = LogSeverity.LogError;
            break;
        case LogType.Assert:
            logLevel = LogSeverity.LogAssert;
            break;
        case LogType.Warning:
            logLevel = LogSeverity.LogWarning;
            break;
        case LogType.Log:
            logLevel = LogSeverity.LogDebug;
            break;
        default:
            break;
        }
        
        if (LogSeverity.Log == logLevel) {
            return;
        }
        
        _HandleException (logLevel, null, condition, stackTrace, true);
    }
    
    private static void _OnUncaughtExceptionHandler (object sender, System.UnhandledExceptionEventArgs args)
    {
        if (args == null || args.ExceptionObject == null) {
            return;
        }
        
        try {
            if (args.ExceptionObject.GetType () != typeof(System.Exception)) {
                return;
            }
        } catch {
            if (UnityEngine.Debug.isDebugBuild == true) {
                UnityEngine.Debug.Log ("BuglyAgent: Failed to report uncaught exception");
            }
            
            return;
        }
        
        if (!IsInitialized) {
            return;
        }
        
        if (_uncaughtAutoReportOnce) {
            return;
        }
        
        _HandleException ((System.Exception)args.ExceptionObject, null, true);
    }
    
    private static void _HandleException (System.Exception e, string message, bool uncaught)
    {
        if (e == null) {
            return;
        }
        
        if (!IsInitialized) {
            return;
        }
        
        string name = e.GetType ().Name;
        string reason = e.Message;
        
        if (!string.IsNullOrEmpty (message)) {
            reason = string.Format ("{0}{1}***{2}", reason, Environment.NewLine, message);
        }
        
        StringBuilder stackTraceBuilder = new StringBuilder ("");
        
        StackTrace stackTrace = new StackTrace (e, true);
        int count = stackTrace.FrameCount;
        for (int i = 0; i < count; i++) {
            StackFrame frame = stackTrace.GetFrame (i);
            
            stackTraceBuilder.AppendFormat ("{0}.{1}", frame.GetMethod ().DeclaringType.Name, frame.GetMethod ().Name);
            
            ParameterInfo[] parameters = frame.GetMethod ().GetParameters ();
            if (parameters == null || parameters.Length == 0) {
                stackTraceBuilder.Append (" () ");
            } else {
                stackTraceBuilder.Append (" (");
                
                int pcount = parameters.Length;
                
                ParameterInfo param = null;
                for (int p = 0; p < pcount; p++) {
                    param = parameters [p];
                    stackTraceBuilder.AppendFormat ("{0} {1}", param.ParameterType.Name, param.Name);
                    
                    if (p != pcount - 1) {
                        stackTraceBuilder.Append (", ");
                    }
                }
                param = null;
                
                stackTraceBuilder.Append (") ");
            }
            
            string fileName = frame.GetFileName ();
            if (!string.IsNullOrEmpty (fileName) && !fileName.ToLower ().Equals ("unknown")) {
                fileName = fileName.Replace ("\\", "/");
                
                int loc = fileName.ToLower ().IndexOf ("/assets/");
                if (loc < 0) {
                    loc = fileName.ToLower ().IndexOf ("assets/");
                }
                
                if (loc > 0) {
                    fileName = fileName.Substring (loc);
                }
                
                stackTraceBuilder.AppendFormat ("(at {0}:{1})", fileName, frame.GetFileLineNumber ());
            }
            stackTraceBuilder.AppendLine ();
        }
        
        // report
        _reportException (uncaught, name, reason, stackTraceBuilder.ToString ());
    }
    
    private static void _reportException (bool uncaught, string name, string reason, string stackTrace)
    {
        if (string.IsNullOrEmpty (name)) {
            return;
        }
        
        if (string.IsNullOrEmpty (stackTrace)) {
            stackTrace = StackTraceUtility.ExtractStackTrace ();
        }
        
        if (string.IsNullOrEmpty (stackTrace)) {
            stackTrace = "Empty";           
        } else {
            
            try {
                string[] frames = stackTrace.Split ('\n');
                
                if (frames != null && frames.Length > 0) {
                    
                    StringBuilder trimFrameBuilder = new StringBuilder ();
                    
                    string frame = null;
                    int count = frames.Length;
                    for (int i = 0; i < count; i++) {
                        frame = frames [i];
                        
                        if (string.IsNullOrEmpty (frame) || string.IsNullOrEmpty (frame.Trim ())) {
                            continue;
                        }
                        
                        frame = frame.Trim ();
                        
                        // System.Collections.Generic
                        if (frame.StartsWith ("System.Collections.Generic.") || frame.StartsWith ("ShimEnumerator")) {
                            continue;
                        }
                        if (frame.StartsWith ("Bugly")) {
                            continue;
                        }
                        if (frame.Contains ("..ctor")) {
                            continue;
                        }
                        
                        int start = frame.ToLower ().IndexOf ("(at");
                        int end = frame.ToLower ().IndexOf ("/assets/");
                        
                        if (start > 0 && end > 0) {
                            trimFrameBuilder.AppendFormat ("{0}(at {1}", frame.Substring (0, start).Replace (":", "."), frame.Substring (end));
                        } else {
                            trimFrameBuilder.Append (frame.Replace (":", "."));
                        }
                        
                        trimFrameBuilder.AppendLine ();
                    }
                    
                    stackTrace = trimFrameBuilder.ToString ();
                }
            } catch {
                PrintLog(LogSeverity.LogWarning,"{0}", "Error to parse the stack trace");
            }
            
        }
        
        PrintLog (LogSeverity.LogError, "ReportException: {0} {1}\n*********\n{2}\n*********", name, reason, stackTrace);
        
        _uncaughtAutoReportOnce = uncaught && _autoQuitApplicationAfterReport;
        
        ReportException (uncaught ? EXCEPTION_TYPE_UNCAUGHT : EXCEPTION_TYPE_CAUGHT, name, reason, stackTrace, uncaught && _autoQuitApplicationAfterReport);
    }
    
    private static void _HandleException (LogSeverity logLevel, string name, string message, string stackTrace, bool uncaught)
    {
        if (!IsInitialized) {
            DebugLog (null, "It has not been initialized.");
            return;
        }
        
        if (logLevel == LogSeverity.Log) {
            return;
        }
        
        if ((uncaught && logLevel < _autoReportLogLevel)) {
            DebugLog (null, "Not report exception for level {0}", logLevel.ToString ());
            return;
        }
        
        string type = null;
        string reason = null;
        
        if (!string.IsNullOrEmpty (message)) {
            try {
                if ((LogSeverity.LogException == logLevel) && message.Contains ("Exception")) {
                    
                    Match match = new Regex (@"^(?<errorType>\S+):\s*(?<errorMessage>.*)", RegexOptions.Singleline).Match (message);
                    
                    if (match.Success) {
                        type = match.Groups ["errorType"].Value.Trim();
                        reason = match.Groups ["errorMessage"].Value.Trim ();
                    }
                } else if ((LogSeverity.LogError == logLevel) && message.StartsWith ("Unhandled Exception:")) {
                    
                    Match match = new Regex (@"^Unhandled\s+Exception:\s*(?<exceptionName>\S+):\s*(?<exceptionDetail>.*)", RegexOptions.Singleline).Match(message);
                    
                    if (match.Success) {
                        string exceptionName = match.Groups ["exceptionName"].Value.Trim();
                        string exceptionDetail = match.Groups ["exceptionDetail"].Value.Trim ();
                        
                        // 
                        int dotLocation = exceptionName.LastIndexOf(".");
                        if (dotLocation > 0 && dotLocation != exceptionName.Length) {
                            type = exceptionName.Substring(dotLocation + 1);
                        } else {
                            type = exceptionName;
                        }
                        
                        int stackLocation = exceptionDetail.IndexOf(" at ");
                        if (stackLocation > 0) {
                            // 
                            reason = exceptionDetail.Substring(0, stackLocation);
                            // substring after " at "
                            string callStacks = exceptionDetail.Substring(stackLocation + 3).Replace(" at ", "\n").Replace("in <filename unknown>:0","").Replace("[0x00000]","");
                            //
                            stackTrace = string.Format("{0}\n{1}", stackTrace, callStacks.Trim());
                            
                        } else {
                            reason = exceptionDetail;
                        }
                        
                        // for LuaScriptException
                        if(type.Equals("LuaScriptException") && exceptionDetail.Contains(".lua") && exceptionDetail.Contains("stack traceback:")) {
                            stackLocation = exceptionDetail.IndexOf("stack traceback:");
                            if(stackLocation > 0) {
                                reason = exceptionDetail.Substring(0, stackLocation);
                                // substring after "stack traceback:"
                                string callStacks = exceptionDetail.Substring(stackLocation + 16).Replace(" [", " \n[");
                                
                                //
                                stackTrace = string.Format("{0}\n{1}", stackTrace, callStacks.Trim());
                            }
                        }
                    }
                    
                }
            } catch {
                
            }
            
            if (string.IsNullOrEmpty (reason)) {
                reason = message;
            }
        }
        
        if (string.IsNullOrEmpty (name)) {
            if (string.IsNullOrEmpty (type)) {
                type = string.Format ("Unity{0}", logLevel.ToString ());
            }
        } else {
            type = name;
        }
        
        _reportException (uncaught, type, reason, stackTrace);
    }
    
    private static bool _uncaughtAutoReportOnce = false;
    
    #endregion
    
}