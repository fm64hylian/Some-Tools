using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class FMPlayfabLogin : MonoBehaviour
{
    public static void LoginCustomID(string customId, Action<LoginResult> OnLoginSuccess) {
        LoginCustomID(PlayFabSettings.TitleId, customId, OnLoginSuccess, OnLoginFailure);
    }

    public static void LoginCustomID(string titleID, string customId, Action<LoginResult> OnLoginSuccess, Action<PlayFabError> OnLoginFailure) {
        //IVPlayfabLogin.LoginCustomID(PlayFabSettings.TitleId, "64646464", LoginSuccess, PlayFabError);
        var request = new LoginWithCustomIDRequest {
            TitleId = titleID,
            CustomId = customId,
            CreateAccount = false
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

    }


    public static void LoginMail(string mail, string pass,  Action<LoginResult> OnLoginSuccess)
    {
        LoginMail(PlayFabSettings.TitleId, mail,pass, OnLoginSuccess, OnLoginFailure);
    }

    public static void LoginMail(string titleID, string mail, string pass, Action<LoginResult> OnLoginSuccess, Action<PlayFabError> OnLoginFailure) {
        var request = new LoginWithEmailAddressRequest
        {
            TitleId = titleID,
            Email = mail,
            Password = pass
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    public static void LoginPlayfabUSer(string user, string pass, Action<LoginResult> OnLoginSuccess)
    {
        LoginPlayfabUSer(PlayFabSettings.TitleId, user, pass, OnLoginSuccess, OnLoginFailure);
    }

    public static void LoginPlayfabUSer(string titleID, string user, string pass, Action<LoginResult> OnLoginSuccess, Action<PlayFabError> OnLoginFailure)
    {
        var request = new LoginWithPlayFabRequest
        {
            TitleId = titleID,
            Username = user,
            Password = pass
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
    }    

    public static bool IsClientLoggedIn() {
        return PlayFabClientAPI.IsClientLoggedIn();
    }

    static void OnLoginFailure(PlayFabError error) {
        Debug.Log("could not login, "+error.ErrorMessage);
    }

    //TODO register
	/*        RegisterPlayFabUserRequest registerRequest = new RegisterPlayFabUserRequest() {
            Username = userInputR.text,
            Email = mailInputR.text,
            Password = passInputR.text
        };

        PlayFabClientAPI.RegisterPlayFabUser(registerRequest,OnUSerRegistered, OnRegisterError);*/    

    //check later
    /*
#if UNITY_ANDROID
        PlayFabClientAPI.LoginWithAndroidDeviceID(
            // Request
            new LoginWithAndroidDeviceIDRequest
            {
                CreateAccount = true,
                AndroidDevice = SystemInfo.deviceModel,
                OS = SystemInfo.operatingSystem,
                AndroidDeviceId = SystemInfo.deviceUniqueIdentifier
            },
            // Success
            (LoginResult result) => 
            {
                Debug.Log("Login completed.");
                IsLoggedIn = true;
            },
            // Failure
            (PlayFabError error) => 
            {
                Debug.LogError("Login failed.");
                Debug.LogError(error.GenerateErrorReport());
            }
            );
#elif UNITY_IOS
        PlayFabClientAPI.LoginWithIOSDeviceID(
            // Request
            new LoginWithIOSDeviceIDRequest
            {
                CreateAccount = true,
                DeviceId = SystemInfo.deviceUniqueIdentifier,
                DeviceModel = SystemInfo.deviceModel,
                OS = SystemInfo.operatingSystem
            },
            // Success
            (LoginResult result) => 
            {
                Debug.Log("Login completed.");
                IsLoggedIn = true;
            },
            // Failure
            (PlayFabError error) => 
            {
                Debug.LogError("Login failed.");
                Debug.LogError(error.GenerateErrorReport());
            }
            );
#else
        PlayFabClientAPI.LoginWithCustomID(
            // Request
            new LoginWithCustomIDRequest
            {
                CustomId = System.Guid.NewGuid().ToString(),
                CreateAccount = true
            },
            // Success
            (LoginResult result) =>
            {
                Debug.Log("Login completed.");
                IsLoggedIn = true;
                onSuccess(result);
            },
            // Failure
            (PlayFabError error) =>
            {
                Debug.LogError("Login failed.");
                Debug.LogError(error.GenerateErrorReport());
                onFailed(error);
            }
            );
#endif
    }
     */
}
