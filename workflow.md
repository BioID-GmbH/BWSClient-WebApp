 
# How this sample implementation works

This web application offers three main features: Liveness Detection, Face Deepfake Detection, and PhotoVerify.
It can distinguish a live person from fakes such as images or videos using a standard camera,
and detect whether a face in an image or video is a deepfake.
It also compares an ID photo with live samples, including liveness detection.

The implementation is based on Razor Pages together with HTML5, CSS and JavaScript.
The web application uses ASP.NET for server-side processing and Bootstrap for responsive design. 

More details about each feature are explained below.

## The workflow for Liveness Detection

1. Access the webcam by requesting permission from the user.
2. Once you have obtained permission, capturing of live webcam images begins.
3. By pressing the button, the app captures a total of two images. The first image is taken immediately upon pressing the button,
while the BioID Motion Detection automatically detects the required movement and triggers the capture of the second image.
4. After both images are captured, the uploading process begins.
5. Upon successful upload, the web server calls the [BioID Web Service (BWS)][bwsreference] and returns the result to the client.

## The workflow for PhotoVerify

1. Access the webcam by requesting permission from the user.
2. Once you have obtained permission, capturing of live webcam images begins.
3. Select the ID photo.
4. By pressing the button, the app captures a total of two images. The first image is taken immediately upon pressing the button,
while the BioID Motion Detection automatically detects the required movement and triggers the capture of the second image.
5. After both images are captured and the ID photo has been selected, the uploading process begins.
6. Upon successful upload, the web server calls the [BioID Web Service (BWS)][bwsreference] and returns the result to the client.

## The workflow for Face Deepfake Detection

1. By pressing the button, you can select a file to verify for deepfake content. The file can be either an image or a video. 
2. Then press the processing button, and the file upload process will begin.
3. After a successful upload, the web server calls the [BioID Web Service (BWS)][bwsreference] and returns the result to the client.


### Capturing images from webcam video using HTML5 
Please take a look at the code. In each web view of every component, there is a canvas and a button element,
located in the files: Pages/LivenessDetection.cshtml, Pages/PhotoVerify.cshtml, and Pages/NewDeepfakeDetection.cshtml.

You need a _canvas_ for drawing the live webcam video. 

The _class_ attribute specifies the layout of the canvas (mw-100 → Max-width = 100%).
The id attribute helps us to identify the canvas and get access from javascript.

A button element is defined for starting image capturing for liveness detection. 


```html
<canvas class="align-bottom mw-100" id="drawingcanvas"></canvas>
<button id="capture" class="btn btn-primary">Start</button>
```

These two html elements are the minimum requirement to capture and process the image data.

### Display live webcam video
To start and display the live webcam video, we create a video element in JavaScript.
When the DOMContentLoaded event is fired, the function startVideo(video, initCanvases) is called, and some listeners for the buttons are enabled.

```js
document.addEventListener("DOMContentLoaded", () => {
 
           document.getElementById('capture').addEventListener('click', capture);
 
           var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-toggle="popover"]'))
           popoverTriggerList.map(function (popoverTriggerEl) { return new bootstrap.Popover(popoverTriggerEl) });
 
           document.getElementById('howtovideo').addEventListener('click', () => {
               let howtovideo = document.getElementById('howtovideo');
               if (howtovideo.paused) { howtovideo.play(); }
               else { howtovideo.pause(); }
           });
           document.getElementById('closehowtovideo').addEventListener('click', () => { document.getElementById('howtovideodiv').remove(); });
 
           startVideo(video, initCanvases);
       });
```

The _startVideo_ function helps to access the camera's video stream, while _initCanvases_ receives this video stream.
The _startVideo_ function is implemented in the _videoCapture.js_ file (located in wwwroot/js/). Feel free to use this file for your implementation.

The size (width/height) of the canvas is specified for portrait mode. Typically, a webcam video delivers images in landscape mode.
However, for capturing face images, we do not need the information on the left and right sides of a landscape image. 
Therefore, the size of the image is adjusted to portrait mode. For example, with a 640x480 pixel camera image,
areas on the left and right are removed to create a 360x480 pixel image. This approach benefits users
with limited bandwidth by reducing the upload size by almost 50%.

We recommend using a lossless compression algorithm for the image, with PNG format being our preferred choice.
When comparing PNG to JPG at the best quality settings, PNG offers better performance for our system.

Inside initCanvases an interval-timer is started to grab about 20 frames per second and call the processFrame().

```js
function initCanvases(videoElement, mediaStream) {
          // we prefer 3 : 4 face image resolution
          let aspectratio = videoElement.videoWidth / videoElement.videoHeight < 3 / 4 ? videoElement.videoWidth / videoElement.videoHeight : 3 / 4;
          drawingCanvas.height = videoElement.videoHeight;
          drawingCanvas.width = drawingCanvas.height * aspectratio;
          motionCanvas.height = motionAreaHeight;
          motionCanvas.width = motionCanvas.height * aspectratio;
 
          drawingCanvas.title = `Capturing ${videoElement.videoWidth}x${videoElement.videoHeight}px (cropped to ${drawingCanvas.width}x${drawingCanvas.height}) from ${mediaStream.getVideoTracks()[0].label}.`
 
          // mirror live preview
          let ctx = drawingCanvas.getContext('2d');
          ctx.translate(drawingCanvas.width, 0);
          ctx.scale(-1, 1);
          // set an interval-timer to grab about 20 frames per second
          setInterval(processFrame, 50);
      }
```

