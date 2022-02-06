﻿namespace Charterio.Services.Data.Ticket
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Charterio.Common;
    using Charterio.Data;
    using Charterio.Data.Models;
    using Charterio.Services.Data.Flight;
    using Charterio.Services.Data.SendGrid;
    using Charterio.Web.ViewModels;
    using Charterio.Web.ViewModels.Ticket;
    using global::Data.Models;

    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext db;
        private readonly IFlightService flightService;
        private readonly IAllotmentService allotmentService;
        private readonly ISendGrid emailSender;

        public TicketService(
            ApplicationDbContext db,
            IFlightService flightService,
            IAllotmentService allotmentService,
            ISendGrid emailSender)
        {
            this.db = db;
            this.flightService = flightService;
            this.allotmentService = allotmentService;
            this.emailSender = emailSender;
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

        public TicketViewModel GetTicketById(int ticketId)
        {
            var targetTicket = this.db.Tickets.Where(x => x.Id == ticketId)
                .Select(x => new TicketViewModel
                {
                    TicketId = ticketId,
                    TicketCode = x.TicketCode,
                    UserId = x.UserId,
                    StartAptName = x.Offer.StartAirport.Name,
                    EndAptName = x.Offer.EndAirport.Name,
                    StartInLocal = x.Offer.StartTimeUtc.AddHours(x.Offer.StartAirport.UtcPosition),
                    EndInLocal = x.Offer.EndTimeUtc.AddHours(x.Offer.EndAirport.UtcPosition),
                    PaxList = this.db.TicketPassengers.Where(p => p.TicketId == ticketId)
                        .Select(p => new TicketPaxViewModel
                        {
                            PaxTitle = p.PaxTitle,
                            PaxFirstName = p.PaxFirstName,
                            PaxLastName = p.PaxLastName,
                            PaxDob = p.DOB,
                        })
                        .ToList(),
                })
                .FirstOrDefault();

            return targetTicket;
        }

        public bool IsTicketIdValid(int ticketId)
        {
            if (!this.db.Tickets.Where(x => x.Id == ticketId).Any())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<string> MarkTicketAsPaidviaStripe(int ticketId, string transactionId, string transactionCode, double amount)
        {
            // Change ticket status
            var targetTicket = this.db.Tickets.Where(x => x.Id == ticketId).FirstOrDefault();
            targetTicket.TicketStatusId = 1;

            // Insert Payment
            var payment = new Payment
            {
                PaymentMethodId = 1,
                TransactionId = transactionId,
                TransactionCode = transactionCode,
                Amount = amount,
                IsSuccessful = true,
            };

            this.db.Payments.Add(payment);
            this.db.SaveChanges();

            await this.SendConfirmationEmailAsync(ticketId);

            return "OK";
        }

        public double CalculateTicketPrice(int ticketId)
        {
            var price = this.db.Tickets.Where(x => x.Id == ticketId).Select(x => new { PerPerson = x.Offer.Price, }).FirstOrDefault();
            var ticketPax = this.db.TicketPassengers.Where(x => x.TicketId == ticketId).Count();

            return price.PerPerson * ticketPax;
        }

        private static string RandomString(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task SendConfirmationEmailAsync(int ticketId)
        {
            var user = this.db.Tickets.Where(x => x.Id == ticketId).Select(x => new { Email = x.User.Email, }).FirstOrDefault();
            var ticket = this.db.Tickets.Where(x => x.Id == ticketId).FirstOrDefault();
            var flightDetails = this.db.Tickets.Where(x => x.Id == ticketId).Select(x => new FlightViewModel()
            {
                Id = 0,
                Start = x.Offer.StartAirport.Name,
                End = x.Offer.EndAirport.Name,
                StartDate = x.Offer.StartTimeUtc,
                StartUtcPosition = x.Offer.StartAirport.UtcPosition,
                EndDate = x.Offer.EndTimeUtc,
                EndUtcPosition = x.Offer.EndAirport.UtcPosition,
                Luggage = x.Offer.Luggage,
                Catering = x.Offer.Categing,
                FlightNumber = x.Offer.Flight.Number,
                Price = 0,
                DistanceInKm = 0,
            }).FirstOrDefault();

            var paxList = this.db.TicketPassengers.Where(x => x.TicketId == ticketId).Select(x => new TicketPaxViewModel
            {
                    PaxTitle = x.PaxTitle,
                    PaxFirstName = x.PaxFirstName,
                    PaxLastName = x.PaxLastName,
                    PaxDob = x.DOB,
            }).ToList();

            if (user != null && ticket != null)
            {
                var html = new EmailHtmlTemplate().GenerateTemplate(ticket.TicketCode, paxList, flightDetails);
                await this.emailSender.SendEmailAsync(GlobalConstants.SystemEmail, GlobalConstants.SystemName, user.Email, $"Flight Ticket {ticket.TicketCode}", html);
            }
        }
    }
}
