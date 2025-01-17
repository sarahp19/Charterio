﻿namespace Charterio.Data.Seeding
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Charterio.Data.Models;

    internal class PaymentsSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Payments.Any())
            {
                return;
            }

            await dbContext.Payments.AddAsync(new Payment { PaymentMethodId = 1, TransactionId = "NODATA_1", Amount = 147.4, IsSuccessful = true });
            await dbContext.Payments.AddAsync(new Payment { PaymentMethodId = 1, TransactionId = "NODATA_2", Amount = 88.30, IsSuccessful = true });
            await dbContext.Payments.AddAsync(new Payment { PaymentMethodId = 1, TransactionId = "NODATA_3", Amount = 188.30, IsSuccessful = true });
            await dbContext.Payments.AddAsync(new Payment { PaymentMethodId = 1, TransactionId = "NODATA_4", Amount = 288.30, IsSuccessful = true });
            await dbContext.Payments.AddAsync(new Payment { PaymentMethodId = 2, TransactionId = "NODATA_5", Amount = 178.30, IsSuccessful = true });
            await dbContext.Payments.AddAsync(new Payment { PaymentMethodId = 2, TransactionId = "NODATA_6", Amount = 588.30, IsSuccessful = true });
        }
    }
}