The processFrame function is called for every grabbed frame.
For each incoming image the motion is analysed compared to the first image. The activation of the motion detection starts by clicking the capture button.
The implementation of the BioID Motion Detection is implemented in the _ideoCapture.js_.

```js
function processFrame() {
            let w = drawingCanvas.width, h = drawingCanvas.height, aspectratio = w / h;
            let cutoff = video.videoWidth - (video.videoHeight * aspectratio);
            let ctx = drawingCanvas.getContext('2d');
            ctx.drawImage(video, cutoff / 2, 0, video.videoWidth - cutoff, video.videoHeight, 0, 0, w, h);

            if (capturing) {
                // scale current image into the motion canvas
                let motionctx = motionCanvas.getContext('2d');
                motionctx.drawImage(drawingCanvas, w / 8, h / 8, w - w / 4, h - h / 4, 0, 0, motionCanvas.width, motionCanvas.height);
                let currentImageData = motionctx.getImageData(0, 0, motionCanvas.width, motionCanvas.height);

                if (template) {
                    let movement = motionDetection(currentImageData, template);
                    // trigger if movementPercentage is above threshold (default: when 20% of maximum movement is exceeded)
                    if (movement > motionThreshold) {
                        capturing = false;
                        template = null;
                        drawingCanvas.toBlob(handleImage2)
                        console.log('captured second image');
                    }
                } else {
                    // use as template
                    template = createTemplate(currentImageData);
                    // capture the current image
                    drawingCanvas.toBlob(setImage1)
                    console.log('captured first image');
                }
            }

            ctx.beginPath();
            ctx.arc(w / 2, h / 2, w * 0.4, 0, 2 * Math.PI);
            ctx.lineWidth = 3;
            ctx.strokeStyle = '#fff';
            ctx.stroke();
            ctx.rect(0, 0, w, h);
            ctx.fillStyle = 'rgba(220, 220, 220, 0.8)';
            ctx.fill('evenodd');
        }
```

> #### UX
> A white circle is displayed on the canvas. Outside the circle the image is faded, to motivate the user to position
his/her head inside this circle. Everything outside the circle is not considered relevant data, as only the center 
of the image is analysed by the motion algorithm. Thus, for the best performance we require frontal faces inside the circle.

Our experience has shown the best results with the proposed canvas. The layout for this canvas is up to 
you but it is important to capture frontal and fully visible centered faces.
This way you also avoid failures like no fully visible face, etc.

### BioID Motion Detection
BioID Motion Detection algorithm is mandatory for capturing suitable images. The implementation is in site.js.

If the image capturing is activated, the first image is taken immediately. The second image is triggered by the BioID Motion Detection, as soon as enough movement is detected.


**The motion region is only a tiny part inside the white circle area. The javascript based implementation of the BioID Motion Algorithm is optimized and works with all devices independent of the cpu performance. If you increase the motion area, don`t forget that slow devices might not be working fluently.**

Please use our code as it is to achieve the best result for the liveness detection.

> #### UX
> Our experience for this motion detection threshold is the default setting. In general we differentiate static camera (PC/Laptop) from mobile camera. With a mobile camera you get additional movement from holding the device. To avoid accidential image triggering, the threshold for mobile devices is higher.

`const motionThreshold = isMobileDevice() ? 50 : 20;`

We offer the function _isMobileDevice_() (wwwroot/js/site.js) to detect, if the javascript is running on a mobile device or not.

### Start Capturing of 2 images and call BWS Liveness Detection
If the user presses the capture button the capture function is called and the capture state (boolean) is true. The processFrame function processes the live video stream and activates the motion detection analyzation with the capture state.

If the motion reaches the threshold the second image is uploading and the capture process and motion detection stops. Both images are uploaded inside a form as blob data. Take a look at_toBlob_ function.

```js
function sendImages() {
            document.getElementById('captureSpinner').style.display = "none";
            document.getElementById('progressSpinner').style.display = "inline-block";

            var formData = new FormData(document.getElementById('capture-form'));
            formData.append('image1', firstCapturedImage);
            formData.append('image2', secondCapturedImage);
            formData.append('isMobile', isMobileDevice().toString());
            xhr.open("POST", "/LivenessDetection");
            xhr.send(formData);
        }
```

### Call BWS Liveness Detection API - [Reference][livenessreference]

```c#
// _bws is a BioID Web Service gRPC client that is already configured in Program.cs
var liveimage1 = Request.Form.Files["image1"];
var liveimage2 = Request.Form.Files["image2"];

using MemoryStream liveSteram1 = new();
using MemoryStream liveStream2 = new();

