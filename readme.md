# BWSClient WebApp - Liveness Detection, Face Deepfake Detection & PhotoVerify using BioID Web Service 3

## Overview
The **BWSClient WebApp** is an ASP.NET Core Web application that demonstrates key biometric features such as **Liveness Detection**, **Face Deepfake Detection**, and **PhotoVerify** using BioID's Web Service (BWS). This web application captures images from the user's webcam, converts them into byte arrays, and sends them to the **BWS gRPC service** via a gRPC client. The processed results are then displayed in the user interface.

The web app captures images from the webcam and convert them into byte arrays. These byte arrays are then transmitted to the BWS gRPC service via a gRPC client,
and the returned results are displayed in the UI.

You can explore and clone the repository to better understand the implementation of these biometric features. Contributions to the project are welcome!
<p align="center">
<a href="https://youtu.be/R-ySuJ-KiAQ"><img src="https://img.youtube.com/vi/R-ySuJ-KiAQ/0.jpg" width="50%"></a>
</p>

If you're curious about the functionalities mentioned above and would like to explore the app without needing to build it,
you can do so on our [BioID Playground][playground], where you can get a first impression of how our biometric services work.

## Key Features
- **Liveness Detection**: Ensures the presence of a live person during biometric check.
- **Face Deepfake Detection**: Identifies whether the face in the image is a deepfake or manipulated.
- **PhotoVerify**: Matches a selfie with an ID photo while performing liveness checks.

## Technologies Used
- **ASP.NET Core 8**: A powerful, cross-platform web framework.
- **gRPC**: For high-performance communication between the client and the BWS.
- **C#**: The main programming language for backend development.
- **HTML5**, **CSS**, **JavaScript**: For front-end development and user interface.
- **ProtoBuf**: Defines the data structure for communication with gRPC.

## Project Structure
- `Properties/`
    - `launchsettings.json` - For configuring how the application is launched and debugged during development.
  The port on which the app runs can also be set here.It is only relevant if you start in Visual Studio.
- `Helper/` - Contains extension methods that enhance functionality and provide additional capabilities within the application.
- `Pages/` - Contains Razor Pages that define the application's UI and functionality.
- `protos/` - Contains a ProtoBuf file that is used to define the structure of data for gRPC services.
- `appsettings.json` - This file is for application specific configurations in an ASP.NET Core application.
- `Program.cs` - This is the main entry point of the application.

## Get Started
The **BWSClient WebApp** is ready to use and provides a sample web application built with **.NET 8**. It runs on all major platforms (Windows, Linux, and macOS). Here’s how to get started:

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

### What to Expect
- After setting up, the app will allow you to capture images through your webcam.
- These images are sent to the **BWS gRPC service**, which processes them for **Liveness Detection, Face Deepfake Detection**, and **PhotoVerify**.
- The results are then displayed on the user interface, showing the status of the biometric check.

### How it Works in Detail 
[Read more](workflow.md)

### Learn More

- [Liveness Detection][liveness] – Learn how BioID detects presentation attacks using facial biometrics.


### Need Help?
- Check out the [BioID Playground][playground] to see how the features work in real-time without building the app.
- Explore the [BioID Documentation][developerdocu] for more detailed information on using the BWS API.



[dotnet8]: https://dotnet.microsoft.com/download "Download .NET8"
[dotnettools]: https://dotnet.microsoft.com/platform/tools ".NET Tools & Editors"
[bioidaccountregister]: https://account.bioid.com/Account/Register "Register a BioID account" 
[trial]: https://bwsportal.bioid.com/ "Create a free trial subscription"
[bwsportal]: https://bwsportal.bioid.com "BWS Portal"
[liveness]: https://www.bioid.com/liveness-detection/ "PAD and Deepfake detection"
[photoverify]: https://www.bioid.com/identity-verification-photoverify/ "PhotoVerify"
[deepfake]: https://www.bioid.com/deepfake-detection/ "Face DeeepFake Detection"
[playground]: https://playground.bioid.com "BioID Playground"
[developerdocu]: https://developer.bioid.com/BWS/NewBws "BioID Developer"


