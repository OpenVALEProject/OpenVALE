using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public static class ConfigurationUtil  {
    public enum CursorAttachment{none,hand, hmd};
    public enum CursorType { none, crosshair, snapped};
    public enum AudioEngineType {SLABServer,AudioServer3}

	public static string HRTFDir = ".\\HRTFs";
	//public static string HRTFName = "s81HT_FG_HD280_E100.slh";
	public static string wavDir = ".\\WaveFiles";
	public static string wavName = "whitenoise1s.wav";
	public static string channelMap = "0,1,2,3,4,5";
	public static string outChannelMap = "0,1";
	public static string sigGenPath = "";
	public static string FIRTaps = "256";
	public static bool useRift = false;
    public static bool useVive = false;
	public static bool isDebug = true;
	public static string IODevice = "";
    public static bool isUseWand = false;
    public static bool isTestMode = false;
    public static string inputDevice = "W";
    public static string spatialAudioServer = "";
    public static string levelName = "OpenVALE";
    public static bool useSpatialAudioServer = true;
    public static CursorType currentCursorType = CursorType.none;
    public static CursorAttachment currentCursorAttachment = CursorAttachment.none;
    public static bool waitingForResponse = false;
    public static Socket waitingClient = null;
    public static float waitStartTime = 0.0f;
    public static bool waitingForRecenter = false;
    public static Vector3 recenterPosition = Vector3.zero;
    public static float recenterTolerance = 0;
    public static AudioEngineType engineType = AudioEngineType.SLABServer;

}
