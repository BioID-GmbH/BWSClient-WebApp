# BWSClient WebApp for BioID Web Service 3

## Overview
The **BWSClient WebApp** is an ASP.NET Web application and demonstrates the functionality of
[Liveness Detection][liveness], [Face Deepfake Detection][deepfake] and [PhotoVerify][photoverify].

The web app captures images from the webcam and convert them into byte arrays. These byte arrays are then transmitted to the BWS gRPC service via a gRPC client,
and the returned results are displayed in the UI.

Feel free to clone the repository and explore the code to better understand the implementation of each feature. Contributions are welcome!

If you're curious about the functionalities mentioned above and would like to explore the app without needing to build it,
you can do so on our [BioID Playground][playground], where you can get a first impression of how our biometric services work.

## Technologies
- ASP.NET Core 8
- gRPC
- C#
- HTML5 
- CSS
- JavaScript

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

Download a [development tool][dotnettools] for Windows, Linux, or macOS. You can use your preferred development environment,
such as Visual Studio, Visual Studio Code, Visual Studio for Mac, the .NET Core CLI, or other .NET tools.


> #### Before starting the service, follow these steps.
> - You need a **BioID Account** with a **confirmed** email address. If you don’t have one, [create a BioID account][bioidaccountregister].
> - You can create a free [trial subscription][trial] of the BioID Web Service (BWS) once you've created your BioID account.
> - After you have signed in to the BWS Portal and created the trial subscription with the help of a wizard, you still need to create a BWS 3 client.
> - The client can be created with the help of a creation wizard.
> - If you have created a client, click on `Show client keys` to open the dialog box that displays the `ClientId` and `Keys` for your client.

>  **The ClientId and Key will be explained in detail later on where to insert them.** 
  

### Installation
  
1. Clone the repository.
   ```cmd
    git clone https://github.com/BioID-GmbH/BWSClient-WebApp.git
    ```

2. Change the current directory to the BWSClient-WebApp folder
   ```cmd
    cd BWSClient-WebApp
    ```
3. Navigate to the project folder and install the dependencies.
    ```cmd
    dotnet restore
    ```

4. Add your BWS gRPC clientId and access key to the `appsettings.json` file so that you can communicate with our BWS.
Instructions on where to obtain these are provided above.
The settings file is located in the root folder of the app.


![gRPC endpoint, client Id and access key](/bwsSettings.png)

5. Launch the application.
    ```cmd
    dotnet run --project BioID.BWS.WebApp.csproj
    ```
### How it works 
[Read more](workflow.md)



[dotnet8]: https://dotnet.microsoft.com/download "Download .NET8"
[dotnettools]: https://dotnet.microsoft.com/platform/tools ".NET Tools & Editors"
[bioidaccountregister]: https://account.bioid.com/Account/Register "Register a BioID account" 
[trial]: https://bwsportal.bioid.com/ "Create a free trial subscription"
[bwsportal]: https://bwsportal.bioid.com "BWS Portal"
[liveness]: https://www.bioid.com/liveness-detection/ "Presentation attack detection."
[photoverify]: https://www.bioid.com/identity-verification-photoverify/ "PhotoVerify"
[deepfake]: https://www.bioid.com/deepfake-detection/ "Face DeeepFake Detection"
[playground]: https://playground.bioid.com "BioID Playground"


