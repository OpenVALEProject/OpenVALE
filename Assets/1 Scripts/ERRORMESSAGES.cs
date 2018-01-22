using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ERRORMESSAGES
{
    public enum ErrorType : int
    {	// Audio App Constants
        ERR_AS_NONE = 0, 		// None
        ERR_AS_CMDSYN = 1,		// Command syntax error
        ERR_AS_CMDNOTRECOGNIZED = 2,
        ERR_AS_XYZPARSEFAILURE = 3,
        ERR_AS_MASKPARSEFAILURE = 4,
        ERR_AS_BOOLPARSEFAILURE = 5,
        ERR_AS_PARAMETEROUTOFRANGE = 6,
        ERR_AS_SPEAKERNOTFOUND = 7,
        ERR_AS_COLORPARSEFAILURE = 8,

    };

    static class ErrorStrings
    {
        public static string[] ERROR_STRINGS =
        {
          "None",
          "Command syntax error",
          "Command not found",
          "Failed to parse XYZ",
          "Failed to parse Mask",
          "Failed to parse boolean",
          "Paramter not in expected value range",
          "Speaker ID not found",
          "Failed to parse color",
        };
    }
}
