using UnityEngine;
using UnityEngine.VR.WSA.WebCam;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ShowImageOnPanel : MonoBehaviour {

    private bool swCamaraInUse=false;
    System.DateTime myTime;
    PhotoCapture photoCaptureObject = null;

    string EMOTIONKEY = "069ebcd5c9a54f30a6d9970ed6e20a0f"; // replace with your Emotion API Key
    string emotionURL = "https://api.projectoxford.ai/emotion/v1.0/recognize";
    string VISIONKEY = "2d91252b838a40e7adbb755a613b5175"; // replace with your Computer Vision API Key
    string visionURL = "https://api.projectoxford.ai/vision/v1.0/analyze";

    public GameObject ImageFrameObject; // The object to place the image on
    public GameObject Text1;
    public GameObject Text2;
    public int TimeDelay = 5;
    public int Score = 100;
    public List<FaceObject> faces { get; private set; }
    // Use this for initialization
    void Start () {

        myTime = System.DateTime.Now.AddSeconds(10);
    }
    // Update is called once per frame
    void Update () {
        if (!swCamaraInUse)
        {
            if ((System.DateTime.Now - myTime).TotalSeconds > TimeDelay)
            {
                myTime = System.DateTime.Now;
                swCamaraInUse = true;
                PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
            }
        }
	} 
    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;
        try
        {
            captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
        }
        catch (System.Exception)
        {

            //throw;
        }
        
    }
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }
    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // Create our Texture2D for use and set the correct resolution
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            // Do as we wish with the texture such as apply it to a material, etc.
            ImageFrameObject.GetComponent<Renderer>().material.mainTexture = targetTexture;

            StartCoroutine(GetEmotionFromImages2(targetTexture.EncodeToJPG()));
            StartCoroutine(GetVisionData(targetTexture.EncodeToJPG()));

        }
        // Clean up
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        swCamaraInUse = false;
    }

    /// <summary>
    /// Get emotion data from the Cognitive Services Emotion API
    /// Stores the response into the responseData string
    /// </summary>
    /// <returns> IEnumerator - needs to be called in a Coroutine </returns>
    IEnumerator GetEmotionFromImages2(byte[] bytes)
    {
        //byte[] bytes = UnityEngine.Windows.File.ReadAllBytes("");
        //Debug.Log("Start");
        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key", EMOTIONKEY },
            { "Content-Type", "application/octet-stream" }
        };

        WWW www = new WWW(emotionURL, bytes, headers);

        yield return www;
        var responseData = www.text; // Save the response as JSON string

        try
        {
            faces = new List<FaceObject>();
            JSONObject dataArray = new JSONObject(responseData);
            for (int i = 0; i < dataArray.Count; i++)
            {
                var current = dataArray[i];
                faces.Add(new FaceObject(current.list[0].ToString(), current.list[1].ToString()));
            }

            Text1.GetComponent<UnityEngine.UI.Text>().text = "";

            int p = 1;
            foreach (var face in faces)
            {
                // Text2.GetComponent<UnityEngine.UI.Text>().text += face.faceRectangle + "\n";
                Text1.GetComponent<UnityEngine.UI.Text>().text +=  "Person " + p.ToString() +  " emotion  is '" +  face.GetHighestWeighedEmotion().name + "'\n";
                p += 1;
                switch (face.GetHighestWeighedEmotion().name)
                { 
                    case "happiness" :
                    Score += 1;
                        break;
                    case "neutral":
                        break;
                    default:
                        Score -= 1;
                        break;
                }
                //foreach (var em in face.emotions.OrderByDescending(e => e.value))
                //{
                //    Text1.GetComponent<UnityEngine.UI.Text>().text += em.name + " : " + em.value + "\n";
                //}

                Text1.GetComponent<UnityEngine.UI.Text>().text += "\n";

            }
            Text1.GetComponent<UnityEngine.UI.Text>().text += "Score: " + Score + "\n\n\n";
        }
        catch (System.Exception X)
        {

                Text1.GetComponent<UnityEngine.UI.Text>().text +="Error: "+ X.Message+  "\n";

        }

    }
    /// <summary>
    /// Get emotion data from the Cognitive Services Emotion API
    /// Stores the response into the responseData string
    /// </summary>
    /// <returns> IEnumerator - needs to be called in a Coroutine </returns>
    IEnumerator GetVisionData(byte[] bytes)
    {
        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key", VISIONKEY },
            { "Content-Type", "application/octet-stream" }
        };
        WWW www = new WWW(visionURL, bytes, headers);
        yield return www;

        try
        {
            var responseData = www.text; // Save the response as JSON string
            JSONObject dataArray = new JSONObject(responseData);
            JSONObject _categories = dataArray.list[0]; // Get the list of categories
            var x = new FoundImageObject(_categories);

            Text2.GetComponent<UnityEngine.UI.Text>().text = "Scene Information:\n";
            
            foreach (var item in x.categories)
            {
                Text2.GetComponent<UnityEngine.UI.Text>().text += item.name +": " + item.score + "\n";
            }

        }
        catch (System.Exception X)
        {
                Text2.GetComponent<UnityEngine.UI.Text>().text +=  "Error: " + X.Message + "\n";

            // throw;
        }

    }
}


[System.Serializable]
public class FaceObject
{
    public string faceRectangle { get; private set; }
    public List<Emotion> emotions { get; private set; }

    public FaceObject(string rect, string scorelist)
    {
        faceRectangle = rect;
        emotions = ConvertScoresToEmotionDictionary(scorelist);
        //Debug.Log("Highest Emotion: " + GetHighestWeighedEmotion().ToString());

    }
    /// <summary>
    /// Convert a JSON-formatted string from the Emotion API call into a List of Emotions
    /// </summary>
    /// <param name="scores"></param>
    /// <returns></returns>
    public List<Emotion> ConvertScoresToEmotionDictionary(string scores)
    {
        List<Emotion> emotes = new List<Emotion>();
        JSONObject _scoresJSON = new JSONObject(scores);
        for (int i = 0; i < _scoresJSON.Count; i++)
        {
            Emotion e = new Emotion(_scoresJSON.keys[i], float.Parse(_scoresJSON.list[i].ToString()));
            emotes.Add(e);
        }
        return emotes;
    }

    /// <summary>
    /// Get the highest scored emotion 
    /// </summary>
    /// <returns></returns>
    public Emotion GetHighestWeighedEmotion()
    {
        Emotion max = emotions[0];
        foreach (Emotion e in emotions)
        {
            if (e.value > max.value)
            {
                max = e;
            }
        }
        return max;
    }
}

public class Emotion
{
    public string name { get; private set; }
    public float value { get; private set; }

    public Emotion(string name, float value)
    {
        this.name = name;
        this.value = value;
    }

    override public string ToString()
    {
        return name + " : " + value;
    }
}
