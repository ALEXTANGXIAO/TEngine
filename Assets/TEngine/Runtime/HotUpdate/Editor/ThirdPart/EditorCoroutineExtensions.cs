/*
Copyright (c) 2017 Marijn Zwemmer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using UnityEditor;

namespace Huatuo.Editor.ThirdPart
{
	public static class EditorCoroutineExtensions
	{
		public static EditorCoroutines.EditorCoroutine StartCoroutine(this EditorWindow thisRef, IEnumerator coroutine)
		{
			return EditorCoroutines.StartCoroutine(coroutine, thisRef);
		}

		public static EditorCoroutines.EditorCoroutine StartCoroutine(this EditorWindow thisRef, string methodName)
		{
			return EditorCoroutines.StartCoroutine(methodName, thisRef);
		}

		public static EditorCoroutines.EditorCoroutine StartCoroutine(this EditorWindow thisRef, string methodName,
			object value)
		{
			return EditorCoroutines.StartCoroutine(methodName, value, thisRef);
		}

		public static void StopCoroutine(this EditorWindow thisRef, IEnumerator coroutine)
		{
			EditorCoroutines.StopCoroutine(coroutine, thisRef);
		}

		public static void StopCoroutine(this EditorWindow thisRef, string methodName)
		{
			EditorCoroutines.StopCoroutine(methodName, thisRef);
		}

		public static void StopAllCoroutines(this EditorWindow thisRef)
		{
			EditorCoroutines.StopAllCoroutines(thisRef);
		}
	}
}
