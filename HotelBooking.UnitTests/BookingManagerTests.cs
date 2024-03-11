using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.ComponentModel;
using HotelBooking.Infrastructure.Repositories;
using Moq;
using System.Collections;
using System.Collections.Generic;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

 


        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);

        }

        [Fact]
        public void FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Action act = () => bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);

            // Assert
            var bookingForReturnedRoomId = bookingRepository.GetAll().Where(
                b => b.RoomId == roomId
                && b.StartDate <= date
                && b.EndDate >= date
                && b.IsActive);

            Assert.Empty(bookingForReturnedRoomId);
        }








        



        [Theory]
        [InlineData("2024-03-05", "2024-03-07", 1, 3)] // dates fully occupied
        [InlineData("2024-03-01", "2024-03-03", 1, 0)] // dates available
        [InlineData("2024-03-02", "2024-03-05", 1, 2)] // some dates available
        [InlineData("2024-03-05", "2024-03-07", 2, 0)] // another room available
        public void GetFullyOccupiedDates_BookingOneDay_ReturnsExpectedDates(string startDateStr, string endDateStr, int noOfRooms, int expectedCount)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateStr);
            var endDate = DateTime.Parse(endDateStr);

            var roomRepositoryMock = new Mock<IRepository<Room>>();
            var bookingRepositoryMock = new Mock<IRepository<Booking>>();

            // Mocking room 
            var rooms = Enumerable.Range(1, noOfRooms).Select(_ => new Room()).ToList();
            roomRepositoryMock.Setup(repo => repo.GetAll()).Returns(rooms.AsQueryable());


            // Mocking booking
            Booking booking = new Booking();
            booking.StartDate = new DateTime(2024, 03, 04);
            booking.EndDate = new DateTime(2024, 03, 08);
            booking.Room = rooms[0];
            booking.IsActive = true;

            List<Booking> bookings = new List<Booking>();
            bookings.Add(booking);


            bookingRepositoryMock.Setup(repo => repo.GetAll()).Returns(bookings.AsQueryable());




            var bookingmanager = new BookingManager(bookingRepositoryMock.Object, roomRepositoryMock.Object);

            // Act
            var result = bookingmanager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Equal(expectedCount, result.Count);
            // Add more assertions as needed
        }



        [Fact]
        public void CreateBooking_BookingAvailable_AddsBooking()
        {
            // Arrange
            // create booking for the mock
            var booking = new Booking
            {
                StartDate = DateTime.Now.AddDays(3),
                EndDate = DateTime.Now.AddDays(5)
            };

            // create room for the mock
            var room = new Room();
            room.Description = "TestRoom";
            room.Id = 0;
            List<Room> rooms = [room];

            // initialize the mock
            var roomRepositoryMock = new Mock<IRepository<Room>>();
            var bookingRepositoryMock = new Mock<IRepository<Booking>>();

            roomRepositoryMock.Setup(repo => repo.GetAll()).Returns(rooms);
            bookingRepositoryMock.Setup(repo => repo.Add(It.IsAny<Booking>()));
            bookingRepositoryMock.Setup(repo => repo.GetAll()).Returns(new List<Booking>());

            var bookingManager = new BookingManager(bookingRepositoryMock.Object, roomRepositoryMock.Object);

            // Act
            bool result = bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result); // Booking should be created successfully

            // Verify that bookingRepository.Add() was called with the correct argument
            bookingRepositoryMock.Verify(repo => repo.Add(booking), Times.Once);
        }

    }
}
