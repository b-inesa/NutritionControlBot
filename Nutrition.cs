using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using System.Net;
using Newtonsoft.Json;
using bot.Models;


namespace bot
{
    public class Nutrition
    {
        private static string _token { get; set; } = "5544698928:AAHY7l9rS9NeH3Ywmmfmt1OfWKSe6WtgZRw";     
        Dictionary<long,Client> allclients=new Dictionary<long,Client>();
        
        TelegramBotClient botClient = new TelegramBotClient(_token);
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessage(botClient, update.Message);
                return;
            }
            else if (update?.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackQuery(botClient, update.CallbackQuery);
                return;
            }
        }
        private async Task HandlerCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            Client client = null;

            for (int i = 0; i < allclients.Keys.Count; i++)
            {
                if (callbackQuery.Message.Chat.Id == allclients.Keys.ToList()[i])
                {
                    client = allclients[allclients.Keys.ToList()[i]];
                }
                else continue;
            }
            if (client == null)
            {
                client = new Client();
                allclients.Add(callbackQuery.Message.Chat.Id, client);
            }

            if (callbackQuery?.Data == "daystatictics")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter the date \n(for example, 17.06.2022)");
                client.datedaybool = true;
            }
            else if(callbackQuery?.Data == "monthstatistics")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter the month number \n(for example, 6, that is June)");
                client.datemonthbool = true;
            }
        }

        private async Task HandlerMessage(ITelegramBotClient botClient, Message message)
        {
            Client client = null;
            
            for(int i=0;i<allclients.Keys.Count;i++)
            {
                if (message.Chat.Id == allclients.Keys.ToList()[i])
                {
                    client = allclients[allclients.Keys.ToList()[i]];
                }
                else continue;
            }
            if(client == null)
            {
                client = new Client();
                allclients.Add(message.Chat.Id, client);
            }

            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Hi! I'm nutritioncontrolbot.");
                await botClient.SendTextMessageAsync(message.Chat.Id, "Here you can keep track of how many calories you have consumed, whether you have followed the set calorie norm, find out how many calories in a certain product and get nutritional information about the product by barcode.\nChoose a command.");
                return;
            }
            else if (message.Text == "/getcaloriesvalues")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the product \n(for example, 1 large apple)");
                client.productbool = true;

            }            
            else if (message.Text == "/setdailycalorienorm")
            {               
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter your daily calorie norm \n(for example, 2000)");
                client.daynormbool = true;
                return;
            }           
            else if (message.Text == "/addeatenproduct")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the product \n(for example, 1 large apple or 200 g fig)");
                client.addeatenproductbool = true;
            }
            else if (message.Text == "/showdailycalorienorm")
            {
                client.dateshownorm = message.Date.ToString("d");
                client.resultshownorm = await ShowCaloriesNorm(client.dateshownorm, message.Chat.Id);
                if (client.resultshownorm == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You haven't yet set a daily calorie norm");
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Your daily calorie norm: \n{client.resultshownorm}");
                }
                return;
            }
            else if (message.Text == "/checkcalorienorm")
            {
                client.datechecknorm = message.Date.ToString("d");
                client.resultchecknorm = await ControlCalories(client.datechecknorm, message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, client.resultchecknorm);
            }
            else if (message.Text == "/addproducttomyproductlist")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the product name");
                client.addnamebool = true;
                return;
            }  
            else if (message.Text == "/showmyproductlist")
            {
                client.resultproductlist = await ShowOwnListProduct(message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, client.resultproductlist);
                return;
            }
            else if (message.Text == "/addeatenproductfrommyproductlist")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the name of product from the your product list");
                client.addowneatenproductbool = true;
            }            
            else if (message.Text == "/deleteproductfrommyproductlist")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the product");
                client.deleteproductbool = true;

            }       
            else if (message.Text == "/showproductinformationbybarcode")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the barcode number");
                client.barcodenumberbool = true;
            }      
            else if (message.Text == "/showstatistics")
            {
                InlineKeyboardMarkup keyboardMarkup = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("day", "daystatictics"),
                        InlineKeyboardButton.WithCallbackData("month", "monthstatistics")
                    }
                });
                await botClient.SendTextMessageAsync(message.Chat.Id, "Choose a time period", replyMarkup: keyboardMarkup);

            }     
            else if (message.Text == "/foodhistory") 
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the date \n(for example, 17.06.2022)");
                client.foodhistorybool = true;
            }
            else if (message.Type == MessageType.Text && client.productbool == true)
            {
                client.product = message.Text;
                client.calories = await GetCalories(client.product);
                if (client.calories == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You have entered incomplete or incorrect information \nTry again.");
                    client.productbool = true;
                }
                else
                {
                    client.productbool = false;
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"In {client.product} are {client.calories} calories");
                    return;
                }
            }
            else if (message.Type == MessageType.Text && client.daynormbool == true)
            {
                try
                {
                    client.Daynorm = Convert.ToInt32(message.Text);
                    client.datenorm = message.Date.ToString("d");
                    await SetCalories(client.datenorm, message.Chat.Id, client.Daynorm);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Daily calorie norm:\n{client.Daynorm}");
                    client.daynormbool = false;
                    return;
                }
                catch (Exception)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You have entered incorrect quantity \nTry again");
                    client.daynormbool = true;
                }
                return;
            }
            else if (message.Type == MessageType.Text && client.addeatenproductbool == true)
            {
                client.product = message.Text;
                client.dateeaten = message.Date.ToString("d");
                client.resulteaten = await AddEatenProduct(client.dateeaten, message.Chat.Id, client.product);
                if (client.resulteaten == "You have entered incomplete or incorrect information \nTry again.")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, client.resulteaten);
                    client.addeatenproductbool = true;
                }
                else
                {
                    client.addeatenproductbool = false;
                    await botClient.SendTextMessageAsync(message.Chat.Id, client.resulteaten);
                    return;
                }
                return;
            }
            else if (message.Type == MessageType.Text && client.addnamebool == true)
            {
                client.namevalue = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the amount of calories");
                client.addcaloriesbool = true;
                client.addnamebool = false;
                return;
            }
            else if (message.Type == MessageType.Text && client.addcaloriesbool == true)
            {
                try
                {
                    client.caloriesvalue = Convert.ToInt32(message.Text);
                    if (client.caloriesvalue == 0)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "The amount of calories can't be 0\nTry entering a value other than 0 again");
                        client.addcaloriesbool = true;
                        client.addproteinsbool = false;
                    }
                    else
                    {
                        client.addcaloriesbool = false;
                        client.addproteinsbool = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the amount of proteins");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You entered incorrect data \nTry again");
                    client.addcaloriesbool = true;
                    client.addproteinsbool = false;
                }
                return;
            }
            else if (message.Type == MessageType.Text && client.addproteinsbool == true)
            {
                try
                {
                    client.proteinvalue = Convert.ToDouble(message.Text);
                    client.addproteinsbool = false;
                    client.addfatsbool = true;
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the amount of fats");
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You entered incorrect data \nTry again");
                    client.addproteinsbool = true;
                    client.addfatsbool = false;
                }
                return;
            }
            else if (message.Type == MessageType.Text && client.addfatsbool == true)
            {
                try
                {
                    client.fatsvalue = Convert.ToDouble(message.Text);
                    client.addfatsbool = false;
                    client.addcarbohydratesbool = true;
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the amount of carbohydrates");
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You entered incorrect data \nTry again");
                    client.addcarbohydratesbool = false;
                    client.addfatsbool = true;

                }
                return;
            }
            else if (message.Type == MessageType.Text && client.addcarbohydratesbool == true)
            {
                try
                {
                    client.carbohydratesvalue = Convert.ToDouble(message.Text);
                    client.addcarbohydratesbool = false;

                    client.resultnewproduct = await AddProductToOwnList(message.Chat.Id, client.namevalue, client.caloriesvalue, client.proteinvalue, client.fatsvalue, client.carbohydratesvalue);
                    await botClient.SendTextMessageAsync(message.Chat.Id, client.resultnewproduct);
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You entered incorrect data \nTry again");
                    client.addcarbohydratesbool = true;
                }
            }
            else if (message.Type == MessageType.Text && client.addowneatenproductbool == true)
            {
                client.dateownproduct = message.Date.ToString("d");
                client.resultownproduct = await AddEatenProductFromOwnList(client.dateownproduct, message.Chat.Id, message.Text);
                await botClient.SendTextMessageAsync(message.Chat.Id, client.resultownproduct);
                client.addowneatenproductbool = false;
            }
            else if (message.Type == MessageType.Text && client.deleteproductbool == true)
            {
                client.productdelete = message.Text;
                client.resultproductdelete = await DeletProductOwnList(message.Chat.Id, client.productdelete);
                await botClient.SendTextMessageAsync(message.Chat.Id, client.resultproductdelete);
                client.deleteproductbool = false;
            }
            else if (message.Type == MessageType.Text && client.barcodenumberbool == true)
            {
                client.barcodenumber = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the portion size \n(1 serving is 100 grams \nfor example, enter 2,5, that is 250 grams)");
                client.barcodenumberbool = false;
                client.portionbool = true;
            }
            else if (message.Type == MessageType.Text && client.portionbool == true)
            {
                try
                {
                    client.portion = Convert.ToDouble(message.Text);
                    if (client.portion == 0)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Portion size can't be 0 \nTry entering a value other than 0 again");
                        client.portionbool = true;
                    }
                    else
                    {
                        client.portionbool = false;
                        client.datebarcode = message.Date.ToString("d");
                        client.resultbarcode = await BarcodeInfo(client.datebarcode, message.Chat.Id, client.barcodenumber, client.portion);
                        await botClient.SendTextMessageAsync(message.Chat.Id, client.resultbarcode);
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You have entered incorrect portion size \nTry again");
                    client.portionbool = true;
                }
            }
            else if (message.Type == MessageType.Text && client.datedaybool == true)
            {
                try
                {
                    client.datem= Convert.ToDateTime(message.Text);
                    client.dateday = message.Text;
                    client.resultday = await DayStatictics(client.dateday, message.Chat.Id);
                    client.datedaybool = false;
                    await botClient.SendTextMessageAsync(message.Chat.Id, client.resultday);
                }
                catch(Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You have entered incorrect information \nTry again");
                    client.datedaybool = true;
                }
            }
            else if (message.Type == MessageType.Text && client.datemonthbool == true)
            {
                try
                {
                    client.dated = Convert.ToInt16(message.Text);
                    client.datemonth = message.Text;
                    client.resultmonth = await MonthStatictics(client.datemonth, message.Chat.Id);
                    await botClient.SendTextMessageAsync(message.Chat.Id, client.resultmonth);
                    client.datemonthbool = false;
                }
                catch(Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You have entered incorrect information \nTry again");
                    client.datemonthbool = true;
                }
            }
            else if(message.Type == MessageType.Text && client.foodhistorybool == true)
            {
                try
                {
                    client.dateh = Convert.ToDateTime(message.Text);
                    client.datehistory = message.Text;
                    client.resulthistory = await FoodHistory(client.datehistory, message.Chat.Id);
                    await botClient.SendTextMessageAsync(message.Chat.Id, client.resulthistory);
                    client.foodhistorybool = false;
                }
                catch(Exception ex)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You have entered incorrect information \nTry again");
                    client.foodhistorybool = true;
                }
            }
        }

        public async Task<int> GetCalories(string product)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/Calories?product={product}")
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var caloriesresponse = JsonConvert.DeserializeObject<CaloriesModel>(body);
                var result = caloriesresponse.calories;
                return result;
            }
        }
        public async Task SetCalories(string date, long id, int calories)
        {            
            SetCaloriesModel scm=new SetCaloriesModel();
            scm._userid = id;
            scm._normcalories = calories;
            scm._date = date;
            var json = JsonConvert.SerializeObject(scm);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://localhost:7103/SetCaloriesNorm?date={date}&userid={id}&daycalories={calories}";
            using var client = new HttpClient();

            var response = await client.PostAsync(url, data);                 
        }
        public async Task<string> AddEatenProduct(string date, long id, string product)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/AddEatenProduct?date={date}&userid={id}&product={product}")
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();                
                return body;
            }           
        }
        public async Task<int> ShowCaloriesNorm(string date,long userid)
        { 
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/ShowCaloriesNorm?date={date}&userid={userid}")
            };    
            
            using (var response = await client.SendAsync(request))
            { 
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();                   
                var result = Convert.ToInt32(body);                    
                return result;
            }
        }
        public async Task<string> ControlCalories(string date, long userid)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/ControlNormCalories?date={date}&userid={userid}")
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();                
                return result;
            }
        }
        public async Task<string> AddProductToOwnList(long userid, string name, int calories, double proteins, double fats, double carbohydrates)
        {
            AddProductOwnList ownproduct = new AddProductOwnList();
            ownproduct.Userid=userid;
            ownproduct.Name=name;
            ownproduct.Calories=calories;
            ownproduct.Proteins=proteins;
            ownproduct.Fats = fats;
            ownproduct.Carbohydrates = carbohydrates;

            var jsonproduct = JsonConvert.SerializeObject(ownproduct);
            var data = new StringContent(jsonproduct, Encoding.UTF8, "application/json");
            var url = $"https://localhost:7103/AddProductOwnList?userid={userid}&name={name}&calories={calories}&proteins={proteins}&fats={fats}&carbohydrates={carbohydrates}";
            using var client = new HttpClient();

            var response = await client.PostAsync(url, data);

            string result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
        public async Task<string> ShowOwnListProduct(long userid)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/ShowListOwnProducts?userid={userid}")
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();               
                return body;
            }
        }
        public async Task<string> AddEatenProductFromOwnList(string date, long userid, string product)
        {
            EatenProductModel eatenOwnProduct = new EatenProductModel();
            eatenOwnProduct.Date=date;
            eatenOwnProduct.Userid = userid;
            eatenOwnProduct.Name=product;

            var json = JsonConvert.SerializeObject(eatenOwnProduct);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://localhost:7103/AddOwnEatenProduct?date={date}&userid={userid}&product={product}";

            using var client = new HttpClient();

            var response = await client.PostAsync(url, data);

            string result = response.Content.ReadAsStringAsync().Result;
            return result;
        } 
        public async Task<string> DeletProductOwnList(long userid, string product)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7103/DeleteProductOwnList/7?product=7");
                var response = await client.DeleteAsync($"DeleteProductOwnList/{userid}?product={product}");
                var result = response.Content.ReadAsStringAsync().Result;                
                return result;
            }
             
        }
        public async Task<string> BarcodeInfo(string date, long userid, string barcode, double portion)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/AddBarcodeProduct?barcode={barcode}&portion={portion}&date={date}&userid={userid}")
            }; 

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();                
                return body;
            }
        }
        public async Task<string> DayStatictics(string date, long userid)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/DayStatictic?date={date}&userid={userid}")
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }
        public async Task<string> MonthStatictics(string date, long userid)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/MonthStatictics?userid={userid}&date={date}")
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }
        public async Task<string> FoodHistory(string date, long userid)
        {

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7103/FoodHistory?date={date}&userid={userid}")
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }
    }

}
