﻿using Microsoft.WindowsAzure.MobileServices;
using myBot.DataModels;
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
        private IMobileServiceTable<contorsoDB> timelineTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://contorsobank1.azurewebsites.net");
            this.timelineTable = this.client.GetTable<contorsoDB>();
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

        public async Task AddTimeline(contorsoDB timeline)
        {
            await this.timelineTable.InsertAsync(timeline);
        }

        public async Task<List<contorsoDB>> GetTimelines()
        {
            return await this.timelineTable.ToListAsync();
        }
    }
}
