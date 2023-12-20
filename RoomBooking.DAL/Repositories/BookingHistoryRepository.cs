using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Repositories
{
    public class BookingHistoryRepository : BaseRepository<BookingHistory>, IBookingHistoryRepository
    {
        public BookingHistoryRepository(IConfiguration configuration) : base(configuration)
        {
        }

    }
}
