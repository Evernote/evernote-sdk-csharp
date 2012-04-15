//
//  Sample.cs
//  
//  A simple application demonstrating the use of the Evernote API
//  on Windows Phone 7.
//
//  Before running this sample, you must change the API consumer key
//  and consumer secret to the values that you received from Evernote.
//
//  Evernote API sample code is provided under the terms specified 
//  in the file LICENSE.txt which was included with this distribution.
//
using System;
using System.Linq;
using System.Windows;
using Thrift.Protocol;
using Thrift.Transport;
using Evernote.EDAM.Type;
using Evernote.EDAM.UserStore;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Error;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace EvernoteTest
{
    public class EDAMTest
    {
        // NOTE: You must fill in the consumer key and consumer secret that you 
        //       received from Evernote. To obtain an API key, visit
        //       http://www.evernote.com/about/developer/api
        private const string ConsumerKey = "";
        private const string ConsumerSecret = "";

        // Change the following variable to "www.evernote.com" to access the production service
        private const string EvernoteHost = "sandbox.evernote.com";
        private const string EDAMBaseUrl = "https://" + EvernoteHost;

        // UserStore service endpoint
        private const string UserStoreUrl = EDAMBaseUrl + "/edam/user";

        public static void Run()
        {
            ViewModel.TheViewModel.Notes.Clear();
            ViewModel.TheViewModel.Notebooks.Clear();
            ThreadPool.QueueUserWorkItem(RunImpl);
        }

        public static void RunImpl(object state)
        {
            // Username and password of the Evernote user account to access
            const string username = ""; // Enter your username here
            const string password = ""; // Enter your password here

            if ((ConsumerKey == String.Empty) || (ConsumerSecret == String.Empty))
            {
                ShowMessage("Please provide your API key in Sample.cs");
                return;
            }
            if ((username == String.Empty) || (password == String.Empty))
            {
                ShowMessage("Please provide your username and password in Sample.cs");
                return;
            }

            // Instantiate the libraries to connect the service
            TTransport userStoreTransport = new THttpClient(new Uri(UserStoreUrl));
            TProtocol userStoreProtocol = new TBinaryProtocol(userStoreTransport);
            UserStore.Client userStore = new UserStore.Client(userStoreProtocol);

            // Check that the version is correct
            bool versionOK =
                userStore.checkVersion("C# EDAMTest",
                   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MAJOR,
                   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MINOR);
            InvokeOnUIThread(() => ViewModel.TheViewModel.VersionOK = versionOK);
            Debug.WriteLine("Is my EDAM protocol version up to date? " + versionOK);
            if (!versionOK)
            {
                return;
            }

            // Now we are going to authenticate
            AuthenticationResult authResult;
            try
            {
                authResult = userStore.authenticate(username, password, ConsumerKey, ConsumerSecret);
            }
            catch (EDAMUserException ex)
            {
                HandleAuthenticateException(EvernoteHost, ConsumerKey, ex);
                return;
            }
            Debug.WriteLine("We are connected to the service");

            // User object received after authentication
            User user = authResult.User;
            String authToken = authResult.AuthenticationToken;
            InvokeOnUIThread(() => ViewModel.TheViewModel.AuthToken = authToken);

            Debug.WriteLine("Authentication successful for: " + user.Username);
            Debug.WriteLine("Authentication token = " + authToken);

            TTransport noteStoreTransport = new THttpClient(new Uri(authResult.NoteStoreUrl));
            TProtocol noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
            NoteStore.Client noteStore = new NoteStore.Client(noteStoreProtocol);

            // Listing all the user's notebook
            List<Notebook> notebooks = noteStore.listNotebooks(authToken);
            Debug.WriteLine("Found " + notebooks.Count + " notebooks:");
            InvokeOnUIThread(() => notebooks.ForEach(notebook => ViewModel.TheViewModel.Notebooks.Add(notebook)));

            // Find the default notebook
            Notebook defaultNotebook = notebooks.Single(notebook => notebook.DefaultNotebook);

            // Printing the names of the notebooks
            foreach (Notebook notebook in notebooks)
            {
                Debug.WriteLine("  * " + notebook.Name);
            }

            // Listing the first 10 notes in the default notebook
            NoteFilter filter = new NoteFilter { NotebookGuid = defaultNotebook.Guid };
            NoteList notes = noteStore.findNotes(authToken, filter, 0, 10);
            InvokeOnUIThread(() => notes.Notes.ForEach(note => ViewModel.TheViewModel.Notes.Add(note)));
            foreach (Note note in notes.Notes)
            {
                Debug.WriteLine("  * " + note.Title);
            }

            // Creating a new note in the default notebook
            Debug.WriteLine("Creating a note in the default notebook: " + defaultNotebook.Name);

            Note newNote = new Note
                            {
                                NotebookGuid = defaultNotebook.Guid,
                                Title = "Test note from EDAMTest.cs",
                                Content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                          "<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" +
                                          "<en-note>Here's an Evernote test note<br/>" +
                                          "</en-note>"
                            };

            Note createdNote = noteStore.createNote(authToken, newNote);

            ShowMessage("Successfully created new note with GUID: " + createdNote.Guid);
        }

        private static void ShowMessage(string error)
        {
            Debug.WriteLine(error);
            InvokeOnUIThread(() => MessageBox.Show(error));
        }

        private static void HandleAuthenticateException(string evernoteHost, string consumerKey, EDAMUserException ex)
        {
            String parameter = ex.Parameter;
            EDAMErrorCode errorCode = ex.ErrorCode;

            ShowMessage("Authentication failed (parameter: " + parameter + " errorCode: " + errorCode + ")");

            if (errorCode == EDAMErrorCode.INVALID_AUTH)
            {
                if (parameter == "consumerKey")
                {
                    ShowMessage("Your consumer key was not accepted by " + evernoteHost +
                                ". This sample client application requires a client API key. " + 
                                "If you requested a web service API key, you must authenticate using OAuth. " +
                                "If you do not have an API key from Evernote, you can request one from http://www.evernote.com/about/developer/api");
                }
                else if (parameter == "username")
                {
                    ShowMessage("You must authenticate using a username and password from " + evernoteHost);
                    if (evernoteHost == "www.evernote.com" == false)
                    {
                        ShowMessage("Note that your production Evernote account will not work on " + evernoteHost + ", " +
                                    "you must register for a separate test account at https://" + evernoteHost + "/Registration.action");
                    }
                }
                else if (parameter == "password")
                {
                    ShowMessage("The password that you entered is incorrect");
                }
            }
        }

        private static void InvokeOnUIThread(Action action)
        {
            Deployment.Current.Dispatcher.BeginInvoke(action);
        }
    }

}
