using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[Route("api/v1/[controller]")]
[ApiController]
public class FaceRecognitionController : ControllerBase
{
    private readonly IFaceClient faceClient;

    public FaceRecognitionController()
    {
        // Replace 'yourKey' and 'yourEndpoint' with your Face API key and endpoint
        var apiKey = "185e56cc7af243ee9c5a1b2f9d50c0b5";
        var endpoint = "https://quangdiep.cognitiveservices.azure.com/";

        faceClient = new FaceClient(new ApiKeyServiceClientCredentials(apiKey))
        {
            Endpoint = endpoint
        };
    }

    [HttpPost("recognize-face")]
    public async Task<IActionResult> RecognizeFace()
    {
        try
         {
            // Retrieve image from request
            var file = Request.Form.Files[0];
            using var stream = file.OpenReadStream();

            // Detect faces in the image
            var faceAttributes = new List<FaceAttributeType> { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Emotion };
            var faces = await faceClient.Face.DetectWithStreamAsync(stream, true, false, faceAttributes);

            if (faces.Count > 0)
            {
                // Assuming only one face is detected for simplicity
                var detectedFace = faces[0];
                var detectedUser = $"{detectedFace.FaceAttributes.Gender}, Age: {detectedFace.FaceAttributes.Age}, Emotion: {GetEmotion(detectedFace.FaceAttributes.Emotion)}";

                return Ok(new { Username = detectedUser });
            }
            else
            {
                return NotFound("No face detected.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    private string GetEmotion(Emotion emotion)
    {
        // Choose the dominant emotion
        return emotion.ToRankedList().FirstOrDefault().Key;
    }
}
