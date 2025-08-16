using System.Collections;
using System.Collections.Generic;
using core.modules;
using static core.GameManager;
using UnityEngine;
using core.secure;
using System.Runtime.InteropServices;
using System;

namespace core.modules
{
    public class CodeFortManager : BaseModule
    {
        private IntPtr nativeInstance;

        // TODO
        // Communication with library
        // Server comms
        // Make it async

        public override void onInitialize()
        {
            // Initialize the systems required by CodeFort
            // Call up the dll for a session
            nativeInstance = CodeFort_Create();
            string sessionID = RequestSession();
            Debug.Log("Session loaded from DRM library: " + sessionID);
        }

        ~CodeFortManager()
        {
            CodeFort_Destroy(nativeInstance);
        }

        public void EmitSurfaceAlert()
        {

        }


        [DllImport("CodeFort")]
        private static extern IntPtr CodeFort_Create();

        [DllImport("CodeFort")]
        private static extern void CodeFort_Destroy(IntPtr instance);

        [DllImport("CodeFort")]
        private static extern IntPtr CodeFort_RequestSession(IntPtr instance);

        [DllImport("CodeFort")]
        private static extern bool CodeFort_RequestFileKey(IntPtr instance, string fileID, byte[] outKey, int keySize);

        [DllImport("CodeFort")]
        private static extern bool CodeFort_DecryptFile(IntPtr instance, string filePath, byte[] key, int keySize);

        public string RequestSession()
        {
            IntPtr strPtr = CodeFort_RequestSession(nativeInstance);
            if (strPtr == IntPtr.Zero) return null;
            return Marshal.PtrToStringAnsi(strPtr); // convert C string to C# string
        }

        public bool RequestFileKey(string fileID, byte[] key) => CodeFort_RequestFileKey(nativeInstance, fileID, key, key.Length);

        public bool DecryptFile(string filePath, byte[] key) => CodeFort_DecryptFile(nativeInstance, filePath, key, key.Length);

    }
}