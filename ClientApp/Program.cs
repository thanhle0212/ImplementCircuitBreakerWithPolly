using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        private static AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        static Program()
        {
            _circuitBreakerPolicy = Policy
                    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .CircuitBreakerAsync(1, TimeSpan.FromSeconds(10), OnBreak, OnReset, OnHalfOpen);
        }

        static async Task Main(string[] args)
        {
            var apiClient = new HttpClient();
            int i = 0;
            while (true)
            {
                i++;
                Console.WriteLine($"{i}. Start calling to Web API");
                Console.WriteLine("\n");
                Console.WriteLine("-------------------------------------------------------------------------------------------");
                // Start calling to WebAPI
                var apiResponse = new HttpResponseMessage();
                try
                {
                    apiResponse = await _circuitBreakerPolicy.ExecuteAsync(
                        () => apiClient.GetAsync("https://localhost:5001/weatherforecast", HttpCompletionOption.ResponseContentRead)
                        );
                    var json = await apiResponse.Content.ReadAsStringAsync();
                    // End calling to WebAPI

                    Console.WriteLine($"Http Status Code: {apiResponse.StatusCode}");
                    Console.WriteLine("\n");
                    Console.WriteLine($"Response: {json}");
                    Console.WriteLine("\n");
                    Console.WriteLine($"{i}. End calling to Web API");
                    Console.WriteLine("\n");
                    Console.WriteLine("-------------------------------------------------------------------------------------------");
                    Console.WriteLine("Type any key and press Enter to make new calling to Web API");
                    Console.WriteLine("-------------------------------------------------------------------------------------------");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
        }

        private static void OnHalfOpen()
        {
            Console.WriteLine("Connection half open - Circuit Breaker State is HALF-OPEN");
        }

        private static void OnReset(Context context)
        {
            Console.WriteLine("Connection reset - Circuit Breaker State is CLOSED");
        }

        private static void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, Context context)
        {
            Console.WriteLine("Connection is Closed - Circuit Breaker State is OPEN");
        }
    }
}
