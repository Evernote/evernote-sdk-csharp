Evernote SDK for C#
========================================

Evernote API version 1.22

Overview
--------
This SDK contains wrapper code used to call the Evernote Cloud API from C# applications.

The SDK also contains two sample solutions. The solution in sample/client demonstrates the basic use of the SDK in a .NET application like you might build for the Windows desktop. The solution in sample/wp7 demonstrates the basic use of the SDK in a Windows Phone 7 Silverlight application.

Prerequisites
-------------
In order to use the code in this SDK, you need to obtain an API key from http://dev.evernote.com/documentation/cloud. You'll also find full API documentation on that page.

In order to run the sample code, you need a user account on the sandbox service where you will do your development. Sign up for an account at https://sandbox.evernote.com/Registration.action 

In order to run the client client sample code, you need a developer token. Get one at https://sandbox.evernote.com/api/DeveloperToken.action

Getting Started - Windows
-------------------------
The project in sample\client demonstrates the basics of using the Evernote API, using developer tokens to simplify the authentication process while you're learning. 

1. Open sample\client\Evernote.sln
2. In the Solution Explorer, open the SampleApp project and then the EDAMTest.cs file.
3. Scroll down to the top of the EDAMTest class and fill in your Evernote developer token.
4. Build the solution, which produces a command-line application.
5. Run the sample app

Getting Started - Windows Phone 7
---------------------------------
1. Open sample\wp7\EvernoteSample.sln. 
2. In the Solution Explorer, open the EvernoteSample project and then the Sample.cs file.
3. Scroll down to the top of the EDAMTest class and fill in your Evernote developer token.
4. Build the solution and run it in the Windows Phone Emulator.
5. Click the "Start the connection process" button in the sample app.
