using DocPlannerEntry.Shared;
using DocPlannerEntry.SlotManagement.Model.Availability;
using DocPlannerEntry.SlotManagement.Model.TakeSlot;
using DocPlannerEntry.SlotManagement.Service;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

namespace DocPlannerEntry.Tests
{
    public class SlotManagerTests
    {
        private readonly IOptions<SlotServiceSettings> _options;
        private readonly Mock<ILogger<SlotManager>> _loggerMock;

        public SlotManagerTests()
        {
            _options = Options.Create(new SlotServiceSettings()
            {
                BaseUrl = "testUrl",
                AvailabilityUrl = "anotherTestUrl",
                TakeSlotUrl = "yetAnotherTestUrl",
                UserName = "testUserName",
                Password = "password"
            });

            _loggerMock = new Mock<ILogger<SlotManager>>();
        }

        [Fact]
        public async void TakeSlot_HappyPath_True()
        {
            //Arrange 
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("http://testUrl.com");

            var _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var slotManager = new SlotManager(_options, _factoryMock.Object, _loggerMock.Object);

            var slotReservationRequest = new SlotReservationRequest()
            {
                FacilityId = Guid.NewGuid(),
                Start = DateTime.Now,
                End = DateTime.Now.AddMinutes(10),
                Comments = "someTestComments",
                Patient = new Patient()
                {
                    Name = "John",
                    SecondName = "Connor",
                    Email = "john.connor@gmail.com",
                    Phone = "000111222"

                }
            };

            //Act
            var result = await slotManager.TakeSlot(slotReservationRequest.FacilityId, slotReservationRequest.Start, slotReservationRequest.End, slotReservationRequest.Comments, slotReservationRequest.Patient);

            //Assert
            Assert.Equal((true, "Slot has been reserved successfully"), result);
        }

        [Fact]
        public async void TakeSlot_StartDateIsAfterEndDate_False()
        {
            //Arrange 
            var _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()));

            var slotManager = new SlotManager(_options, _factoryMock.Object, _loggerMock.Object);

            var slotReservationRequest = new SlotReservationRequest()
            {
                FacilityId = Guid.NewGuid(),
                Start = DateTime.Now.AddMinutes(10),
                End = DateTime.Now,
                Comments = "someTestComments",
                Patient = new Patient()
                {
                    Name = "John",
                    SecondName = "Connor",
                    Email = "john.connor@gmail.com",
                    Phone = "000111222"

                }
            };

            //Act
            var result = await slotManager.TakeSlot(slotReservationRequest.FacilityId, slotReservationRequest.Start, slotReservationRequest.End, slotReservationRequest.Comments, slotReservationRequest.Patient);