// Check if first live image raw data is available
if (liveimage1 != null) await liveimage1.CopyToAsync(liveSteram1).ConfigureAwait(false);

// Check if second live image raw data is available
if (liveimage2 != null) await liveimage2.CopyToAsync(liveStream2).ConfigureAwait(false);

ByteString image1 = ByteString.CopyFrom(liveSteram1.ToArray());
ByteString image2 = ByteString.CopyFrom(liveStream2.ToArray());

var livenessRequest = new LivenessDetectionRequest();
livenessRequest.LiveImages.Add(new ImageData() { Image = image1 });
livenessRequest.LiveImages.Add(new ImageData() { Image = image2 });

var livenessCall = _bws.LivenessDetectionAsync(livenessRequest, new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
var response = await livenessCall.ResponseAsync.ConfigureAwait(false);
```
Please take a look at [LiveDetection API reference][livenessreference] section 'Response' for detailed information. 

## Call BWS PhotoVerify API - [Reference][photoverifyreference]

For PhotoVerify, you only need to add the ID-Photo to the form data on the client side. After uploading the images you see the BWS PhotoVerify call below: 

> ### UX
> Prompt the user to take a snapshot of their ID or passport photo (without holographic reflection). Please note: Only the portrait image is required for PhotoVerify call. A full UX guide for <a href="https://youtu.be/EMYCZdBDT54">ID photo capture</a> can be found on YouTube. 

```c#
var idphoto = Request.Form.Files["idphoto"];
var liveimage1 = Request.Form.Files["image1"];
var liveimage2 = Request.Form.Files["image2"];

using MemoryStream idStream = new();
using MemoryStream liveStream1 = new();
using MemoryStream liveStream2 = new();

await idphoto.CopyToAsync(idStream).ConfigureAwait(false);
await liveimage1.CopyToAsync(liveStream1).ConfigureAwait(false);
await liveimage2.CopyToAsync(liveStream2).ConfigureAwait(false);

ByteString photo = ByteString.CopyFrom(idStream.ToArray());
ByteString image1 = ByteString.CopyFrom(liveStream1.ToArray());
ByteString image2 = ByteString.CopyFrom(liveStream2.ToArray());

var request = new PhotoVerifyRequest { Photo = photo };
request.LiveImages.Add(new ImageData() { Image = image1 });
request.LiveImages.Add(new ImageData() { Image = image2 });

var photoVerifyCall = _bws.PhotoVerifyAsync(request, new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
var response = await photoVerifyCall.ResponseAsync.ConfigureAwait(false);
```
Please take a look at [PhotoVerify API reference][photoverifyreference] section 'Response' for detailed information. 

### Call BWS Face Deepfake Detection API 
#### Depending on the type of content entered, either [LivenessDetection][liveness] is called for images or [VideoLivenessDetection][videoliveness] for videos.

```c#
// Call for image
 var imageFile = Request.Form.Files["image"];

if (imageFile != null)
{
    using MemoryStream ms = new();
    await imageFile.CopyToAsync(ms).ConfigureAwait(false);
    var image = ms.ToArray();

    // Create request
    var livenessRequest = new LivenessDetectionRequest();
    livenessRequest.LiveImages.Add(new ImageData() { Image = ByteString.CopyFrom(image) });

    var livenessCall = _bwsWebServiceClient.LivenessDetectionAsync(livenessRequest, headers: new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
    var response = await livenessCall.ResponseAsync.ConfigureAwait(false);
}

// Call for video
var videoFile = Request.Form.Files["video"];
if (videoFile != null)
{
    using MemoryStream ms = new();
    await videoFile.CopyToAsync(ms).ConfigureAwait(false);
    var video = ms.ToArray();

    // Add video sample to the grpc service request
    var videoRequest = new VideoLivenessDetectionRequest()
    {
        Video = ByteString.CopyFrom(video)
    };
    var videoLivenessCall = _bwsWebServiceClient.VideoLivenessDetectionAsync(videoRequest, headers: new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
    var response = await videoLivenessCall.ResponseAsync.ConfigureAwait(false);
}
```


Have a look here for more information on face [liveness detection][liveness] (including [Deepfakes][deepfake]).

You can find more information about our [face recognition software][facerecognition] technology at our website.

Here is all about our [face recognition company BioID][bioid].



[bwsreference]: https://developer.bioid.com/bws/newbws "New BWS Reference"
[liveness]: https://www.bioid.com/liveness-detection/ "Presentation attack detection."
[videoliveness]: https://developer.bioid.com/bws/grpc/videolivenessdetection " Video presentation attack detection."
[photoverify]: https://www.bioid.com/identity-verification-photoverify/ "PhotoVerify"
[deepfake]: https://www.bioid.com/deepfake-detection/ "Deepfake"
[bioid]: https://www.bioid.com "BioID - be recognized"
[facerecognition]: https://www.bioid.com/face-recognition-software/
[livenessreference]: https://developer.bioid.com/bws/grpc/livenessdetection "LivenessDetection gRPC API"
[photoverifyreference]: https://developer.bioid.com/bws/grpc/photoverify "PhotoVerify gRPC API"