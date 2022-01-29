﻿namespace Charterio.Services.Data.Ticket
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Charterio.Data;
    using Charterio.Data.Models;
    using Charterio.Services.Data.Flight;
    using Charterio.Web.ViewModels.Ticket;
    using global::Data.Models;

    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext db;
        private readonly IFlightService flightService;
        private readonly IAllotmentService allotmentService;

        public TicketService(ApplicationDbContext db, IFlightService flightService, IAllotmentService allotmentService)
        {
            this.db = db;
            this.flightService = flightService;
            this.allotmentService = allotmentService;
        }

        public int CreateTicket(TicketCreateViewModel input)
        {
            var countOfExistingTickets = this.db.Tickets.Count();

            // validate offer id
            if (!this.flightService.IsFlightExisting(input.OfferId))
            {
                return 0;
            }

            // validate available seats
            if (!this.allotmentService.AreSeatsAvailable(input.OfferId, input.PaxList.Count))
            {
                return 0;
            }

            // Ticket status 3: Waiting for payment, Issuer = 1: website
            var ticket = new Ticket()
            {
                TicketCode = RandomString(4) + "-" + RandomString(4) + "-" + countOfExistingTickets.ToString("D5"),
                TicketStatusId = 3,
                TicketIssuerId = 1,
                OfferId = input.OfferId,
                UserId = input.UserId,
            };

            this.db.Tickets.Add(ticket);
            this.db.SaveChanges();

            var lastAddedTicket = this.db.Tickets.OrderByDescending(x => x.Id).FirstOrDefault();

            // Adding passangers
            var passengers = new List<TicketPassenger>();
            foreach (var pax in input.PaxList)
            {
                var newPax = new TicketPassenger
                {
                    TicketId = lastAddedTicket.Id,
                    PaxTitle = pax.PaxTitle,
                    PaxFirstName = pax.PaxFirstName,
                    PaxLastName = pax.PaxLastName,
                    DOB = pax.Dob,
                };
                passengers.Add(newPax);
            }

            this.db.TicketPassengers.AddRange(passengers);
            this.db.SaveChanges();

            return lastAddedTicket.Id;
        }

        private static string RandomString(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}