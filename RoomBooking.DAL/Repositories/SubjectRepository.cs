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
    public class SubjectRepository : BaseRepository<Subject>, ISubjectRepository
    {
        public SubjectRepository(IConfiguration configuration) : base(configuration)
        {

        }
        /// <summary>
        /// Thực hiện lấy lớp đã đăng ký
        /// </summary>
        /// CretedBy: bqdiep (02/11/2023)
        public async Task<List<ClassSubject>> GetClassActive(ClassParam param, MySqlConnection cnn)
        {
            var res = new Class();
            DynamicParameters dynamicParameters = new DynamicParameters();
            var storeName = $"Proc_Get_Class_Active";
            dynamicParameters.Add("@UserID", param.UserID);
            dynamicParameters.Add("@BudgetYear", param.BudgetYear);
            dynamicParameters.Add("@SubjectID", param.SubjectID);
            List<ClassSubject> data = (List<ClassSubject>)await cnn.QueryAsync<ClassSubject>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);
            CloseMyConnection();
            return data;
        }
    }
}
