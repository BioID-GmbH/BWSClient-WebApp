﻿'use strict';

// Simple method to start video capturing on the provided media player element
// using MediaDevices.getUserMedia() (this may require the getUserMedia polyfill).
// On success an optional callback function is called.
// Example usage:
// var video = document.createElement('video');
// startVideo(video, initCanvases);
// function initCanvases(videoElement, mediaStream) { ... }
function startVideo(videoElement, successCallback, highRes = false, facingMode = "user") {
    const constraints = highRes ? { audio: false, video: { width: { ideal: 4096 } } } : { audio: false, video: { facingMode: facingMode, height: { ideal: 600, min: 480 } } };
    navigator.mediaDevices.getUserMedia(constraints)
        .then(function (mediaStream) {
            // Virtual camera devices are denied!
            if (isVirtualDevice(mediaStream)) return;
            console.log('Media stream created:', mediaStream.getVideoTracks()[0]);
            videoElement.srcObject = mediaStream;
            videoElement.onloadedmetadata = function (e) {
                console.log('Playing live media stream');
                videoElement.play();
                if (successCallback) { successCallback(videoElement, mediaStream); }
            };
        })
        .catch(function (err) {
            console.log('getUserMedia() error: ', err);
            alert(err.name + ': ' + err.message);
        });
}

// Stop video capturing of the provided media player element.
// Returns true when video was stopped, false if it was not running.
function stopVideo(videoElement) {
    let stream = videoElement.srcObject;
    if (stream) {
        stream.getTracks().forEach(track => {
            track.stop();
        });
        videoElement.pause();
        videoElement.srcObject = null;
        return true;
    }
    return false;
}

// Create a template for cross-correlation from the given canvas pixel data (ImageData object).
function createTemplate(imageData) {
    // cut out the template:
    // we use a small width, quarter-size image around the center as template
    var template = {
        centerX: imageData.width / 2,
        centerY: imageData.height / 2,
        width: imageData.width / 4,
        height: imageData.height / 4 + imageData.height / 8
    };
    template.xPos = template.centerX - template.width / 2;
    template.yPos = template.centerY - template.height / 2;
    template.buffer = new Uint8ClampedArray(template.width * template.height);

    let counter = 0;
    let p = imageData.data;
    for (var y = template.yPos; y < template.yPos + template.height; y++) {
        // we use only the green plane here
        let bufferIndex = (y * imageData.width * 4) + template.xPos * 4 + 1;
        for (var x = template.xPos; x < template.xPos + template.width; x++) {
            let templatepixel = p[bufferIndex];
            template.buffer[counter++] = templatepixel;
            // we use only the green plane here
            bufferIndex += 4;
        }
    }
    console.log('Created new cross-correlation template', template);
    return template;
}

// Perform motion detection by a normalized cross-correlation.
// The normalized cross-correlation is calculated between the template of the
// first image (see createTemplate) and each incoming image. This algorithm is 
// basically called "Template Matching".
// We use the normalized cross correlation to be independent of lighting changes.
// Note that we calculate the correlation of template and image over the whole image area.
// Returns the movement-percentage (i.e. a value between 0% and 100%).
function motionDetection(imageData, template) {
    let bestHitX = 0,
        bestHitY = 0,
        maxCorr = 0,
        searchWidth = imageData.width / 4,
        searchHeight = imageData.height / 4,
        p = imageData.data;

    for (var y = template.centerY - searchHeight; y <= template.centerY + searchHeight - template.height; y++) {
        for (var x = template.centerX - searchWidth; x <= template.centerX + searchWidth - template.width; x++) {
            let nominator = 0,
                denominator = 0,
                templateIndex = 0;

            // Calculate the normalized cross-correlation coefficient for this position
            for (var ty = 0; ty < template.height; ty++) {
                // we use only the green plane here
                let bufferIndex = x * 4 + 1 + (y + ty) * imageData.width * 4;
                for (var tx = 0; tx < template.width; tx++) {
                    var imagepixel = p[bufferIndex];
                    nominator += template.buffer[templateIndex++] * imagepixel;
                    denominator += imagepixel * imagepixel;
                    // we use only the green plane here
                    bufferIndex += 4;
                }
            }

            // The NCC coefficient is then (watch out for division-by-zero errors for pure black images):
            let ncc = 0.0;
            if (denominator > 0) {
                ncc = nominator * nominator / denominator;
            }
            // Is it higher than what we had before?
            if (ncc > maxCorr) {
                maxCorr = ncc;
                bestHitX = x;
                bestHitY = y;
            }
        }
    }
    // now the most similar position of the template is (bestHitX, bestHitY). Calculate the difference from the origin:
    let distX = bestHitX - template.xPos,
        distY = bestHitY - template.yPos,
        movementDiff = Math.sqrt(distX * distX + distY * distY);
    // the maximum movement possible is a complete shift into one of the corners, i.e:
    let maxDistX = searchWidth - template.width / 2,
        maxDistY = searchHeight - template.height / 2,
        maximumMovement = Math.sqrt(maxDistX * maxDistX + maxDistY * maxDistY);

    // the percentage of the detected movement is therefore:
    let movementPercentage = movementDiff / maximumMovement * 100;
    if (movementPercentage > 100) {
        movementPercentage = 100;
    }
    console.log('Calculated movement: ', movementPercentage);
    return movementPercentage;
}

