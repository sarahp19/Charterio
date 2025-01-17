﻿namespace Charterio.Services.Data.Tests
{
    using System;
    using System.Linq;

    using Charterio.Data;
    using Charterio.Data.Models;
    using Charterio.Services.Data.Flight;
    using Charterio.Web.ViewModels.Administration.Flight;
    using Charterio.Web.ViewModels.Search;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Xunit;

    public class FlightServiceTests
    {
        // Bug found > test written
        [Fact]
        public void IfTargetOfferIsInactiveInWeb_GetFlightById_ReturnsNull()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightById").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var offer = new Offer
            {
                Name = "TestOffer",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 1,
                StartTimeUtc = DateTime.UtcNow,
                EndTimeUtc = DateTime.UtcNow.AddSeconds(1),
                Price = 1,
                CurrencyId = 1,
                AllotmentCount = 1,
                IsActiveInWeb = false,
            };
            dbContext.Offers.Add(offer);
            Assert.Null(flightService.GetById(1));
        }

        // Bug found > test written
        [Fact]
        public void IfFlightIsInThePast_GetFlightById_ReturnsNull()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightTestOffer").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var offer = new Offer
            {
                Name = "TestOffer",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 1,
                StartTimeUtc = DateTime.UtcNow.AddDays(-2),
                EndTimeUtc = DateTime.UtcNow.AddDays(-1),
                Price = 1,
                CurrencyId = 1,
                AllotmentCount = 1,
                IsActiveInWeb = false,
            };
            dbContext.Offers.Add(offer);
            Assert.Null(flightService.GetById(1));
        }

        // Bug found > test written
        [Fact]
        public void IfFlightIsInThePast_GetFlightsBySearchTerms_ReturnsEmptyList()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightPastFlight").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var offer = new Offer
            {
                Name = "TestOffer",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 1,
                StartTimeUtc = DateTime.UtcNow.AddDays(-2),
                EndTimeUtc = DateTime.UtcNow.AddDays(-1),
                Price = 1,
                CurrencyId = 1,
                AllotmentCount = 1,
                IsActiveInWeb = false,
            };

            dbContext.Offers.Add(offer);
            var terms = new SearchViewModel
            {
                StartFlightDate = DateTime.UtcNow.AddDays(-1),
                EndFlightDate = DateTime.UtcNow,
                PaxCount = 1,
            };
            Assert.Equal(0, flightService.GetFlightsBySearchTerms(terms).Count);
        }

        [Fact]
        public void GetAllAirportsReturnsListOfAirports()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_Airports").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var airport = new Airport
            {
                IataCode = "SOF",
                Name = "Sofia Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };

            dbContext.Airports.Add(airport);
            dbContext.SaveChanges();

            var list = flightService.GetAllAirports();
            Assert.Equal(1, list.Count);
            Assert.Equal("SOF", list.FirstOrDefault().IataCode);
        }

        [Fact]
        public void GetAllAirportsReturnsZeroIfNoAirportInDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_NoAirports").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var list = flightService.GetAllAirports();
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void GetFlightsBySearchTermsReturnsCorrectFlightOffers()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_CorrectOffer").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var startAirport = new Airport
            {
                IataCode = "LON",
                Name = "London Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };
            var endAirport = new Airport
            {
                IataCode = "AMS",
                Name = "Amsterdam Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };

            dbContext.Airports.Add(startAirport);
            dbContext.Airports.Add(endAirport);

            dbContext.SaveChanges();

            var offerFirst = new Offer
            {
                Name = "Charter > London - Amsterdam",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 2,
                StartTimeUtc = new DateTime(2023, 5, 26, 11, 20, 00).ToUniversalTime(),
                EndTimeUtc = new DateTime(2023, 5, 26, 13, 05, 00).ToUniversalTime(),
                Price = 189,
                CurrencyId = 1,
                AllotmentCount = 25,
                IsActiveInWeb = true,
                IsActiveInAdmin = true,
                CreatedOn = new DateTime(2022, 1, 20, 13, 05, 00).ToUniversalTime(),
                Categing = "1 bottle of water",
                Luggage = "20 kg checked in luggage, 5 kg cabin luggage",
            };
            dbContext.Offers.Add(offerFirst);
            dbContext.SaveChanges();

            var offers = dbContext.Offers.ToArray();

            var termsCorrect = new SearchViewModel
            {
                StartApt = "LON",
                EndApt = "AMS",
                StartFlightDate = new DateTime(2023, 5, 20, 12, 20, 00).ToUniversalTime(),
                EndFlightDate = new DateTime(2023, 5, 30, 12, 05, 00).ToUniversalTime(),
                PaxCount = 1,
            };

            var list = flightService.GetFlightsBySearchTerms(termsCorrect);

            Assert.Equal(1, list.Count);
        }

        [Fact]
        public void IsFlightExistingReturnsFalse()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightExistingFail").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            Assert.False(flightService.IsFlightExisting(1));
        }

        [Fact]
        public void IsFlightExistingReturnsTrue()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightExistingOk").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var startAirport = new Airport
            {
                IataCode = "LON",
                Name = "London Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };
            var endAirport = new Airport
            {
                IataCode = "AMS",
                Name = "Amsterdam Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };

            dbContext.Airports.Add(startAirport);
            dbContext.Airports.Add(endAirport);

            dbContext.SaveChanges();

            var offerFirst = new Offer
            {
                Name = "Charter > London - Amsterdam",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 2,
                StartTimeUtc = new DateTime(2023, 5, 26, 11, 20, 00).ToUniversalTime(),
                EndTimeUtc = new DateTime(2023, 5, 26, 13, 05, 00).ToUniversalTime(),
                Price = 189,
                CurrencyId = 1,
                AllotmentCount = 25,
                IsActiveInWeb = true,
                IsActiveInAdmin = true,
                CreatedOn = new DateTime(2022, 1, 20, 13, 05, 00).ToUniversalTime(),
                Categing = "1 bottle of water",
                Luggage = "20 kg checked in luggage, 5 kg cabin luggage",
            };
            dbContext.Offers.Add(offerFirst);
            dbContext.SaveChanges();

            Assert.True(flightService.IsFlightExisting(1));
        }

        [Fact]

        public void Cheapest3FlightReturnData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightExisting").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var startAirport = new Airport
            {
                IataCode = "LON",
                Name = "London Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };
            var endAirport = new Airport
            {
                IataCode = "AMS",
                Name = "Amsterdam Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };

            dbContext.Airports.Add(startAirport);
            dbContext.Airports.Add(endAirport);

            dbContext.SaveChanges();

            var offerFirst = new Offer
            {
                Name = "Charter > London - Amsterdam",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 2,
                StartTimeUtc = new DateTime(2023, 5, 26, 11, 20, 00).ToUniversalTime(),
                EndTimeUtc = new DateTime(2023, 5, 26, 13, 05, 00).ToUniversalTime(),
                Price = 189,
                CurrencyId = 1,
                AllotmentCount = 25,
                IsActiveInWeb = true,
                IsActiveInAdmin = true,
                CreatedOn = new DateTime(2022, 1, 20, 13, 05, 00).ToUniversalTime(),
                Categing = "1 bottle of water",
                Luggage = "20 kg checked in luggage, 5 kg cabin luggage",
            };
            var second = offerFirst;
            dbContext.Offers.Add(offerFirst);
            dbContext.Offers.Add(second);
            dbContext.SaveChanges();

            Assert.NotNull(flightService.GetCheapest3Flights());
        }

        [Fact]

        public void GetOfferPriceReturnsDouble()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightPrice").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var startAirport = new Airport
            {
                IataCode = "LON",
                Name = "London Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };
            var endAirport = new Airport
            {
                IataCode = "AMS",
                Name = "Amsterdam Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };

            dbContext.Airports.Add(startAirport);
            dbContext.Airports.Add(endAirport);

            dbContext.SaveChanges();

            var offerFirst = new Offer
            {
                Name = "Charter > London - Amsterdam",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 2,
                StartTimeUtc = new DateTime(2023, 5, 26, 11, 20, 00).ToUniversalTime(),
                EndTimeUtc = new DateTime(2023, 5, 26, 13, 05, 00).ToUniversalTime(),
                Price = 189,
                CurrencyId = 1,
                AllotmentCount = 25,
                IsActiveInWeb = true,
                IsActiveInAdmin = true,
                CreatedOn = new DateTime(2022, 1, 20, 13, 05, 00).ToUniversalTime(),
                Categing = "1 bottle of water",
                Luggage = "20 kg checked in luggage, 5 kg cabin luggage",
            };
            dbContext.Offers.Add(offerFirst);
            dbContext.SaveChanges();

            Assert.Equal(189, flightService.GetOfferPrice(1));
        }

        [Fact]
        public void GetOfferAirportsAsStringReturnCorrectData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_FlightPrice").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var startAirport = new Airport
            {
                IataCode = "LON",
                Name = "London Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };
            var endAirport = new Airport
            {
                IataCode = "AMS",
                Name = "Amsterdam Airport",
                UtcPosition = 0,
                Latitude = 1,
                Longtitude = 2,
            };

            dbContext.Airports.Add(startAirport);
            dbContext.Airports.Add(endAirport);

            dbContext.SaveChanges();

            var offerFirst = new Offer
            {
                Name = "Charter > London - Amsterdam",
                FlightId = 1,
                StartAirportId = 1,
                EndAirportId = 2,
                StartTimeUtc = new DateTime(2023, 5, 26, 11, 20, 00).ToUniversalTime(),
                EndTimeUtc = new DateTime(2023, 5, 26, 13, 05, 00).ToUniversalTime(),
                Price = 189,
                CurrencyId = 1,
                AllotmentCount = 25,
                IsActiveInWeb = true,
                IsActiveInAdmin = true,
                CreatedOn = new DateTime(2022, 1, 20, 13, 05, 00).ToUniversalTime(),
                Categing = "1 bottle of water",
                Luggage = "20 kg checked in luggage, 5 kg cabin luggage",
            };
            dbContext.Offers.Add(offerFirst);
            dbContext.SaveChanges();

            Assert.Equal("LON - AMS", flightService.GetOfferAirportsAsString(1));
        }

        // Admin services
        [Fact]
        public void GetAllFlightsReturnsCorrectNumber()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_AdminFlights").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var company = new Company
            {
                Name = "TestName",
            };
            var plane = new Plane
            {
                Model = "Test Model",
            };

            dbContext.Planes.Add(plane);
            dbContext.Companies.Add(company);

            dbContext.SaveChanges();

            var flight = new Flight
            {
                CompanyId = 1,
                PlaneId = 1,
                Number = "Test Number",
            };
            dbContext.Flights.Add(flight);

            dbContext.SaveChanges();

            var countFlights = flightService.GetAllFlights().Count();

            Assert.Equal(1, countFlights);
        }

        [Fact]
        public void AddFlightIncreaseFlightCount()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_AddFlightsNew").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var company = new Company
            {
                Name = "TestName",
            };
            var plane = new Plane
            {
                Model = "Test Model",
            };

            dbContext.Planes.Add(plane);
            dbContext.Companies.Add(company);

            dbContext.SaveChanges();

            flightService.Add(new Web.ViewModels.Administration.Flight.FlightAdminAddViewModel
            {
                Number = "test",
                CompanyId = "1",
                PlaneId = "1",
            });

            var countFlights = flightService.GetAllFlights().Count();

            Assert.Equal(1, countFlights);
        }

        [Fact]
        public void GetFlightByIdReturnsCorrectData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Database_For_Tests_AddFlights").Options;
            var dbContext = new ApplicationDbContext(options);
            var allotmentService = new AllotmentService(dbContext);
            var flightService = new FlightService(dbContext, allotmentService);

            var company = new Company
            {
                Name = "TestName",
            };
            var plane = new Plane
            {
                Model = "Test Model",
            };

            dbContext.Planes.Add(plane);
            dbContext.Companies.Add(company);

            dbContext.SaveChanges();
            flightService.Add(new Web.ViewModels.Administration.Flight.FlightAdminAddViewModel
            {
                Number = "test",
                CompanyId = "1",
                PlaneId = "1",
            });

            var flight = flightService.GetById(1);
            Assert.Equal("test", flight.Number);
        }
    }
}
