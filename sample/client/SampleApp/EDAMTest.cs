/*
  A simple Evernote API demo application that authenticates with the
  Evernote web service, lists all notebooks and notes in the user's account,
  and creates a simple test note in the default notebook.
  
  Before running this sample, you must fill in your Evernote developer token.

  To build (Windows):
    Open and build Evernote.sln

  To build & run (Unix Mono):
    xbuild Evernote.sln
    mono SampleApp/bin/Debug/EDAMTest.exe
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

        // Real applications authenticate with Evernote using OAuth, but for the
        // purpose of exploring the API, you can get a developer token that allows
        // you to access your own Evernote account. To get a developer token, visit 
        // https://sandbox.evernote.com/dev/DeveloperToken.action
        String authToken = "your developer token";

        if (authToken == "your developer token") {
          Console.WriteLine("Please fill in your developer token");
          Console.WriteLine("To get a developer token, visit https://sandbox.evernote.com/dev/DeveloperToken.action");
          return;
        }

        // Once you have completed your development on our sandbox server, we will 
        // activate your API key on our production servers. To use the production servers, 
        // simply change "sandbox.evernote.com" to "www.evernote.com".
        String evernoteHost = "sandbox.evernote.com";
                
        Uri userStoreUrl = new Uri("https://" + evernoteHost + "/edam/user");
        TTransport userStoreTransport = new THttpClient(userStoreUrl);
        TProtocol userStoreProtocol = new TBinaryProtocol(userStoreTransport);
        UserStore.Client userStore = new UserStore.Client(userStoreProtocol);
        
        bool versionOK =
            userStore.checkVersion("Evernote EDAMTest (C#)",
        	   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MAJOR,
        	   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MINOR);
        Console.WriteLine("Is my Evernote API version up to date? " + versionOK);
        if (!versionOK) {
            return;
        }

        // Get the URL used to interact with the contents of the user's account
        // When your application authenticates using OAuth, the NoteStore URL will
        // be returned along with the auth token in the final OAuth request.
        // In that case, you don't need to make this call.
        String noteStoreUrl = userStore.getNoteStoreUrl(authToken);

        TTransport noteStoreTransport = new THttpClient(new Uri(noteStoreUrl));
        TProtocol noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
        NoteStore.Client noteStore = new NoteStore.Client(noteStoreProtocol);

        // List all of the notebooks in the user's account        
        List<Notebook> notebooks = noteStore.listNotebooks(authToken);
        Console.WriteLine("Found " + notebooks.Count + " notebooks:");
        foreach (Notebook notebook in notebooks) {
            Console.WriteLine("  * " + notebook.Name);
        }
        
        Console.WriteLine();
        Console.WriteLine("Creating a note in the default notebook");
        Console.WriteLine();
                
        // To create a new note, simply create a new Note object and fill in 
        // attributes such as the note's title.
        Note note = new Note();
        note.Title = "Test note from EDAMTest.cs";

        // To include an attachment such as an image in a note, first create a Resource
        // for the attachment. At a minimum, the Resource contains the binary attachment 
        // data, an MD5 hash of the binary data, and the attachment MIME type. It can also 
        // include attributes such as filename and location.
        ImageConverter converter = new ImageConverter();
        byte[] image = (byte[])converter.ConvertTo(Resources.enlogo, typeof(byte[]));
        byte[] hash = new MD5CryptoServiceProvider().ComputeHash(image);
        
        Data data = new Data();
        data.Size = image.Length;
        data.BodyHash = hash;
        data.Body = image;
        
        Resource resource = new Resource();
        resource.Mime = "image/png";
        resource.Data = data;

        // Now, add the new Resource to the note's list of resources
        note.Resources = new List<Resource>();
        note.Resources.Add(resource);

        // To display the Resource as part of the note's content, include an <en-media>
        // tag in the note's ENML content. The en-media tag identifies the corresponding
        // Resource using the MD5 hash.
        string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();

        // The content of an Evernote note is represented using Evernote Markup Language
        // (ENML). The full ENML specification can be found in the Evernote API Overview
        // at http://dev.evernote.com/documentation/cloud/chapters/ENML.php
        note.Content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" +
            "<en-note>Here's the Evernote logo:<br/>" +
            "<en-media type=\"image/png\" hash=\"" + hashHex + "\"/>" +
            "</en-note>";

        // Finally, send the new note to Evernote using the createNote method
        // The new Note object that is returned will contain server-generated
        // attributes such as the new note's unique GUID.
        Note createdNote = noteStore.createNote(authToken, note);
        
        Console.WriteLine("Successfully created new note with GUID: " + createdNote.Guid);
    }
}
