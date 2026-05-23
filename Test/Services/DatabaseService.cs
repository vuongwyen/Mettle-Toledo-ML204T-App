using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Test.Services;

namespace Test
{
    /// <summary>
    /// Facade Offline-First: chịu trách nhiệm ghi dữ liệu cân + đồng bộ hóa nền lên Central Server.
    /// Thay thế DataRepository khi cần tích hợp NetworkService.
    /// Thread-safe: dùng SemaphoreSlim để serialize mọi truy cập SQLite.
    /// </summary>
    public sealed class DatabaseService : IDisposable
    {
        private readonly SemaphoreSlim _dbLock = new(1, 1);
        private readonly System.Threading.Timer _syncTimer;
        private readonly string _deviceId;
        private const int SyncBatchSize = 100;

        public DatabaseService(string deviceId)
        {
            _deviceId = deviceId;

            // Background timer chạy mỗi 30 giây; lần đầu sau 30s để app load xong
            _syncTimer = new System.Threading.Timer(
                callback: _ => _ = SyncPendingAsync(),
                state: null,
                dueTime: TimeSpan.FromSeconds(30),
                period: TimeSpan.FromSeconds(30));
        }

        // ──────────────────────────────────────────────────────────────────
        // PUBLIC: Lưu record theo chiến lược Offline-First
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lưu một bản đo cân. Thử push lên server ngay nếu có mạng; nếu không, lưu đệm SQLite.
        /// </summary>
        public async Task SaveRecordAsync(ScaleRecord record)
        {
            bool isOnline = await NetworkService.Instance.CheckConnectionAsync();

            if (!isOnline)
            {
                // Offline → ghi SQLite với IsSynced = 0
                await InsertAsync(record, isSynced: false);
                System.Diagnostics.Debug.WriteLine("[DatabaseService] Offline – buffered locally.");
                return;
            }

            // Online → thử push ngay
            var apiRecord = MapToApiModel(record);
            var syncedIds = await NetworkService.Instance.PushDataAsync(
                new System.Collections.Generic.List<Models.ScaleRecord> { apiRecord },
                _deviceId);

            bool pushOk = syncedIds.Count > 0;
            await InsertAsync(record, isSynced: pushOk);

            System.Diagnostics.Debug.WriteLine(
                pushOk ? "[DatabaseService] Pushed & saved (IsSynced=1)."
                        : "[DatabaseService] Push failed – saved as pending (IsSynced=0).");
        }

        // ──────────────────────────────────────────────────────────────────
        // BACKGROUND: Đẩy bù các record chưa sync
        // ──────────────────────────────────────────────────────────────────

        private async Task SyncPendingAsync()
        {
            bool isOnline = await NetworkService.Instance.CheckConnectionAsync();
            if (!isOnline) return;

            await _dbLock.WaitAsync();
            List<(long Id, Models.ScaleRecord ApiModel)> batch;
            try
            {
                batch = GetPendingBatch(SyncBatchSize);
            }
            finally
            {
                _dbLock.Release();
            }

            if (batch.Count == 0) return;

            System.Diagnostics.Debug.WriteLine($"[DatabaseService] Syncing {batch.Count} pending record(s)...");

            var syncedIds = await NetworkService.Instance.PushDataAsync(
                batch.Select(x => x.ApiModel).ToList(),
                _deviceId);

            if (syncedIds.Count == 0) return;

            // Đánh dấu đã sync dựa trên Guid map về local Id
            var syncedGuids = new HashSet<Guid>(syncedIds);
            var localIdsToMark = batch
                .Where(x => syncedGuids.Contains(x.ApiModel.Id))
                .Select(x => x.Id)
                .ToList();

            await _dbLock.WaitAsync();
            try
            {
                MarkAsSynced(localIdsToMark);
            }
            finally
            {
                _dbLock.Release();
            }

            System.Diagnostics.Debug.WriteLine($"[DatabaseService] Marked {localIdsToMark.Count} record(s) as synced.");
        }

        // ──────────────────────────────────────────────────────────────────
        // PRIVATE: SQLite helpers (thread-safe qua _dbLock)
        // ──────────────────────────────────────────────────────────────────

