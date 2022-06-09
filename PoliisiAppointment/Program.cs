// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PoliisiAppointment;

const string BASE_URL = "https://asiointi.poliisi.fi/ajanvaraus-fe/reserve";
const string API_URL = "https://asiointi.poliisi.fi/ajanvaraus-fe/api/timereservation/";



var locations = new Dictionary<int, string>
{
    { 5501, "Helsingin pääpoliisiasema, lupapalvelut [Pasilanraitio 11]" },
    { 5531, "Espoon pääpoliisiasema [Nihtisillankuja 4 A 4]" },
    { 5561, "Vantaan pääpoliisiasema, lupapalvelut [Ratatie 11 C, 6 krs]" }
};



string BuildApiUrl(int locationId)
{
    return $"{API_URL}{locationId}";
}

Console.Write("Enter end month number (i.e 06)");
string endMonth = Console.ReadLine();
Console.Write("Enter end date (i.e 15)");
string endDate = Console.ReadLine();


while (true)
{
    Console.WriteLine($"{DateTime.Now}\nStart scraping data for \"Identity card for a foreign citizen\"\n");

    foreach (var location in locations)
    {
        try
        {
            Console.WriteLine($"Slots for location {locations[location.Key]}");
            var slots = await GetSlots(location.Key);

            ShowResults(slots);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error with parsing {location.Key} {e.Message}");
        }
    }

    var timeSpanDelay = new TimeSpan(0, 1, 0);
    Console.WriteLine($"\nWaiting next search {timeSpanDelay:g}\n");
    await Task.Delay(timeSpanDelay);
}


void ShowResults(ApiResponce? responce)
{
    if (responce != null && responce.slots != null && responce.slots.Any())
    {
        var g = responce.slots.Where(d => !d.Value.closed && d.Value.timeSlots.Count > 0)
            .OrderBy(d => d.Key);

        if (!g.Any())
        {
            Console.WriteLine($"No time slots ");
        }
        else
        {
            foreach (var b in g)
            {
                Console.WriteLine($"Date: {b.Key.Date:d} | {b.Value.timeSlots.Count} free slots");
            }
        }
    }
    else
    {
        Console.WriteLine($"Something is wrong");
    }
}


async Task<ApiResponce?> GetSlots(int locationKey)
{
    string xsrf;
    ApiResponce? responce;

    using var httpClient = new HttpClient();

    var resp = await httpClient.GetStringAsync(BASE_URL);

    xsrf = resp.RegexStringValue("window._csrf = \"(?<value>.*?)\"");

    httpClient.DefaultRequestHeaders
        .Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header

    httpClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN", xsrf);


    var json =
        $"{{\"participantMultiplier\":1," +
        $"\"siteId\":\"{locationKey}\"," +
        $"\"prereserve\":false," +
        $"\"startDate\":\"2022-04-01T21:00:00.000Z\"," +
        $"\"endDate\":\"2022-{endMonth}-{endDate}T20:59:59.999Z\"," +
        $"\"ajanvarausServiceType\":" +
        $"{{\"group\":{{\"groupCode\":\"01\",\"groupPrio\":\"1\",\"nameFi\":\"Passit ja henkilökortit\",\"nameSv\":\"Pass och identitetskort\",\"nameEn\":\"Passports and identity cards\",\"nameSe\":\"Opássat ja persovdnagoarttat\"}},\"typeUICode\":\"0404\",\"typeSystemCode\":\"AV0404\",\"nameFi\":\"Ulkomaalaisen henkilökortti\",\"nameSv\":\"Identitetskort för utlänning\",\"nameEn\":\"Identity card for a foreign citizen\",\"nameSe\":\"Olgoriikka persovdnagoarta\",\"homeDepartmentOnly\":false,\"caseType\":\"HEKO\"}},\"electronicApplication\":false}}";

    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var result = await httpClient.PostAsync(BuildApiUrl(locationKey), content);

    var s = await result.Content.ReadAsStringAsync();

    responce = JsonSerializer.Deserialize<ApiResponce>(s);

    return responce;
}