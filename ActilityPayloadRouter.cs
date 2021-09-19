using IoTHub.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTHub
{
    public class ActilityPayloadRouter : IPayloadRouter
    {
        // This way of routing based on a lookup table with device IDs and stored
        // procedures will not scale, but will do for now.
        private const int CACHE_EXPIRATION_IN_MINUTES = 5;
        private readonly ConcurrentDictionary<string, string> _storedProcedureCache;
        private readonly string _connectionString;

        private readonly object _cacheUpdateTimeLock = new object();
        private DateTime _cacheUpdateTime;

        public ActilityPayloadRouter(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _storedProcedureCache = new ConcurrentDictionary<string, string>();
            _cacheUpdateTime = DateTime.MinValue;
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

        // Lock _cacheUpdateTime only to avoid long delays for other requests when the cache
        // is updated. This will allow other requests to read from the cache while it's being
        // updated.
        private async Task UpdateStoreProcedureCacheIfNeeded()
        {
            lock (_cacheUpdateTimeLock)
            {
                if (DateTime.Now < _cacheUpdateTime.AddMinutes(CACHE_EXPIRATION_IN_MINUTES))
                {
                    return;
                }
                _cacheUpdateTime = DateTime.Now;
            }
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
                string deviceId = reader.GetString(0);
                string storedProcedure = reader.GetString(1);
                _storedProcedureCache[deviceId] = storedProcedure;
            }
        }
    }
}
