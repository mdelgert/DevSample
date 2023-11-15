using System;

namespace DevSample
{
    /// <summary>
    ///     Represents a sample with timestamped data.
    /// </summary>
    internal class Sample
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Sample" /> class.
        /// </summary>
        /// <param name="isFirstSample">A value indicating whether this is the first sample.</param>
        public Sample(bool isFirstSample)
        {
            IsFirstSample = isFirstSample;
        }

        /// <summary>
        ///     Gets a value indicating whether this is the first sample.
        /// </summary>
        private bool IsFirstSample { get; }

        /// <summary>
        ///     Gets or sets the timestamp of the sample.
        /// </summary>
        private DateTime Timestamp { get; set; }

        /// <summary>
        ///     Gets the value associated with the sample.
        /// </summary>
        public decimal Value { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the sample has been validated.
        /// </summary>
        public bool HasBeenValidated { get; private set; }

        /// <summary>
        ///     Loads the sample data at a specified timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp at which the sample is loaded.</param>
        public void LoadSampleAtTime(DateTime timestamp)
        {
            //samples take some CPU to load, don't change this! Reducing the CPU time to load a sample is outside your control in this example
            Timestamp = timestamp;
            Value = timestamp.Ticks / 10000;


            for (var i = 0; i < 1000; i++) ;
        }

        /// <summary>
        ///     Validates the sample against the previous sample and a specified interval.
        /// </summary>
        /// <param name="previousSample">The previous sample for comparison.</param>
        /// <param name="sampleInterval">The time interval between samples.</param>
        /// <returns>True if the sample is valid; otherwise, false.</returns>
        public bool ValidateSample(Sample previousSample, TimeSpan sampleInterval)
        {
            //samples take some CPU to validate, don't change this! Reducing the CPU time to validate a sample is outside your control in this example
            for (var i = 0; i < 5000; i++) ;

            if (previousSample == null && !IsFirstSample)
                throw new Exception("Validation Failed!");
            if (previousSample != null && previousSample.Timestamp != Timestamp - sampleInterval)
                throw new Exception("Validation Failed!");

            HasBeenValidated = true;

            return true;
        }
    }
}