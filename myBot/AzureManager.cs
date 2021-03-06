﻿using Microsoft.WindowsAzure.MobileServices;
using Weather_Bot.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weather_Bot
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<moodTrialDB> timelineTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://bankbot13.azurewebsites.net");
            this.timelineTable = this.client.GetTable<moodTrialDB>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task AddTimeline(moodTrialDB timeline)
        {
            await this.timelineTable.InsertAsync(timeline);
        }

        public async Task<List<moodTrialDB>> GetTimelines()
        {
            return await this.timelineTable.ToListAsync();
        }

        public async Task DeleteTimeline(moodTrialDB timeline)
        {
            await this.timelineTable.DeleteAsync(timeline);
        }

        public async Task UpdateTimeline(moodTrialDB timeline)
        {
            await this.timelineTable.UpdateAsync(timeline);
        }
    }
}
