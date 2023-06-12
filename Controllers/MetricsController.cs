using Data;
using Models;
using Formats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace csharp_crud_api.Controllers;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


[ApiController]
[Route("deviceCal/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly MetricContext _context;
    private static readonly HttpClient client = new HttpClient();
    private static readonly String url = "http://192.168.8.103:8082";


    public MetricsController(MetricContext context)
    {
        _context = context;
    }

    // GET: deviceCal/metrics
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Metric>>> GetMetrics()
    {
        Console.WriteLine("OK");
        return await _context.Metrics.ToListAsync();
    }

    // GET: deviceCal/metrics/id
    [HttpGet("{id}")]
    public async Task<string> GetMetric(int id)
    {
        var values = new Dictionary<string, string>
        {
            { "message", "1" },
        };

        var content = new FormUrlEncodedContent(values);

        var response = await client.PostAsync(url + "/mqtt/send", content);

        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(id);
        return "OK";
    }

    // DAY SAVING
    public async Task<ActionResult<bool>> daySaving(int device_representation, int value, string unit, long time)
    {
        DateTime date = DateTime.Now;
        long ticks = date.Ticks;
        long day = ticks / TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerDay;

        MetricDay? metricDays;
        try
        {
            metricDays = await _context.MetricDays.Where(x => x.DeviceRepresentation == device_representation).FirstAsync();
        }
        catch (System.Exception)
        {
            metricDays = null;
        }

        if (metricDays?.Time / TimeSpan.TicksPerDay != day || metricDays == null)
        {
            MetricDay MetricDay = new MetricDay(device_representation, 1, value, value, unit, time);
            _context.MetricDays.Add(MetricDay);
            await _context.SaveChangesAsync();
            return true;
        }

        metricDays.DeviceRepresentation = device_representation;
        metricDays.NumberSave += 1;
        metricDays.Somme += value;
        metricDays.Average = metricDays.Somme / metricDays.NumberSave;
        await _context.SaveChangesAsync();
        return true;
    }

    // WEEK SAVING
    public async Task<ActionResult<bool>> weekSaving(int device_representation, int value, string unit, long time)
    {
        DateTime date = DateTime.Now;
        long ticks = date.Ticks;
        long week = ticks / TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerDay / 7;

        MetricWeek? metricWeeks;
        try
        {
            metricWeeks = await _context.MetricWeeks.Where(x => x.DeviceRepresentation == device_representation).FirstAsync();
        }
        catch (System.Exception)
        {
            metricWeeks = null;
        }

        if (metricWeeks?.Time / TimeSpan.TicksPerDay / 7 != week || metricWeeks == null)
        {
            MetricWeek MetricWeek = new MetricWeek(device_representation, 1, value, value, unit, time);
            _context.MetricWeeks.Add(MetricWeek);
            await _context.SaveChangesAsync();
            return true;
        }

        metricWeeks.DeviceRepresentation = device_representation;
        metricWeeks.NumberSave += 1;
        metricWeeks.Somme += value;
        metricWeeks.Average = metricWeeks.Somme / metricWeeks.NumberSave;
        await _context.SaveChangesAsync();
        return true;
    }

    // MONTH SAVING
    public async Task<ActionResult<bool>> monthSaving(int device_representation, int value, string unit, long time)
    {
        DateTime date = DateTime.Now;
        int month = date.Month;

        MetricMonth? metricMonths;
        try
        {
            metricMonths = await _context.MetricMonths.Where(x => x.DeviceRepresentation == device_representation).FirstAsync();
        }
        catch (System.Exception)
        {
            metricMonths = null;
        }


        if (metricMonths == null)
        {
            MetricMonth MetricMonth = new MetricMonth(device_representation, 1, value, value, unit, time);
                _context.MetricMonths.Add(MetricMonth);
                await _context.SaveChangesAsync();
                return true;
        } else
        {
            var monthDate = new DateTime(metricMonths!.Time);
            if (monthDate.Month != month)
            {
                MetricMonth MetricMonth = new MetricMonth(device_representation, 1, value, value, unit, time);
                _context.MetricMonths.Add(MetricMonth);
                await _context.SaveChangesAsync();
                return true;
            }
        }

        metricMonths.DeviceRepresentation = device_representation;
        metricMonths.NumberSave += 1;
        metricMonths.Somme += value;
        metricMonths.Average = metricMonths.Somme / metricMonths.NumberSave;
        await _context.SaveChangesAsync();
        return true;
    }

    // ALL SAVING
    public bool AllSaving(int device_representation, int value, string unit, long time)
    {
        List<Metric> metrics = _context.Metrics.ToList();
        int NumberSave;
        double Somme;

        if (metrics.Count == 0)
        {
            NumberSave = 1;
            Somme = value;
        }
        else
        {
            NumberSave = metrics[0].NumberSave + 1;
            Somme = metrics[0].Somme + value;
        }
        double Average = (double)Somme / NumberSave;
        Metric metric = new Metric(device_representation, NumberSave, Somme, Average, unit, time);
        _context.Metrics.Add(metric);
        _context.SaveChangesAsync();

        return true;
    }

    // POST: deviceCal/metrics
    [HttpPost("/{device_representation}/{value}/{unit}")]
    public async Task<ActionResult<bool>> PostMetricAsync(int device_representation, int value, string unit)
    {

        Console.WriteLine(device_representation);
        Console.WriteLine(value);
        Console.WriteLine(unit);

        // Send to API Broker
        using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            device_representation,
            value,
            unit
        }),
        Encoding.UTF8,
        "application/json");

        Console.WriteLine(jsonContent);

        using HttpResponseMessage response = await client.PostAsync(
            url + "/mqtt/sendMetric",
            jsonContent
        );

        Console.WriteLine("OK 1 ");
        response.EnsureSuccessStatusCode();
        Console.WriteLine("OK 2 ");
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine("OK 3 ");

        Console.WriteLine($"{jsonResponse}\n");

        // Get date to millisecond
        DateTime date = DateTime.Now;
        long ticks = date.Ticks;
        long Time = ticks / TimeSpan.TicksPerMillisecond;


        await daySaving(device_representation, value, unit, Time);
        await weekSaving(device_representation, value, unit, Time);
        await monthSaving(device_representation, value, unit, Time);

        // try
        // {
        //     Parallel.Invoke(
        //         // Day part
        //         delegate ()
        //         {
        //             daySaving(device_representation, value, unit, Time);
        //         },
        //         () =>
        //         {
        //             weekSaving(device_representation, value, unit, Time);
        //         }
        //         // () =>
        //         // {
        //         //     monthSaving(device_representation, value, unit, Time);
        //         // }
        //     );
        // }
        // catch (AggregateException? e)
        // {
        //     Console.WriteLine("An action has thrown an exception. THIS WAS UNEXPECTED.\n{0}", e.InnerException!.ToString());
        // }

        // Send metric to broker
        return true;
    }

    private bool MetricExists(int id)
    {
        return _context.Metrics.Any(e => e.Id == id);
    }

    // dummy method to test the connection
    [HttpGet("hello")]
    public string Test()
    {
        return "Hello World!";
    }
}