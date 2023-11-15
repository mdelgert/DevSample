using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevSample
{
    class SampleGenerator
    {
        private readonly DateTime _sampleStartDate;
        private readonly TimeSpan _sampleIncrement;
        private readonly List<Sample> _sampleList;

        public SampleGenerator(DateTime sampleStartDate, TimeSpan sampleIncrement, int samplesToGenerate)
        {
            // Use List<T> constructor with initial capacity this reduces number of reallocations as the list grows.
            _sampleList = new List<Sample>(samplesToGenerate);
            _sampleStartDate = sampleStartDate;
            _sampleIncrement = sampleIncrement;
        }

        /// <summary>
        /// Samples should be a time-descending ordered list
        /// </summary>
        public List<Sample> Samples { get { return _sampleList; } }

        public int SamplesValidated { get; private set; }

        public void LoadSamples(int samplesToGenerate)
        {
            //Complete: can we load samples faster?

            _sampleList.Clear();

            DateTime date = _sampleStartDate;

            for (int i = 0; i < samplesToGenerate; i++)
            {
                Sample s = new Sample(i == 0);
                s.LoadSampleAtTime(date);
                //_sampleList.Insert(0, s);
                _sampleList.Add(s); //800% performance increase
                date += _sampleIncrement;
            }
        }

        public void ValidateSamples()
        {
            //Complete: can we validate samples faster?

            int samplesValidated = 0;

            //80% performance increase
            //for (int i = 0; i < _sampleList.Count; i++)

            // Run loop in reverse order using Parallel.ForEach
            Parallel.ForEach(Partitioner.Create(0, _sampleList.Count), range =>
            {
                for (int i = range.Item2 - 1; i >= range.Item1; i--)
                {
                    //in this sample the ValidateSample is always true but assume that's not always the case
                    if (_sampleList[i].ValidateSample(i > 0 ? _sampleList[i - 1] : null, _sampleIncrement))
                        Interlocked.Increment(ref samplesValidated);
                }
            });

            SamplesValidated = samplesValidated;
        }
    }
}