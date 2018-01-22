using UnityEngine;
using System.Collections;
using System.Xml.XPath;

public class LevelLoader : MonoBehaviour
{

	// Use this for initialization
	public string defaultLevel = "OpenVALE";
    

	void Awake()
	{
        LogSystem.ClearLog();
	    Application.runInBackground = true;
		//Debug.Log("LEVEL LOADER CALLED!!!!!");
		//string[] filedata = System.IO.File.ReadAllLines(".\\OpenVALEConfig.xml");
        XPathDocument config = new XPathDocument(".\\OpenVALEConfig.xml");
        XPathNavigator nav = config.CreateNavigator();
        XPathNavigator nn = nav.SelectSingleNode("/configuration/applicationSettings");
        if (nn == null) return;

        bool status = nn.MoveToFirstChild();



        foreach (string line in filedata)
		{
			string[] options = line.Split(':');
			//Debug.Log(options[0]);
			switch (options[0])
			{
				case "SceneName":
					if (options.Length > 1)
					{
						defaultLevel = options[1].Trim();
					}
					break;



				case "WavPath":

					if (options.Length > 1)
					{
						ConfigurationUtil.wavDir = options[1].Trim();
					}

					break;
				case "HRTFPath":
					if (options.Length > 1)
					{
						ConfigurationUtil.HRTFDir = options[1].Trim();
					}
					break;
				case "sigGenPath":
					if (options.Length > 1)
					{
						ConfigurationUtil.sigGenPath = options[1].Trim();
					}
					break;
				case "HRTFFIRTap":
					if (options.Length > 1)
					{
						ConfigurationUtil.FIRTaps = options[1].Trim();
					}
					break;
				case "OutDevice":
					if (options.Length > 1)
					{
						ConfigurationUtil.outDevice = options[1].Trim();
					}
					break;
				case "ASIOInputMaps":
					if (options.Length > 1)
					{
						ConfigurationUtil.channelMap = options[1].Trim();
					}
					break;
				case "I/ODevice":
					if (options.Length > 1)
					{
						ConfigurationUtil.IODevice = options[1].Trim();
					}
					break;
				case "ASIOOutputMaps":
					if (options.Length > 1)
					{
						ConfigurationUtil.outChannelMap = options[1].Trim();

					} break;
				case "RiftOn":
                    //Debug.Log("Debug true");
					if (options.Length > 1)
					{

                        if (options[1].Trim().ToUpper().Equals("T"))
                        {
                            ConfigurationUtil.useRift = true;
                            //Debug.Log("Debug true");
                        }

                        else
                        {
                            ConfigurationUtil.useRift = false;
                            //Debug.Log("Debug false");
                        }
					}

					break;
                case "TestMode":
                    //Debug.Log("Debug true");
                    if (options.Length > 1)
                    {

                        if (options[1].Trim().ToUpper().Equals("T"))
                        {
                            ConfigurationUtil.isTestMode = true;
                            //Debug.Log("Debug true");
                        }

                        else
                        {
                            ConfigurationUtil.isTestMode = false;
                            //Debug.Log("Debug false");
                        }
                    }

                    break;
                case "UseWand":
                    //Debug.Log("Debug true");
                    if (options.Length > 1)
                    {

                        if (options[1].Trim().ToUpper().Equals("T"))
                        {
                            ConfigurationUtil.isUseWand = true;
                            //Debug.Log("Debug true");
                        }

                        else
                        {
                            ConfigurationUtil.isUseWand = false;
                            //Debug.Log("Debug false");
                        }
                    }

                    break;
                case "InputDevice":
                    if (options.Length > 1)
                    {
                        ConfigurationUtil.inputDevice = options[1].Trim().ToUpper();
                    }

                    break;
				case "DebugLog":
					//Debug.Log("Debug");
					if (options.Length > 1)
					{
						if (options[1].Trim().ToUpper().Equals("T"))
						{
							//Debug.Log("Debug true");
							ConfigurationUtil.isDebug = true;
						}

						else
							ConfigurationUtil.isDebug = false;
					}

					break;

				default:
					LogSystem.Log("Failed to find config variable : " + options[0]);
					//Debug.Log("Failed to find config variable : " + options[0]);
					break;





			}
		}

		Application.LoadLevel(defaultLevel);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
