using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_4_Model_Process.Models
{

    public class Client
    {
        public TimeSpan ArrivalTime { get; set; }
        public TimeSpan WaitingTime { get; set; }
        public TimeSpan CurrentServiceTime { get; set; }
        public TimeSpan ServiceTime { get; set; }
    }

    public class BarberShop
    {
        private List<Client> clients;
        private Random random;
        public List<Client> Clients => clients;

        public BarberShop()
        {
            clients = new List<Client>();
            random = new Random();
        }

        public void Simulate(int simulationTime)
        {
            int arrivalTimeMean = 18; // Mean arrival time in minutes
            int arrivalTimeStdDev = 6; // Standard deviation of arrival time in minutes
            int serviceTimeMean = 16; // Mean service time in minutes
            int serviceTimeStdDev = 4; // Standard deviation of service time in minutes

            var arrivalTimeDistribution = GetRandomValue(arrivalTimeMean, arrivalTimeStdDev);
            var serviceTimeDistribution = GetRandomValue(serviceTimeMean, serviceTimeStdDev);
            var queue = new Queue<Client>();
            var currentTime = TimeSpan.FromMinutes(0);

            for (int i = 0; i < simulationTime; i++)
            {
                // Client arrival
                if (currentTime.TotalMinutes >= arrivalTimeDistribution)
                {
                    var client = new Client
                    {
                        ArrivalTime = TimeSpan.FromMinutes(arrivalTimeDistribution),
                        CurrentServiceTime = TimeSpan.FromMinutes(serviceTimeDistribution),
                        ServiceTime = TimeSpan.FromMinutes(serviceTimeDistribution),
                        WaitingTime = TimeSpan.FromMinutes(0)
                };

                    queue.Enqueue(client);
                    clients.Add(client);

                    arrivalTimeDistribution = GetRandomValue(arrivalTimeMean, arrivalTimeStdDev);
                    serviceTimeDistribution = GetRandomValue(serviceTimeMean, serviceTimeStdDev);
                    currentTime = TimeSpan.FromMinutes(0);
                }

                // Check if there is a client being served
                if (queue.Count > 0)
                {
                    var currentClient = queue.Peek();

                    // Check if the current client has finished being served
                    if (currentClient.CurrentServiceTime <= TimeSpan.Zero)
                    {
                        queue.Dequeue();
                    }
                    else
                    {
                        currentClient.CurrentServiceTime -= TimeSpan.FromMinutes(1); // Reduce service time by 1 minute
                    }

                    foreach(var client in queue) 
                    {
                        if(currentClient != client)
                            client.WaitingTime += TimeSpan.FromMinutes(1);
                    }
                }

                currentTime += TimeSpan.FromMinutes(1);
            }
        }

        private double GetRandomValue(double mean, double stdDev)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = mean + stdDev * randStdNormal;
            return randNormal;
        }
    }

    public class MainViewModel
    {
        public SeriesCollection ChartSeries { get; set; }
        public Func<double, string> Formatter { get; set; }

        public MainViewModel()
        {
            ChartSeries = new SeriesCollection();
            Formatter = value => value.ToString("N0");

            var barberShop = new BarberShop();
            barberShop.Simulate(480); // 8 hours simulation time

            // Получение статистики из BarberShop
            var clientData = barberShop.Clients;

            // Создание серии диаграммы LineSeries и добавление данных
            var lineSeries = new LineSeries
            {
                Title = "Client Service Time",
                Values = new ChartValues<double>(clientData.Select(client => client.ServiceTime.TotalMinutes))
            };

            var lineSeries2 = new LineSeries
            {
                Title = "Client Waiting Time",
                Values = new ChartValues<double>(clientData.Select(client => client.WaitingTime.TotalMinutes))
            };

            var lineSeries3 = new LineSeries
            {
                Title = "Client Arrival Time",
                Values = new ChartValues<double>(clientData.Select(client => client.ArrivalTime.TotalMinutes))
            };

            // Добавление серии в ChartSeries
            ChartSeries.Add(lineSeries);
            ChartSeries.Add(lineSeries2);
            ChartSeries.Add(lineSeries3);
        }

    }
}
