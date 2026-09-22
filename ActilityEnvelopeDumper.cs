using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace IoTHub
{
    public class ActilityEnvelopeDumper : IActilityEnvelopeDumper
    {
        private readonly string _connectionString;

        public ActilityEnvelopeDumper(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task Dump(string envelope)
        {
            using SqlConnection connection = new(_connectionString);

            await connection.OpenAsync();

            using SqlCommand command = new("dbo.insert_actility_envelope_dump", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // -1 for nvarchar(max)
            command.Parameters.Add("envelope", SqlDbType.NVarChar, -1).Value = envelope;

            await command.ExecuteNonQueryAsync();
        }
    }
}