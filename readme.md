# BWSClient WebApp 

## Overview
The **BWSClient WebApp** is an ASP.NET-based application featuring a web view that offers three core functionalities: 
[Liveness Detection][liveness], [Face Deepfake Detection][deepfake] and [PhotoVerify][photoverify].

The webview already includes the functionality to capture images and convert them into binary format,
which can then be transmitted to our services via the gRPC protocol.

Feel free to clone the repository and explore the code to better understand the implementation of each feature. Contributions are welcome!

If you're curious about the functionalities mentioned above and would like to explore the app without needing to install it,
you can do so on our [BioID Playground][playground], where you can get a first impression of how our services operate.

## Technologies
- ASP.NET Core 8
- gRPC
- C#

## Project structure
- `Properties/`
    - `launchsettings.json` - For configuring how the application is launched and debugged during development.
  The port on which the app runs can also be set here.It is only relevant if you start in Visual Studio.
- `Helper/` - Contains extension methods that enhance functionality and provide additional capabilities within the application.
- `Pages/` - Contains Razor Pages that define the application's UI and functionality.
- `protos/` - Contains a ProtoBuf file that is used to define the structure of data for gRPC services.
- `appsettings.json` - This file is for application specific configurations in an ASP.NET Core application.
- `Program.cs` - This is the main entry point of the application.

## Get Started
We offer a ready-to-use sample web app for Liveness Detection, PhotoVerify, and Face Deepfake Detection.
This sample is built with [.NET 8][dotnet8] and runs on Windows, Linux, and macOS.
> Please note: PhotoVerify performs a face match between ID photo and selfie in addition to liveness detection.

Download a [development tool][dotnettools] for Windows, Linux or macOS. Use your favorite development environment like Visual Studio,
Visual Studio Code, Visual Studio for Mac, .NET Core CLI or other .NET Tools.
Download a [development tool][dotnettools] for Windows, Linux, or macOS. You can use your preferred development environment, such as Visual Studio, Visual Studio Code, Visual Studio for Mac, the .NET Core CLI, or other .NET tools."


### How this sample implementation works
The web based implementation uses HTML5 with pure javascript function (please feel free to **use/copy/modify the code for your needs**).
For a fast and responsive page layout we use Bootstrap 5. You can also modify or change this for your needs.

This web application functions as a webservice, capturing images from the webcam and converting them into byte arrays. 
These byte arrays are then transmitted to the BWS gRPC service via a gRPC client,
and the returned results are displayed in the UI.

> #### Before starting the service, follow these steps.
> - You need a **BioID Account** with a **confirmed** email address. If you don’t have one [create BioID account][bioidaccountregister].
> - You can request a free [trial instance][trial] of the BioID Web Service (BWS) once you've created your BioID account.
> - Once you have received your trial access, log in to the [BWS Portal][bwsportal].
> - After logging in to the BWS portal, you will be given a trial subscription to bws. You should then create your own bws client
>  to communicate with the bws service.  The client can be created using a creation wizard.
>  - If you have created a Client, click on `Show Client Key` to open the dialog box that displays the `ClientId` and `Secret` for your Client.
>
>  **The ClientId and Secret will be explained in detail later on where to insert them.** 
 

### Installation
  
1. Clone the repository.
   ```cmd
    git clone https://github.com/BioID-GmbH/BWSClient-WebApp.git
    ```

2. Navigate to the project folder and install the dependencies.
    ```cmd
    dotnet restore
    ```

3. Add your BWS gRPC client ID and secret key to the `appsettings.json` file so that you can communicate with our BWS.
Instructions on where to obtain these are provided above.
The settings file is located in the root folder of the app.


![gRPC client ID and secret key](/bwsSettings.png)


4. Build the app for your target platform. Insert your target platform without `< >` symbol.
    ```cmd
   dotnet build  -configuration Release <target platform> --self-contained true
   ```

5. Launch the application.
    ```cmd
    dotnet run --project BioID.BWS.WebApp
    ```
 
[dotnet8]: https://dotnet.microsoft.com/download "Download .NET8"
[dotnettools]: https://dotnet.microsoft.com/platform/tools ".NET Tools & Editors"
[bioidaccountregister]: https://account.bioid.com/Account/Register "Register a BioID account" 
[trial]: https://bwsportal.bioid.com/register "Register for a trial instance"
[bwsportal]: https://bwsportal.bioid.com "BWS Portal"
[liveness]: https://www.bioid.com/liveness-detection/ "Presentation attack detection."
[photoverify]: https://www.bioid.com/identity-verification-photoverify/ "PhotoVerify"
[deepfake]: https://www.bioid.com/deepfake-detection/ "Face DeeepFake Detection"
[playground]: https://playground.bioid.com "BioID Playground"


