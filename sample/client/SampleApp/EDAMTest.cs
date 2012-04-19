/*
  A simple Evernote API demo application that authenticates with the
  Evernote web service, lists all notebooks and notes in the user's account,
  and creates a simple test note in the default notebook.
  
  Before running this sample, you must change the API consumer key
  and consumer secret to the values that you received from Evernote.

  To build (Windows):
    Open and build Evernote.sln

  To build & run (Unix Mono):
    xbuild Evernote.sln
    mono SampleApp/bin/Debug/EDAMTest.exe username password
*/

using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;
using Evernote.EDAM.Type;
using Evernote.EDAM.UserStore;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Error;
using SampleApp.Properties;

public class EDAMTest {
    public static void Main(string[] args) {

        // NOTE: Provide the consumer key and consumer secret that you received from Evernote
        String consumerKey = "";
        String consumerSecret = "";

        if (String.IsNullOrEmpty(consumerKey) || String.IsNullOrEmpty(consumerSecret)) {
          Console.WriteLine("Please set your API consumer key and secret");
          Console.WriteLine("To get an API key, visit http://dev.evernote.com/documentation/cloud/");
          return;
        }        
        if (args.Length < 2) {
            Console.WriteLine("Arguments:  <username> <password>");
            return;
        }
        String username = args[0];
        String password = args[1];

        String evernoteHost = "sandbox.evernote.com";
        String edamBaseUrl = "https://" + evernoteHost;
        // If using Mono, see http://www.mono-project.com/FAQ:_Security
        
        Uri userStoreUrl = new Uri(edamBaseUrl + "/edam/user");
        TTransport userStoreTransport = new THttpClient(userStoreUrl);
        TProtocol userStoreProtocol = new TBinaryProtocol(userStoreTransport);
        UserStore.Client userStore = new UserStore.Client(userStoreProtocol);
        
        bool versionOK =
            userStore.checkVersion("C# EDAMTest",
        	   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MAJOR,
        	   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MINOR);
        Console.WriteLine("Is my EDAM protocol version up to date? " + versionOK);
        if (!versionOK) {
            return;
        }
        
        AuthenticationResult authResult = null;
        try {
            authResult = userStore.authenticate(username, password,
                                                consumerKey, consumerSecret);
        } catch (EDAMUserException ex) {
            String parameter = ex.Parameter;
            EDAMErrorCode errorCode = ex.ErrorCode;
            
            Console.WriteLine("Authentication failed (parameter: " + parameter + " errorCode: " + errorCode + ")");
            
            if (errorCode == EDAMErrorCode.INVALID_AUTH) {
                if (parameter == "consumerKey") {
                    if (consumerKey == "en-edamtest") {
                        Console.WriteLine("You must replace the variables consumerKey and consumerSecret with the values you received from Evernote.");
                    } else {
                        Console.WriteLine("Your consumer key was not accepted by " + evernoteHost);
                        Console.WriteLine("This sample client application requires a client API key. If you requested a web service API key, you must authenticate using OAuth");
                    }
                    Console.WriteLine("If you do not have an API Key from Evernote, you can request one from http://dev.evernote.com/documentation/cloud/");
                } else if (parameter == "username") {
                    Console.WriteLine("You must authenticate using a username and password from " + evernoteHost);
                    if (evernoteHost == "www.evernote.com" == false) {
                        Console.WriteLine("Note that your production Evernote account will not work on " + evernoteHost + ",");
                        Console.WriteLine("you must register for a separate test account at https://" + evernoteHost + "/Registration.action");
                    }
                } else if (parameter == "password") {
                    Console.WriteLine("The password that you entered is incorrect");
                }
            }
            
            return;
        }
        
        User user = authResult.User;
        String authToken = authResult.AuthenticationToken;
        Console.WriteLine("Authentication successful for: " + user.Username);
        Console.WriteLine("Authentication token = " + authToken);
        
        TTransport noteStoreTransport = new THttpClient(new Uri(authResult.NoteStoreUrl));
        TProtocol noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
        NoteStore.Client noteStore = new NoteStore.Client(noteStoreProtocol);
        
        List<Notebook> notebooks = noteStore.listNotebooks(authToken);
        Console.WriteLine("Found " + notebooks.Count + " notebooks:");
        Notebook defaultNotebook = notebooks[0];
        foreach (Notebook notebook in notebooks) {
            Console.WriteLine("  * " + notebook.Name);
            if (notebook.DefaultNotebook) {
                defaultNotebook = notebook;
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("Creating a note in the default notebook: " +
                          defaultNotebook.Name);
        Console.WriteLine();
        
        ImageConverter converter = new ImageConverter();
        byte[] image = (byte[])converter.ConvertTo(Resources.enlogo, typeof(byte[]));
        byte[] hash = new MD5CryptoServiceProvider().ComputeHash(image);
        string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();
        
        Data data = new Data();
        data.Size = image.Length;
        data.BodyHash = hash;
        data.Body = image;
        
        Resource resource = new Resource();
        resource.Mime = "image/png";
        resource.Data = data;
        
        Note note = new Note();
        note.NotebookGuid = defaultNotebook.Guid;
        note.Title = "Test note from EDAMTest.cs";
        note.Content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" +
            "<en-note>Here's the Evernote logo:<br/>" +
            "<en-media type=\"image/png\" hash=\"" + hashHex + "\"/>" +
            "</en-note>";

        note.Resources = new List<Resource>();
        note.Resources.Add(resource);
        
        Note createdNote = noteStore.createNote(authToken, note);
        
        Console.WriteLine("Successfully created new note with GUID: " + createdNote.Guid);
    }
}