            //Assert
            Assert.Equal((false, "Slot has to start before ending"), result);
        }

        [Fact]
        public async void TakeSlot_PatientDataIsNull_False()
        {
            //Arrange 
            var _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()));

            var slotManager = new SlotManager(_options, _factoryMock.Object, _loggerMock.Object);

            var slotReservationRequest = new SlotReservationRequest()
            {
                FacilityId = Guid.NewGuid(),
                Start = DateTime.Now,
                End = DateTime.Now.AddMinutes(10),
                Comments = "someTestComments",
                Patient = null
            };

            //Act
            var result = await slotManager.TakeSlot(slotReservationRequest.FacilityId, slotReservationRequest.Start, slotReservationRequest.End, slotReservationRequest.Comments, slotReservationRequest.Patient);

            //Assert
            Assert.Equal((false, "Patient data cannot be empty"), result);
        }

        [Fact]
        public async void TakeSlot_PatientDataValidationFailed_False()
        {
            //Arrange 
            var _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()));

            var slotManager = new SlotManager(_options, _factoryMock.Object, _loggerMock.Object);

            var slotReservationRequest = new SlotReservationRequest()
            {
                FacilityId = Guid.NewGuid(),
                Start = DateTime.Now,
                End = DateTime.Now.AddMinutes(10),
                Comments = "someTestComments",
                Patient = new Patient()
                {
                    Name = "",
                    SecondName = "Connor",
                    Email = "john.connor@gmail.com",
                    Phone = "000111222"

                }
            };

            //Act
            var result = await slotManager.TakeSlot(slotReservationRequest.FacilityId, slotReservationRequest.Start, slotReservationRequest.End, slotReservationRequest.Comments, slotReservationRequest.Patient);

            //Assert
            Assert.Equal((false, "'Name' must not be empty."), result);
        }

        [Fact]
        public async void TakeSlot_APIResponseIsNotSuccess_False()
        {
            //Arrange 
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Content = new StringContent(@"{""Something unexpected happened""}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("http://testUrl.com");

            var _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var slotManager = new SlotManager(_options, _factoryMock.Object, _loggerMock.Object);

            var slotReservationRequest = new SlotReservationRequest()
            {
                FacilityId = Guid.NewGuid(),
                Start = DateTime.Now,
                End = DateTime.Now.AddMinutes(10),
                Comments = "someTestComments",
                Patient = new Patient()
                {
                    Name = "John",
                    SecondName = "Connor",
                    Email = "john.connor@gmail.com",
                    Phone = "000111222"

                }
            };

            //Act
            var result = await slotManager.TakeSlot(slotReservationRequest.FacilityId, slotReservationRequest.Start, slotReservationRequest.End, slotReservationRequest.Comments, slotReservationRequest.Patient);

            //Assert
            Assert.Equal((false, "Slot reservation API returned BadRequest"), result);
        }

        [Fact]
        public async void GetAvailableSlots_HappyPath_IEnumerableSlots()
        {
            //Arrange 
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(@"{
    ""Facility"": {
        ""FacilityId"": ""90c9f71c-685f-48e7-a6d5-7898775209ce"",
        ""Name"": ""Las Palmeras"",
        ""Address"": ""Plaza de la independencia 36, 38006 Santa Cruz de Tenerife""
    },
    ""SlotDurationMinutes"": 10,
    ""Monday"": {
        ""WorkPeriod"": {
            ""StartHour"": 9,
            ""EndHour"": 17,
            ""LunchStartHour"": 13,
            ""LunchEndHour"": 14
        },
        ""BusySlots"": [
            {
                ""Start"": ""2024-06-17T09:00:00"",
                ""End"": ""2024-06-17T09:10:00""
            },
            {
                ""Start"": ""2024-06-17T14:40:00"",
                ""End"": ""2024-06-17T14:50:00""
            },
            {
                ""Start"": ""2024-06-17T15:20:00"",
                ""End"": ""2024-06-17T15:30:00""
            }
        ]
    },
    ""Wednesday"": {
        ""WorkPeriod"": {
            ""StartHour"": 9,
            ""EndHour"": 17,
            ""LunchStartHour"": 13,
            ""LunchEndHour"": 14
        },
        ""BusySlots"": [
            {
                ""Start"": ""2024-06-19T15:30:00"",
                ""End"": ""2024-06-19T15:40:00""
            },
            {
                ""Start"": ""2024-06-19T12:00:00"",
                ""End"": ""2024-06-19T12:10:00""
            },
            {
                ""Start"": ""2024-06-19T12:00:00"",
                ""End"": ""2024-06-19T12:10:00""
            },
            {
                ""Start"": ""2024-06-19T12:10:00"",
                ""End"": ""2024-06-19T12:20:00""
            },
            {
                ""Start"": ""2024-06-19T09:30:00"",
                ""End"": ""2024-06-19T09:40:00""
            }
        ]
    },
    ""Friday"": {
        ""WorkPeriod"": {
            ""StartHour"": 8,
            ""EndHour"": 16,
            ""LunchStartHour"": 13,
            ""LunchEndHour"": 14
        },
        ""BusySlots"": [
            {
                ""Start"": ""2024-06-21T15:50:00"",
                ""End"": ""2024-06-21T16:00:00""
            },
            {
                ""Start"": ""2024-06-21T15:50:00"",
                ""End"": ""2024-06-21T16:00:00""
            },
            {
                ""Start"": ""2024-06-21T10:30:00"",
                ""End"": ""2024-06-21T10:40:00""
            }
        ]
    }
}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("http://testUrl.com");

            var _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var slotManager = new SlotManager(_options, _factoryMock.Object, _loggerMock.Object);

            var requestedDate = new DateTimeOffset(2024, 6, 18, 14, 9, 0, new TimeSpan(0,0,0));

            //Act
            var result = await slotManager.GetAvailableSlots(requestedDate);

            //Assert
            Assert.Equal(122, result.Count());
        }

        [Fact]
        public async void GetAvailableSlots_APIResponseIsNotSuccess_IEnumerableSlots()
        {
            //Arrange 
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("http://testUrl.com");

            var _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var slotManager = new SlotManager(_options, _factoryMock.Object, _loggerMock.Object);

            var requestedDate = new DateTimeOffset(2024, 6, 18, 14, 9, 0, new TimeSpan(0, 0, 0));

            //Act
            var result = await slotManager.GetAvailableSlots(requestedDate);

            //Assert
            Assert.Equal(Enumerable.Empty<Slot>(), result);
        }
    }
}