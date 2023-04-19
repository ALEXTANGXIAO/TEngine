// ----------------------------------------
//
//  BuglyInit.cs
//
//  Author:
//       Yeelik, <bugly@tencent.com>
//
//  Copyright (c) 2015 Bugly, Tencent.  All rights reserved.
//
// ----------------------------------------
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuglyInit : MonoBehaviour
{
	/// <summary>
	/// Your Bugly App ID. Every app has a special identifier that allows Bugly to associate error monitoring data with your app.
	/// Your App ID can be found on the "Setting" page of the app you are trying to monitor.
	/// </summary>
	/// <example>A real App ID looks like this: 90000xxxx</example>
	private const string BuglyAppID = "YOUR APP ID GOES HERE";

	void Awake ()
	{
		// Enable the debug log print
		BuglyAgent.ConfigDebugMode (false);
		// Config default channel, version, user 
		BuglyAgent.ConfigDefault (null, null, null, 0);
		// Config auto report log level, default is LogSeverity.LogError, so the LogError, LogException log will auto report
		BuglyAgent.ConfigAutoReportLogLevel (LogSeverity.LogError);
		// Config auto quit the application make sure only the first one c# exception log will be report, please don't set TRUE if you do not known what are you doing.
		BuglyAgent.ConfigAutoQuitApplication (false);
		// If you need register Application.RegisterLogCallback(LogCallback), you can replace it with this method to make sure your function is ok.
		BuglyAgent.RegisterLogCallback (null);

		// Init the bugly sdk and enable the c# exception handler.
		BuglyAgent.InitWithAppId (BuglyAppID);

        // TODO Required. If you do not need call 'InitWithAppId(string)' to initialize the sdk(may be you has initialized the sdk it associated Android or iOS project),
        // please call this method to enable c# exception handler only.
        BuglyAgent.EnableExceptionHandler ();
        
        // TODO NOT Required. If you need to report extra data with exception, you can set the extra handler
        BuglyAgent.SetLogCallbackExtrasHandler (MyLogCallbackExtrasHandler);

		Destroy (this);
	}

   // Extra data handler to packet data and report them with exception.
        // Please do not do hard work in this handler 
    static Dictionary<string, string> MyLogCallbackExtrasHandler ()
    {
        // TODO Test log, please do not copy it
        BuglyAgent.PrintLog (LogSeverity.Log, "extra handler");
        
        // TODO Sample code, please do not copy it
        Dictionary<string, string> extras = new Dictionary<string, string> ();
        extras.Add ("ScreenSolution", string.Format ("{0}x{1}", Screen.width, Screen.height));
        extras.Add ("deviceModel", SystemInfo.deviceModel);
        extras.Add ("deviceName", SystemInfo.deviceName);
        extras.Add ("deviceType", SystemInfo.deviceType.ToString ());
        
        extras.Add ("deviceUId", SystemInfo.deviceUniqueIdentifier);
        extras.Add ("gDId", string.Format ("{0}", SystemInfo.graphicsDeviceID));
        extras.Add ("gDName", SystemInfo.graphicsDeviceName);
        extras.Add ("gDVdr", SystemInfo.graphicsDeviceVendor);
        extras.Add ("gDVer", SystemInfo.graphicsDeviceVersion);
        extras.Add ("gDVdrID", string.Format ("{0}", SystemInfo.graphicsDeviceVendorID));
        
        extras.Add ("graphicsMemorySize", string.Format ("{0}", SystemInfo.graphicsMemorySize));
        extras.Add ("systemMemorySize", string.Format ("{0}", SystemInfo.systemMemorySize));
        extras.Add ("UnityVersion", Application.unityVersion);
        
        BuglyAgent.PrintLog (LogSeverity.LogInfo, "Package extra data");
        return extras;
    }
}

