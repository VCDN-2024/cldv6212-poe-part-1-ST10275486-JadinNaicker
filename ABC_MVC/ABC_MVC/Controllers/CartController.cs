using ABC_MVC.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace ABC_MVC.Controllers
{
    public class CartController : Controller
    {
        private readonly string _queueConnectionString = "DefaultEndpointsProtocol=https;AccountName=st10275496;AccountKey=CpfBmfw/u2CiDAGJGrNOYWedlAYXqrYgH2D+9lPjyacwFuTX+ZR7gv3DugtodgImsQQ2MbypK40f+AStDs84jQ==;EndpointSuffix=core.windows.net";
        private readonly string _queueName = "cartqueue"; // Queue name

        private static readonly List<Product> CartItems = new List<Product>();

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await FetchCartItemsFromQueueAsync();
            return View(CartItems);
        }

        private async Task FetchCartItemsFromQueueAsync()
        {
            QueueClient queueClient = new QueueClient(_queueConnectionString, _queueName);

            if (await queueClient.ExistsAsync()) // Check if the queue exists
            {
                while (true)  // Loop to fetch and process messages until the queue is empty
                {
                    var receivedMessage = await queueClient.ReceiveMessageAsync();

                    if (receivedMessage.Value == null)  // Break the loop if no more messages are available
                        break;

                    var product = JsonSerializer.Deserialize<Product>(receivedMessage.Value.MessageText);
                    if (product != null)
                    {
                        
                        if (!CartItems.Any(p => p.Id == product.Id)) // Add the product to the cart if it's not already present
                        {
                            CartItems.Add(product);
                        }
                    }

                  
                }
            }
        }
    }
}
