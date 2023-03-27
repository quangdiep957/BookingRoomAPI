using Microsoft.Extensions.Configuration;
using RoomBooking.Common.Entities;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Repositories
{
    public class BookingRequestRepository : BaseRepository<BookingRequest>, IBookingRequestRepository
    {
        public BookingRequestRepository(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
