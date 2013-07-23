//
//  Sample.cs
//  
//  A simple application demonstrating the use of the Evernote API
//  on Windows 8.
//
//  Before running this sample, you must fill in your Evernote developer token.
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
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Core;

namespace EvernoteSample
{
    public class EDAMTest
    {
        // Real applications authenticate with Evernote using OAuth, but for the
        // purpose of exploring the API, you can get a developer token that allows
        // you to access your own Evernote account. To get a developer token, visit 
        // https://sandbox.evernote.com/dev/DeveloperToken.action
        static String authToken = "your developer token";

        // Change the following variable to "www.evernote.com" to access the production service
        private const string EvernoteHost = "sandbox.evernote.com";
        private const string EDAMBaseUrl = "https://" + EvernoteHost;

        // UserStore service endpoint
        private const string UserStoreUrl = EDAMBaseUrl + "/edam/user";

        public static async void Run(DependencyObject dispatcherOwner, ViewModel viewModel)
        {
            viewModel.Notes.Clear();
            viewModel.Notebooks.Clear();
            await Task.Run(() => RunImpl(dispatcherOwner, viewModel));
        }

        public static void RunImpl(DependencyObject dispatcherOwner, ViewModel viewModel)
        {
            if (authToken == "your developer token")
            {
                ShowMessage(dispatcherOwner, "Please fill in your devleoper token in Sample.cs");
                return;
            }

            // Instantiate the libraries to connect the service
            TTransport userStoreTransport = new THttpClient(new Uri(UserStoreUrl));
            TProtocol userStoreProtocol = new TBinaryProtocol(userStoreTransport);
            UserStore.Client userStore = new UserStore.Client(userStoreProtocol);

            // Check that the version is correct
            bool versionOK =
                userStore.checkVersion("Evernote EDAMTest (WP7)",
                   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MAJOR,
                   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MINOR);
            InvokeOnUIThread(dispatcherOwner, () => viewModel.IsVersionOk = versionOK);
            Debug.WriteLine("Is my Evernote API version up to date? " + versionOK);
            if (!versionOK)
            {
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

            // Listing all the user's notebook
            List<Notebook> notebooks = noteStore.listNotebooks(authToken);
            Debug.WriteLine("Found " + notebooks.Count + " notebooks:");
            InvokeOnUIThread(dispatcherOwner, () => 
                {
                    foreach (var notebook in notebooks)
                    {
                        viewModel.Notebooks.Add(notebook);
                    }
                });

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
            InvokeOnUIThread(dispatcherOwner, () => 
                {
                    foreach (var note in notes.Notes)
                    {
                        viewModel.Notes.Add(note);
                    }
                });
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

            ShowMessage(dispatcherOwner, "Successfully created new note with GUID: " + createdNote.Guid);
        }

        private static void ShowMessage(DependencyObject dispatcherOwner, string error)
        {
            Debug.WriteLine(error);
            InvokeOnUIThread(dispatcherOwner, () => 
                {
                    MessageDialog message = new MessageDialog(error);
                    message.ShowAsync();
                });
        }

        private static async void InvokeOnUIThread(DependencyObject dispatcherOwner, Action action)
        {
            await dispatcherOwner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }
    }

}