// mediaDevices - getUserMedia - polyfill
// usage: navigator.mediaDevices.getUserMedia({ video: true }).then(function (mediaStream) { ... }).catch(function (err) { ... });
(function () {
    var promisifiedOldGUM = function promisifiedOldGUM(constraints, successCallback, errorCallback) {
        // First get ahold of getUserMedia, if present
        var getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia;
        // Some browsers just don't implement it - return a rejected promise with an error to keep a consistent interface
        if (!getUserMedia) {
            return Promise.reject(new Error('getUserMedia is not implemented in this browser'));
        }
        // Otherwise, wrap the call to the old navigator.getUserMedia with a Promise
        return new Promise(function (successCallback, errorCallback) {
            getUserMedia.call(navigator, constraints, successCallback, errorCallback);
        });
    };

    // Older browsers might not implement mediaDevices at all, so we set an empty object first
    if (navigator.mediaDevices === undefined) {
        navigator.mediaDevices = {};
    }
    // Some browsers partially implement mediaDevices. We can't just assign an object
    // with getUserMedia as it would overwrite existing properties.
    // Here, we will just add the getUserMedia property if it's missing.
    if (navigator.mediaDevices.getUserMedia === undefined) {
        navigator.mediaDevices.getUserMedia = promisifiedOldGUM;
    }
})();

// A low performance HTMLCanvasElement.toBlob polyfill based on toDataURL.
(function () {
    if (!HTMLCanvasElement.prototype.toBlob) {
        Object.defineProperty(HTMLCanvasElement.prototype, 'toBlob', {
            value: function (callback, type, quality) {
                console.log('using toBlob poyfill based on toDataURL');
                var canvas = this;
                setTimeout(function () {
                    var binStr = atob(canvas.toDataURL(type, quality).split(',')[1]),
                        len = binStr.length,
                        arr = new Uint8Array(len);
                    for (var i = 0; i < len; i++) {
                        arr[i] = binStr.charCodeAt(i);
                    }
                    callback(new Blob([arr], { type: type || 'image/png' }));
                });
            }
        });
    }
})();



// Virtual camera devices are denied because we don`t allow video injection! 
function isVirtualDevice(mediaStream) {
    // check if device label is on the blacklist 
    const blacklistDevices = [
        "AlterCam",
        "Avatarify",
        "CameraFi",
        "CamONLive",
        "CamTwist",
        "CamUniversal",
        "ChromaCam",
        "digiCamControl Virtual Webcam",
        "DroidCam",
        "e2eSoft VCAM",
        "EpocCam",
        "FineCam",
        "Iriun",
        "iVCam",
        "Logi Capture",
        "MagicCamera Capture",
        "ManyCam Virtual",
        "Mewcam",
        "MotionCaster Video",
        "OBS Ninja",
        "OBS Virtual Camera",
        "OBS-Camera",
        "Reincubate",
        "Restream Studio",
        "Snap Camera",
        "SparkoCam",
        "SplitCam",
        "StreamlabsOBS",
        "VirtualCamera",
        "WebcamMax Capture",
        "Webcamoid",
        "Wirecast Virtual Camera",
        "XSplit",
        "YouCam"
    ];
    let isBlacklisted = false;
    let deviceLabel = mediaStream.getVideoTracks()[0].label.toLowerCase();
    for (const device of blacklistDevices) {
        if (deviceLabel.includes(device.toLowerCase())) {
            isBlacklisted = true;
            console.log('The selected camera is not supported: ' + deviceLabel);
            if (isBlacklisted) {
                alert('The selected camera is not supported: ' + deviceLabel);
                break;
            }
        }
    }
    return isBlacklisted;
}