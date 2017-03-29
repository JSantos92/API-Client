using Google.Apis.Auth.OAuth2;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apps_API
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/script-dotnet-quickstart.json
        static string[] Scopes = { "https://www.googleapis.com/auth/forms" };
        static string ApplicationName = "Google Apps Script Execution API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/script-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Apps Script Execution API service.
            string scriptId = "MIZ8ME5AgqFMS6rD8ZLQTQxU3ND9pJt_J";
            var service = new ScriptService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Create an execution request object.
            ExecutionRequest request = new ExecutionRequest();
            request.Function = "getFormAnswers";

            // Form url TODO: valor proveniente do powerpoint

            string[] formUrl = new string[] { String.Format("https://" + "docs.google.com/forms/d/11Vnlhtcw_kvjAB5QZFrYXiRUiu5Imlix-H8XOOEp9Vs/edit") };
            
            request.Parameters = formUrl; 

            ScriptsResource.RunRequest runReq =
                    service.Scripts.Run(request, scriptId);

            try
            {
                // Make the API request.
                Operation op = runReq.Execute();

                if (op.Error != null)
                {
                    // The API executed, but the script returned an error.

                    // Extract the first (and only) set of error details
                    // as a IDictionary. The values of this dictionary are
                    // the script's 'errorMessage' and 'errorType', and an
                    // array of stack trace elements. Casting the array as
                    // a JSON JArray allows the trace elements to be accessed
                    // directly.
                    IDictionary<string, object> error = op.Error.Details[0];
                    Console.WriteLine(
                            "Script error message: {0}", error["errorMessage"]);
                    if (error["scriptStackTraceElements"] != null)
                    {
                        // There may not be a stacktrace if the script didn't
                        // start executing.
                        Console.WriteLine("Script error stacktrace:");
                        Newtonsoft.Json.Linq.JArray st =
                            (Newtonsoft.Json.Linq.JArray)error["scriptStackTraceElements"];
                        foreach (var trace in st)
                        {
                            Console.WriteLine(
                                    "\t{0}: {1}",
                                    trace["function"],
                                    trace["lineNumber"]);
                        }
                    }
                }
                else
                {
                    // The result provided by the API needs to be cast into
                    // the correct type, based upon what types the Apps
                    // Script function returns. Here, the function returns
                    // an Apps Script Object with String keys and values.
                    // It is most convenient to cast the return value as a JSON
                    // JObject (folderSet).
                    Newtonsoft.Json.Linq.JObject responseSet =
                            (Newtonsoft.Json.Linq.JObject)op.Response["result"];
                    if (responseSet.Count == 0)
                    {
                        Console.WriteLine("No responses returned!");
                    }
                    else
                    {
                        Console.WriteLine("Responses:");
                        Console.WriteLine(responseSet);
                        /*foreach (var folder in folderSet)
                        {
                            Console.WriteLine(
                                "\t{0} ({1})", folder.Value, folder.Key);
                        }*/
                    }
                }
            }
            catch (Google.GoogleApiException e)
            {
                // The API encountered a problem before the script
                // started executing.
                Console.WriteLine("Error calling API:\n{0}", e);
            }
            Console.Read();
        }
    }
}
