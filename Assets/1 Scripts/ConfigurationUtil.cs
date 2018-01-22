﻿using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public static class ConfigurationUtil  {
    public enum CursorAttachment{none,hand, hmd};
    public enum CursorType { none, crosshair, snapped};

	public static string HRTFDir = "..\\..\\VirtualAudio\\HRTFs";
	//public static string HRTFName = "s81HT_FG_HD280_E100.slh";
	public static string wavDir = "..\\..\\VirtualAudio\\WaveFiles";
	public static string wavName = "whitenoise1s.wav";
	public static string outDevice = "";
	public static string channelMap = "0,1,2,3";
	public static string outChannelMap = "0,1";
	public static string sigGenPath = "";
	public static string FIRTaps = "256";
	public static bool useRift = false;
	public static bool isDebug = true;
	public static string IODevice = "";
    public static bool isUseWand = true;
    public static bool isTestMode = false;
    public static string inputDevice = "W";
    public static string spatialAudioServer = "";
    public static bool useSpatialAudioServer = true;
    public static CursorType currentCursorType = CursorType.none;
    public static CursorAttachment currentCursorAttachment = CursorAttachment.none; //CursorAttachment.hmd;
    public static bool waitingForResponse = false;
    public static Socket waitingClient = null;
    public static float waitStartTime = 0.0f;
    public static bool waitingForRecenter = false;
    public static Vector3 recenterPosition = Vector3.zero;
    public static float recenterTolerance = 0;


}
