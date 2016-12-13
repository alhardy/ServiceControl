﻿namespace ServiceControl.Recoverability
{
    using System;
    using NServiceBus.Logging;

    public class RetryOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RetryOperation));

        public RetryOperation(string requestId, RetryType retryType)
        {
            this.requestId = requestId;
            this.retryType = retryType;
        }

        public IRetryOperationProgressNotifier Notifier { get; set; }
        public int TotalNumberOfMessages { get; private set; }
        public int NumberOfMessagesPrepared { get; private set; }
        public int NumberOfMessagesForwarded { get; private set; }
        public int NumberOfMessagesSkipped { get; set; }
        public DateTime? CompletionTime { get; private set; }
        public DateTime? Last{ get; private set; }
        public bool Failed { get; private set; }
        public string Originator { get; private set; }
        public string Classifier { get; private set; }
        public DateTime Started { get; private set; }
        public RetryState RetryState { get; private set; }
        private readonly string requestId;
        private readonly RetryType retryType;


        public static string MakeOperationId(string requestId, RetryType retryType)
        {
            return $"{retryType}/{requestId}";
        }

        public void Wait(DateTime started, string originator = null, string classifier = null, DateTime? last = null)
        {
            RetryState = RetryState.Waiting;
            NumberOfMessagesPrepared = 0;
            NumberOfMessagesForwarded = 0;
            TotalNumberOfMessages = 0;
            NumberOfMessagesSkipped = 0;
            CompletionTime = null;
            Originator = originator;
            Started = started;
            Failed = false;
            Last = last;
            Classifier = classifier;

            Notifier?.Wait(requestId, retryType, GetProgress(), Started);
        }

        public void Fail()
        {
            Failed = true;
        }

        public void Prepare(int totalNumberOfMessages)
        {
            RetryState = RetryState.Preparing;
            TotalNumberOfMessages = totalNumberOfMessages;
            NumberOfMessagesForwarded = 0;
            NumberOfMessagesPrepared = 0;

            Notifier?.Prepare(requestId, retryType, TotalNumberOfMessages, GetProgress(), Failed, Started);
        }

        public void PrepareBatch(int numberOfMessagesPrepared)
        {
            NumberOfMessagesPrepared = numberOfMessagesPrepared;

            Notifier?.PrepareBatch(requestId, retryType, TotalNumberOfMessages, GetProgress(), Failed, Started);
        }

        public void PrepareAdoptedBatch(int numberOfMessagesPrepared, string originator, string classifier, DateTime startTime, DateTime last)
        {
            Originator = originator;
            Started = startTime;
            Last = last;
            Classifier = classifier;

            PrepareBatch(numberOfMessagesPrepared);
        }

        public void Forwarding()
        {
            RetryState = RetryState.Forwarding;

            Notifier?.Forwarding(requestId, retryType, TotalNumberOfMessages, GetProgress(), Failed, Started);
        }

        public void BatchForwarded(int numberOfMessagesForwarded)
        {
            NumberOfMessagesForwarded += numberOfMessagesForwarded;

            Notifier?.BatchForwarded(requestId, retryType, TotalNumberOfMessages, GetProgress(), Failed, Started);

            CheckForCompletion();
        }

        public void Skip(int numberOfMessagesSkipped)
        {
            NumberOfMessagesSkipped += numberOfMessagesSkipped;
            CheckForCompletion();
        }

        private void CheckForCompletion()
        {
            if (NumberOfMessagesForwarded + NumberOfMessagesSkipped == TotalNumberOfMessages)
            {
                RetryState = RetryState.Completed;
                CompletionTime = DateTime.UtcNow;

                Notifier?.Completed(requestId, retryType, Failed, GetProgress(), Started, CompletionTime.Value, Originator, NumberOfMessagesForwarded, Last ?? DateTime.MaxValue, Classifier);
                Log.Info($"Retry operation {requestId} completed. {NumberOfMessagesSkipped} messages skipped, {NumberOfMessagesForwarded} forwarded. Total {TotalNumberOfMessages}.");
            }
        }
        
        public Progress GetProgress()
        {
            var percentage = RetryOperationProgressCalculator.CalculateProgress(TotalNumberOfMessages, NumberOfMessagesPrepared, NumberOfMessagesForwarded, NumberOfMessagesSkipped, RetryState);
            var roundedPercentage = Math.Round(percentage, 2);
            
            var remaining = TotalNumberOfMessages - (NumberOfMessagesForwarded + NumberOfMessagesSkipped);

            return new Progress(roundedPercentage, NumberOfMessagesPrepared, NumberOfMessagesForwarded, NumberOfMessagesSkipped, remaining);
        }
    }
}