        private async Task InsertAsync(ScaleRecord record, bool isSynced)
        {
            await _dbLock.WaitAsync();
            try
            {
                using var conn = new SqliteConnection(DatabaseHelper.GetConnectionString());
                conn.Open();
                const string sql = @"
                    INSERT INTO ScaleRecords
                        (Timestamp, Weight, Unit, NatCode, Batch, SampleName, Location, IsSynced)
                    VALUES
                        (@Timestamp, @Weight, @Unit, @NatCode, @Batch, @SampleName, @Location, @IsSynced)";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Timestamp", record.Timestamp);
                cmd.Parameters.AddWithValue("@Weight", record.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@Unit", record.Unit ?? "g");
                cmd.Parameters.AddWithValue("@NatCode",    string.IsNullOrEmpty(record.NatCode)     ? (object)DBNull.Value : record.NatCode);
                cmd.Parameters.AddWithValue("@Batch",      string.IsNullOrEmpty(record.Batch)       ? (object)DBNull.Value : record.Batch);
                cmd.Parameters.AddWithValue("@SampleName", string.IsNullOrEmpty(record.SampleName)  ? (object)DBNull.Value : record.SampleName);
                cmd.Parameters.AddWithValue("@Location",   string.IsNullOrEmpty(record.Location)    ? (object)DBNull.Value : record.Location);
                cmd.Parameters.AddWithValue("@IsSynced",   isSynced ? 1 : 0);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                _dbLock.Release();
            }
        }

        /// <summary>Lấy batch record chưa sync. GỌI khi đã giữ _dbLock.</summary>
        private static List<(long Id, Models.ScaleRecord ApiModel)> GetPendingBatch(int limit)
        {
            var result = new List<(long, Models.ScaleRecord)>();
            using var conn = new SqliteConnection(DatabaseHelper.GetConnectionString());
            conn.Open();
            string sql = $@"
                SELECT Id, Timestamp, Weight, Unit, NatCode, Batch, SampleName, Location
                FROM ScaleRecords
                WHERE IsSynced = 0
                ORDER BY Timestamp ASC
                LIMIT {limit}";

            using var cmd    = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                long localId = reader.GetInt64(0);
                var apiModel = new Models.ScaleRecord
                {
                    Id         = Guid.NewGuid(), // Tạo Guid tạm để server nhận diện
                    Timestamp  = new DateTimeOffset(reader.GetDateTime(1), TimeSpan.FromHours(7)),
                    Weight     = (double)decimal.Parse(reader.GetString(2), System.Globalization.CultureInfo.InvariantCulture),
                    Unit       = reader.GetString(3),
                    NatCode    = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Batch      = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    SampleName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Location   = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    IsSynced   = false
                };
                result.Add((localId, apiModel));
            }
            return result;
        }

        /// <summary>Đánh dấu IsSynced = 1 theo danh sách local Id. GỌI khi đã giữ _dbLock.</summary>
        private static void MarkAsSynced(List<long> localIds)
        {
            if (localIds.Count == 0) return;
            using var conn = new SqliteConnection(DatabaseHelper.GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            foreach (long id in localIds)
            {
                using var cmd = new SqliteCommand(
                    "UPDATE ScaleRecords SET IsSynced = 1 WHERE Id = @Id",
                    conn, transaction);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        // ──────────────────────────────────────────────────────────────────
        // MAPPER: Test.ScaleRecord → Test.Models.ScaleRecord
        // ──────────────────────────────────────────────────────────────────

        private static Models.ScaleRecord MapToApiModel(ScaleRecord r) => new()
        {
            Id         = Guid.NewGuid(),
            Timestamp  = new DateTimeOffset(r.Timestamp, TimeSpan.FromHours(7)),
            Weight     = (double)r.Weight,
            Unit       = r.Unit,
            NatCode    = r.NatCode,
            Batch      = r.Batch,
            SampleName = r.SampleName,
            Location   = r.Location,
            IsSynced   = false
        };

        public void Dispose()
        {
            _syncTimer.Dispose();
            _dbLock.Dispose();
        }
    }
}
