using System.Security.Claims;

namespace BioID.BWS.WebApp.Helper
{
    public static class Extensions
    {
        public static string HintFromResult(this string code) => code switch
        {
            null => "",
            "NoFaceFound" => "We did not find a face. Please position your face in the center.",
            "FaceNotFound" => "We did not find a face. Please position your face in the center.",
            "MultipleFacesFound" => "We found multiple faces or a strongly uneven background distracted us. Your face should fill the circle completely.",
            "ThumbnailExtractionFailed" => "We did not find a suitable face. Please position your face in the center and make sure you are fully visible.",
            "UnsuitableImage" => "We did not find a suitable face. Please position your face in the center and make sure you are fully visible.",
            "UnnaturalMotionDetected" => "We observed unnatural motion. Please make sure to look straight in the first picture, then nod your head slightly.",
            "RejectedByActiveLiveDetection" => "We observed unnatural motion. Please make sure to look straight in the first picture, then nod your head slightly. Do NOT move the device you are using.",
            "DontMoveDevice" => "Don’t move the device you are using.",
            _ => ""
        };
    }
}
