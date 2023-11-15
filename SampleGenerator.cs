using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevSample
{
    internal class SampleGenerator
    {
        private readonly TimeSpan _sampleIncrement;
        private readonly List<Sample> _sampleList;
        private readonly DateTime _sampleStartDate;

        public SampleGenerator(DateTime sampleStartDate, TimeSpan sampleIncrement, int samplesToGenerate)
        {
            // Use List<T> constructor with initial capacity this reduces number of reallocation's as the list grows.
            _sampleList = new List<Sample>(samplesToGenerate);
            _sampleStartDate = sampleStartDate;
            _sampleIncrement = sampleIncrement;
        }

        /// <summary>
        ///     Samples should be a time-descending ordered list
        /// </summary>
        public IEnumerable<Sample> Samples => _sampleList;

        public int SamplesValidated { get; private set; }

        public void LoadSamples(int samplesToGenerate)
        {
            //Complete: can we load samples faster?
            _sampleList.Clear();

            var date = _sampleStartDate;

            for (var i = 0; i < samplesToGenerate; i++)
            {
                var s = new Sample(i == 0);

                s.LoadSampleAtTime(date);
                _sampleList.Add(s); //800% performance increase
                date += _sampleIncrement;
            }
        }

        public void ValidateSamples()
        {
            //Complete: can we validate samples faster?

            var samplesValidated = 0;

            // Run loop in reverse order using Parallel.ForEach 80% performance increase
            Parallel.ForEach(Partitioner.Create(0, _sampleList.Count), range =>
            {
                for (var i = range.Item2 - 1; i >= range.Item1; i--)
                    if (_sampleList[i]
                        .ValidateSample(i > 0 ? _sampleList[i - 1] : null,
                            _sampleIncrement)) //in this sample the ValidateSample is always true but assume that's not always the case
                        Interlocked.Increment(ref samplesValidated);
            });

            SamplesValidated = samplesValidated;
        }
    }
}