using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virakal.FiveM.Train.Server
{
    class Server : BaseScript
    {
        private bool TrainSpawned { get; set; } = false;

        public Server()
        {
            Tick += OnFirstTick;
        }

        private async Task OnFirstTick()
        {
            Tick -= OnFirstTick;

            EventHandlers[@"hardcap:playerActivated"] += new Action(OnJoin);

            await Task.FromResult(0);
        }

        private async void OnJoin()
        {
            ActivateTrain();

            await Task.FromResult(0);
        }

        private void ActivateTrain()
        {
            var players = new PlayerList();

            if (!TrainSpawned && players.Count() == 1)
            {
                // Should be some way to check if they're the arbitrator?
                TriggerClientEvent(players.First(), @"virakal:startTrain");
                Debug.WriteLine("Starting the trains.");
                TrainSpawned = true;
            }
            else
            {
                // Rerun in 15 seconds
                Task.Delay(15000).ContinueWith((task) => ActivateTrain());

                if (players.Count() == 0)
                {
                    TrainSpawned = false;
                }
            }
        }
    }
}
