using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chirp.Tests
{
    /// <summary>
    /// TestFixture class is for creating a test server for our End-to-End tests using playwright.
    /// It starts the program from the MiniTwit.Web project and sets the environment variable for the database path.
    /// This insures the database is separate from main program database.
    /// When tests are done the server is stopped and the database file is deleted
    /// </summary>
    public class TestFixture<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public Uri BaseAddress { get; }
        public HttpClient Client { get; }
        public Process? serverProcess;
        private string testingDbFilePath;
        public TestFixture()
        {
            BaseAddress = new Uri("http://localhost:5273");
            Client = CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = BaseAddress
            });
            testingDbFilePath = Path.Combine(Path.GetTempPath(), "CheepTest.db");
        }

        public async Task EnsureServerIsReady()
        {
            // Set the environment variable
            Environment.SetEnvironmentVariable("CHIRPDBPATH", testingDbFilePath);

            serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --project ../../../../../src/MiniTwit.Web",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var tcs = new TaskCompletionSource<bool>();

            serverProcess.OutputDataReceived += (sender, args) =>
                      {
                          if (args.Data != null)
                          {
                              Console.WriteLine(args.Data);
                              //checks if the server is ready
                              if (args.Data.Contains("Application started and listening on port 5273"))
                              {
                                  tcs.SetResult(true);
                              }
                          }
                      };


            serverProcess.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();

            await tcs.Task;
        }

        public void StopServer()
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                serverProcess.Kill(true); //force kill the process
                serverProcess.WaitForExit(5000); //give it 5 seconds to gracefully exit
                serverProcess.Dispose();

            }

            string walFilePath = testingDbFilePath + "-wal";
            string shmFilePath = testingDbFilePath + "-shm";

            // Check if the database file exists and delete it
            if (File.Exists(testingDbFilePath))
            {
                File.Delete(testingDbFilePath);
            }
            // Check if the WAL file exists and delete it
            if (File.Exists(walFilePath))
            {
                File.Delete(walFilePath);
            }
            // Check if the SHM file exists and delete it
            if (File.Exists(shmFilePath))
            {
                File.Delete(shmFilePath);
            }
        }
    }
}