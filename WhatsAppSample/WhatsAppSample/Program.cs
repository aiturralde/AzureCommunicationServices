using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;

namespace AdvancedMessagingQuickstart
{
    class Program
    {
        // La cadena de conexión de Azure Communication Services
        // Debe reemplazarse con su propia cadena de conexión
        private static string connectionString = "endpoint=https://your-resource.communication.azure.com/;accesskey=your-access-key";
        
        // Extraer el endpoint de la cadena de conexión
        private static string endpoint = connectionString.Split(';')[0].Replace("endpoint=", "");
        
        // Extraer la clave de acceso de la cadena de conexión
        private static string accessKey = connectionString.Split(';')[1].Replace("accesskey=", "");

        // Número de teléfono de WhatsApp registrado en Azure Communication Services (con el formato whatsapp:+XXXXXXXXXXX)
        private static string from = "whatsapp:+1234567890";

        // Número de teléfono del destinatario (con el formato whatsapp:+XXXXXXXXXXX)
        private static string to = "whatsapp:+0987654321";

        // Cliente HTTP para enviar solicitudes a la API
        private static HttpClient httpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Azure Communication Services - Advanced Messages quickstart samples.");

            try
            {
                // Configurar el cliente HTTP
                ConfigureHttpClient();

                // Enviar un mensaje de texto simple
                await SendSimpleTextMessage();

                // Enviar un mensaje con botones de respuesta rápida
                await SendInteractiveButtonMessage();

                // Enviar un mensaje de lista
                await SendInteractiveListMessage();

                Console.WriteLine("Mensajes enviados correctamente!");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error HTTP al enviar mensajes: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }

            Console.WriteLine("Presione cualquier tecla para salir...");
            Console.ReadKey();
        }

        /// <summary>
        /// Configura el cliente HTTP con la autenticación correcta
        /// </summary>
        private static void ConfigureHttpClient()
        {
            // Configurar URL base
            httpClient.BaseAddress = new Uri(endpoint);
            
            // Establecer timeout
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            
            // Agregar cabeceras necesarias
            httpClient.DefaultRequestHeaders.Add("api-version", "2023-05-01");
            httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            
            // La autenticación se maneja con la clave de acceso en cada solicitud
        }

        /// <summary>
        /// Envía un mensaje de texto simple a través de WhatsApp
        /// </summary>
        private static async Task SendSimpleTextMessage()
        {
            Console.WriteLine("Enviando mensaje de texto simple...");

            // Crear objeto para mensaje de texto simple
            var messageRequest = new
            {
                senderIdentifier = from,
                recipientIdentifier = to,
                channel = "whatsapp",
                content = new
                {
                    message = "¡Hola! Este es un mensaje simple de WhatsApp enviado desde Azure Communication Services."
                }
            };

            // Convertir objeto a JSON
            string jsonRequest = JsonSerializer.Serialize(messageRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Agregar cabecera de autenticación
            var request = new HttpRequestMessage(HttpMethod.Post, "/messages");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {accessKey}");

            // Enviar mensaje
            var response = await httpClient.SendAsync(request);
            
            // Leer la respuesta
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Mensaje enviado. Código de estado: {response.StatusCode}");
            Console.WriteLine($"Respuesta: {responseContent}");
        }

        /// <summary>
        /// Envía un mensaje interactivo con botones a través de WhatsApp
        /// </summary>
        private static async Task SendInteractiveButtonMessage()
        {
            Console.WriteLine("Enviando mensaje interactivo con botones...");

            // Crear contenido interactivo para WhatsApp
            var interactiveContent = new
            {
                interactive = new
                {
                    type = "button",
                    body = new
                    {
                        text = "Por favor seleccione una opción:"
                    },
                    header = new
                    {
                        type = "text",
                        text = "Opciones Disponibles"
                    },
                    action = new
                    {
                        buttons = new[]
                        {
                            new { type = "reply", reply = new { id = "option_1", title = "Opción 1" } },
                            new { type = "reply", reply = new { id = "option_2", title = "Opción 2" } },
                            new { type = "reply", reply = new { id = "option_3", title = "Opción 3" } }
                        }
                    }
                }
            };

            string interactiveJson = JsonSerializer.Serialize(interactiveContent);

            // Crear objeto para mensaje interactivo
            var messageRequest = new
            {
                senderIdentifier = from,
                recipientIdentifier = to,
                channel = "whatsapp",
                content = new
                {
                    message = interactiveJson
                }
            };

            // Convertir objeto a JSON
            string jsonRequest = JsonSerializer.Serialize(messageRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Agregar cabecera de autenticación
            var request = new HttpRequestMessage(HttpMethod.Post, "/messages");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {accessKey}");

            // Enviar mensaje
            var response = await httpClient.SendAsync(request);
            
            // Leer la respuesta
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Mensaje interactivo con botones enviado. Código de estado: {response.StatusCode}");
            Console.WriteLine($"Respuesta: {responseContent}");
        }

        /// <summary>
        /// Envía un mensaje interactivo con lista a través de WhatsApp
        /// </summary>
        private static async Task SendInteractiveListMessage()
        {
            Console.WriteLine("Enviando mensaje interactivo con lista...");

            // Crear contenido interactivo para WhatsApp
            var interactiveContent = new
            {
                interactive = new
                {
                    type = "list",
                    body = new
                    {
                        text = "Seleccione un elemento para más información:"
                    },
                    header = new
                    {
                        type = "text",
                        text = "Nuestro Catálogo"
                    },
                    action = new
                    {
                        button = "Ver Opciones",
                        sections = new[]
                        {
                            new
                            {
                                title = "Productos Populares",
                                rows = new[]
                                {
                                    new { id = "product_1", title = "Producto 1", description = "Descripción del Producto 1" },
                                    new { id = "product_2", title = "Producto 2", description = "Descripción del Producto 2" }
                                }
                            },
                            new
                            {
                                title = "Servicios",
                                rows = new[]
                                {
                                    new { id = "service_1", title = "Servicio 1", description = "Descripción del Servicio 1" },
                                    new { id = "service_2", title = "Servicio 2", description = "Descripción del Servicio 2" }
                                }
                            }
                        }
                    }
                }
            };

            string interactiveJson = JsonSerializer.Serialize(interactiveContent);

            // Crear objeto para mensaje interactivo
            var messageRequest = new
            {
                senderIdentifier = from,
                recipientIdentifier = to,
                channel = "whatsapp",
                content = new
                {
                    message = interactiveJson
                }
            };

            // Convertir objeto a JSON
            string jsonRequest = JsonSerializer.Serialize(messageRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Agregar cabecera de autenticación
            var request = new HttpRequestMessage(HttpMethod.Post, "/messages");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {accessKey}");

            // Enviar mensaje
            var response = await httpClient.SendAsync(request);
            
            // Leer la respuesta
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Mensaje interactivo con lista enviado. Código de estado: {response.StatusCode}");
            Console.WriteLine($"Respuesta: {responseContent}");
        }
    }
}