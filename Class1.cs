using System.Collections;
using System.Net.Mime;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[BepInPlugin("org.quadruplea.TotemOfUndying", "Totem Of Undying", "1.0.0")]
public class HealthDetectorMod : BaseUnityPlugin
{

    void Awake()
    {
        // my inner harmony
        
        Harmony harmony = new Harmony("org.quadruplea.TotemOfUndying");
        harmony.PatchAll();
        
        
    }
    
    
    
    [HarmonyPatch(typeof(NewMovement), "Update")]
    public class HealthCheckPatch
    {
        

        
        static bool didVideoPlay = false;

        static void Play_Video()
        {
            
            if (didVideoPlay==true){return;}
            
            RenderTexture rt = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
            rt.Create();

            // find maincamera object
            GameObject mainCamera = GameObject.Find("Player/Main Camera");
            if (mainCamera == null)
            {
                Debug.LogError("@maincameranotfound get it because georgenot fo");
                return;
            }

            // check if canvas exists
            GameObject canvasObject = GameObject.Find("VideoCanvas");
            if (canvasObject == null)
            {
                // create canvas
                canvasObject = new GameObject("VideoCanvas");
                Canvas canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = mainCamera.GetComponent<Camera>(); // attach to maincamera
                canvas.planeDistance = 1f; // not too far to the camera
        
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();

                // parent it to maincamera wow so many cameras
                canvasObject.transform.SetParent(mainCamera.transform, false);
            }
            
            GameObject imageObject = new GameObject("VideoScreen");
            imageObject.transform.SetParent(canvasObject.transform, false);
    
            RawImage image = imageObject.AddComponent<RawImage>();
            image.texture = rt;
            
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            
            
            var videoPlayer = imageObject.AddComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayer.url = Application.dataPath + "/../BepInEx/plugins/video.mp4";
            videoPlayer.isLooping = false;
            videoPlayer.targetTexture = rt;
            Debug.Log("Video Path: " + videoPlayer.url);
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource; // enable audio
            AudioSource audioSource = imageObject.AddComponent<AudioSource>(); 
            videoPlayer.SetTargetAudioSource(0, audioSource); // link audio to videoplayer
            audioSource.playOnAwake = false; 
            audioSource.volume = 1f; 


            
            videoPlayer.Play();

            
            videoPlayer.loopPointReached += OnVideoEnd;
        }






        static void OnVideoEnd(VideoPlayer vp)
        {
            Debug.Log("finished video play");

            if (vp != null)
            {
                GameObject canvasObject = GameObject.Find("VideoCanvas");
                if (canvasObject != null)
                {
                    GameObject.Destroy(canvasObject);
                }
            }
        }

    
        static void Postfix(NewMovement __instance)
        {
            if (__instance.hp < 75)
            {
                Play_Video();
                
                if (didVideoPlay==true){return;}
                __instance.hp = 100;
                Debug.Log("totem go brrrr");

                

                

            }
        
        }


    
    

    }
    
}

