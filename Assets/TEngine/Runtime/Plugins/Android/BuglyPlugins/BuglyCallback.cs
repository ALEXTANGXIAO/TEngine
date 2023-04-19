// ----------------------------------------
//
//  BuglyCallbackDelegate.cs
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

public abstract class BuglyCallback
{
	// The delegate of callback handler which Call the Application.RegisterLogCallback(Application.LogCallback)
	/// <summary>
	/// Raises the application log callback handler event.
	/// </summary>
	/// <param name="condition">Condition.</param>
	/// <param name="stackTrace">Stack trace.</param>
	/// <param name="type">Type.</param>
	public abstract void OnApplicationLogCallbackHandler (string condition, string stackTrace, LogType type);

}

