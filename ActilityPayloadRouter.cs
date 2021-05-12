using IoTHub.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHub
{
    public class ActilityPayloadRouter : IPayloadRouter
    {
        private const int CACHE_EXPIRATION_IN_MINUTES = 1;
        private readonly Dictionary<string, string> _storedProcedureCache;
        private readonly string _connectionString;
        private DateTime _storedProcedureCacheLastUpdated;

        public ActilityPayloadRouter(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _storedProcedureCache = new Dictionary<string, string>();
            _storedProcedureCacheLastUpdated = DateTime.MinValue;
        }

        public async Task RoutePayload(ActilityUplinkData payload)
        {
            await UpdateStoreProcedureCacheIfNeeded();
            if (!_storedProcedureCache.TryGetValue(payload.DevEUI_Uplink.DevEUI, out string storedProcedure))
            {
                storedProcedure = "dbo.insert_non_routed_data";
            }
            using SqlConnection sqlConnection = new SqlConnection(_connectionString);
            await sqlConnection.OpenAsync();
            using SqlCommand sqlCommand = new SqlCommand()
            {
                Connection = sqlConnection,
                CommandText = storedProcedure,
                CommandType = System.Data.CommandType.StoredProcedure
            };
            sqlCommand.Parameters.AddWithValue("received", payload.DevEUI_Uplink.Time);
            sqlCommand.Parameters.AddWithValue("deviceId", payload.DevEUI_Uplink.DevEUI);
            sqlCommand.Parameters.AddWithValue("deviceModel", payload.DevEUI_Uplink.CustomerData?.Alr?.Pro);
            sqlCommand.Parameters.AddWithValue("deviceVersion", payload.DevEUI_Uplink.CustomerData?.Alr?.Ver);
            sqlCommand.Parameters.AddWithValue("payload", payload.DevEUI_Uplink.Payload_Hex);
            await sqlCommand.ExecuteNonQueryAsync();
        }

        private async Task UpdateStoreProcedureCacheIfNeeded()
        {
            if (DateTime.Now < _storedProcedureCacheLastUpdated.AddMinutes(CACHE_EXPIRATION_IN_MINUTES))
            {
                return;
            }
            _storedProcedureCache.Clear();
            using SqlConnection sqlConnection = new SqlConnection(_connectionString);
            await sqlConnection.OpenAsync();
            using SqlCommand sqlCommand = new SqlCommand()
            {
                Connection = sqlConnection,
                CommandText = "dbo.get_device_and_stored_procedure",
                CommandType = System.Data.CommandType.StoredProcedure
            };
            using SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _storedProcedureCache.Add(reader.GetString(0), reader.GetString(1));
            }
            _storedProcedureCacheLastUpdated = DateTime.Now;
        }
    }
}
