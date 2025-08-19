using MachineCheck.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MachineCheck.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<MachineData> GetMachineCheckDataAsync(long machineId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.GaskaPobierzDanePrzegladuMaszyny", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MachineId", machineId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new MachineData
                            {
                                Code = reader["Code"].ToString(),
                                Name = reader["Name"].ToString(),
                                NrEwidencyjny = reader["NrEwidencyjny"].ToString(),
                                InternalCode = reader["InternalNumber"].ToString(),
                                SerialNumber = reader["SerialNumber"].ToString(),
                                BuyDate = DateTime.Parse(reader["BuyDate"].ToString()),
                                LastCheckDate = reader["LastCheckDate"] != DBNull.Value ? DateTime.Parse(reader["LastCheckDate"].ToString()) : (DateTime?)null,
                                FirstCheckDate = reader["FirstCheckDate"] != DBNull.Value ? DateTime.Parse(reader["FirstCheckDate"].ToString()) : (DateTime?)null,
                                CheckInterval = int.TryParse(reader["CheckInterval"]?.ToString(), out var interval) ? interval : (int?)null,
                                DaysBeforeNotif = int.TryParse(reader["DaysBeforeNotif"]?.ToString(), out var daysVal) ? daysVal : (int?)null,
                                NotifEmail = reader["NotifEmail"] != DBNull.Value ? reader["NotifEmail"].ToString() : null,
                                IsSaved = reader["FirstCheckDate"] != DBNull.Value
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> ExecuteMachineCheckAsync(long machineId, int opeId, string fileName, byte[] filebytes, string fileExt)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.GaskaWykonajPrzegladMaszyny", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@MachineId", machineId);
                    command.Parameters.AddWithValue("@OpeId", opeId);
                    command.Parameters.AddWithValue("@ProtocolName", fileName);
                    command.Parameters.AddWithValue("@ProtocolData", filebytes);
                    command.Parameters.AddWithValue("@ProtocolExt", fileExt);

                    var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(returnParameter);

                    await command.ExecuteNonQueryAsync();

                    int result = (int)returnParameter.Value;
                    return result == 0;
                }
            }
        }

        public async Task<bool> SaveMachineCheckDataAsync(MachineData machine)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.GaskaZaktualizujDanePrzegladuMaszyny", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@MachineId", machine.Id);
                    command.Parameters.AddWithValue("@FirstCheckDate", machine.FirstCheckDate.HasValue ? (object)machine.FirstCheckDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@CheckInterval", machine.CheckInterval.HasValue ? (object)machine.CheckInterval.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@DaysBeforeNotif", machine.DaysBeforeNotif.HasValue ? (object)machine.DaysBeforeNotif.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@NotifEmail", !string.IsNullOrEmpty(machine.NotifEmail) ? (object)machine.NotifEmail : DBNull.Value);
                    command.Parameters.AddWithValue("@SerialNumber", !string.IsNullOrEmpty(machine.SerialNumber) ? (object)machine.SerialNumber : DBNull.Value);

                    var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(returnParameter);

                    await command.ExecuteNonQueryAsync();

                    int result = (int)returnParameter.Value;
                    return result == 1;
                }
            }
        }

        public async Task<List<MachineCheckRecord>> GetMachineCheckHistoryAsync(long machineId)
        {
            var list = new List<MachineCheckRecord>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.GaskaHistorycznePrzegladyMaszyny", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MachineId", machineId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new MachineCheckRecord
                            {
                                CheckId = (int)reader["CheckId"],
                                CheckDate = DateTime.Parse(reader["Date"].ToString()),
                                OperatorName = reader["Operator"].ToString(),
                                ProtocolPdf = reader["Protocol"] != DBNull.Value ? (byte[])reader["Protocol"] : null
                            });
                        }
                    }
                }
            }

            return list;
        }

        public async Task<bool> DeleteMachineCheckAsync(int checkId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.GaskaUsunPrzegladMaszyny", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CheckId", checkId);

                    var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(returnParameter);

                    await command.ExecuteNonQueryAsync();

                    int result = (int)returnParameter.Value;
                    return result == 0;
                }
            }
        }
    }
}