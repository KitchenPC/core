using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KitchenPC
{
   public static class Extensions
   {
      public struct ReadLockHelper : IDisposable
      {
         readonly ReaderWriterLockSlim readerWriterLock;

         public ReadLockHelper(ReaderWriterLockSlim readerWriterLock)
         {
            readerWriterLock.EnterReadLock();
            this.readerWriterLock = readerWriterLock;
         }

         public void Dispose()
         {
            this.readerWriterLock.ExitReadLock();
         }
      }

      public struct UpgradeableReadLockHelper : IDisposable
      {
         readonly ReaderWriterLockSlim readerWriterLock;

         public UpgradeableReadLockHelper(ReaderWriterLockSlim readerWriterLock)
         {
            readerWriterLock.EnterUpgradeableReadLock();
            this.readerWriterLock = readerWriterLock;
         }

         public void Dispose()
         {
            this.readerWriterLock.ExitUpgradeableReadLock();
         }
      }

      public struct WriteLockHelper : IDisposable
      {
         readonly ReaderWriterLockSlim readerWriterLock;

         public WriteLockHelper(ReaderWriterLockSlim readerWriterLock)
         {
            readerWriterLock.EnterWriteLock();
            this.readerWriterLock = readerWriterLock;
         }

         public void Dispose()
         {
            this.readerWriterLock.ExitWriteLock();
         }
      }

      public static string Truncate(this string value, int maxLength)
      {
         return String.IsNullOrEmpty(value) ? value : value.Substring(0, Math.Min(value.Length, maxLength));
      }

      /// <summary>Returns an enumeration, or an empty list if the value is null.</summary>
      public static IEnumerable<T> NeverNull<T>(this IEnumerable<T> value)
      {
         return value ?? Enumerable.Empty<T>();
      }

      public static void ForEach<T>(this IEnumerable<T> query, Action<T> method)
      {
         foreach (var i in query)
         {
            method(i);
         }
      }

      public static ReadLockHelper ReadLock(this ReaderWriterLockSlim readerWriterLock)
      {
         return new ReadLockHelper(readerWriterLock);
      }

      public static UpgradeableReadLockHelper UpgradableReadLock(this ReaderWriterLockSlim readerWriterLock)
      {
         return new UpgradeableReadLockHelper(readerWriterLock);
      }

      public static WriteLockHelper WriteLock(this ReaderWriterLockSlim readerWriterLock)
      {
         return new WriteLockHelper(readerWriterLock);
      }
   }
}