using System;
using System.Collections.Generic;
using System.Linq;
using Test.Data;

namespace Test
{
    /// <summary>
    /// Handles all SQLite database operations for ScaleRecord persistence.
    /// Uses Entity Framework Core for robust data access.
    /// </summary>
    public class DataRepository
    {
        /// <summary>
        /// Inserts a new scale measurement into the database.
        /// </summary>
        public void Insert(ScaleRecord record)
        {
            // [R-02] Serialize concurrent SQLite access with DatabaseService background timer
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    context.ScaleRecords.Add(record);
                    context.SaveChanges();
                }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        /// <summary>
        /// Inserts multiple scale measurements efficiently using a transaction.
        /// </summary>
        public void InsertBatch(IEnumerable<ScaleRecord> records)
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        context.ScaleRecords.AddRange(records);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        /// <summary>
        /// Gets all scale measurements from the database.
        /// </summary>
        public List<ScaleRecord> GetAll()
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    return context.ScaleRecords.OrderByDescending(r => r.Timestamp).ToList();
                }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        public int GetTodayCount()
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    var today = DateTime.Now.Date;
                    return context.ScaleRecords.Count(r => r.Timestamp.Date == today);
                }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        public decimal GetBatchTotal(string batch)
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    var today = DateTime.Now.Date;
                    IQueryable<ScaleRecord> query = context.ScaleRecords.Where(r => r.Timestamp.Date == today);
                    
                    if (!string.IsNullOrEmpty(batch))
                    {
                        query = query.Where(r => r.Batch == batch);
                    }

                    return query.Sum(r => r.Weight);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DataRepository.GetBatchTotal] Failed: {ex.Message}");
                return 0m;
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        public int GetUnsyncedCount()
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    return context.ScaleRecords.Count(r => !r.IsSynced);
                }
            }
            catch
            {
                return 0;
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        /// <summary>
        /// Xóa hàng loạt bản ghi theo danh sách ID.
        /// </summary>
        public void DeleteBatch(IEnumerable<long> ids)
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    var recordsToDelete = context.ScaleRecords.Where(r => ids.Contains(r.Id)).ToList();
                    if (recordsToDelete.Any())
                    {
                        context.ScaleRecords.RemoveRange(recordsToDelete);
                        context.SaveChanges();
                    }
                }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một bản ghi đã có.
        /// </summary>
        public void Update(ScaleRecord record)
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
                using (var context = new ScaleDbContext())
                {
                    // Attach and update to avoid loading from DB first
                    context.ScaleRecords.Update(record);
                    context.SaveChanges();
                }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }
    }
}
