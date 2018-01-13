using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virakal.FiveM.Train.Client
{
    public class Client : BaseScript
    {
        private ICollection<Vector3> TrainSpawns { get; } = new List<Vector3>();
        private ICollection<Model> TrainModels { get; } = new List<Model>();
        private Random Random { get; } = new Random();
        private Vector3 MetroSpawn1 { get; } = new Vector3(40.2f, -1201.3f, 31);
        private Vector3 MetroSpawn2 { get; } = new Vector3(-618, -1476.8f, 16.2f);
        private Model DriverModel = new Model(@"s_m_m_lsmetro_01");

        public Client()
        {
            SetupTrainSpawns();
            SetupTrainModels();

            Tick += OnFirstTick;
        }

        private void SetupTrainSpawns()
        {
            TrainSpawns.Add(new Vector3(2533, 2833, 38));
            TrainSpawns.Add(new Vector3(2606, 2927, 40));
            TrainSpawns.Add(new Vector3(2463, 3872, 38.8f));
            TrainSpawns.Add(new Vector3(1164, 6433, 32));
            TrainSpawns.Add(new Vector3(537, -1324.1f, 29.1f));
            TrainSpawns.Add(new Vector3(219.1f, -2487.7f, 6));
        }

        private void SetupTrainModels()
        {
            TrainModels.Add(new Model(@"freight"));
            TrainModels.Add(new Model(@"freightcar"));
            TrainModels.Add(new Model(@"freightgrain"));
            TrainModels.Add(new Model(@"freightcont1"));
            TrainModels.Add(new Model(@"freightcont2"));
            TrainModels.Add(new Model(@"freighttrailer"));
            TrainModels.Add(new Model(@"tankercar"));
            TrainModels.Add(new Model(@"metrotrain"));
            TrainModels.Add(DriverModel);
        }

        private async Task OnFirstTick()
        {
            Tick -= OnFirstTick;

            EventHandlers[@"virakal:startTrain"] += new Action(OnStartTrain);

            await Task.FromResult(0);
        }

        private async void OnStartTrain()
        {
            Debug.WriteLine("The train has arrived.");

            // Remove existing trains
            API.DeleteAllTrains();

            // Load all train models
            var tasks = TrainModels.Select(model => model.Request(5000));
            bool[] success = await Task.WhenAll(tasks);

            if (!success.All((x) => x))
            {
                Debug.WriteLine("Failed to load a train model. Bailing!");
                return;
            }

            var trains = new List<int>(3);

            // Create the main train
            Vector3 spawn = TrainSpawns.ElementAt(Random.Next(TrainSpawns.Count()));
            bool faceForward = Random.Next(0, 1) == 1;
            int variation = Random.Next(0, 22);
            int metroVariation = 24;

            trains.Add(API.CreateMissionTrain(variation, spawn.X, spawn.Y, spawn.Z, faceForward));

            // Create metro trains
            trains.Add(API.CreateMissionTrain(metroVariation, MetroSpawn1.X, MetroSpawn1.Y, MetroSpawn1.Z, true));
            trains.Add(API.CreateMissionTrain(metroVariation, MetroSpawn2.X, MetroSpawn2.Y, MetroSpawn2.Z, true));

            // Add drivers and set as mission entities
            foreach (var train in trains)
            {
                API.CreatePedInsideVehicle(
                    train,
                    26, // Human ped type
                    (uint)DriverModel.Hash,
                    (int)VehicleSeat.Driver,
                    true,
                    true
                );

                API.SetEntityAsMissionEntity(train, true, true);

                // Add blip to train (delete this later)
                var blip = new Blip(API.AddBlipForEntity(train))
                {
                    Color = BlipColor.FranklinGreen,
                    Name = "Train",
                    Sprite = BlipSprite.Tonya
                };
            }

            Debug.WriteLine("Loaded trains!");
        }
    }
}
